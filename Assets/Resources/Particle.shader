Shader "Unlit/Particle"
{
  Properties
  {
    _PosTex("Position Texture", 2D) = "white" {}
    _VelTex("Velocity Texture", 2D) = "white" {}
    [HDR]_Color("Main Color", Color) = (1,1,1,1)
  }
    SubShader
  {
    Tags{ "Queue" = "AlphaTest" "RenderType" = "Transparent" "IgnoreProjector" = "True" }

    Blend OneMinusDstColor One
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
        float4 color : COLOR;
      };

      struct v2f
      {
        float4 vertex : SV_POSITION;
        float2 uv : TEXCOORD0;
        float4 color : COLOR;
      };

        sampler2D _PosTex;
        sampler2D _VelTex;

        float4 _VelTex_ST;
        float4 _Color;

        int _ScreenWidth;
        int _ScreenHeight;

        v2f vert(appdata v)
        {
          v2f o;

          float4 realPosition = tex2Dlod(_PosTex, float4(v.vertex.xyz, 0));
          float4 realVelocity = tex2Dlod(_VelTex, float4(v.vertex.xyz, 0));

          realPosition.x = realPosition.x * _ScreenWidth;
          realPosition.y = realPosition.y * _ScreenHeight;
          realPosition.z = realPosition.z * _ScreenHeight;

          //realPosition.z = 1.0f;
          realPosition.w = 1.0f;

          o.uv = TRANSFORM_TEX(v.uv, _VelTex);
          o.vertex = UnityObjectToClipPos(realPosition);

          return o;
        }

        fixed4 frag(v2f v) : SV_Target
        {
          //_Color.a = 1.0f;
          //return v.color;
          //return float4(1.0f, 0.0f, 0.0f, 1.0f);
          return _Color;
        }

          ENDCG
      }

      // Render scaled background geometry
      CGPROGRAM
        #pragma surface surf Standard vertex:vert

        float4 _OutlineColor;
        float _OutlineSize;

        // Linearly expand using surface normal
        void vert(inout appdata_full v) {
          v.vertex.xyz += 123;
        }

        struct Input {
          float2 uv_MainTex;
        };

        void surf(Input IN, inout SurfaceOutputStandard o) {
          o.Albedo = _OutlineColor.rgb;
        }
      ENDCG

  }
}