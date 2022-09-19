// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/CartoonSun" 
{
	Properties 
	{
		_ColorBG("Background Color", Color) = (0, 0, 0, 1)
		_ColorFG("Stripe Color", Color) = (1, 1, 1, 1)
		_ColorSN("Sun Color", Color) = (1, 1, 1, 1)
		_SunP("Sun Power", Range(0.0, 3.0)) = 1.0
		_StripeN("Stripe Count", Range(0.0, 100.0)) = 10.0
		_StripeRange("Stripe Range", Range(0.0, 10.0)) = 2.0
		_SunRangeX("Sun Range X", Range(0.01, 4.0)) = 1.0
		_SunRangeY("Sun Range Y", Range(0.01, 4.0)) = 1.0
		_SunPosX("Sun Pos X", Range(-1.0, 1.0)) = 1.0
		_SunPosY("Sun Pos Y", Range(-1.0, 1.0)) = 1.0
		_SpiralFactor("Spiral Factor", Range(0.0, 10.0)) = 1.0
		_SpiralDirection("Spiral Direction", Range(-1.0, 1.0)) = 0.0
		_SpiralCenterX("Spiral Center X", Float) = 0.0
		_SpiralCenterY("Spiral Center Y", Float) = 0.0
		_SpinRate("Spin Rate", Range(1.0, 40.0)) = 1.0
		_MainTex ("Background Texture", 2D) = "" {}
		_BGBlend("Background Blend", Range(0.0, 1.0)) = 0.0
        _Safe2PITime("Safe, Cycling, Time", Float) = 0.0
	}
	SubShader 
	{
		Pass 
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0

			struct meshdata
			{
				float4 vertex : POSITION;
				float4 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float4 uv : TEXCOORD0;
				float4 uv_real : TEXCOORD1;
			};

            sampler2D _MainTex;
			fixed4 _ColorBG;
			fixed4 _ColorFG;
			fixed4 _ColorSN;
			float  _SunP;
			float  _StripeN;
			float  _StripeRange;
			float  _SunRangeX;
			float  _SunRangeY;
			float  _SunPosX;
			float  _SunPosY;
			float  _SpiralFactor;
			float  _SpiralDirection;
			float  _SpiralCenterX;
			float  _SpiralCenterY;
			float  _SpinRate;
            float  _Safe2PITime;
            float  _BGBlend;

			v2f vert(meshdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = 2.0 * float4(v.texcoord.xy, 0, 0);
				o.uv.x -= 1.0;
				o.uv.y -= 1.0;
				
				o.uv.x += _SpiralCenterX;
				o.uv.y += _SpiralCenterY;
                
                o.uv_real = float4(v.texcoord.xy, 0, 0);
				
				return o;
			}


			//float4 c = (1.0f - dist) * _ColorBG + dist * _ColorFG;
			fixed4 frag(v2f i) : SV_Target
			{
				const float pi = 3.141592;
				const float4 zero = float4(0, 0, 0, 0);
				const float4 one = float4(1, 1, 1, 1);

				//calculate sun brightness
				float2 sun_range = float2(1.0f / _SunRangeX, 1.0f / _SunRangeY);

				float raw_dist = length(i.uv);
                float2 p = float2(abs(i.uv.x + _SunPosX), abs(i.uv.y + _SunPosY));
				float dist = 1.0 - length(p * sun_range);

                //calculate the stripes
				float sprial_angle = raw_dist * _SpiralFactor;

				float2 spiral_left = float2(raw_dist * cos(sprial_angle), raw_dist * sin(sprial_angle)) * abs( clamp(_SpiralDirection,-1.0,0.0) );
				float2 spiral_right = float2(raw_dist * sin(sprial_angle), raw_dist * cos(sprial_angle)) * clamp(_SpiralDirection,0.0,1.0);

				float2 spiral_pos = i.uv.xy + spiral_left + spiral_right;

				float k = atan2(spiral_pos.y, spiral_pos.x);

				//float4 stripe_colorFG = _ColorFG 
                //* ceil(
                //    clamp(
                //         sin(  ((_Time.g * _SpinRate) + k) * _StripeN  )                           
                //         ,0.0,1.0)
                //       )
                //      ;
                
                //If the built-in Time is used then it eventually produces floating point error after about 1 week of the game running...
                //float4 stripe_colorFG = _ColorFG 
                //* ceil(
                //    clamp(
                //         sin(  ((_Safe2PITime * _SpinRate) + k) * _StripeN  )                           
                //         ,0.0,1.0)
                //       )
                //      ;          
                float4 stripe_colorFG = _ColorFG 
                * ceil(
                    clamp(
                         sin(  ((k * _StripeN) + _Safe2PITime * floor(_SpinRate))  )                           
                         ,0.0,1.0)
                       )
                      ;

                float c_dist = _StripeRange - raw_dist;
                
				float4 stripe_colorFG_clamp = clamp(stripe_colorFG, zero, one);
                
				float4 stripe_colorBG = _ColorBG;
                
				float4 stripe_colorBG_clamp = clamp(stripe_colorBG, zero, one);
                
                //float r_dist = dist / 
                
                
                c_dist = clamp(c_dist,0.0,1.0);

                //blend the colors together
                //float4 stripe_color = ceil(stripe_colorFG_clamp) * stripe_colorFG_clamp * _ColorFG.a;                
                //float4 background_color = (one - ceil(stripe_colorFG_clamp) * _ColorFG.a) * stripe_colorBG_clamp + stripe_color;   
                
                float4 stripe_color = ceil(stripe_colorFG_clamp) * stripe_colorFG_clamp * _ColorFG.a;                
                float4 background_color = (one - ceil(stripe_colorFG_clamp) * _ColorFG.a) * stripe_colorBG_clamp + stripe_color;  
                
                background_color = c_dist * background_color + (1.0 - c_dist) * stripe_colorBG_clamp;
                

                //float4 background_color = stripe_colorFG_clamp;                 
				
                //calculate sun color
				float4 sun_color;
                       sun_color.rgb = _ColorSN.rgb;
                       sun_color.a = clamp(dist,0.0,1.0);
                       
                       //blend the sun
                float3 c_sun = _SunP * sun_color.a * sun_color.rgb + (1.0 - sun_color.a) * background_color.rgb;  
                
                float4 c_final;
                       c_final.rgb = c_sun;
                       c_final.a = 1.0;                       
                       
                       //c_final = c_final * (1.0 - _BGBlend) + _BGBlend * tex2D ( _MainTex, i.uv_real.xy );
                       
                float4 blendedColor = (c_final * 2.0 * tex2D ( _MainTex, i.uv_real.xy ));
                
                       c_final = c_final * (1.0 - _BGBlend) + _BGBlend * blendedColor;
				
				return c_final;
			}
			ENDCG
		}
	} 
}
