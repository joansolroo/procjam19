Shader "Unlit/Over Draw"
{
	SubShader
	{
	   Tags { "Queue" = "Opaque" }

		 ZTest Always    // always draw this
		 ZWrite Off
		 Blend OneMinusDstColor One   // soft additive blend

	   Pass
	   {
		  CGPROGRAM
		  #pragma vertex vert
		  #pragma fragment frag

		  #include "UnityCG.cginc"

		  struct appdata
		  {
			 float4 vertex : POSITION;
			 float2 uv : TEXCOORD0;
		  };

		  struct v2f
		  {
			 float4 vertex : SV_POSITION;
		  };

		  half4 _OverDrawColor;

		  v2f vert(appdata v)
		  {
			 v2f o;
			 o.vertex = UnityObjectToClipPos(v.vertex);
			 return o;
		  }

		  fixed4 frag(v2f i) : SV_Target
		  {
			 return _OverDrawColor;
		  }
		  ENDCG
	   }
	}
}