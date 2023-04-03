Shader "Unlit/ParticleInitialPosition"
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
            #pragma shader_feature THREE_D_MODE_ON

            #include "UnityCG.cginc"

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

            float random(float2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453123);
            }

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
                float d = distance(float2(0.0f, 0.0f), i.pos) * 0.001f;

                float x = sin(i.uv.x * d  * 3.1415f)* 20.0f;
                float y = cos(i.uv.y * d * 3.1415f)* 20.0f;
                
#if defined(THREE_D_MODE_ON)
                float z = cos(i.uv.y * d * 3.1415f)* 20.0f;
                return float4(x, y, z, 0);
#else
                return float4(x, y, 0, 0);
#endif

            }
            ENDCG
        }
    }
}