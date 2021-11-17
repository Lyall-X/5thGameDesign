using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace UnityEditor
{
    internal class FXCommonKeyShaderGUI : ShaderGUI 
    {

        public enum BlendMode
        {
            Additive,
            AlphaBlend
        }
        private static class Styles
        {
            public static readonly string[] mainMode = new string[]{"Off","Color"};
            public static readonly string[] noiseMode = new string[]{"Off","Turbulence","Multiply","Add"};
            public static readonly string[] disMode = new string[]{"Off","Soft","Hard"};
            public static readonly string[] maskMode = new string[]{"Off","LinearU","LinearV","StepU","StepV"};
            public static readonly string[] blendNames = System.Enum.GetNames (typeof (BlendMode));
        }

        private Material[] mats;
        private static string[] propNames = new string[]{
            "_Tex01Type","_MainTex","_TintColor","_Tex01RemapMin","_Tex01RemapMax",
            "_Tex01ColorInten","_Tex01PanU","_Tex01PanV","_Tex01AlphaInten",
            "_Tex02Type","_NoiseTex","_Tex02Inten","_Tex02PanU","_Tex02PanV",
            "_Tex03Type","_DisTex","_Tex03DissolveOffset","_DisCol","_DisColB",
            "_Tex03RemapMin","_Tex03RemapMax","_Tex03ColorInten","_Tex03AlphaInten","_Tex03SideWidth",
            "_Tex04Type","_MaskTex","_Tex04Inten","_Tex04OffsetStep",
            "_Fres","_FresPow","_FresAlphaInten","_FresAlphaAdd",
            "_FresCol","_FresRemapMin","_FresRemapMax","_FresSideInten",
            "_Sin","_SinRemapMin","_SinRemapMax","_SinRate",
            "_Mode","_SrcBlend","_DstBlend","_ZWrite","_Cull"
        };
        private Dictionary<string, MaterialProperty> propsAll;


        private void FindProperties (MaterialProperty[] props)
        {
            propsAll = new Dictionary<string, MaterialProperty>();
            foreach(string propName in propNames){
                propsAll.Add(propName, FindProperty (propName, props));
            }
        }

        private void SetupMaterialWithBlendMode(BlendMode blendMode)
        {
            MaterialProperty src = propsAll["_SrcBlend"];
            MaterialProperty dst = propsAll["_DstBlend"];
            switch (blendMode)
            {
            case BlendMode.Additive:
                src.floatValue = (float)UnityEngine.Rendering.BlendMode.SrcAlpha;
                dst.floatValue = (float)UnityEngine.Rendering.BlendMode.One;
                break;
            case BlendMode.AlphaBlend:
                src.floatValue = (float)UnityEngine.Rendering.BlendMode.SrcAlpha;
                dst.floatValue = (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha;
                break;
            }
        }

        // public static void MaterialChanged(Material material)
        // {
        //     SetupMaterialWithBlendMode(material, (BlendMode)material.GetFloat(Styles.propertyMode));
        // }
        private void BlendModePopup(MaterialEditor materialEditor)
        {
            MaterialProperty blendMode = propsAll["_Mode"];
            EditorGUI.showMixedValue = blendMode.hasMixedValue;
            var mode = (BlendMode)blendMode.floatValue;
            
            if(mats.Length == 1)
            {
                mode = (BlendMode)EditorGUILayout.Popup("Rendering Mode", (int)mode, Styles.blendNames);
                SetupMaterialWithBlendMode(mode);
                blendMode.floatValue = (float)mode;
            }
            else
            {
                EditorGUI.BeginChangeCheck();
                mode = (BlendMode)EditorGUILayout.Popup("Rendering Mode", (int)mode, Styles.blendNames);
                if (EditorGUI.EndChangeCheck())
                {
                    materialEditor.RegisterPropertyChangeUndo("Rendering Mode");
                    SetupMaterialWithBlendMode(mode);
                    blendMode.floatValue = (float)mode;
                }
            }
            
            EditorGUI.showMixedValue = false;
        
        }

        private void MainTexSet(MaterialEditor materialEditor)
        {
            MaterialProperty mode = propsAll["_Tex01Type"];
            materialEditor.ShaderProperty(mode, "Tex_01(RGBA):Off/Color");
            if(mode.floatValue > 0){
                materialEditor.ShaderProperty(propsAll["_MainTex"], "Tex_01");
                materialEditor.ShaderProperty(propsAll["_TintColor"], "Tex_01 Color");

                materialEditor.ShaderProperty(propsAll["_Tex01RemapMin"], "Tex_01 Color Remap Min");
                materialEditor.ShaderProperty(propsAll["_Tex01RemapMax"], "Tex_01 Color Remap Max");
                materialEditor.ShaderProperty(propsAll["_Tex01ColorInten"], "Tex_01 Color Intensity");
                materialEditor.ShaderProperty(propsAll["_Tex01PanU"], "Tex_01 Color Panner U");
                materialEditor.ShaderProperty(propsAll["_Tex01PanV"], "Tex_01 Color Panner V");
                materialEditor.ShaderProperty(propsAll["_Tex01AlphaInten"], "Tex_01 Alpha Intensity");
            }
            else{
                materialEditor.ShaderProperty(propsAll["_TintColor"], "Tex_01 Color");
                materialEditor.ShaderProperty(propsAll["_Tex01RemapMin"], "Tex_01 Color Remap Min");
                materialEditor.ShaderProperty(propsAll["_Tex01RemapMax"], "Tex_01 Color Remap Max");
                materialEditor.ShaderProperty(propsAll["_Tex01ColorInten"], "Tex_01 Color Intensity");
            }
        }

        private void NoiseTexSet(MaterialEditor materialEditor)
        {
            MaterialProperty mode = propsAll["_Tex02Type"];
            materialEditor.ShaderProperty(mode, "Tex_02(R):Off/Turbulence/Multiply/Add");
            if(mode.floatValue > 0){
                materialEditor.ShaderProperty(propsAll["_NoiseTex"], "Tex_02");
                materialEditor.ShaderProperty(propsAll["_Tex02Inten"], "Tex_02 Intensity");
                materialEditor.ShaderProperty(propsAll["_Tex02PanU"], "Tex_02 Panner U");
                materialEditor.ShaderProperty(propsAll["_Tex02PanV"], "Tex_02 Panner V");
            }
        }

        private void DissolveTexSet(MaterialEditor materialEditor)
        {
            MaterialProperty mode = propsAll["_Tex03Type"];
            materialEditor.ShaderProperty(mode, "Tex_03(R):Off/SoftDissolve/HardDissolve");
            if(mode.floatValue > 0){
                materialEditor.ShaderProperty(propsAll["_DisTex"], "Tex_03(Dissolve)");
                materialEditor.ShaderProperty(propsAll["_Tex03DissolveOffset"], "Tex_03 Dissolve Offset");
                materialEditor.ShaderProperty(propsAll["_DisCol"], "Tex_03 Side Color01");
                materialEditor.ShaderProperty(propsAll["_DisColB"], "Tex_03 Side Color02");
                materialEditor.ShaderProperty(propsAll["_Tex03RemapMin"], "Tex_03 Side Color Remap Min");
                materialEditor.ShaderProperty(propsAll["_Tex03RemapMax"], "Tex_03 Side Color Remap Max");
                materialEditor.ShaderProperty(propsAll["_Tex03ColorInten"], "Tex_03 Side Color Intensity");
                materialEditor.ShaderProperty(propsAll["_Tex03AlphaInten"], "Tex_03 Side Alpha Intensity");
                materialEditor.ShaderProperty(propsAll["_Tex03SideWidth"], "Tex_03 Side Width");
            }
        }

        private void MaskTexSet(MaterialEditor materialEditor)
        {
            MaterialProperty mode = propsAll["_Tex04Type"];
            materialEditor.ShaderProperty(mode, "Tex_04(R):Off/Mask(Panner Linear/Step)");
            if(mode.floatValue > 0){
                materialEditor.ShaderProperty(propsAll["_MaskTex"], "Tex_04(Mask)");
                materialEditor.ShaderProperty(propsAll["_Tex04Inten"], "Tex_04 Intensity");
                materialEditor.ShaderProperty(propsAll["_Tex04OffsetStep"], "Tex_04 Offset(Step)");
            }
        }

        private void FresnelSet(MaterialEditor materialEditor)
        {
            MaterialProperty mode = propsAll["_Fres"];
            materialEditor.ShaderProperty(mode, "Enable(Fresnel)");
            if(mode.floatValue > 0){
                materialEditor.ShaderProperty(propsAll["_FresPow"], "Fresnel Power");
                materialEditor.ShaderProperty(propsAll["_FresAlphaInten"], "Fresnel Alpha Intensity");
                materialEditor.ShaderProperty(propsAll["_FresAlphaAdd"], "Fresnel Alpha Add");
                materialEditor.ShaderProperty(propsAll["_FresCol"], "Fresnel Side Color");
                materialEditor.ShaderProperty(propsAll["_FresRemapMin"], "Fresnel Side Remap Min");
                materialEditor.ShaderProperty(propsAll["_FresRemapMax"], "Fresnel Side Remap Max");
                materialEditor.ShaderProperty(propsAll["_FresSideInten"], "Fresnel Side Intensity");
            }
        }

        private void AlphaSet(MaterialEditor materialEditor)
        {
            MaterialProperty mode = propsAll["_Sin"];
            materialEditor.ShaderProperty(mode, "Enable(SinAlpha)");
            if(mode.floatValue > 0){
                materialEditor.ShaderProperty(propsAll["_SinRemapMin"], "Sin Remap Min");
                materialEditor.ShaderProperty(propsAll["_SinRemapMax"], "Sin Remap Max");
                materialEditor.ShaderProperty(propsAll["_SinRate"], "Sin Rate");
            }
        }

        public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader)
        {
            base.AssignNewShaderToMaterial(material,oldShader,newShader);
            if(oldShader == null)
                return;
            if(!oldShader.name.Contains("RO/Effect/"))
            {
                material.SetFloat("_Mode", 1f);
                material.SetFloat("_ZWrite", 0.0f);
            }
            if(newShader.name.Contains("Base"))
            {
                material.SetFloat("_Fres", 0f);
                material.SetFloat("_Sin", 0f);
            }
        }

        public override void OnGUI (MaterialEditor materialEditor, MaterialProperty[] props)
        {
            base.OnGUI(materialEditor, props);
            var objs = Selection.objects;
            mats = new Material[objs.Length];
            for(int i=0; i< objs.Length; i++)
            {
                if(objs[i].GetType() == typeof(Material))
                {
                    mats[i] = objs[i] as Material;
                }
                else if(objs[i].GetType() == typeof(UnityEngine.GameObject))
                {
                    GameObject obj = (UnityEngine.GameObject)objs[i];
                    mats[i] = obj.GetComponent<Renderer>().sharedMaterial;
                }
            }
            if(mats == null || mats.Length == 0)
                return;
            FindProperties (props);
            EditorGUILayout.LabelField("===================↑不要改↑=================");
            // if(GUILayout.Button("Your ButtonText")) {
            //  //add everthing the button would do.
            // }
            materialEditor.ShaderProperty(propsAll["_Cull"], "Cull");
            materialEditor.ShaderProperty(propsAll["_ZWrite"], "Zwrite");
            BlendModePopup(materialEditor);
            MainTexSet(materialEditor);
            NoiseTexSet(materialEditor);
            DissolveTexSet(materialEditor);
            MaskTexSet(materialEditor);
            if(mats[0].shader.name.Contains("Better"))
            {
                FresnelSet(materialEditor);
                AlphaSet(materialEditor);
            }
        }
    }
} // namespace UnityEditor