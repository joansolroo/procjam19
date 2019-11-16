// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Perso/PhongShader" {
	Properties{
		_Color("Color", Color) = (1, 1, 1, 1) //The color of our object
		_Tex("Texture", 2D) = "white" {} //Optional texture
		_NoiseThreshold("Noise threshold", Float) = 0.5
		_Color1("Starting gradient", Color) = (1, 1, 1, 1)
		_Color2("Ending gradient", Color) = (1, 1, 1, 1)
		_LightPower("Light power", Float) = 1.1
		_Shininess("Shininess", Float) = 10 //Shininess
		_SpecColor("Specular Color", Color) = (1, 1, 1, 1) //Specular highlights color
	}

	CGINCLUDE
		float rand(int seed)
		{
			return frac(sin(dot(float3(seed, seed, seed), float3(12.9898, 78.233, 45.5432))) * 43758.5453);
		}
		float rand(float3 seed)
		{
			return frac(sin(dot(seed, float3(12.9898, 78.233, 45.5432))) * 43758.5453);
		}
	ENDCG

		SubShader{
			Tags { "RenderType" = "Opaque" }
			LOD 200
				Cull Off
			Pass {
				Tags { "LightMode" = "ForwardBase" }

				CGPROGRAM
		
					#pragma vertex vert
					#pragma fragment frag

					#include "UnityCG.cginc"

					uniform float4 _LightColor0;

					sampler2D _Tex;
					float4 _Tex_ST;
					float _NoiseThreshold;

					uniform float4 _Color;
					uniform float4 _Color1;
					uniform float4 _Color2;
					uniform float _LightPower;
					uniform float4 _SpecColor;
					uniform float _Shininess;
					
					struct v2f
					{
						float4 pos : POSITION;
						float3 normal : NORMAL;
						float3 uv : TEXCOORD0;
						float4 posWorld : TEXCOORD1;
					};

					v2f vert(appdata_base v)
					{
						v2f o;

						o.posWorld = mul(unity_ObjectToWorld, v.vertex);
						o.normal = normalize(mul(float4(v.normal, 0.0), unity_WorldToObject).xyz);
						o.pos = UnityObjectToClipPos(v.vertex);
						float2 uv = float2(TRANSFORM_TEX(v.texcoord, _Tex));
						o.uv = float3(uv.x, uv.y, rand(mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz) + rand(o.normal));
						return o;
					}

					fixed4 frag(v2f i) : COLOR
					{
						float3 normalDirection = normalize(i.normal);
						float3 viewDirection = normalize(_WorldSpaceCameraPos - i.posWorld.xyz);

						float3 vert2LightSource = _WorldSpaceLightPos0.xyz - i.posWorld.xyz;
						float oneOverDistance = 1.0 / length(vert2LightSource);
						float attenuation = lerp(1.0, oneOverDistance, _WorldSpaceLightPos0.w);
						float3 lightDirection = _WorldSpaceLightPos0.xyz - i.posWorld.xyz * _WorldSpaceLightPos0.w;

						float3 ambientLighting = UNITY_LIGHTMODEL_AMBIENT.rgb * _Color.rgb;
						float3 diffuseReflection = attenuation * _LightColor0.rgb * _Color.rgb * max(0.0, dot(normalDirection, lightDirection));
						float3 specularReflection;
						if (dot(i.normal, lightDirection) < 0.0)
						{
							specularReflection = float3(0.0, 0.0, 0.0);
						}
						else
						{
							specularReflection = attenuation * _LightColor0.rgb * _SpecColor.rgb * pow(max(0.0, dot(reflect(-lightDirection, normalDirection), viewDirection)), _Shininess);
						}

						float4 tex = tex2D(_Tex, i.uv.xy);
						float3 color = (ambientLighting + diffuseReflection) * tex.xyz + specularReflection;
						if (tex.a < 0.01)
						{
							if(rand((int)(100 * i.uv.z) + ((int)i.uv.x + 1) + 50*(int)(i.uv.y + 1)) < _NoiseThreshold)
							{
								float t = saturate(1 - rand(((int)i.uv.x + 1) * (int)(i.uv.y + 1)));
								color = _LightPower * lerp(_Color1, _Color2, t).xyz;
							}
						}
						return float4(color, 1.0);
					}
				ENDCG
			}
			/*Pass {
			  Tags { "LightMode" = "ForwardAdd" }
				Blend One One

				CGPROGRAM
			  #pragma vertex vert
			  #pragma fragment frag

			  #include "UnityCG.cginc"

			  uniform float4 _LightColor0;

			  sampler2D _Tex;
			  float4 _Tex_ST;
			  float _NoiseThreshold;

			  uniform float4 _Color;
			  uniform float4 _Color1;
			  uniform float4 _Color2;
			  uniform float4 _SpecColor;
			  uniform float _Shininess;

			  struct appdata
			  {
				  float4 vertex : POSITION;
				  float3 normal : NORMAL;
				  float2 uv : TEXCOORD0;
			  };

			  struct v2f
			  {
				  float4 pos : POSITION;
				  float3 normal : NORMAL;
				  float2 uv : TEXCOORD0;
				  float4 posWorld : TEXCOORD1;
			  };

			  v2f vert(appdata v)
			  {
				  v2f o;

				  o.posWorld = mul(unity_ObjectToWorld, v.vertex);
				  o.normal = normalize(mul(float4(v.normal, 0.0), unity_WorldToObject).xyz);
				  o.pos = UnityObjectToClipPos(v.vertex);
				  o.uv = TRANSFORM_TEX(v.uv, _Tex);

				  return o;
			  }

			  float rand(int seed)
			  {
				  return frac(sin(dot(float3(seed, seed, seed), float3(12.9898, 78.233, 45.5432))) * 43758.5453);
			  }

			  fixed4 frag(v2f i) : COLOR
			  {
				  float3 normalDirection = normalize(i.normal);
				  float3 viewDirection = normalize(_WorldSpaceCameraPos - i.posWorld.xyz);

				  float3 vert2LightSource = _WorldSpaceLightPos0.xyz - i.posWorld.xyz;
				  float oneOverDistance = 1.0 / length(vert2LightSource);
				  float attenuation = lerp(1.0, oneOverDistance, _WorldSpaceLightPos0.w);
				  float3 lightDirection = _WorldSpaceLightPos0.xyz - i.posWorld.xyz * _WorldSpaceLightPos0.w;

				  float3 diffuseReflection = attenuation * _LightColor0.rgb * _Color.rgb * max(0.0, dot(normalDirection, lightDirection));
				  float3 specularReflection;
				  if (dot(i.normal, lightDirection) < 0.0)
				  {
					specularReflection = float3(0.0, 0.0, 0.0);
				  }
				  else
				  {
					specularReflection = attenuation * _LightColor0.rgb * _SpecColor.rgb * pow(max(0.0, dot(reflect(-lightDirection, normalDirection), viewDirection)), _Shininess);
				  }

				  //float3 color = (diffuseReflection)* tex2D(_Tex, i.uv) + specularReflection;
			      float4 texColor = tex2D(_Tex, i.uv);
				  
				  if (texColor.a < 0.01)
				  {
					  texColor.a = 1.0;
					  if (rand(((int)i.uv.x + 1) * (int)(i.uv.y + 1)) < _NoiseThreshold)
					  {
						  float t = saturate(1 - rand(((int)i.uv.x + 1) * (int)(i.uv.y + 1)));
						  texColor = 2.0*lerp(_Color1, _Color2, t);
						  texColor.a = 1.0;
					  }
				  }

				  float3 color = (diffuseReflection) * texColor.xyz + specularReflection;
				  return float4(color, 1.0);
			  }
		  ENDCG
			}*/
		}
}

