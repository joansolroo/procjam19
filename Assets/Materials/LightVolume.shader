// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Perso/LightVolume"
{
    Properties
    {
		_Color("Main Color", Color) = (1,1,1,0.5)
        _MainTex ("Texture", 2D) = "white" {}
		//_Dust("Dust noise", float) = 0
		//_DustOffset("Dust offset", Vector) = (0,0,0,0)
		_Power("Light power", float) = 0
    }
    SubShader
    {
		Tags {"Queue" = "Transparent" "RenderType" = "Transparent" }
        LOD 100

		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

			fixed4 _Color;
			//float _Dust;
			//float2 _DustOffset;
			float _Power;
			sampler2D _MainTex;
			float4 _MainTex_ST;

            struct v2f
            {
				float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
				float3 normalDir : TEXCOORD2;
				float4 posWorld : TEXCOORD3;
                UNITY_FOG_COORDS(1)
            };

            v2f vert (appdata_base v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord;
				o.posWorld = mul(unity_ObjectToWorld, v.vertex);
				o.normalDir = UnityObjectToWorldNormal(v.normal);
				if (o.uv.x == 1.0)
					o.normalDir = float3(0, 0, 0);
				UNITY_TRANSFER_FOG(o, o.pos);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				//float3 v = _WorldSpaceCameraPos - i.posWorld.xyz;
				float3 viewDirection = normalize(_WorldSpaceCameraPos - i.posWorld.xyz);
				float3 normal = normalize(i.normalDir + 0.001*viewDirection);
				float d = saturate(dot(normal, viewDirection));
				//float dust = saturate(tex2D(_MainTex, (i.pos.xy + _DustOffset)/1024).x - 0.5) * d;

				fixed4 col = _Color;
				col.a *= pow(i.uv.x, 2) * d;// +_Dust * dust;
                UNITY_APPLY_FOG(i.fogCoord, col);

				//float3 u = 0.5*normalize(i.n) + 0.5;
				//col = fixed4(u, 1);// u.x, u.y, u.z, 1);

				return col;
            }
            ENDCG
        }
    }
}
