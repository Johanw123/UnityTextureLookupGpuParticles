Shader "Unlit/ParticleInitialPosition"
{
	Properties
	{    
    _PosTex("Pos Texture", 2D) = "white" {}
		_VelTex("Vel Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" }

		Pass
		{
			CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      #pragma target 3.0
			
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

			v2f vert (appdata v)
			{
				v2f o; 
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _PosTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
        float4 pos = tex2D(_PosTex, i.uv);
        float4 velocity = tex2D(_VelTex, i.uv);

        float d = distance(float2(0.0f,0.0f), i.pos);

				return float4(i.pos.x / 100, i.pos.y / 100, 1, 1);
        //return float4(pos.y * i.uv.x * velocity.y * 4, pos.x * velocity.x * i.uv.y * 4, 1, 1);
      //  return float4(i.vertex.x * 0.0001f - 0.001f, i.vertex.y * 0.0001f - 0.001f, 1, 1);
			}

			ENDCG
		}
	}
}
