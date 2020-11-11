Shader "Unlit/HzbInstance"
{
 
		Properties{
			_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Cutoff("Cutoff", float) = 0.5
		}
			SubShader{
				Tags{
				"Queue" = "Geometry+200"
				"IgnoreProjector" = "True"
				 
				"DisableBatching" = "True"
			}
				Cull Off
	LOD 200
	//ColorMask RGB
			 
					CGPROGRAM
			 
					#pragma surface surf Lambert     addshadow exclude_path:deferred
					#pragma multi_compile_instancing
					#pragma instancing_options procedural:setup
			#include "UnityCG.cginc"
					sampler2D _MainTex;
		fixed _Cutoff;

					struct appdata
						{
							float4 vertex : POSITION;
 							float2 texcoord : TEXCOORD0;
							UNITY_VERTEX_INPUT_INSTANCE_ID
						};

					struct Input {
						float2 uv_MainTex;
  
					};


					#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
		 
						 StructuredBuffer<float3> posVisibleBuffer;

					#endif

				void setup()
				{
			#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED

					 
				 
					float3 position =  posVisibleBuffer[unity_InstanceID] ;
 
					float rot =frac( sin(position.x)*100)*3.14*2;
					float crot, srot;
					sincos(rot, srot, crot);
				   unity_ObjectToWorld._11_21_31_41 = float4(crot, 0, srot, 0);
				   unity_ObjectToWorld._12_22_32_42 = float4(0, 1, 0, 0);
				   unity_ObjectToWorld._13_23_33_43 = float4(-srot, 0, crot, 0);
				   unity_ObjectToWorld._14_24_34_44 = float4(position.xyz,1);


				   unity_WorldToObject = unity_ObjectToWorld;
				   unity_WorldToObject._14_24_34 *= -1;
				   unity_WorldToObject._11_22_33 = 1.0f / unity_WorldToObject._11_22_33;
		   #endif
			   }


		 


				void surf(Input IN, inout SurfaceOutput o)
				{



					fixed4 c = tex2D(_MainTex, IN.uv_MainTex );

				 	clip(c.a - _Cutoff);
					o.Albedo = c.rgb;

					o.Alpha = 1;
					 
				}
				ENDCG
		}
			  FallBack "Legacy Shaders/Transparent/VertexLit"
	}