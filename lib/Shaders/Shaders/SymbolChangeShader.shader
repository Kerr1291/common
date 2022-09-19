// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/SymbolChangeShader" 
{
	Properties 
	{        
		_MainTex ("Texture One", 2D) = "" {}
		_TintColor("Color: Tint Color", Color) = (1.0,1.0,1.0,1.0)
		_InvFade("Range: Soft Particle Factor", Range(0.0, 1.0)) = 1.0 
		_CopyBGTex ("Texture Two", 2D) = "" {}
		_AddBG("Texture Selector", Range(0.0, 1.0)) = 0.0
		_TintAmount("Use Texture or Color", Range(0.0, 1.0)) = 0.0
        _ColorTint("Override Color", Color) = (1.0,1.0,1.0,1.0)
	}

	SubShader 
	{
        Tags { 
            "RenderType" = "Transparent" 
			"Queue"="Transparent"
            }

			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha

		Pass {
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
            #pragma target 3.0

			struct appdata
			{
				float4 vertex: POSITION;
				float2 uv: TEXCOORD0;
				float4 color:COLOR;
			};

            struct v2f
            {
                float4  vertex : SV_POSITION;
                half2  uv : TEXCOORD0;
				float4 color:COLOR;
            };


            sampler2D _MainTex;
            sampler2D _CopyBGTex;
            
			float  _AddBG;
			float  _TintAmount;
			float4 _TintColor;
			float _InvFade;
			float4 _MainTex_ST;
			float4 _ColorTint;
            
			v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv,_MainTex);
				o.color = v.color;
                return o;
            }         

			fixed4 frag(v2f i):COLOR
			{                                          
                
                //calculate colors
                float4 background_color = tex2D(_MainTex, i.uv);
				float4 texture_color = tex2D(_CopyBGTex, i.uv);                                
                                              
                //blend colors (this line allows choosing between alpha blending or additive blending of background colors)
                float3 c_bg = (1.0 - _AddBG) * (background_color.rgb)  +        _AddBG  * (texture_color.rgb);
                       c_bg = (1.0 - _TintAmount) * (c_bg.rgb)         +   _TintAmount  * (_ColorTint.rgb);
                
				fixed4 c_final;
                       c_final.rgb = c_bg.rgb;
                       c_final.a = background_color.a;
                       
                       
				return c_final*i.color;


                       
			}
			ENDCG
		}
	} 
}
