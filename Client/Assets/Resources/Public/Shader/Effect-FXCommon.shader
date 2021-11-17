Shader "Effect/FXCommon" {
    Properties
    {
        // _ColParam: xy(remap-min/max);zw(pan-u/v)
        // _ColParamB: x(alpha intensity);y(color intensity);w(On/Off);
        [HideInInspector] [Enum(Off, 0, Color,1)] _Tex01Type("Tex_01(RGBA):Off/Color", Float) = 1
        [HideInInspector] _MainTex("Tex_01", 2D) = "white" {}
        [HideInInspector] _TintColor("Tex_01 Color", Color) = (1,1,1,1)
        [HideInInspector] _Tex01RemapMin("Tex_01 Color Remap Min", Float) = 0
        [HideInInspector] _Tex01RemapMax("Tex_01 Color Remap Max", Float) = 1
        [HideInInspector] _Tex01ColorInten("Tex_01 Color Intensity", Float) = 1
        [HideInInspector] _Tex01PanU("Tex_01 Color Panner U", Float) = 0
        [HideInInspector] _Tex01PanV("Tex_01 Color Panner V", Float) = 0
        [HideInInspector] _Tex01AlphaInten("Tex_01 Alpha Intensity", Float) = 1

        //扰动
        // _NoiseParam: xy(pan-u/v);z(intensity);w(mode(0:Off;1:Turbulence;2:Multiply;3:Add))
        [HideInInspector] [Enum(Off, 0, Turbulence, 1, Multiply, 2, Add, 3)] _Tex02Type("Tex_02(R):Off/Turbulence/Multiply/Add", Float) = 0
        [HideInInspector] _NoiseTex("Tex_02", 2D) = "white" {}
        [HideInInspector] _Tex02Inten("Tex_02 Intensity", Float) = 0
        [HideInInspector] _Tex02PanU("Tex_02 Panner U", Float) = 0
        [HideInInspector] _Tex02PanV("Tex_02 Panner V", Float) = 0

        //溶解
        // _DisParam: x(sideAlphaIntensity);y(sideWidth);z(offset);w(mode(0:Off;1:Soft;2:Hard))
        // _DisParamB: xy(side color remap-min/max);z(side color intensity)
        [HideInInspector] [Enum(Off, 0, SoftDissolve, 1, HardDissolve, 2)] _Tex03Type("Tex_03(R):Off/SoftDissolve/HardDissolve", Float) = 0
        [HideInInspector] _DisTex("Tex_03(Dissolve)", 2D) = "black" {}
        [HideInInspector] _Tex03DissolveOffset("Tex_03 Dissolve Offset", Float) = 0
        [HideInInspector] _DisCol("Dissolve Side Color", Color) = (1,1,1,1)
        [HideInInspector] _DisColB("Dissolve Side ColorB", Color) = (1,1,1,1)
        [HideInInspector] _Tex03RemapMin("Tex_03 Side Color Remap Min", Float) = 0
        [HideInInspector] _Tex03RemapMax("Tex_03 Side Color Remap Max", Float) = 1
        [HideInInspector] _Tex03ColorInten("Tex_03 Side Color Intensity", Float) = 1
        [HideInInspector] _Tex03AlphaInten("Tex_03 Side Alpha Intensity", Float) = 1
        [HideInInspector] _Tex03SideWidth("Tex_03 Side Width", Float) = 0.1

        //遮罩
        // _MaskParam: x(intensity);y(offset);w(Off/Offset/Step)
        [HideInInspector] [Enum(Off, 0, LinearU, 1, LinearV, 2, StepU, 3, StepV, 4)] _Tex04Type("Tex_04(R):Off/Mask(Panner Linear/Step)", Float) = 0
        [HideInInspector] _MaskTex("Tex_04(Mask)", 2D) = "white" {}
        [HideInInspector] _Tex04Inten("Tex_04 Intensity", Float) = 1
        [HideInInspector] _Tex04OffsetStep("Tex_04 Offset", Float) = 0

        //菲涅尔
        // _FresParam: x(intensity);y(Power);z(FresAlphaAdd);w(On/Off)
        // _FresSideParam: xy(remap-min/max);z(intensity)
        [HideInInspector][Toggle] _Fres("Enable(Fresnel)", Float) = 0
        [HideInInspector] _FresPow("Fresnel Power", Float) = 1
        [HideInInspector] _FresAlphaInten("Fresnel Alpha Intensity", Float) = 1
        [HideInInspector] _FresAlphaAdd("Fresnel Alpha Add", Float) = 0
        [HideInInspector] _FresCol("Fresnel Side Color", Color) = (1,1,1,1)
        [HideInInspector] _FresRemapMin("Fresnel Side Remap Min", Float) = 0
        [HideInInspector] _FresRemapMax("Fresnel Side Remap Max", Float) = 1
        [HideInInspector] _FresSideInten("Fresnel Side Intensity", Float) = 1

        //SinAlpha
        // _AlphaParam: xy(remap-min/max);z(Speed);w(On/Off)
        [HideInInspector][Toggle] _Sin("Enable(SinAlpha)", Float) = 0
        [HideInInspector] _SinRemapMin("Sin Remap Min", Float) = 0
        [HideInInspector] _SinRemapMax("Sin Remap Max", Float) = 1
        [HideInInspector] _SinRate("Sin Rate", Float) = 1

        [HideInInspector] _Mode("__mode", Float) = 1.0
        [HideInInspector] _SrcBlend("__src", Float) = 5.0
        [HideInInspector] _DstBlend("__dst", Float) = 10.0
        [HideInInspector][Toggle] _ZWrite("ZWrite On", Float) = 0.0
        [HideInInspector] [Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull", Float) = 2
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("ZTest", Float) = 4
        [Toggle(FOG_ENABLE)] _FogOn("Fog On", Float) = 0

        _OffsetFactor("Offset Factor", Float) = 0
        _OffsetUnits("Offset Units", Float) = 0
    }

    SubShader
        {
            Tags { "QUEUE" = "Transparent"}
            Pass
            {
                Blend[_SrcBlend][_DstBlend]
                ZWrite[_ZWrite] Lighting Off
                Cull [_Cull]
                ZTest [_ZTest]
                Offset [_OffsetFactor],[_OffsetUnits]
                // AlphaTest GEqual

                Name "ForwardLit"
                Tags{"LightMode" = "Always"}

                HLSLPROGRAM
                #pragma vertex vert  
                #pragma fragment frag
                #pragma multi_compile __ _ADDITIVE_ON
                #pragma multi_compile __ LOCAL_HEIGHT_FOG
                #pragma multi_compile __ FOG_LINEAR
                #pragma multi_compile __ FOG_ENABLE

                #include "./FXCommonHLSL.hlsl"

                ENDHLSL
            }
        }
    CustomEditor "FXCommonKeyShaderGUI"
}
