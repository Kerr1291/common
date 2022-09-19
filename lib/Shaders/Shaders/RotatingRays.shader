Shader "Custom/RotatingRays"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_LightColor("Light Color",Color)=(1,1,1,1)

		//attenuation values
		_aConstant("Constant attenuation",float)=1
		_aLinear("Linear attenuation",float)=0.7
		_aQuadratic("Quadratic attenuation",float)=1.8

		_rayBetweenAngle("Angle BetweenR ays",float)=30
		_rayAngle("Ray angle", float)=5

		_NoiseSpeed("Speed",float)=0.5
		_Intensity("Light intensity",float)=0.2

		_CurrentTime("Time", float)=1
		_GradientColor("Fade color",Color)=(1,1,1,1)
		_Rotation("RotationTime", float)=0
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
		Cull Off
		Lighting Off
		ZWrite Off 
		//ZTest Always
		Blend One OneMinusSrcAlpha
		//Blend SrcAlpha One

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			#include "classicNoise2d.cginc" // import noise functions

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
			float _aConstant;
			float _aLinear;
			float _aQuadratic;
			fixed4 _LightColor;
			float _rayBetweenAngle;
			float _rayAngle;
			float _Intensity;
			float _NoiseSpeed;
			float _CurrentTime;
			fixed4 _GradientColor;
			float _Rotation;

			fixed4 CalculateNoiseColor(float2 n_uv)
			{
			    float noiseColorTime=5;
				float n_a = cnoise(float3(n_uv * 5., 1.) + noiseColorTime * _NoiseSpeed * -1.) * _Intensity;// + _Ambient; 
				float n_b = cnoise(float3(n_uv * 10., 1.) + noiseColorTime * _NoiseSpeed * -1.) * .9; 
				float n_c = cnoise(float3(n_uv * 20., 1.) + noiseColorTime * _NoiseSpeed * -2.) * .9; 
				float n_d = pow(cnoise(float3(n_uv * 30., 1.) + noiseColorTime * _NoiseSpeed * -2.), 2.) * .9;
				
				float noise = n_a + n_b + n_c + n_d;
				noise = (noise < 0.)? 0. : noise;
				fixed4 col = fixed4(noise, noise, noise, 1.);
				return col;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				const float pi = 3.141592;
				fixed4 col = tex2D(_MainTex, i.uv);

				float2 position=float2(0.5,0.5);

				//check if the pixel is inside one of our rays first

				//calculate the angle of the pixel
				float angleNoise= cnoise(i.uv + _Rotation);
				float pixelAngle=atan2(i.uv.y-position.y,i.uv.x-position.x);
				pixelAngle*=(180.0/pi);
				pixelAngle+=_Rotation*5;
				//check in between what angles we would between
				//_rayAngle+=angleNoise;
				//_rayBetweenAngle*=angleNoise;
				float div=pixelAngle/_rayBetweenAngle;
				//div+=angleNoise;

				float floorAngle=floor(div)*_rayBetweenAngle;
				float ceilAngle=ceil(div)*_rayBetweenAngle;

				//check the angle is in one of the rays
				if(pixelAngle>floorAngle-_rayAngle && pixelAngle<floorAngle+_rayAngle ||
				   pixelAngle>ceilAngle-_rayAngle && pixelAngle<ceilAngle+_rayAngle)

				{
					//calculate distance to the center, where the light is currently
					float distance=length(position-i.uv);
					_LightColor=CalculateNoiseColor(i.uv);
					float attenuation=1.0/(_aConstant+(_aLinear*distance)+(_aQuadratic*distance*distance));
				
					//blend the colors, using attenuation as the lerp value
					//col=_LightColor*attenuation+col*(1-attenuation);
					col+=(_LightColor+_GradientColor)*attenuation;
					col.a=saturate(col.a);
					//col=col*1.2;
					//col=_LightColor;
				}
				return col;
			}
			ENDCG
		}
	}
}
