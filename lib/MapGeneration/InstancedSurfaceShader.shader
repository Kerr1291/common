 Shader "Instanced/InstancedSurfaceShader" {
    Properties {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model
        #pragma surface surf Standard addshadow fullforwardshadows
        #pragma multi_compile_instancing
        #pragma instancing_options procedural:setup

        sampler2D _MainTex;

        struct Input {
            float2 uv_MainTex;
        };

    #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
        StructuredBuffer<float4> positionBuffer;
        StructuredBuffer<float4> rotationBuffer;
    #endif
	
		float4 multQuat(float4 q1, float4 q2) {
			return float4(
			q1.w * q2.x + q1.x * q2.w + q1.z * q2.y - q1.y * q2.z,
			q1.w * q2.y + q1.y * q2.w + q1.x * q2.z - q1.z * q2.x,
			q1.w * q2.z + q1.z * q2.w + q1.y * q2.x - q1.x * q2.y,
			q1.w * q2.w - q1.x * q2.x - q1.y * q2.y - q1.z * q2.z
			);
		}

		float4x4 inverse(float4x4 input)
		{
		#define minor(a,b,c) determinant(float3x3(input.a, input.b, input.c))
 
			float4x4 cofactors = float4x4(
				minor(_22_23_24, _32_33_34, _42_43_44),
				-minor(_21_23_24, _31_33_34, _41_43_44),
				minor(_21_22_24, _31_32_34, _41_42_44),
				-minor(_21_22_23, _31_32_33, _41_42_43),
 
				-minor(_12_13_14, _32_33_34, _42_43_44),
				minor(_11_13_14, _31_33_34, _41_43_44),
				-minor(_11_12_14, _31_32_34, _41_42_44),
				minor(_11_12_13, _31_32_33, _41_42_43),
 
				minor(_12_13_14, _22_23_24, _42_43_44),
				-minor(_11_13_14, _21_23_24, _41_43_44),
				minor(_11_12_14, _21_22_24, _41_42_44),
				-minor(_11_12_13, _21_22_23, _41_42_43),
 
				-minor(_12_13_14, _22_23_24, _32_33_34),
				minor(_11_13_14, _21_23_24, _31_33_34),
				-minor(_11_12_14, _21_22_24, _31_32_34),
				minor(_11_12_13, _21_22_23, _31_32_33)
				);
		#undef minor
			return transpose(cofactors) / determinant(input);
		}
		
		float3 rotateVector( float4 quat, float3 vec ) {
			// https://twistedpairdevelopment.wordpress.com/2013/02/11/rotating-a-vector-by-a-quaternion-in-glsl/
			float4 qv = multQuat( quat, float4(vec, 0.0) );
			return multQuat( qv, float4(-quat.x, -quat.y, -quat.z, quat.w) ).xyz;
		}

        void setup()
        {
        #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
            float4 data = positionBuffer[unity_InstanceID];
            float4 rotation = rotationBuffer[unity_InstanceID];
            //data.xyz = rotateVector(rotation,data.xyz);

            //unity_ObjectToWorld._11_21_31_41 = float4(data.w, 0, 0, 0);
            //unity_ObjectToWorld._12_22_32_42 = float4(0, data.w, 0, 0);
            //unity_ObjectToWorld._13_23_33_43 = float4(0, 0, data.w, 0);
            //unity_ObjectToWorld._14_24_34_44 = float4(data.xyz, 1);

			float4 q = rotation;
			float qxsq = 2 * q.x * q.x;
			float qysq = 2 * q.y * q.y;
			float qzsq = 2 * q.z * q.z;
			float qxqy = 2 * q.x * q.y;
			float qzqw = 2 * q.z * q.w;
			float qxqz = 2 * q.x * q.z;
			float qyqw = 2 * q.y * q.w;
			float qyqz = 2 * q.y * q.z;
			float qxqw = 2 * q.x * q.w;

			float m11 = 1 - qysq - qzsq;
			float m12 = qxqy - qzqw;
			float m13 = qxqz + qyqw;

			float m21 = qxqy + qzqw;
			float m22 = 1 - qxsq - qzsq;
			float m23 = qyqz - qxqw;

			float m31 = qxqz - qyqw;
			float m32 = qyqz + qxqw;
			float m33 = 1 - qxsq - qysq;
			
            unity_ObjectToWorld._11_21_31_41 = float4(data.w * m11, m12, m13, 0);
            unity_ObjectToWorld._12_22_32_42 = float4(m21, data.w * m22, m23, 0);
            unity_ObjectToWorld._13_23_33_43 = float4(m31, m32, data.w * m33, 0);
            unity_ObjectToWorld._14_24_34_44 = float4(data.xyz, 1);

            unity_WorldToObject = unity_ObjectToWorld;
			
			//need to properly calculate the inverse when there are rotations....
			unity_WorldToObject = inverse(unity_ObjectToWorld);

			//this only works when there are no rotations....
            //unity_WorldToObject._14_24_34 *= -1;
            //unity_WorldToObject._11_22_33 = 1.0f / unity_WorldToObject._11_22_33;
        #endif
        }

        half _Glossiness;
        half _Metallic;

        void surf (Input IN, inout SurfaceOutputStandard o) {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}