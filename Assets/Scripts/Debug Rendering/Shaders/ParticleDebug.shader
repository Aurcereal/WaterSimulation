// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Unlit/ParticleDebug"
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
                float2 uv : TEXCOORD0;
            };

            struct vOut
            {
                float4 vertex : SV_POSITION;
                float3 normal : TEXCOORD1;
                float3 color : TEXCOORD2;
                float2 uv : TEXCOORD3;
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
                //o.normal = mul(float4(v.normal.xyz, 0.), unity_WorldToObject).xyz;
                o.color = colorBuffer[instanceID].rgb;
                o.vertex = mul(UNITY_MATRIX_VP, float4(worldPos, 1.));

                o.uv = v.uv;
                
                return o;
            }

            fixed4 frag(vOut i) : SV_Target
            {

                float3 ambient = 0.4;
                
                float diffuse = max(0., dot(normalize(i.normal), float3(1.,1.,1.)/sqrt(3.)));
                float3 diffuseContribution = i.color * diffuse * 1.5;

                float2 p = i.uv*2.-1.;
                float sqrDist = dot(p,p);
                if(sqrDist >= 1.) discard;

                return float4(i.color,1.);//float4(ambient + diffuseContribution, 1.);
            }
            ENDCG
        }
    }
}