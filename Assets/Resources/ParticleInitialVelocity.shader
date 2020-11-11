Shader "Unlit/ParticleInitialVelocity"
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
        float d = distance(float2(0.5f,0.5f), i.pos);

        //return float4(i.pos.y * 0.01f + d * 0.002f - 0.2f, i.pos.y * 0.001f + d * 0.002f - 0.2f, 1, 1);
        return float4(i.pos.x * 0.01f - 0.5f, i.pos.y * 0.01f - 0.3f, 1, 1);
				//return float4(sin(i.pos.x), sin(i.pos.y), 1, 1);
			}

			ENDCG
		}
	}
}
