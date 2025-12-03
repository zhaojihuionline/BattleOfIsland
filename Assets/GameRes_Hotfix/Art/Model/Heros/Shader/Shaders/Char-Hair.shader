Shader "F3-Shaders/CharHair-PBR"
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

        [Header(Anisotropic Hair)]
        [Toggle(_ANISOTROPIC_HAIR_ON)] _AnisotropicHair("Enable Anisotropic Hair", Float) = 0
        _AnisotropicStrength("Anisotropic Strength", Range(0, 5)) = 1.0
        _AnisotropicGloss("Anisotropic Gloss", Range(0.1, 10)) = 3.0
        _AnisotropicShift("Anisotropic Shift", Range(-5, 5)) = 0.0
        _AnisotropicShiftMap("Anisotropic Shift Map", 2D) = "white" {}
        _AnisotropicSpecularColor("Anisotropic Specular Color", Color) = (1,1,1,1)

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
            #pragma target 4.5

            // -------------------------------------
            // Shader Stages
            #pragma vertex vert
            #pragma fragment frag

			#pragma multi_compile _ _ENABLE_FRESNEL_ON
			#pragma multi_compile _ _ENABLE_FROZEN_ON
			#pragma multi_compile _ _ENABLE_BEATEN_RIM_ON
            #pragma shader_feature _ANISOTROPIC_HAIR_ON

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
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile _ EVALUATE_SH_MIXED EVALUATE_SH_VERTEX
            #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma instancing_options renderinglayer
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            // -------------------------------------
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            TEXTURE2D(_BaseMap);                SAMPLER(sampler_BaseMap);
            TEXTURE2D(_BumpMap);                SAMPLER(sampler_BumpMap);
            TEXTURE2D(_EmissionMap);            SAMPLER(sampler_EmissionMap);
            TEXTURE2D(_MetallicAoGlossMap);     SAMPLER(sampler_MetallicAoGlossMap);
            TEXTURE2D(_FrozenTex);		        SAMPLER(sampler_FrozenTex);

            #ifdef _ANISOTROPIC_HAIR_ON
            TEXTURE2D(_AnisotropicShiftMap);    SAMPLER(sampler_AnisotropicShiftMap);
            #endif

            CBUFFER_START(UnityPerMaterial)
                half4 	_BaseColor;
                float4 	_BaseMap_ST;

                float4 	_BumpMap_ST;
                half 	_BumpScale;

                half    _Metallic;
                half    _Smoothness;

                half3	_EmissionColor;

                #ifdef _ANISOTROPIC_HAIR_ON
                half    _AnisotropicStrength;
                half    _AnisotropicGloss;
                half    _AnisotropicShift;
                float4  _AnisotropicShiftMap_ST;
                half3   _AnisotropicSpecularColor;
                #endif

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

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float4 tangentOS    : TANGENT;
                float2 texcoord     : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float2 uv           : TEXCOORD0;
                float3 positionWS   : TEXCOORD1;
                float3 normalWS     : TEXCOORD2;
                float3 viewDirWS    : TEXCOORD3;
                float4 shadowCoord  : TEXCOORD4;
                #ifdef _ANISOTROPIC_HAIR_ON
                float3 tangentWS    : TEXCOORD5;
                float3 bitangentWS  : TEXCOORD6;
                #endif
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            // 各项异性高光计算函数
            #ifdef _ANISOTROPIC_HAIR_ON
            half3 CalculateAnisotropicSpecular(float2 uv, half3 normalWS, half3 viewDirWS, half3 lightDirWS, half3 tangentWS, half3 bitangentWS)
            {
                // 采样偏移贴图
                half shift = SAMPLE_TEXTURE2D(_AnisotropicShiftMap, sampler_AnisotropicShiftMap, uv * _AnisotropicShiftMap_ST.xy + _AnisotropicShiftMap_ST.zw).r;
                shift = shift * 2.0 - 1.0; // 从[0,1]映射到[-1,1]
                shift *= _AnisotropicShift;
                
                // 计算修改后的切线方向
                half3 modifiedTangent = normalize(tangentWS + shift * bitangentWS);
                
                // 计算各项异性需要的向量
                half3 H = normalize(lightDirWS + viewDirWS);
                half dotTH = dot(modifiedTangent, H);
                half sinTH = sqrt(1.0 - dotTH * dotTH);
                
                // 使用指数函数模拟头发高光
                half anisotropic = pow(sinTH, _AnisotropicGloss * 100.0) * _AnisotropicStrength;
                
                // 考虑视角因素
                half NdotV = max(dot(normalWS, viewDirWS), 0.0);
                anisotropic *= (1.0 - NdotV) * 0.5 + 0.5;
                
                return anisotropic * _AnisotropicSpecularColor;
            }
            #endif

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);

                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = normalInput.normalWS;
                output.viewDirWS = GetWorldSpaceNormalizeViewDir(vertexInput.positionWS);
                output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);

                #ifdef _MAIN_LIGHT_SHADOWS
                output.shadowCoord = GetShadowCoord(vertexInput);
                #endif

                #ifdef _ANISOTROPIC_HAIR_ON
                output.tangentWS = normalInput.tangentWS;
                output.bitangentWS = normalInput.bitangentWS;
                #endif

                return output;
            }

            // 修复法线变换函数
            half3 TransformTangentToWorldFixed(half3 normalTS, half3x3 tangentToWorld)
            {
                return normalize(mul(normalTS, tangentToWorld));
            }

            half4 frag(Varyings IN) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);

                half2 uv = IN.uv;

                // 采样基础纹理
                half4 albedoAlpha = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv) * _BaseColor;
                albedoAlpha.a = (albedoAlpha.a < _Cutoff) ? 0.0 : albedoAlpha.a;
                if (albedoAlpha.a == 0.0) discard;

                // 采样法线贴图 - 修复版本
                half3 normalWS = normalize(IN.normalWS);
                
                #ifdef _NORMALMAP
                half4 normalSample = SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, TRANSFORM_TEX(uv, _BumpMap));
                half3 normalTS = UnpackNormalScale(normalSample, _BumpScale);
                
                // 只在启用各项异性时使用完整的切线空间变换
                #ifdef _ANISOTROPIC_HAIR_ON
                half3x3 tangentToWorld = half3x3(IN.tangentWS, IN.bitangentWS, IN.normalWS);
                normalWS = TransformTangentToWorldFixed(normalTS, tangentToWorld);
                #else
                // 简化法线变换 - 只考虑主要法线方向
                normalWS = normalize(IN.normalWS + normalTS.x * cross(IN.normalWS, float3(0, 1, 0)) + 
                                         normalTS.y * cross(IN.normalWS, float3(1, 0, 0)));
                #endif
                #endif

                // 采样发射纹理
                #ifdef _EMISSION
                half3 emission = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, uv).rgb * _EmissionColor;
                #else
                half3 emission = _EmissionColor;
                #endif

                // 采样金属度/AO/光滑度贴图
                #ifdef _CE_METALLIC_AO_GLOSS_MIX_MAP
                half4 mix = SAMPLE_TEXTURE2D(_MetallicAoGlossMap, sampler_MetallicAoGlossMap, uv);
                half metallic = mix.r;
                half smoothness = mix.b;
                half occlusion = mix.g;
                #else
                half metallic = _Metallic;
                half smoothness = _Smoothness;
                half occlusion = 1.0;
                #endif

                // 主光照计算
                Light mainLight = GetMainLight(IN.shadowCoord);
                half3 attenuatedLightColor = mainLight.color * (mainLight.distanceAttenuation * mainLight.shadowAttenuation);

                // 漫反射计算
                half NdotL = saturate(dot(normalWS, mainLight.direction));
                half3 diffuse = albedoAlpha.rgb * attenuatedLightColor * NdotL;

                // 镜面反射计算 (简化版Blinn-Phong)
                half3 viewDirWS = normalize(IN.viewDirWS);
                half3 halfDir = normalize(mainLight.direction + viewDirWS);
                half NdotH = saturate(dot(normalWS, halfDir));
                half specularPower = exp2(10 * smoothness + 1);
                half3 specular = attenuatedLightColor * pow(NdotH, specularPower) * metallic;

                // 各项异性高光
                #ifdef _ANISOTROPIC_HAIR_ON
                half3 anisotropicSpecular = CalculateAnisotropicSpecular(
                    uv, 
                    normalWS, 
                    viewDirWS, 
                    mainLight.direction,
                    IN.tangentWS,
                    IN.bitangentWS
                );
                specular += anisotropicSpecular * attenuatedLightColor;
                #endif

                // 环境光
                half3 ambient = SampleSH(normalWS) * albedoAlpha.rgb * occlusion;

                // 组合最终颜色
                half3 finalColor = ambient + diffuse + specular + emission;

                // 应用附加特效
                #ifdef _ENABLE_FRESNEL_ON
                // Fresnel效果
                half fresnel = pow(1.0 - saturate(dot(normalWS, viewDirWS)), _FresnelRange);
                finalColor += fresnel * _FresnelColor * _FresnelIntensity;
                #endif

                return half4(finalColor, albedoAlpha.a);
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
            #pragma vertex vert
            #pragma fragment frag

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _ALPHATEST_ON

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 texcoord     : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float2 uv           : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half _Cutoff;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(positionWS);
                output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                
                half4 albedoAlpha = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv) * _BaseColor;
                clip(albedoAlpha.a - _Cutoff);
                
                return 0;
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Lit"
}