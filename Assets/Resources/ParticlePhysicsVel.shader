Shader "Unlit/ParticlePhysicsVel"
{
  Properties
  {
    _VelTex("Vel Texture", 2D) = "white" {}
    _PosTex("Pos Texture", 2D) = "white" {}
    _MousePos("Mouse Position", Vector) = (0,0,0,0)
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
         float4 _VelTex_ST;
         sampler2D _PosTex; 
         sampler2D _MainTex;

         float4 _MousePos;
         float4 _GravityDirection;

         float _CameraScaleFactor;
         float _GravityScale;
         float _DeltaTime;
         float _MouseForce;


         float _CenterGravityForce;

         v2f vert(appdata v)
         {
           v2f o;
           o.vertex = UnityObjectToClipPos(v.vertex);
           o.uv = TRANSFORM_TEX(v.uv, _VelTex);

           return o;
         }

         fixed4 frag(v2f i) : SV_Target
         {
          float4 velocity = tex2D(_VelTex, i.uv);
          float4 pos = tex2D(_PosTex, i.uv);

          float3 pullLocation = float3(0, 0, 0);
          float3 pullDirection = (pullLocation - pos.xyz);
          float d = distance(0, pullDirection);

          //Apply gravity
          //velocity.xy -= _DeltaTime * _GravityScale * float2(_GravityDirection.x, _GravityDirection.y);

          //Gravity towards center screen
          float d2 = 1 + pow(d, 2.0f);
          float gravity_force = 6.67f * pow(10, -2) * _CenterGravityForce / d2;
          velocity.xyz += gravity_force * pullDirection * _DeltaTime;

          //Pull towards mouse if button is down
          if (_MouseForce != 0)
          {
            float2 pullDirection = (_MousePos.xy - pos.xy);
            float d = distance(pos, pullDirection);
            
            velocity.xy += pullDirection * (_MouseForce * _CameraScaleFactor / pow(d, 1.5f)) * _DeltaTime;
          }

          //Limit maximum speed
          velocity.x = clamp(velocity.x, -10.0f, 10.0f);
          velocity.y = clamp(velocity.y, -10.0f, 10.0f);
          velocity.z = clamp(velocity.z, -10.0f, 10.0f);

          velocity.w = 1.0f;

          return velocity;
         }

         ENDCG
       }
    }
}
