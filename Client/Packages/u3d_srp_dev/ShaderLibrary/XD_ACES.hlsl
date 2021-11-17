#ifndef UNITY_XDACES_INCLUDED
#define UNITY_XDACES_INCLUDED

/////////////////REED_UE4_ACES_BEGIN/////////////////

float FilmSlope;// = 0.91;
float FilmToe;// = 0.53;
float FilmShoulder;// = 0.23;
float FilmBlackClip;// = 0;
float FilmWhiteClip;// = 0.035;

float3 UE4ACES(float3 aces)
{
	// "Glow" module constants
	const float RRT_GLOW_GAIN = 0.05;
	const float RRT_GLOW_MID = 0.08;

	float saturation = rgb_2_saturation(aces);
	float ycIn = rgb_2_yc(aces);
	float s = sigmoid_shaper((saturation - 0.4) / 0.2);
	float addedGlow = 1.0 + glow_fwd(ycIn, RRT_GLOW_GAIN * s, RRT_GLOW_MID);
	aces *= addedGlow;

	const float RRT_RED_SCALE = 0.82;
	const float RRT_RED_PIVOT = 0.03;
	const float RRT_RED_HUE = 0.0;
	const float RRT_RED_WIDTH = 135.0;

	// --- Red modifier --- //
	float hue = rgb_2_hue(aces);
	float centeredHue = center_hue(hue, RRT_RED_HUE);
	float hueWeight;
	{
		hueWeight = smoothstep(0.0, 1.0, 1.0 - abs(2.0 * centeredHue / RRT_RED_WIDTH));
		hueWeight *= hueWeight;
	}
	//float hueWeight = Square( smoothstep(0.0, 1.0, 1.0 - abs(2.0 * centeredHue / RRT_RED_WIDTH)) );

	aces.r += hueWeight * saturation * (RRT_RED_PIVOT - aces.r) * (1.0 - RRT_RED_SCALE);

	// Use ACEScg primaries as working space
	float3 acescg = max(0.0, ACES_to_ACEScg(aces));

	// Pre desaturate
	acescg = lerp(dot(acescg, AP1_RGB2Y).xxx, acescg, 0.96);

	const half ToeScale = 1 + FilmBlackClip - FilmToe;
	const half ShoulderScale = 1 + FilmWhiteClip - FilmShoulder;

	const float InMatch = 0.18;
	const float OutMatch = 0.18;

	float ToeMatch;
	if (FilmToe > 0.8)
	{
		// 0.18 will be on straight segment
		ToeMatch = (1 - FilmToe - OutMatch) / FilmSlope + log10(InMatch);
	}
	else
	{
		// 0.18 will be on toe segment

		// Solve for ToeMatch such that input of InMatch gives output of OutMatch.
		const float bt = (OutMatch + FilmBlackClip) / ToeScale - 1;
		ToeMatch = log10(InMatch) - 0.5 * log((1 + bt) / (1 - bt)) * (ToeScale / FilmSlope);
	}

	float StraightMatch = (1 - FilmToe) / FilmSlope - ToeMatch;
	float ShoulderMatch = FilmShoulder / FilmSlope - StraightMatch;

	half3 LogColor = log10(acescg);
	half3 StraightColor = FilmSlope * (LogColor + StraightMatch);

	half3 ToeColor = (-FilmBlackClip) + (2 * ToeScale) / (1 + exp((-2 * FilmSlope / ToeScale) * (LogColor - ToeMatch)));
	half3 ShoulderColor = (1 + FilmWhiteClip) - (2 * ShoulderScale) / (1 + exp((2 * FilmSlope / ShoulderScale) * (LogColor - ShoulderMatch)));

	ToeColor = LogColor < ToeMatch ? ToeColor : StraightColor;
	ShoulderColor = LogColor > ShoulderMatch ? ShoulderColor : StraightColor;

	half3 t = saturate((LogColor - ToeMatch) / (ShoulderMatch - ToeMatch));
	t = ShoulderMatch < ToeMatch ? 1 - t : t;
	t = (3 - 2 * t)*t*t;
	half3 linearCV = lerp(ToeColor, ShoulderColor, t);

	// Post desaturate
	linearCV = lerp(dot(float3(linearCV), AP1_RGB2Y), linearCV, 0.93);

	// Returning positive AP1 values
	//return max(0, linearCV);

	// Convert to display primary encoding
	// Rendering space RGB to XYZ
	float3 XYZ = mul(AP1_2_XYZ_MAT, linearCV);

	// Apply CAT from ACES white point to assumed observer adapted white point
	XYZ = mul(D60_2_D65_CAT, XYZ);

	// CIE XYZ to display primaries
	linearCV = mul(XYZ_2_REC709_MAT, XYZ);

	return linearCV;

}

/////////////////REED_UE4_ACES_END/////////////////


/////////////////REED_T3_ACES_BEGIN/////////////////

#if defined(SHADER_API_SWITCH) // We need more accuracy on Nintendo Switch to avoid NaN on extremely high values.
float3 T3NeutralCurve(float3 x, float T3_a, float T3_b, float T3_c, float T3_d, float T3_e, float T3_f)
#else
real3 T3NeutralCurve(float3 x, float T3_a, float T3_b, float T3_c, float T3_d, float T3_e, float T3_f)
#endif
{
	return ((x * (T3_a * x + T3_c * T3_b) + T3_d * T3_e) / (x * (T3_a * x + T3_b) + T3_d * T3_f)) - T3_e / T3_f;
}

float T3_a;
float T3_b;
float T3_c;
float T3_d;
float T3_e;
float T3_f;
float T3_whiteLevel;
//float T3_whiteClip;


real3 TonemapT3ACES(real3 x)
{
	// Tonemap
	//float T3_a = 0.2;
	//float T3_b = 0.29;
	//float T3_c = 0.24;
	//float T3_d = 0.272;
	//float T3_e = 0.02;
	//float T3_f = 0.3;
	//float T3_whiteLevel = 5.3;
	float T3_whiteClip = 1.0;


	real3 whiteScale = (1.0).xxx / T3NeutralCurve(T3_whiteLevel, T3_a, T3_b, T3_c, T3_d, T3_e, T3_f);
	x = T3NeutralCurve(x * whiteScale, T3_a, T3_b, T3_c, T3_d, T3_e, T3_f);
	x *= whiteScale;

	// Post-curve white point adjustment
	x /= T3_whiteClip.xxx;

	return x;
}

/////////////////REED_T3_ACES_END/////////////////

#endif