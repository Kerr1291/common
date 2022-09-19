Shader "Custom/Bloom"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_SecondaryTexture("Secondary Texture", 2D)="white" {}
		_AddBG("Blend between textures", Range (0.0,1.0)) = 0.0
		_Radius("Blur Radius", float) = 3.0
		_BloomIntensity("Bloom Intensity", float)=1.0
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
			sampler2D _SecondaryTexture;
			sampler2D _GlowText;
            float4 _GlowText_TexelSize;
            float _Radius;
			float _AddBG;
			float _BloomIntensity;

			fixed4 gaussianBlur(sampler2D tex, float2 uv)
            {
				float kernel[5]={0.1980031,0.2009955,0.202003,0.2009955,0.1980031};
                //this will be our RGBA sum
				fixed4 result = fixed4(0,0,0,0);
				int kernelIndex = 0;
				for (int j = 2; j >= -2; --j)
				{
					fixed2 neighbor = uv + fixed2(0,(1.0/_GlowText_TexelSize.w)*(float)j);

					if (neighbor[0] >= 0 && neighbor[1] >= 0)
					{
						result += kernel[kernelIndex] * tex2D(_GlowText, neighbor);
					}
					kernelIndex++;
				}
                return fixed4(result.rgb, 1.0);
            }

			fixed4 frag (v2f i) : SV_Target
			{
				float resX = _GlowText_TexelSize.z;
				float resY = _GlowText_TexelSize.w;
                float4 blurX = gaussianBlur(_GlowText, i.uv);
                fixed4 glow = blurX;

                // sample camera texture
                float4 mainTex = tex2D(_SecondaryTexture, i.uv)*_AddBG+tex2D(_MainTex, i.uv)*(1-_AddBG); 

				//return mainTex;
                //return tex2D(_GlowText, i.uv); // TEST: glow map input only

                return mainTex + (tex2D(_GlowText, i.uv)*_BloomIntensity); // TEST: glow map (no blur) + camera
                //return mainTex + glow;
			}
			ENDCG
		}
	}
}
