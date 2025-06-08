// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Unlit/InstancedParticle3D"
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

            StructuredBuffer<float3> positionBuffer;
            StructuredBuffer<float4> colorBuffer;

            struct vIn
            {
                float4 vertex : POSITION;
                float4 normal : NORMAL;
            };

            struct vOut
            {
                float4 vertex : SV_POSITION;
                float3 normal : TEXCOORD1;
                float3 color : TEXCOORD2;
            };

            float _Radius;

            vOut vert (vIn v, uint instanceID : SV_InstanceID)
            {
                vOut o;

                float3 objectPos = v.vertex.xyz * _Radius;
                float3 worldPos = objectPos + positionBuffer[instanceID];
                o.vertex = mul(UNITY_MATRIX_VP, float4(worldPos, 1.));

                o.normal = mul(float4(v.normal.xyz, 0.), unity_WorldToObject).xyz;
                o.color = colorBuffer[instanceID].rgb;
                
                return o;
            }

            fixed4 frag(vOut i) : SV_Target
            {
                return float4(1.-i.vertex.zzz, 1.);

                float3 ambient = 0.4;
                
                float diffuse = max(0., dot(normalize(i.normal), float3(1.,1.,1.)/sqrt(3.)));
                float3 diffuseContribution = i.color * diffuse * 1.5;

                return float4(i.color,1.);//float4(ambient + diffuseContribution, 1.);
            }
            ENDCG
        }
    }
}