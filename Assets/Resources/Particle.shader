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
      #pragma shader_feature THREE_D_MODE_ON
      
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
#if defined(THREE_D_MODE_ON)
          float4 realPosition = tex2Dlod(_PosTex, float4(0, 0, 0, 0));
#else
          float4 realPosition = tex2Dlod(_PosTex, float4(v.vertex.xy, 0, 0));
#endif

          realPosition.x = realPosition.x * _ScreenWidth;
          realPosition.y = realPosition.y * _ScreenHeight;
          
#if defined(THREE_D_MODE_ON)
          realPosition.z = realPosition.z * _ScreenHeight;
#endif
          
          realPosition.w = 1.0f;

          o.uv = TRANSFORM_TEX(v.uv, _VelTex);
          o.vertex = UnityObjectToClipPos(realPosition);

          return o;
        }

        fixed4 frag(v2f v) : SV_Target
        {
          return _Color;
        }

          ENDCG
      }
  }
}