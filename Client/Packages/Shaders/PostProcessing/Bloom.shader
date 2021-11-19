Shader "Hidden/Universal Render Pipeline/Bloom"
{
    Properties
    {
        _MainTex("Source", 2D) = "white" {}
    }

    HLSLINCLUDE

        #pragma multi_compile_local _ _USE_RGBM

        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Filtering.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Shaders/PostProcessing/Common.hlsl"

        TEXTURE2D_X(_MainTex);
        TEXTURE2D_X(_MainTexLowMip);

        float4 _MainTex_TexelSize;
        float4 _MainTexLowMip_TexelSize;

        float4 _Params; // x: scatter, y: clamp, z: threshold (linear), w: threshold knee
        float4 _Bloom_PPOnlyInfo; // xy: PP size, zw: PP center
		float _Bloom_PPDepthPeelingValue;

        #define Scatter             _Params.x
        #define ClampMax            _Params.y
        #define Threshold           _Params.z
        #define ThresholdKnee       _Params.w

        half4 EncodeHDR(half3 color)
        {
        #if _USE_RGBM
            half4 outColor = EncodeRGBM(color);
        #else
            half4 outColor = half4(color, 1.0);
        #endif

        #if UNITY_COLORSPACE_GAMMA
            return half4(sqrt(outColor.xyz), outColor.w); // linear to γ
        #else
            return outColor;
        #endif
        }

        half3 DecodeHDR(half4 color)
        {
        #if UNITY_COLORSPACE_GAMMA
            color.xyz *= color.xyz; // γ to linear
        #endif

        #if _USE_RGBM
            return DecodeRGBM(color);
        #else
            return color.xyz;
        #endif
        }

        // Draw only part view vertex shader - Begin
        // Bloom's input vertex is from 0 to 1 in xy coordinates
        Varyings CropVert(Attributes input)
        {
            float2 size = float2(_Bloom_PPOnlyInfo.x, _Bloom_PPOnlyInfo.y);
            float2 offset = float2(_Bloom_PPOnlyInfo.z, _Bloom_PPOnlyInfo.w);

            Varyings output;
            UNITY_SETUP_INSTANCE_ID(input);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
            output.positionCS = TransformObjectToHClip(input.positionOS.xyz * float3(size, 1.0f) + float3(offset, 0.0f));
            output.uv = input.uv * size + offset;
            return output;
        }
        //Draw only part view vertex shader - End

        half4 FragPrefilter(Varyings input) : SV_Target
        {
			input.uv.x = clamp(input.uv.x, 0.0f, 1.0f);
			input.uv.y = clamp(input.uv.y, 0.0f, 1.0f);
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
            float2 uv = UnityStereoTransformScreenSpaceTex(input.uv);
#if XRP_OPT_DEPTH_PASS && XRP_OPT_CHARACTER_PP_ONLY
            half4 colorAll = SAMPLE_TEXTURE2D_X(_MainTex, sampler_LinearClamp, uv).xyzw;
            half3 color = colorAll.xyz * step(_Bloom_PPDepthPeelingValue, colorAll.w);
#else
            half3 color = SAMPLE_TEXTURE2D_X(_MainTex, sampler_LinearClamp, uv).xyz;
#endif

        #if UNITY_COLORSPACE_GAMMA
            color = SRGBToLinear(color);
        #endif

            // User controlled clamp to limit crazy high broken spec
            color = min(ClampMax, color);

            // Thresholding
            half brightness = Max3(color.r, color.g, color.b);
            half softness = clamp(brightness - Threshold + ThresholdKnee, 0.0, 2.0 * ThresholdKnee);
            softness = (softness * softness) / (4.0 * ThresholdKnee + 1e-4);
            half multiplier = max(brightness - Threshold, softness) / max(brightness, 1e-4);
            color *= multiplier;

            return EncodeHDR(color);
        }

        half4 FragBlurH(Varyings input) : SV_Target
        {
			input.uv.x = clamp(input.uv.x, 0.0f, 1.0f);
			input.uv.y = clamp(input.uv.y, 0.0f, 1.0f);
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
            float texelSize = _MainTex_TexelSize.x * 2.0;
            float2 uv = UnityStereoTransformScreenSpaceTex(input.uv);

            // 9-tap gaussian blur on the downsampled source
            half3 c0 = DecodeHDR(SAMPLE_TEXTURE2D_X(_MainTex, sampler_LinearClamp, uv - float2(texelSize * 4.0, 0.0)));
            half3 c1 = DecodeHDR(SAMPLE_TEXTURE2D_X(_MainTex, sampler_LinearClamp, uv - float2(texelSize * 3.0, 0.0)));
            half3 c2 = DecodeHDR(SAMPLE_TEXTURE2D_X(_MainTex, sampler_LinearClamp, uv - float2(texelSize * 2.0, 0.0)));
            half3 c3 = DecodeHDR(SAMPLE_TEXTURE2D_X(_MainTex, sampler_LinearClamp, uv - float2(texelSize * 1.0, 0.0)));
            half3 c4 = DecodeHDR(SAMPLE_TEXTURE2D_X(_MainTex, sampler_LinearClamp, uv                               ));
            half3 c5 = DecodeHDR(SAMPLE_TEXTURE2D_X(_MainTex, sampler_LinearClamp, uv + float2(texelSize * 1.0, 0.0)));
            half3 c6 = DecodeHDR(SAMPLE_TEXTURE2D_X(_MainTex, sampler_LinearClamp, uv + float2(texelSize * 2.0, 0.0)));
            half3 c7 = DecodeHDR(SAMPLE_TEXTURE2D_X(_MainTex, sampler_LinearClamp, uv + float2(texelSize * 3.0, 0.0)));
            half3 c8 = DecodeHDR(SAMPLE_TEXTURE2D_X(_MainTex, sampler_LinearClamp, uv + float2(texelSize * 4.0, 0.0)));

            half3 color = c0 * 0.01621622 + c1 * 0.05405405 + c2 * 0.12162162 + c3 * 0.19459459
                        + c4 * 0.22702703
                        + c5 * 0.19459459 + c6 * 0.12162162 + c7 * 0.05405405 + c8 * 0.01621622;

            return EncodeHDR(color);
        }

        half4 FragBlurV(Varyings input) : SV_Target
        {
			input.uv.x = clamp(input.uv.x, 0.0f, 1.0f);
			input.uv.y = clamp(input.uv.y, 0.0f, 1.0f);
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
            float texelSize = _MainTex_TexelSize.y;
            float2 uv = UnityStereoTransformScreenSpaceTex(input.uv);

            // Optimized bilinear 5-tap gaussian on the same-sized source (9-tap equivalent)
            half3 c0 = DecodeHDR(SAMPLE_TEXTURE2D_X(_MainTex, sampler_LinearClamp, uv - float2(0.0, texelSize * 3.23076923)));
            half3 c1 = DecodeHDR(SAMPLE_TEXTURE2D_X(_MainTex, sampler_LinearClamp, uv - float2(0.0, texelSize * 1.38461538)));
            half3 c2 = DecodeHDR(SAMPLE_TEXTURE2D_X(_MainTex, sampler_LinearClamp, uv                                      ));
            half3 c3 = DecodeHDR(SAMPLE_TEXTURE2D_X(_MainTex, sampler_LinearClamp, uv + float2(0.0, texelSize * 1.38461538)));
            half3 c4 = DecodeHDR(SAMPLE_TEXTURE2D_X(_MainTex, sampler_LinearClamp, uv + float2(0.0, texelSize * 3.23076923)));

            half3 color = c0 * 0.07027027 + c1 * 0.31621622
                        + c2 * 0.22702703
                        + c3 * 0.31621622 + c4 * 0.07027027;

            return EncodeHDR(color);
        }

        half3 Upsample(float2 uv)
        {
			uv.x = clamp(uv.x, 0.0f, 1.0f);
			uv.y = clamp(uv.y, 0.0f, 1.0f);
            half3 highMip = DecodeHDR(SAMPLE_TEXTURE2D_X(_MainTex, sampler_LinearClamp, uv));

        #if _BLOOM_HQ && !defined(SHADER_API_GLES)
            half3 lowMip = DecodeHDR(SampleTexture2DBicubic(TEXTURE2D_X_ARGS(_MainTexLowMip, sampler_LinearClamp), uv, _MainTexLowMip_TexelSize.zwxy, (1.0).xx, unity_StereoEyeIndex));
        #else
            half3 lowMip = DecodeHDR(SAMPLE_TEXTURE2D_X(_MainTexLowMip, sampler_LinearClamp, uv));
        #endif

            return lerp(highMip, lowMip, Scatter);
        }

        half4 FragUpsample(Varyings input) : SV_Target
        {
			input.uv.x = clamp(input.uv.x, 0.0f, 1.0f);
			input.uv.y = clamp(input.uv.y, 0.0f, 1.0f);
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
            half3 color = Upsample(UnityStereoTransformScreenSpaceTex(input.uv));
            return EncodeHDR(color);
        }

        half4 DualFilterDownsample(Varyings input) : SV_Target
        {
			input.uv.x = clamp(input.uv.x, 0.0f, 1.0f);
			input.uv.y = clamp(input.uv.y, 0.0f, 1.0f);
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
            float2 halfPixel = _MainTex_TexelSize.xy * 0.5f;
            float2 uv = UnityStereoTransformScreenSpaceTex(input.uv);

            half3 sum = DecodeHDR(SAMPLE_TEXTURE2D_X(_MainTex, sampler_LinearClamp, uv)) * 4.0f;
            sum += DecodeHDR(SAMPLE_TEXTURE2D_X(_MainTex, sampler_LinearClamp, uv - halfPixel.xy));
            sum += DecodeHDR(SAMPLE_TEXTURE2D_X(_MainTex, sampler_LinearClamp, uv + halfPixel.xy));
            sum += DecodeHDR(SAMPLE_TEXTURE2D_X(_MainTex, sampler_LinearClamp, uv + float2(halfPixel.x, -halfPixel.y)));
            sum += DecodeHDR(SAMPLE_TEXTURE2D_X(_MainTex, sampler_LinearClamp, uv - float2(halfPixel.x, -halfPixel.y)));

            return EncodeHDR(sum / 8.0f);
        }

        half4 DualFilterUpsample(Varyings input) : SV_Target
        {
			input.uv.x = clamp(input.uv.x, 0.0f, 1.0f);
			input.uv.y = clamp(input.uv.y, 0.0f, 1.0f);
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
            float2 halfPixel = _MainTexLowMip_TexelSize.xy * 0.5f;
            float2 uv = UnityStereoTransformScreenSpaceTex(input.uv);

            half3 highMip = DecodeHDR(SAMPLE_TEXTURE2D_X(_MainTex, sampler_LinearClamp, uv));
            half3 sum = DecodeHDR(SAMPLE_TEXTURE2D_X(_MainTexLowMip, sampler_LinearClamp, uv + float2(-halfPixel.x * 2.0f, 0.0f)));
            sum += DecodeHDR(SAMPLE_TEXTURE2D_X(_MainTexLowMip, sampler_LinearClamp, uv + float2(-halfPixel.x, halfPixel.y))) * 2.0f;
            sum += DecodeHDR(SAMPLE_TEXTURE2D_X(_MainTexLowMip, sampler_LinearClamp, uv + float2(0.0f, halfPixel.y * 2.0f)));
            sum += DecodeHDR(SAMPLE_TEXTURE2D_X(_MainTexLowMip, sampler_LinearClamp, uv + float2(halfPixel.x, halfPixel.y))) * 2.0f;
            sum += DecodeHDR(SAMPLE_TEXTURE2D_X(_MainTexLowMip, sampler_LinearClamp, uv + float2(halfPixel.x * 2.0f, 0.0f)));
            sum += DecodeHDR(SAMPLE_TEXTURE2D_X(_MainTexLowMip, sampler_LinearClamp, uv + float2(halfPixel.x, -halfPixel.y))) * 2.0f;
            sum += DecodeHDR(SAMPLE_TEXTURE2D_X(_MainTexLowMip, sampler_LinearClamp, uv + float2(0.0f, -halfPixel.y * 2.0f)));
            sum += DecodeHDR(SAMPLE_TEXTURE2D_X(_MainTexLowMip, sampler_LinearClamp, uv + float2(-halfPixel.x, -halfPixel.y))) * 2.0f;

            return EncodeHDR(lerp(highMip, sum / 12.0f, Scatter));
        }

        half4 FragDownsample(Varyings input)  : SV_Target
        {
			input.uv.x = clamp(input.uv.x, 0.0f, 1.0f);
			input.uv.y = clamp(input.uv.y, 0.0f, 1.0f);
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
            float2 uv = UnityStereoTransformScreenSpaceTex(input.uv);
            return SAMPLE_TEXTURE2D_X(_MainTex, sampler_LinearClamp, uv).xyzw;
        }

    ENDHLSL

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"}
        LOD 100
        ZTest Always ZWrite Off Cull Off

        Pass
        {
            Name "Bloom Prefilter"

            HLSLPROGRAM
                #pragma vertex Vert
                #pragma fragment FragPrefilter
            ENDHLSL
        }

        Pass
        {
            Name "Bloom Blur Horizontal"

            HLSLPROGRAM
                #pragma vertex Vert
                #pragma fragment FragBlurH
            ENDHLSL
        }

        Pass
        {
            Name "Bloom Blur Vertical"

            HLSLPROGRAM
                #pragma vertex Vert
                #pragma fragment FragBlurV
            ENDHLSL
        }

        Pass
        {
            Name "Bloom Upsample"

            HLSLPROGRAM
                #pragma vertex Vert
                #pragma fragment FragUpsample
                #pragma multi_compile_local _ _BLOOM_HQ
            ENDHLSL
        }

        Pass
        {
            Name "Bloom DualFilter Downsample"

            HLSLPROGRAM
                #pragma vertex Vert
                #pragma fragment DualFilterDownsample
                #pragma multi_compile_local _ _BLOOM_HQ
            ENDHLSL
        }

        Pass
        {
            Name "Bloom DualFilter Upsample"

            HLSLPROGRAM
                #pragma vertex Vert
                #pragma fragment DualFilterUpsample
                #pragma multi_compile_local _ _BLOOM_HQ
            ENDHLSL
        }

        Pass
        {
            Name "Bloom Prefilter Crop"

            HLSLPROGRAM
                #pragma vertex CropVert
                #pragma fragment FragPrefilter
            ENDHLSL
        }

        Pass
        {
            Name "Bloom Blur Horizontal Crop"

            HLSLPROGRAM
                #pragma vertex CropVert
                #pragma fragment FragBlurH
            ENDHLSL
        }

        Pass
        {
            Name "Bloom Blur Vertical Crop"

            HLSLPROGRAM
                #pragma vertex CropVert
                #pragma fragment FragBlurV
            ENDHLSL
        }

        Pass
        {
            Name "Bloom Upsample Crop"

            HLSLPROGRAM
                #pragma vertex CropVert
                #pragma fragment FragUpsample
                #pragma multi_compile_local _ _BLOOM_HQ
            ENDHLSL
        }

        Pass
        {
            Name "Bloom DualFilter Downsample Crop"

            HLSLPROGRAM
                #pragma vertex CropVert
                #pragma fragment DualFilterDownsample
                #pragma multi_compile_local _ _BLOOM_HQ
            ENDHLSL
        }

        Pass
        {
            Name "Bloom DualFilter Upsample Crop"

            HLSLPROGRAM
                #pragma vertex CropVert
                #pragma fragment DualFilterUpsample
                #pragma multi_compile_local _ _BLOOM_HQ
            ENDHLSL
        }

        Pass
        {
            Name "Bloom Downsample Crop"

            HLSLPROGRAM
                #pragma vertex CropVert
                #pragma fragment FragDownsample
                #pragma multi_compile_local _ _BLOOM_HQ
            ENDHLSL
        }
    }
}
