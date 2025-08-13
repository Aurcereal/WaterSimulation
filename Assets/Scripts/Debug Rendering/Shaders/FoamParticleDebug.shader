// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Unlit/FoamParticleDebug"
{
    Properties
    {
        _Radius ("Radius", Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
            #pragma editor_sync_compilation
			#pragma target 4.5

            struct FoamParticle {
                float3 position;
                float3 velocity;
                float remainingLifetime;
                int debugType;
            };

            StructuredBuffer<FoamParticle> foamParticleBuffer;

            struct vIn
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct vOut
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            const float _FoamRadius;

            vOut vert (vIn v, uint instanceID : SV_InstanceID)
            {
                vOut o;

                float3 camRi = unity_CameraToWorld._m00_m10_m20; // Row major
                float3 camUp = unity_CameraToWorld._m01_m11_m21;

                float3 objectPos = v.vertex.xyz * _FoamRadius;
                objectPos = objectPos.x * camRi + objectPos.y * camUp; // Orient towards cam
                float3 worldPos = objectPos + foamParticleBuffer[instanceID].position;
                o.vertex = mul(UNITY_MATRIX_VP, float4(worldPos, 1.));

                o.uv = v.uv;
                
                return o;
            }

            fixed4 frag(vOut i) : SV_Target
            {
                float2 p = i.uv*2.-1.;
                float sqrDist = dot(p,p);
                if(sqrDist >= 1.) discard;

                return 1.;
            }
            ENDCG
        }
    }
}