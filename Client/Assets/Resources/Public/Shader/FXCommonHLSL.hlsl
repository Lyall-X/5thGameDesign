#ifndef FXCOMMONKEY_HLSL_INCLUDED
#define FXCOMMONKEY_HLSL_INCLUDED

#include "Assets/Resources/Public/Shader/Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

uniform sampler2D _MainTex;
uniform sampler2D _MaskTex;
uniform sampler2D _NoiseTex;
uniform sampler2D _DisTex;

uniform half4 _MainTex_ST;
uniform half4 _MaskTex_ST;
uniform half4 _NoiseTex_ST;
uniform half4 _DisTex_ST;

uniform half _Tex01Type;
uniform half _Tex01RemapMin;
uniform half _Tex01RemapMax;
uniform half _Tex01ColorInten;
uniform half _Tex01PanU;
uniform half _Tex01PanV;
uniform half _Tex01AlphaInten;

uniform half _Tex02Type;
uniform half _Tex02Inten;
uniform half _Tex02PanU;
uniform half _Tex02PanV;

uniform half _Tex03Type;
uniform half _Tex03DissolveOffset;
uniform half _Tex03RemapMin;
uniform half _Tex03RemapMax;
uniform half _Tex03ColorInten;
uniform half _Tex03AlphaInten;
uniform half _Tex03SideWidth;

uniform half _Tex04Type;
uniform half _Tex04Inten;
uniform half _Tex04OffsetStep;

uniform half _Fres;
uniform half _FresPow;
uniform half _FresAlphaInten;
uniform half _FresAlphaAdd;
uniform half _FresRemapMin;
uniform half _FresRemapMax;
uniform half _FresSideInten;

uniform half _Sin;
uniform half _SinRemapMin;
uniform half _SinRemapMax;
uniform half _SinRate;

uniform half4 _TintColor;
uniform half4 _DisCol;
uniform half4 _DisColB;
uniform half4 _FresCol;

uniform half _Mode;



struct appdata_effect
{
    float4 vertex : POSITION;
    half4 uv : TEXCOORD0;
    half4 uv1 : TEXCOORD1;
    half4 color : COLOR;
    float3 normal : NORMAL;
};

struct v2f
{
    float4 pos : SV_POSITION;
    half4 uv : TEXCOORD0;// texcoord: xy(uv);z(mask offset);w(dissolve)
    half4 uv1 : TEXCOORD1;// texcoord1: xy(uv);zw(main/dissolve uv)

    half4 color   : COLOR;
    float3 viewDirWS : TEXCOORD2;
    float4 normalWS : TEXCOORD3;
};


half3 Remap(half3 s, half a1, half a2, half b1, half b2)
{
    s.x = b1 + (s.x - a1) * (b2 - b1) / (a2 - a1);
    s.y = b1 + (s.y - a1) * (b2 - b1) / (a2 - a1);
    s.z = b1 + (s.z - a1) * (b2 - b1) / (a2 - a1);
    s = max(0, s);
    return s;
}

half Remap(half s, half a1, half a2, half b1, half b2)
{
    s = b1 + (s - a1) * (b2 - b1) / (a2 - a1);
    return s;
}

v2f vert(appdata_effect v)
{
    v2f o;
    float4 positionWS = mul(UNITY_MATRIX_M, v.vertex);
    o.pos = mul(UNITY_MATRIX_VP, positionWS);
    float deltaTime = _Time.y % 100.0f;
    half2 custom1 = v.uv.zw;
    half2 custom2 = v.uv1.zw;

    half2 colPan = half2(_Tex01PanU, _Tex01PanV);
    half2 noisePan = half2(_Tex02PanU, _Tex02PanV);

    if (_Tex01Type > 0){
        o.uv.xy = TRANSFORM_TEX(v.uv.xy, _MainTex);
        o.uv.xy += deltaTime * colPan;
        o.uv.xy += custom2;
    }

    if (_Tex02Type > 0)
    {
        o.uv.zw = TRANSFORM_TEX(v.uv.xy, _NoiseTex);
        o.uv.zw += deltaTime * noisePan;
    }

    if (_Tex03Type > 0)
    {
        o.uv1.xy = TRANSFORM_TEX(v.uv.xy, _DisTex);
        o.uv1.xy += deltaTime * colPan;
        o.uv1.xy += custom2;
    }

    if (_Tex04Type > 0)
    {
        o.uv1.zw = TRANSFORM_TEX(v.uv.xy, _MaskTex);
        half offset = 0;
        if(_Tex04Type > 2.5){//step
            offset = round(custom1.x) * _Tex04OffsetStep;
            if(_Tex04Type < 3.5)//stepU
                o.uv1.z += offset;//3
            else//stepV
                o.uv1.w += offset;//4
        }
        else{//offset
            offset = custom1.x + _Tex04OffsetStep;
            if(_Tex04Type < 1.5)//offsetU
                o.uv1.z += offset;//1
            else//offsetV
                o.uv1.w += offset;//2
        }
    }



    o.color = v.color;
    o.color.rgb = o.color.rgb;

    float3 viewDirWS = SafeNormalize(GetCameraPositionWS() - positionWS.xyz);
    o.viewDirWS.xyz = viewDirWS;
    o.normalWS.xyz = normalize(mul((half3x3)UNITY_MATRIX_M, v.normal));

    o.normalWS.w = custom1.y;

    return o;
}

half4 frag(v2f i) : COLOR
{
    float2 mainUV = i.uv.xy;
    float2 disUV = i.uv1.xy;
    float2 maskUV = i.uv1.zw;
    half noise = 1;
    half3 mainCol = 1;
    half alpha = 1;
    half3 finalCol = 1;

    float fresnel = 1-saturate(dot(i.normalWS.xyz, i.viewDirWS.xyz));
    fresnel = pow(fresnel,_FresPow);

    if (_Tex02Type > 0)
    {
        noise = tex2D(_NoiseTex, i.uv.zw).r * _Tex02Inten;
        if(_Tex02Type < 1.5){
            //turbulence
            mainUV += noise;
            disUV += noise;
            maskUV += noise;
        }
    }

    if (_Tex01Type > 0){
        half4 mainMap = tex2D(_MainTex, mainUV);
        mainCol *= mainMap.rgb;
        alpha *= saturate(mainMap.a * _Tex01AlphaInten);
    }

    if (_Tex02Type > 1.5)
    {
        if(_Tex02Type < 2.5){
            //multiply
            mainCol *= noise;
            alpha *= noise;
        }
        else{
            //add
            mainCol += noise;
            alpha += noise;
        }
        finalCol = mainCol;
    }


    if(_Tex04Type > 0)
    {
        alpha *= tex2D(_MaskTex, maskUV).r * _Tex04Inten;
    }


    if (_Fres > 0)//fresAlpha
    {
        half fresAlpha = fresnel * _FresAlphaInten;
        fresAlpha += _FresAlphaAdd;
        alpha *= fresAlpha;
    }

    mainCol = Remap(mainCol.rgb,0,1,_Tex01RemapMin,_Tex01RemapMax);
    finalCol = max(mainCol,0) * _TintColor.rgb;
    mainCol = saturate(mainCol);

    if (_Tex03Type > 0)
    {
        float curve = 1-clamp(i.normalWS.w,-0.01,1) + _Tex03DissolveOffset; //custom1.y
        half dis = clamp(tex2D(_DisTex, disUV).r * alpha,-0.01,1);
        half disAlpha;
        if(_Tex03Type > 1.5)
        {
            //hard dissolve
            disAlpha = step(curve, dis);
            half edge = step(curve + _Tex03SideWidth,dis);
            edge = saturate(disAlpha - edge);

            half side = saturate(Remap(dis,0,1,_Tex03RemapMin,_Tex03RemapMax));
            half3 sideCol = lerp(_DisColB.rgb,_DisCol.rgb,side) * _Tex03ColorInten;
            finalCol = lerp(finalCol,sideCol,edge);
        }
        else
        {
            //soft dissolve
            disAlpha = dis - curve;
        }
        disAlpha = saturate(disAlpha * _Tex03AlphaInten * dis);
        alpha *= disAlpha;
    }

    if (_Fres > 0)
    {
        half3 fresCol = fresnel * mainCol;
        fresCol = saturate(Remap(fresCol,0,1,_FresRemapMin,_FresRemapMax));
        finalCol = lerp(finalCol,_FresCol.rgb * _FresSideInten,fresCol);
    }

    if (_Sin > 0)
    {
        float sinTime = sin(_Time.y % 100.0f * _SinRate);
        sinTime = Remap(sinTime,-1,1,_SinRemapMin,_SinRemapMax);
        alpha *= sinTime;
    }

    half4 c = i.color;
    c.rgb *= finalCol * _Tex01ColorInten;
    c.a *= alpha;
    c.a = saturate(c.a);
    return c;
}
#endif
