Shader "F3-Shaders/Char-PBR"
{
    Properties
    {
        [MainTexture] _BaseMap("Albedo", 2D) = "white" {}
        [MainColor] _BaseColor("Color", Color) = (1,1,1,1)

		[Normal] _BumpMap("Normal Map", 2D) = "bump" {}
		_BumpScale("Normal Scale", Range(0, 10)) = 1
		
        _MetallicAoGlossMap("MetallicAoGlossMap Metallic(R)-AO(G)-Smoothness(B)", 2D) = "black" {}
        _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
        _Smoothness("Smoothness", Range(0.0, 1.0)) = 0.5

        [Header(Emission)]
        [HDR] _EmissionColor("Emission Color", Color) = (0,0,0)
		_EmissionMap("Emission", 2D) = "white" {}

		[Header(Fresnel)]
		[Toggle(_ENABLE_FRESNEL_ON)] _EnableFresnel("Enable Fresnel", Float) = 0
		_FresnelColor("Fresnel Color", Color) = (0, 0, 0)
		_FresnelIntensity("Fresnel Intensity", Range( 0 , 10)) = 1
		_FresnelRange("Fresnel Range", Range( 0 , 10)) = 3

        [Header(BeatenRim)]
		[HideInInspector][Toggle(_ENABLE_BEATEN_RIM_ON)] _EnableBeatenRim("Enable Beaten Rim", Float) = 0
		[HideInInspector]_BeatenRimColor("Beaten Rim Color", Color) = (192, 32, 32)
		[HideInInspector]_BeatenRimIntensity("Beaten Rim Intensity", Range( 0 , 10)) = 3
		[HideInInspector]_BeatenRimRange("Beaten Rim Range", Range( 0 , 10)) = 3

		[Header(Frozen)]
		[HideInInspector][Toggle(_ENABLE_FROZEN_ON)] _EnableFrozen("Enable Frozen", Float) = 0
		[HideInInspector]_FrozenTex("Frozen", 2D) = "white" {}
		[HideInInspector]_FrozenColor("Frozen Color Tint", Color) = (0.5, 0.5, 0.5)
		[HideInInspector]_FrozenIntensity("Frozen Intensity", Range( 0 , 10)) = 5
		[HideInInspector]_FrozenRange("Frozen Range", Range( 0 , 10)) = 1

        [HideInInspector] _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

        // Blending state
        [HideInInspector] _Surface("__surface", Float) = 0.0
        [HideInInspector] _Blend("__blend", Float) = 0.0
        [HideInInspector] _Cull("__cull", Float) = 2.0
        [HideInInspector] [ToggleUI] _AlphaClip("__clip", Float) = 0.0
        [HideInInspector] _SrcBlend("__src", Float) = 1.0
        [HideInInspector] _DstBlend("__dst", Float) = 0.0
        [HideInInspector] _SrcBlendAlpha("__srcA", Float) = 1.0
        [HideInInspector] _DstBlendAlpha("__dstA", Float) = 0.0
        [HideInInspector] _ZWrite("__zw", Float) = 1.0
        [HideInInspector] _BlendModePreserveSpecular("_BlendModePreserveSpecular", Float) = 1.0
        [HideInInspector] _AlphaToMask("__alphaToMask", Float) = 0.0

        [HideInInspector] [ToggleUI] _ReceiveShadows("Receive Shadows", Float) = 0.0
        // Editmode props
        [HideInInspector] _QueueOffset("Queue offset", Float) = 0.0

    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
            "Queue" = "Geometry"
            "RenderPipeline" = "UniversalPipeline"
            "UniversalMaterialType" = "Lit"
            "IgnoreProjector" = "True"
        }
        LOD 300

        // ------------------------------------------------------------------
        //  Forward pass. Shades all light in a single pass. GI + emission + Fog
        Pass
        {
            // Lightmode matches the ShaderPassName set in UniversalRenderPipeline.cs. SRPDefaultUnlit and passes with
            // no LightMode tag are also rendered by Universal Render Pipeline
            Name "ForwardLit"
            Tags
            {
                "LightMode" = "UniversalForward"
            }

            // -------------------------------------
            // Render State Commands
            Blend[_SrcBlend][_DstBlend], [_SrcBlendAlpha][_DstBlendAlpha]
            ZWrite[_ZWrite]
            Cull[_Cull]
            AlphaToMask[_AlphaToMask]

            HLSLPROGRAM
            #pragma target 2.0

            // -------------------------------------
            // Shader Stages
            #pragma vertex LitPassVertex
            #pragma fragment frag

			#pragma multi_compile _ _ENABLE_FRESNEL_ON
			#pragma multi_compile _ _ENABLE_FROZEN_ON
			#pragma multi_compile _ _ENABLE_BEATEN_RIM_ON

            // -------------------------------------
            // Material Keywords
            #pragma multi_compile_local _ _NORMALMAP
            #pragma shader_feature_local _RECEIVE_SHADOWS_OFF
            #pragma multi_compile_local_fragment _ _ALPHATEST_ON
            #pragma multi_compile_local_fragment _ _ALPHAPREMULTIPLY_ON _ALPHAMODULATE_ON
            #pragma multi_compile_local_fragment _ _EMISSION
            #pragma multi_compile_local_fragment _ _CE_METALLIC_AO_GLOSS_MIX_MAP

            // -------------------------------------
            // Universal Pipeline keywords
            #pragma multi_compile _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            // #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ EVALUATE_SH_MIXED EVALUATE_SH_VERTEX
            #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma instancing_options renderinglayer
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            // -------------------------------------
            //#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

            TEXTURE2D(_MetallicAoGlossMap);     SAMPLER(sampler_MetallicAoGlossMap);
            TEXTURE2D(_FrozenTex);		        SAMPLER(sampler_FrozenTex);

            CBUFFER_START(UnityPerMaterial)
                half4 	_BaseColor;
                float4 	_BaseMap_ST;

                float4 	_BumpMap_ST;
                half 	_BumpScale;

                half    _Metallic;
                half    _Smoothness;

                half3	_EmissionColor;

                half 	_FresnelIntensity;
                float	_FresnelRange;
                half3	_FresnelColor;

				half3 	_BeatenRimColor;
				half	_BeatenRimIntensity;
				float	_BeatenRimRange;

                float4 	_FrozenTex_ST;
                half3  	_FrozenColor;
                half   	_FrozenIntensity;
                float  	_FrozenRange;

                half _Cutoff;
                half _Surface;
            CBUFFER_END

            inline void InitializeStandardLitSurfaceData(float2 uv, out SurfaceData surfaceData) {
                surfaceData = (SurfaceData)0;
            }
			// -------------------------------------
            // Includes
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitForwardPass.hlsl"
            #include "Assets\GameRes_Hotfix\Art\Model\Heros\Shader\Shaders/CharExtra.hlsl"

			half4 frag(Varyings IN) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);

				half2 uv = IN.uv;

				half4 albedoAlpha = SampleAlbedoAlpha(uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap)) * _BaseColor;
                albedoAlpha.a = AlphaDiscard(albedoAlpha.a, _Cutoff);
				half3 normalTS = SampleNormal(TRANSFORM_TEX(uv, _BumpMap), TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap), _BumpScale);
				half3 emission = SampleEmission(uv, _EmissionColor, TEXTURE2D_ARGS(_EmissionMap, sampler_EmissionMap)).rgb;

				SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.alpha = albedoAlpha.a;
				surfaceData.albedo = AlphaModulate(albedoAlpha.rgb, albedoAlpha.a);

            #ifdef _CE_METALLIC_AO_GLOSS_MIX_MAP
                half4 mix = SAMPLE_TEXTURE2D(_MetallicAoGlossMap, sampler_MetallicAoGlossMap, uv);
				surfaceData.metallic = mix.r;
                surfaceData.smoothness = mix.b;
				surfaceData.occlusion = mix.g;
            #else // _CE_METALLIC_AO_GLOSS_MIX_MAP
                surfaceData.metallic = _Metallic;
                surfaceData.smoothness = _Smoothness;
                surfaceData.occlusion = 1;
            #endif // _CE_METALLIC_AO_GLOSS_MIX_MAP

                surfaceData.specular = half3(0.0, 0.0, 0.0);

				surfaceData.normalTS = normalTS;
				surfaceData.emission = emission;

				InputData inputData;
				InitializeInputData(IN, surfaceData.normalTS, inputData);

				surfaceData.emission += ApplyExtraEmission(uv, inputData.normalWS, inputData.viewDirectionWS);

                half4 color = UniversalFragmentPBR(inputData, surfaceData);
				color.a = OutputAlpha(color.a, IsSurfaceTypeTransparent(_Surface));

				return color;
			}
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags
            {
                "LightMode" = "ShadowCaster"
            }

            // -------------------------------------
            // Render State Commands
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
            #pragma target 2.0

            // -------------------------------------
            // Shader Stages
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            // -------------------------------------
            // Material Keywords
            // #pragma shader_feature_local _ALPHATEST_ON
            // #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            // -------------------------------------
            // Universal Pipeline keywords

            // -------------------------------------
            // Unity defined keywords
            // #pragma multi_compile_fragment _ LOD_FADE_CROSSFADE

            // This is used during shadow map generation to differentiate between directional and punctual light shadows, as they use different formulas to apply Normal Bias
            // #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            // -------------------------------------
            // Includes
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }
    }
    FallBack "F3-Shaders/Char-Unlit"
    CustomEditor "F3Shaders.CharShaderGUI"
}
