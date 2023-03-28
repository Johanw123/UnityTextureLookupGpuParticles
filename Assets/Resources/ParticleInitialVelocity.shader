﻿Shader "Unlit/ParticleInitialVelocity"
{
    Properties
    {
        _PosTex("Pos Texture", 2D) = "white" {}
        _VelTex("Vel Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"

            float random(float2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453123);
            }

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
            };

            sampler2D _VelTex;
            sampler2D _PosTex;
            float4 _PosTex_ST;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _PosTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float r = random(i.uv);
                float d = distance(float2(0.0f, 0.0f), i.pos) * 0.01f;
                return float4(i.uv.x, i.uv.y, 0, 0) * d * r;
            }
            ENDCG
        }
    }
}