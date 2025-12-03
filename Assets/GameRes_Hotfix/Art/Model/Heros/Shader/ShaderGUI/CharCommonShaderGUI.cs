using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;


namespace F3Shaders
{
    public static class CharCommonShaderGUI
    {
        public static class KeywordStrings
        {
            public const string _METALLIC_AO_GLOSS_MIX_MAP = "_CE_METALLIC_AO_GLOSS_MIX_MAP";
            public const string _MATCAP_AO_GLOSS_MIX_MAP = "_CE_MATCAP_AO_GLOSS_MIX_MAP";
        }

        public enum KeywordSwitch
        {
            Off = 0,
            On,
        }

        public static class Styles
        {
            public static GUIContent FrozenTexText =
                    EditorGUIUtility.TrTextContent("Fronzen Map", "Sets and configures the map for the fronzen effect.");
            public static GUIContent metallicAoGlossMapText =
                    EditorGUIUtility.TrTextContent("Metallic-AO-Gloss Map", "Sets and configures the map for PBR.");
            public static GUIContent matcapAoGlossMapText =
                    EditorGUIUtility.TrTextContent("Matcap-AO-Gloss Map", "Sets and configures the map for matcap.");
            public static GUIContent matcapMapText =
                    EditorGUIUtility.TrTextContent("Matcap Map", "Sets and configures the map for matcap.");
        }

        public class CommonProperties
        {
            public MaterialProperty bumpMapProp;
            public MaterialProperty bumpScaleProp;

            public MaterialProperty specularColorProp;
            public MaterialProperty specularShininessProp;

            public MaterialProperty matcapMixMapProp;
            public MaterialProperty matcapProp;
            public MaterialProperty matcapIntensityProp;

            public MaterialProperty metallicAoGlossMapProp;
            public MaterialProperty metallicProp;
            public MaterialProperty smoothnessProp;

            public MaterialProperty frozenEnable;
            public MaterialProperty frozenTex;
            public MaterialProperty frozenColor;
            public MaterialProperty frozenIntensity;
            public MaterialProperty frozenRange;

            public MaterialProperty fresnelEnable;
            public MaterialProperty fresnelColor;
            public MaterialProperty fresnelIntensity;
            public MaterialProperty fresnelRange;

            public MaterialProperty beatenRimEnable;
            public MaterialProperty beatenRimColor;
            public MaterialProperty beatenRimIntensity;
            public MaterialProperty beatenRimRange;

            public readonly List<MaterialProperty> ExtraProperties = new List<MaterialProperty>();

            private readonly List<MaterialProperty> FoundProperties = new List<MaterialProperty>();

            public CommonProperties(MaterialProperty[] properties)
            {
                bumpMapProp = FindProperty("_BumpMap", properties, false);
                bumpScaleProp = FindProperty("_BumpScale", properties, false);

                specularColorProp = FindProperty("_SpecColor", properties, false);
                specularShininessProp = FindProperty("_Shininess", properties, false);

                matcapMixMapProp = FindProperty("_MixMap", properties, false);
                matcapProp = FindProperty("_Matcap", properties, false);
                matcapIntensityProp = FindProperty("_MatcapIntensity", properties, false);

                metallicAoGlossMapProp = FindProperty("_MetallicAoGlossMap", properties, false);

                metallicProp = FindProperty("_Metallic", properties, false);
                smoothnessProp = FindProperty("_Smoothness", properties, false);

                frozenEnable =          FindProperty("_EnableFrozen", properties, false);
                frozenTex =             FindProperty("_FrozenTex", properties, false);
                frozenColor =           FindProperty("_FrozenColor", properties, false);
                frozenIntensity =       FindProperty("_FrozenIntensity", properties, false);
                frozenRange =           FindProperty("_FrozenRange", properties, false);

                fresnelEnable =         FindProperty("_EnableFresnel", properties, false);
                fresnelColor =          FindProperty("_FresnelColor", properties, false);
                fresnelIntensity =      FindProperty("_FresnelIntensity", properties, false);
                fresnelRange =          FindProperty("_FresnelRange", properties, false);

                beatenRimEnable =       FindProperty("_EnableBeatenRim", properties, false);
                beatenRimColor =        FindProperty("_BeatenRimColor", properties, false);
                beatenRimIntensity =    FindProperty("_BeatenRimIntensity", properties, false);
                beatenRimRange =        FindProperty("_BeatenRimRange", properties, false);

                ExtraProperties.AddRange(properties.Except(FoundProperties).Where(x => !WellknownPropertyNames.Contains(x.name)));
            }

            private static readonly HashSet<string> WellknownPropertyNames = new HashSet<string>()
            {
                "_BaseMap", "_BaseColor", "_EmissionMap", "_EmissionColor"
            };
            private MaterialProperty FindProperty(string name, MaterialProperty[] properties, bool propertyIsMandatory=false)
            {
                var p = BaseShaderGUI.FindProperty(name, properties, propertyIsMandatory);
                FoundProperties.Add(p);
                return p;
            }
        }

        public static bool HasEmissionProperty(Material material)
            => material.HasProperty("_EmissionMap") || material.HasProperty("_EmissionColor");

        private static void DefaultShaderProperty(MaterialEditor materialEditor, MaterialProperty property)
            => materialEditor.ShaderProperty(property, property.displayName);

        public static void DoFresnelArea(CommonProperties properties, MaterialEditor materialEditor, Material material)
        {
            if (properties.fresnelEnable != null)
            {
                DefaultShaderProperty(materialEditor, properties.fresnelEnable);
                using (new EditorGUI.IndentLevelScope())
                {
                    DefaultShaderProperty(materialEditor, properties.fresnelColor);
                    DefaultShaderProperty(materialEditor, properties.fresnelIntensity);
                    DefaultShaderProperty(materialEditor, properties.fresnelRange);
                }
            }
        }

        public static void DoFrozenArea(CommonProperties properties, MaterialEditor materialEditor, Material material)
        {
            if (properties.frozenEnable != null)
            {
                DefaultShaderProperty(materialEditor, properties.frozenEnable);
                using (new EditorGUI.IndentLevelScope())
                {
                    materialEditor.TexturePropertySingleLine(Styles.FrozenTexText, properties.frozenTex, properties.frozenColor);
                    using (new EditorGUI.IndentLevelScope())
                        materialEditor.TextureScaleOffsetProperty(properties.frozenTex);
                    DefaultShaderProperty(materialEditor, properties.frozenIntensity);
                    DefaultShaderProperty(materialEditor, properties.frozenRange);
                }
            }
        }

        public static void DoBeatenRimArea(CommonProperties properties, MaterialEditor materialEditor, Material material)
        {
            if (properties.beatenRimEnable != null)
            {
                DefaultShaderProperty(materialEditor, properties.beatenRimEnable);
                using (new EditorGUI.IndentLevelScope())
                {
                    DefaultShaderProperty(materialEditor, properties.beatenRimColor);
                    DefaultShaderProperty(materialEditor, properties.beatenRimIntensity);
                    DefaultShaderProperty(materialEditor, properties.beatenRimRange);
                }
            }
        }

        public static void DoPBRMetallicAoGlossArea(CommonProperties properties, MaterialEditor materialEditor, Material material)
        {
            if (properties.metallicAoGlossMapProp != null)
            {
                if (material.GetTexture("_MetallicAoGlossMap"))
                {
                    materialEditor.TexturePropertySingleLine(Styles.metallicAoGlossMapText, properties.metallicAoGlossMapProp);
                } else
                {
                    materialEditor.TexturePropertySingleLine(Styles.metallicAoGlossMapText, properties.metallicAoGlossMapProp, properties.metallicProp);
                    using (new EditorGUI.IndentLevelScope())
                        DefaultShaderProperty(materialEditor, properties.smoothnessProp);
                }
            }
        }

        public static void DoMatcapMetallicAoGlossArea(CommonProperties properties, MaterialEditor materialEditor, Material material)
        {
            if (properties.matcapMixMapProp != null)
            {
                materialEditor.TexturePropertySingleLine(Styles.matcapAoGlossMapText, properties.matcapMixMapProp);
                using (new EditorGUI.IndentLevelScope())
                {
                    materialEditor.TexturePropertySingleLine(Styles.matcapMapText, properties.matcapProp, properties.matcapIntensityProp);
                }
                //if (material.GetTexture("_MixMap"))
                //{
                //}
                //else
                //{
                //    materialEditor.TexturePropertySingleLine(Styles.metallicAoGlossMapText, properties.metallicAoGlossMapProp, properties.metallicProp);
                //    using (new EditorGUI.IndentLevelScope())
                //        DefaultShaderProperty(materialEditor, properties.smoothnessProp);
                //}
            }
        }

        public static void DoSpecularSmoothArea(CommonProperties properties, MaterialEditor materialEditor, Material material)
        {
            if (properties.specularColorProp != null)
            {
                DefaultShaderProperty(materialEditor, properties.specularColorProp);
                using (new EditorGUI.IndentLevelScope())
                {
                    DefaultShaderProperty(materialEditor, properties.specularShininessProp);
                }
            }
        }

        public static void Inputs(CommonProperties properties, MaterialEditor materialEditor, Material material)
        {
            if (properties.bumpMapProp != null) {
                BaseShaderGUI.DrawNormalArea(materialEditor, properties.bumpMapProp, properties.bumpScaleProp);
                using (new EditorGUI.IndentLevelScope())
                    materialEditor.TextureScaleOffsetProperty(properties.bumpMapProp);
            }

            DoSpecularSmoothArea(properties, materialEditor, material);

            DoPBRMetallicAoGlossArea(properties, materialEditor, material);

            DoMatcapMetallicAoGlossArea(properties, materialEditor, material);
        }
        public static void DrawExtraInputs(CommonProperties properties, MaterialEditor materialEditor, Material material)
        {
            DoFresnelArea(properties, materialEditor, material);
            //DoFrozenArea(properties, materialEditor, material);
            //DoBeatenRimArea(properties, materialEditor, material);

            foreach (var p in properties.ExtraProperties)
            {
                if (!p.flags.HasFlag(MaterialProperty.PropFlags.HideInInspector))
                {
                    DefaultShaderProperty(materialEditor, p);
                }
            }
        }

        public static void SetMaterialKeywords(Material material)
        {
            CoreUtils.SetKeyword(material, KeywordStrings._METALLIC_AO_GLOSS_MIX_MAP,
                material.HasProperty("_MetallicAoGlossMap") && material.GetTexture("_MetallicAoGlossMap"));

            CoreUtils.SetKeyword(material, KeywordStrings._MATCAP_AO_GLOSS_MIX_MAP,
                material.HasProperty("_MixMap") && material.GetTexture("_MixMap"));

            bool hasSpecColor = material.HasProperty("_SpecColor") && material.GetColor("_SpecColor").maxColorComponent > 0;
            CoreUtils.SetKeyword(material, "_SPECULAR_COLOR", hasSpecColor);
            CoreUtils.SetKeyword(material, "_GLOSSINESS_FROM_BASE_ALPHA", hasSpecColor);

#if false
            if (material.HasProperty("_EnableBeatenRim"))
            {
                CoreUtils.SetKeyword(material, "_ENABLE_BEATEN_RIM_ON", false);
                material.SetColor("_BeatenRimColor", ColorUtils.ToRGBA(0xC02020));
                material.SetFloat("_BeatenRimIntensity", 3);
                material.SetFloat("_BeatenRimRange", 3);
            }

            if (material.HasProperty("_EnableFrozen"))
            {
                CoreUtils.SetKeyword(material, "_ENABLE_FROZEN_ON", false);
                material.SetColor("_FrozenColor", ColorUtils.ToRGBA(0xA0A0A0));
                material.SetFloat("_FrozenIntensity", 5);
                material.SetFloat("_FrozenRange", 1);
            }
#endif
        }
    }
}