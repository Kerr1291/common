// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/DancingLightsShader" 
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
		_InputTex ("Input Texture", 2D) = "" {}
		_NoiseTex ("Noise Texture", 2D) = "" {}
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
            sampler2D _NoiseTex;
            sampler2D _InputTex;
            
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
            
            
            float hue2rgb(float p, float q, float t)
            {
                if(t < 0.0) t += 1.0;
                if(t > 1.0) t -= 1.0;
                if(t < 1.0/6.0) return p + (q - p) * 6.0 * t;
                if(t < 1.0/2.0) return q;
                if(t < 2.0/3.0) return p + (q - p) * (2.0/3.0 - t) * 6.0;
                return p;
            }
            
            float3 hslToRgb(float h, float s, float l)
            {
                float3 rgb;
                if(s == 0.0)
                {
                    rgb = float3( l,l,l );
                }
                else
                {
                    float q = l < 0.5 ? l * (1.0 + s) : l + s - l * s;
                    float p = 2.0 * l - q;
                    //rgb = float3( hue2rgb(p, q, h + 0.33333), hue2rgb(p, q, h), hue2rgb(p, q, h - 0.33333) );
                    rgb = float3( hue2rgb(p, q, h + 0.33333) * 1.2, hue2rgb(p, q, h) * .7, hue2rgb(p, q, h - 0.33333)  * .2 );
                }

                return rgb;
            }
                        
            float3 mod289(float3 x) 
            {
                return x - floor(x * (1.0 / 289.0)) * 289.0;
            } 

            float4 mod289(float4 x)
            {
                return x - floor(x * (1.0 / 289.0)) * 289.0;
            }
             
            float4 permute(float4 x)
            {
                return mod289(((x*34.0)+1.0)*x);
            }
             
            float4 taylorInvSqrt(float4 r)
            {
                return 1.79284291400159 - 0.85373472095314 * r;
            }
             
            float2 fade(float2 t) {
                return t*t*t*(t*(t*6.0-15.0)+10.0);
            }

            float3 fade(float3 t) {
              return t*t*t*(t*(t*6.0-15.0)+10.0);
            }
            
            float intersectAuroraPlane( float3 ro, float3 rd, float planeZ )
            {
                return ( -ro.z + planeZ ) / rd.z;
            }

            float intersectWaterPlane( float3 ro, float3 rd, float waterY )
            {
                return ( -ro.y + waterY ) / rd.y;
            }
            
            float mod(float x, float y)
            {
              return fmod(x,y);
              //return x - y * floor(x/y);
            }

            float cnoise(float3 P)
            {
              float3 Pi0 = floor(P); // Integer part for indexing
              float3 Pi1 = Pi0 + float3(1.0,1.0,1.0); // Integer part + 1
              Pi0 = mod289(Pi0);
              Pi1 = mod289(Pi1);
              float3 Pf0 = frac(P); // fracional part for interpolation
              float3 Pf1 = Pf0 - float3(1.0,1.0,1.0); // fracional part - 1.0
              float4 ix = float4(Pi0.x, Pi1.x, Pi0.x, Pi1.x);
              float4 iy = float4(Pi0.yy, Pi1.yy);
              float4 iz0 = Pi0.zzzz;
              float4 iz1 = Pi1.zzzz;

              float4 ixy = permute(permute(ix) + iy);
              float4 ixy0 = permute(ixy + iz0);
              float4 ixy1 = permute(ixy + iz1);

              float4 gx0 = ixy0 * (1.0 / 7.0);
              float4 gy0 = frac(floor(gx0) * (1.0 / 7.0)) - 0.5;
              gx0 = frac(gx0);
              float4 gz0 = float4(0.5,0.5,0.5,0.5) - abs(gx0) - abs(gy0);
              float4 sz0 = step(gz0, float4(0.0,0.0,0.0,0.0));
              gx0 -= sz0 * (step(0.0, gx0) - 0.5);
              gy0 -= sz0 * (step(0.0, gy0) - 0.5);

              float4 gx1 = ixy1 * (1.0 / 7.0);
              float4 gy1 = frac(floor(gx1) * (1.0 / 7.0)) - 0.5;
              gx1 = frac(gx1);
              float4 gz1 = float4(0.5,0.5,0.5,0.5) - abs(gx1) - abs(gy1);
              float4 sz1 = step(gz1, float4(0.0,0.0,0.0,0.0));
              gx1 -= sz1 * (step(0.0, gx1) - 0.5);
              gy1 -= sz1 * (step(0.0, gy1) - 0.5);

              float3 g000 = float3(gx0.x,gy0.x,gz0.x);
              float3 g100 = float3(gx0.y,gy0.y,gz0.y);
              float3 g010 = float3(gx0.z,gy0.z,gz0.z);
              float3 g110 = float3(gx0.w,gy0.w,gz0.w);
              float3 g001 = float3(gx1.x,gy1.x,gz1.x);
              float3 g101 = float3(gx1.y,gy1.y,gz1.y);
              float3 g011 = float3(gx1.z,gy1.z,gz1.z);
              float3 g111 = float3(gx1.w,gy1.w,gz1.w);

              float4 norm0 = taylorInvSqrt(float4(dot(g000, g000), dot(g010, g010), dot(g100, g100), dot(g110, g110)));
              g000 *= norm0.x;
              g010 *= norm0.y;
              g100 *= norm0.z;
              g110 *= norm0.w;
              float4 norm1 = taylorInvSqrt(float4(dot(g001, g001), dot(g011, g011), dot(g101, g101), dot(g111, g111)));
              g001 *= norm1.x;
              g011 *= norm1.y;
              g101 *= norm1.z;
              g111 *= norm1.w;

              float n000 = dot(g000, Pf0);
              float n100 = dot(g100, float3(Pf1.x, Pf0.yz));
              float n010 = dot(g010, float3(Pf0.x, Pf1.y, Pf0.z));
              float n110 = dot(g110, float3(Pf1.xy, Pf0.z));
              float n001 = dot(g001, float3(Pf0.xy, Pf1.z));
              float n101 = dot(g101, float3(Pf1.x, Pf0.y, Pf1.z));
              float n011 = dot(g011, float3(Pf0.x, Pf1.yz));
              float n111 = dot(g111, Pf1);

              float3 fade_xyz = fade(Pf0);
              float4 n_z = lerp(float4(n000, n100, n010, n110), float4(n001, n101, n011, n111), fade_xyz.z);
              float2 n_yz = lerp(n_z.xy, n_z.zw, fade_xyz.y);
              float n_xyz = lerp(n_yz.x, n_yz.y, fade_xyz.x); 
              return 2.2 * n_xyz;
            }

            float FBM( float2 uv, float z )
            {
                float lacunarity = 2.0;
                float gain = 0.25;
                float amplitude = 1.0;
                float frequency = 1.0;
                float sum = 0.0;
                for(int i = 0; i < 4; ++i)
                {
                    //float3 fz = float3( uv * frequency, z );
                    float3 fz = float3( uv.x * frequency, uv.y * frequency, z );
                    sum += amplitude * cnoise(fz);
                    amplitude *= gain;
                    frequency *= lacunarity;
                }
                return sum;
            }

            float calcAurora( float3 ro, float3 rd, out float3 pt, out float3 color )
            {
                float angle = 1.0;
                float3 aro = ro;
                float3 ard = rd;
                
                float at = intersectAuroraPlane( aro, ard, 1.1 );
                pt = aro + at * ard;
                
                float2 uv = pt.xy - float2( -0.6, 0.5 );
                
                float3 fft = hslToRgb( 
                    uv.y * 0.4 - 0.05 + tex2D( _NoiseTex, float2( uv.x * 0.05 + _Time.g * 0.002, 0.0 ) ).x * 1.0
                    , 1.0, 1.0 ) * 0.95;
                
                fft *= tex2D( _InputTex, float2( mod( abs( ( uv.x + _Time.r * .4 )) * 0.35, 1.0 ), 0.0 ) ).xyz * 0.9
                    +  tex2D( _InputTex, float2( mod( abs( ( (uv.x + _Time.r * .4)) * 0.1 ) * 0.35, 1.0 ), 0.0 ) ).xyz * 0.7;
                float dist = 1.0 - min( 1.0, max( 0.0, length( float2( 0.5, 0.1 ) - uv ) * 0.8 ) );
                color = float3( fft ) * smoothstep( 0.0, 1.0, abs(dist) );
                
                
                /*
                
                float3 stars = float3( 0.0, 0.1, 0.2 );
                for( float i = 0.0; i < 60.0; i+=1.0 ) 
                {
                    float3 star = tex2D( _NoiseTex, float2( i * 0.03, 0.1 ) ).xyz; // 0-1;0-1
                    star.x = mod( star.x - _Time.g * 0.0015 * (tex2D( _NoiseTex, float2( i * 0.03, 0.0 ) ).x * 3.0 + 1.0)
                                 , 1.0 );
                    star.x -= 0.3;
                    star.x *= 2.3;
                    star.z = ( star.z * 0.6 + 0.4 ) * 300.0;
                    float lumi = smoothstep( 0.0, 1.0, max( 0.0, min( 1.0, ( 1.1 - length( uv - star.xy ) * star.z ) ) ) ) * 0.8 *
                        abs( sin( _Time.g * 0.1 * (1.0+i*0.111) ) * tex2D( _NoiseTex, float2( i * 0.3, 0.0 ) ).x * 0.6 );
                        
                    stars += float3( min( float3( 1.0, 1.0, 1.0 ), color + float3( 0.5,0.5,0.5 ) ) * lumi );
                }
                color += stars;
                
                */
                
                // pseudo landscape
                //color *= clamp( (uv.y*3.0-FBM(uv * 10.0, 0.) * .2) * 50.0 - 10.0, 0.0, 1.0 );
                color *= clamp( (uv.y*3.0 * 1.0) * 88.0 + 100.0, 0.0, 1.0 );
                
                
                return at;
            }

            float pattern( float2 uv )
            {
                return FBM( uv, _Time.g );
            }

            float3 colorize( float2 uv, float3 ro, float3 rd )
            {
                // aurora
                float3 aurora = float3(0.0,0.0,0.0);
                float3 apt = float3(0.0,0.0,0.0);
                float at = calcAurora( ro, rd, apt, aurora );
                
                // water
                float waterH = 0.44;
                if ( apt.y < waterH )
                {
                    float wt = intersectWaterPlane( ro, rd, waterH );
                    {
                        float3 wpt = ro + wt * rd;
                        float2 uvfbm = uv * 100.0;
                        uvfbm.y *= 1.5;
                        uvfbm.y += _Time.g * 0.1;
                        float2 disturb = float2( pattern( uvfbm ), pattern( uvfbm + float2( 5.2, 1.3 ) ) );
                        disturb *= 0.1;
                        float3 normal = normalize( float3( disturb.x, 1.0, disturb.y ) );
                        float3 R = reflect( normalize( -rd ), normal );
                        at = calcAurora( wpt, R, apt, aurora );
                    }
                    float dist = min( 1.0, max( 0.0, length( float2( 0.5, waterH ) - uv ) * 0.8 ) );
                    aurora *= dist;
                }
                
                return aurora;
            }     
            
            

			fixed4 frag(v2f i):COLOR
			{   
                //calculate colors
                float4 background_color = tex2D(_MainTex, i.uv);
				float4 texture_color = tex2D(_CopyBGTex, i.uv);  

                float2 aUV = i.uv;
                aUV.x /= 1920.0;
                aUV.y /= 1080.0;
                
                float3 ro = float3( 0.0, 1.0, -0.2 );
                float3 rd = float3( i.uv - float2(0.5, 0.5), 1.0 );
                rd.x *= 1920.0 / 1080.0 * 1.0;
                
                float3 AC = colorize( i.uv, ro, rd );
                                              
                //blend colors (this line allows choosing between alpha blending or additive blending of background colors)
                float3 c_bg = (1.0 - _AddBG) * (background_color.rgb)  +        _AddBG  * (texture_color.rgb);
                       //c_bg = (1.0 - _TintAmount) * (c_bg.rgb)         +   _TintAmount  * (AC);
                       c_bg = (c_bg.rgb)         +   _TintAmount  * length(AC) * _TintColor;
                
				fixed4 c_final;
                       c_final.rgb = c_bg.rgb;
                       c_final.a = background_color.a;
                       
                       
				return c_final*i.color;


                       
			}
			ENDCG
		}
	} 
}

