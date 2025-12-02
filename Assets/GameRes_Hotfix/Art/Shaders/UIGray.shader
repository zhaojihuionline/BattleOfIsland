Shader "UI/GrayScaleImage"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _GrayAmount ("Gray Amount", Range (0,1)) = 1.0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            Cull Off
            Lighting Off
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _GrayAmount;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                // 灰度化公式（常见版本）：灰度 = dot(rgb, float3(0.3,0.59,0.11))
                fixed gray = dot(col.rgb, fixed3(0.3,0.59,0.11));
                // 混合彩色与灰度
                col.rgb = lerp(col.rgb, fixed3(gray,gray,gray), _GrayAmount);
                return col;
            }
            ENDCG
        }
    }
}
