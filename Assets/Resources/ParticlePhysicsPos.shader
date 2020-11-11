Shader "Unlit/ParticlePhysicsPos"
{
	Properties
	{    
    _PosTex("Pos Texture", 2D) = "white" {}
		_VelTex("Vel Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "Queue" = "Background" }
		Cull Off
		ZWrite Off

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
				float4 vertex : POSITION;
			};

			sampler2D _VelTex;
      sampler2D _PosTex;
      float4 _PosTex_ST;

      float _DeltaTime;
			float _ThreeDFactor;

			v2f vert (appdata v)
			{
				v2f o; 
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _PosTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
        float4 pos = tex2D(_PosTex, i.uv);
        float4 velocity = tex2D(_VelTex, i.uv);

				//Simply add velocity to position of each particle
        pos.xyz += velocity.xyz * _DeltaTime * 2.0f;
				//pos.xyz += velocity.xyz * _DeltaTime * 20.0f;
				//pos.xyz = (pos.xyz + 0.5);
				
				//Set position z-axis to 0.0f to simulate particles on a 2d plane.
				pos.z *= _ThreeDFactor;

        pos.w = 1.0f;

        return pos;
			}

			ENDCG
		}
	}
}