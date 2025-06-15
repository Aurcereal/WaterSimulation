// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Unlit/ParticleAdditiveDensity"
{
    Properties
    {
        _Radius ("Radius", Float) = 1.0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        LOD 100
        ZWrite Off

        Pass
        {
            Blend One One

            CGPROGRAM
            #pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
            #pragma editor_sync_compilation
			#pragma target 4.5

            StructuredBuffer<float3> positionBuffer;

            struct vIn
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct vOut
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
            };

            float _Radius;

            vOut vert (vIn v, uint instanceID : SV_InstanceID)
            {
                vOut o;
                
                float3 camRi = unity_CameraToWorld._m00_m10_m20; // Row major
                float3 camUp = unity_CameraToWorld._m01_m11_m21;

                float3 objectPos = v.vertex.xyz * _Radius;
                objectPos = objectPos.x * camRi + objectPos.y * camUp; // Orient towards cam
                float3 worldPos = objectPos + positionBuffer[instanceID];
                o.worldPos = worldPos;
                o.vertex = mul(UNITY_MATRIX_VP, float4(worldPos, 1.));

                o.uv = v.uv;
                
                return o;
            }

            fixed4 frag(vOut i, out float Depth : SV_Depth) : SV_Target
            {
                float2 p = i.uv*2.0-1.0;
                float sqrDist = dot(p, p);

                if(sqrDist >= 1.0) discard;

                //
                float sphereZ = _Radius * sqrt(1.-sqrDist);

                // maybe make spherical depth, like get the z val of sphere.. check seb lague mabe
                return 0.001;//sphereZ*0.001;
            }
            ENDCG
        }
    }
}