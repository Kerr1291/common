// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/SunRays"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}

		_Position("Sun Position", Vector)=(0.5,0.5,0,0)
		_LightColor("Light Color",Color)=(1,1,1,1)
		_AttenuationColor("Attenuation Color",Color)=(1,1,1,1)

		//attenuation values
		_aConstant("Constant attenuation",float)=1
		_aLinear("Linear attenuation",float)=0.7
		_aQuadratic("Quadratic attenuation",float)=1.8

		_rayAngle("Ray angle", float)=5
		_numberRays("Number of rays", Int)=4

		_rayLenghtMin("Min Legnth of rays",float)=10
		_rayLenghtMax("Max Legnth of rays",float)=10

		_Intensity("Light intensity",float)=0.2
		_Rotation("RotationTime", float)=0
		_AnimationTime("Animation timers", Vector)=(0,0,0,0)
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
		//Blend One OneMinusSrcAlpha
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
			fixed4 _AttenuationColor;
			float _rayBetweenAngle;
			float _rayAngle;
			float _rayLenghtMax;
			float _rayLenghtMin;
			float4 _MainTex_TexelSize;
			float _Intensity;
			float _Rotation;
			int _numberRays;
			float4 _Position;
			float4 _AnimationTime;

			//returns the distance of the pixel from the ray
			float IsOnRay(float rayAngle, float2 pixelPosition, float2 lightPosition, float _rayLenght)
			{
				const float pi = 3.141592;
				

				float rayAngleRadians=rayAngle*(pi/180.0);
				
				float2 rayPosition=float2(_rayLenght*cos(rayAngleRadians),_rayLenght*sin(rayAngleRadians))+lightPosition;
				
				//if legnth of the pixel we are testing is more than the ray, we can 
				//finish early
				float distance=length(rayPosition-pixelPosition);
				if(distance>_rayLenght)
					return -1.0;
				
				
				//calculate the ray direction
				
				float2 rayDirection=normalize(lightPosition-rayPosition);

				float2 pixelDirection=normalize(pixelPosition-rayPosition);

				//calculate angle in reference to the ray POSITION
				float angle=acos(dot(rayDirection,pixelDirection));

				angle*=(180.0/pi);
				if(angle>_rayAngle)
					return -1.0;

				return distance/_rayLenght;
			}

			float2 ConvertToPixelCoordinates(float2 uv)
			{
				return float2(uv.x*_MainTex_TexelSize.z,uv.y*_MainTex_TexelSize.w);
			}

			fixed4 frag (v2f i) : SV_Target
			{
				const float pi = 3.141592;
				fixed4 col = tex2D(_MainTex, i.uv);
				float2 position=_Position.xy;
				float2 positionPixels=ConvertToPixelCoordinates(position);
				float2 currentPixel=ConvertToPixelCoordinates(i.uv);
				//check if the pixel is inside one of our rays first
				
				//calculate the angle of the pixel
				float pixelAngle=atan2(currentPixel.y-positionPixels.y,currentPixel.x-positionPixels.x);
				//float angleNoise= cnoise(pixelAngle/pi)*5;
				
				pixelAngle*=(180.0/pi);
				float angleNoise= cnoise(pixelAngle/360)*10;
				pixelAngle-=_Rotation;
				//check in between what angles we would between

				_rayBetweenAngle= 360.0/_numberRays;
				//_rayBetweenAngle+=angleNoise;
				float div=pixelAngle/_rayBetweenAngle;

				float floorAngle=floor(div)*_rayBetweenAngle;
				float ceilAngle=floorAngle+_rayBetweenAngle;
				floorAngle+=_Rotation;
				ceilAngle+=_Rotation;

				floorAngle=floorAngle%360;
				ceilAngle=ceilAngle%360;
				//angleNoise= cnoise(pixelAngle/360*floorAngle)*50;
				float animationTime=clamp(_AnimationTime.x,0,1);
				angleNoise= abs(cnoise(float2(pixelAngle/360,animationTime)));
				float _rayLenght=_rayLenghtMax*angleNoise+_rayLenghtMin*(1-angleNoise);
				//check the angle is in one of the rays

				float distance=max(IsOnRay(floorAngle,currentPixel,positionPixels, _rayLenght),IsOnRay(ceilAngle,currentPixel,positionPixels, _rayLenght));
				if(distance> 0)
				{
					//_LightColor=CalculateNoiseColor(i.uv);
					_LightColor=(_LightColor*distance)+(_AttenuationColor*(1-distance));

				   distance=1.0-distance;
					float attenuation=1.0/(_aConstant+(_aLinear*distance)+(_aQuadratic*distance*distance));
					attenuation/=0.5;							
                   //angleNoise= abs(cnoise(float2(pixelAngle/360,_AnimationTime.y*5)));
				   animationTime=clamp(_AnimationTime.y,0,1);
				   float intensity=(_Intensity+2.5f)*animationTime+_Intensity*(1-animationTime);
					//blend the colors, using attenuation as the lerp value
					col=(intensity*_LightColor*attenuation)+col*(1-attenuation);

				}

				return col;
			}
			ENDCG
		}
	}
}
