// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/GaussianBlur"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Offset ("Offset", vector) = (0.0,0.0,0.0,0.0)
		_Radius("Blur Radius", float) = 3.0
	}
	SubShader
	{
		Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
        }
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

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
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;
			
			float4 _MainTex_TexelSize;
			float kernel[5];
			float _Radius;
			float4 _Offset;

			fixed4 frag (v2f i) : SV_Target
			{
			    //float kernel[5]={0.1980031,0.2009955,0.202003,0.2009955,0.1980031};
				fixed4 result = fixed4(0,0,0,0);
				int kernelIndex = 0;
				float2 offset = float2(_Offset.x,_Offset.y);
				for (int j = -2; j <= 2; ++j)
				{
					fixed2 neighbor = offset + i.uv + fixed2((_Radius/_MainTex_TexelSize.z)*(float)j,0);

					if (neighbor[0] >= 0 && neighbor[1] >= 0)
					{
						result += kernel[kernelIndex] * tex2D(_MainTex,neighbor);
					}
					kernelIndex++;
				}
				result.a=1.0;
				return result;
			}
			ENDCG
		}

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
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;
			
			float4 _MainTex_TexelSize;
			float kernel[5];
			float _Radius;

			fixed4 frag (v2f i) : SV_Target
			{
			    //float kernel[5]={0.1980031,0.2009955,0.202003,0.2009955,0.1980031};
				fixed4 result = fixed4(0,0,0,0);
				int kernelIndex = 0;
				for (int j = -2; j <= 2; ++j)
				{
					fixed2 neighbor = i.uv + fixed2(0,(_Radius/_MainTex_TexelSize.w)*(float)j);

					if (neighbor[0] >= 0 && neighbor[1] >= 0)
					{
						result += kernel[kernelIndex] * tex2D(_MainTex, neighbor);
					}
					kernelIndex++;
				}

				return result;
			}
			ENDCG
		}
	}
}
