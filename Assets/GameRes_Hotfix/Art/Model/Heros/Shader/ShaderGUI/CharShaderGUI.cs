using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace F3Shaders
{
    public class CharShaderGUI : BaseShaderGUI
    {
        // Properties
        private CharCommonShaderGUI.CommonProperties shadingModelProperties;

        // collect properties from the material properties
        public override void FindProperties(MaterialProperty[] properties)
        {
            base.FindProperties(properties);
            shadingModelProperties = new CharCommonShaderGUI.CommonProperties(properties);
        }

        // material changed check
        public override void ValidateMaterial(Material material)
        {
            SetMaterialKeywords(material, CharCommonShaderGUI.SetMaterialKeywords);
        }

        // material main surface options
        public override void DrawSurfaceOptions(Material material)
        {
            if (material == null)
                throw new ArgumentNullException("material");

            // Use default labelWidth
            EditorGUIUtility.labelWidth = 0f;

            base.DrawSurfaceOptions(material);
        }

        public override void DrawBaseProperties(Material material)
        {
            base.DrawBaseProperties(material);
            using (new EditorGUI.IndentLevelScope())
                DrawTileOffset(materialEditor, baseMapProp);
        }

        // material main surface inputs
        public override void DrawSurfaceInputs(Material material)
        {
            base.DrawSurfaceInputs(material);

            CharCommonShaderGUI.Inputs(shadingModelProperties, materialEditor, material);

            if (CharCommonShaderGUI.HasEmissionProperty(material))
                DrawEmissionProperties(material, true);

            CharCommonShaderGUI.DrawExtraInputs(shadingModelProperties, materialEditor, material);
        }

        public override void DrawAdvancedOptions(Material material)
        {
            base.DrawAdvancedOptions(material);
        }

        public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader)
        {
            base.AssignNewShaderToMaterial(material, oldShader, newShader);
        }
    }
}