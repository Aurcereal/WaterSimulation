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
            };

            StructuredBuffer<FoamParticle> foamParticleBuffer;

            struct vIn
            {
                float4 vertex : POSITION;
                float4 normal : NORMAL;
            };

            struct vOut
            {
                float4 vertex : SV_POSITION;
                float3 normal : TEXCOORD1;
            };

            float _Radius;

            vOut vert (vIn v, uint instanceID : SV_InstanceID)
            {
                vOut o;

                float3 objectPos = v.vertex.xyz * _Radius;
                float3 worldPos = objectPos + foamParticleBuffer[instanceID].position;
                o.vertex = mul(UNITY_MATRIX_VP, float4(worldPos, 1.));

                o.normal = mul(float4(v.normal.xyz, 0.), unity_WorldToObject).xyz;
                
                return o;
            }

            fixed4 frag(vOut i) : SV_Target
            {
                return float4(1.,1.,1.,1.);
            }
            ENDCG
        }
    }
}