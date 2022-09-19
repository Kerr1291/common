// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/SpaceStarfield"
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
        
        _Safe2PITime("Safe, Cycling, Time", Float) = 0.0      
        _Resolution("Current Resolution", Vector) = (1,1,1,1)      
        _Rotation("Current Rotation", Vector) = (0,0,0,0)
        
        _iterations("Iterations", Float) = 17
        _formuparam("Formula Param", Float) = 0.53
        
        _volsteps("Volume Steps", Float) = 20
        _stepsize("Step Size", Float) = 0.1
        
        _zoom  ("Zoom", Float) = 0.800
        _tile  ("Tile", Float) = 0.850
        _speed ("Speed", Float) = 0.010 
        
        _brightness("Brightness", Float) = 0.0015
        _darkmatter("Dark Matter", Float) = 0.300
        _distfading("Distance Fading", Float) = 0.730
        _saturation("Saturation", Float) = 0.850
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
            
            //Required From script...
			float4 _Rotation;
			float4 _Resolution;
            float  _Safe2PITime;
            /////
            
            float _iterations ;
            float _formuparam ;
                  
            float _volsteps ;
            float _stepsize ;
                  
            float _zoom   ;
            float _tile   ;
            float _speed  ;
                  
            float _brightness ;
            float _darkmatter ;
            float _distfading ;
            float _saturation ;
            
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
                /*
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
                */
	
                //get coords and direction
                float2 uv=i.uv.xy/_Resolution.xy-.5;
                uv.y*=_Resolution.y/_Resolution.x;
                
                float3 dir = float3(uv*_zoom,1.);
                float time= _Time.y * _speed +.25;
                            
                //mouse rotation
                float a1=.5+_Rotation.x/_Resolution.x*2.;
                float a2=.8+_Rotation.y/_Resolution.y*2.;

                float2x2 rot1 = float2x2(cos(a1),sin(a1),-sin(a1),cos(a1));
                float2x2 rot2 = float2x2(cos(a2),sin(a2),-sin(a2),cos(a2));
                dir.xz = mul( rot1, dir.xz );
                dir.xy = mul( rot2, dir.xy );
                float3 from = float3(1.,.5,0.5);
                from += float3(time*2., time, -2.);
                from.xz = mul( rot1, from.xz );
                from.xy = mul( rot2, from.xy );
                
                //volumetric rendering
                float s=0.1,fade=1.;
                float3 v=float3(0,0,0);
                for (int r=0; r<_volsteps; r++) 
                {
                    float3 p=from+s*dir*.5;
                    p = abs(float3(_tile,_tile,_tile)-fmod(p,float3(_tile*2.,_tile*2.,_tile*2.))); // tiling fold
                    float pa,a=pa=0.;
                    for (int i=0; i<_iterations; i++) 
                    { 
                        p=abs(p)/dot(p,p)-_formuparam; // the magic formula
                        a+=abs(length(p)-pa); // absolute sum of average change
                        pa=length(p);
                    }
                    float dm=max(0.,_darkmatter-a*a*.001); //dark matter
                    a*=a*a; // add contrast
                    if (r>6) fade*=1.-dm; // dark matter, don't render near
                    //v+=float3(dm,dm*.5,0.);
                    v+=fade;
                    v+=float3(s,s*s,s*s*s*s)*a*_brightness*fade; // coloring based on distance
                    fade*=_distfading; // distance fading
                    s+=_stepsize;
                }
                v=lerp(float3(length(v),length(v),length(v)),v,_saturation); //color adjust
                
                
				fixed4 c_final;
                c_final.rgb = v*.01;
                c_final.a = 1.;	 
                
                float4 background_color = tex2D(_MainTex, i.uv);
                c_final.rgb = (1.0 - _AddBG) * (background_color.rgb)  +        _AddBG  * (c_final.rgb);
                
                
                return c_final;
			}
			ENDCG
		}
	} 
}
