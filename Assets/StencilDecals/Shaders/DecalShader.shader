Shader "Custom/DecalShader"
{
	Properties
	{
		_MainTex ("Diffuse", 2D) = "white" {}
	}
	SubShader
	{
		Tags{"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}
		Stencil
		{
			Ref 1
			Comp equal
		}
		Pass
		{
			Fog { Mode Off }
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma target 3.0
			#pragma multi_compile _ _SUPPORT_NORMAL
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct v2f
			{
				float4 pos : SV_POSITION;
				half2 uv : TEXCOORD0;
				float4 screenPos : TEXCOORD1;
				float3 ray : TEXCOORD2;
			};

			v2f vert (float3 v : POSITION)
			{
				v2f o;
				o.pos = UnityObjectToClipPos (v);
				o.uv = v.xz+0.5;
				o.screenPos = ComputeScreenPos (o.pos);
				o.ray = mul(UNITY_MATRIX_MV, float4(v, 1)).xyz * float3(1,1,-1);
				return o;
			}

			sampler2D _MainTex;

#ifdef _SUPPORT_NORMAL
			sampler2D _CameraDepthNormalsTexture;

			fixed4 frag(v2f i) : SV_Target
			{
				i.ray = i.ray * (_ProjectionParams.z / i.ray.z);
				float2 uv = i.screenPos.xy / i.screenPos.w;
				float depth;
				float3 viewNormal;

				float4 encode = tex2D(_CameraDepthNormalsTexture, uv);
				DecodeDepthNormal(encode, depth, viewNormal);

				float4 vpos = float4(i.ray * depth,1);
				float3 wpos = mul(unity_CameraToWorld, vpos).xyz;
				float3 opos = mul(unity_WorldToObject, float4(wpos,1)).xyz;
				float3 decalNormal = mul(UNITY_MATRIX_T_MV, viewNormal);
				opos.xz -= opos.y * decalNormal.xz;
				clip(float3(0.5, 0.5, 0.5) - abs(opos.xyz));

				i.uv = opos.xz + 0.5;
				fixed4 col = tex2D(_MainTex, i.uv);
				return col;
			}

#else
			sampler2D _CameraDepthTexture;

			fixed4 frag(v2f i) : SV_Target
			{
				i.ray = i.ray * (_ProjectionParams.z / i.ray.z);
				float2 uv = i.screenPos.xy / i.screenPos.w;
				float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv);
				depth = Linear01Depth(depth);
				float4 vpos = float4(i.ray * depth,1);
				float3 wpos = mul(unity_CameraToWorld, vpos).xyz;
				float3 opos = mul(unity_WorldToObject, float4(wpos,1)).xyz;

				clip(float3(0.5,0.5,0.5) - abs(opos.xyz));

				i.uv = opos.xz + 0.5;
				fixed4 col = tex2D(_MainTex, i.uv);
				return col;
			}
#endif
			ENDCG
		}		

	}

	Fallback Off
}
