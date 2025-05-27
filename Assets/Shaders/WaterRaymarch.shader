// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Unlit/WaterRaymarch"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {} 
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
			//#pragma target 4.5

            StructuredBuffer<float3> positions;
            RWStructuredBuffer<float> masses;
            RWStructuredBuffer<float> densities;
            //StructuredBuffer<float4> colors;

            sampler2D _MainTex;

            const int ParticleCount;
            const float SmoothingRadius;
            const float SqrSmoothingRadius;
            const float InvSmoothingRadius;

            #include "../Scripts/Sim/Resources/MathHelper.hlsl"
            #include "../Scripts/Sim/Resources/ParticleMath3D.hlsl"
            #include "../Scripts/Sim/Resources/SpatialHash3D.hlsl"

            #define PI 3.141592

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

            vOut vert (vIn v)
            {
                vOut o;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                
                return o;
            }

            float CalculateDensity(float3 pos) {
                float totalDensity = 0.;

                int3 centerCellPos = posToCell(pos);
                int3 currCell;
                int particleIndex;

                for(int x=-1; x<=1; x++) {
                    for(int y=-1; y<=1; y++) {
                        for(int z=-1; z<=1; z++) {
                            currCell = centerCellPos + int3(x, y, z);

                            int key = getCellKey(currCell);
                            int currIndex = getStartIndex(key);

                            if(currIndex != -1) {
                                while(currIndex < ParticleCount && particleCellKeyEntries[currIndex].key == key) {

                                    particleIndex = particleCellKeyEntries[currIndex].particleIndex;
                                    
                                    float sqrDist = dot(positions[particleIndex] - pos, positions[particleIndex] - pos);
                                    if(sqrDist <= SmoothingRadius*SmoothingRadius) {
                                        totalDensity += masses[particleIndex] * SmoothingKernelPow2(SmoothingRadius, sqrt(sqrDist));
                                    }

                                    currIndex++;
                                }
                            }
                        }
                    }
                }

                return totalDensity;
            }

            //
            const float FovY;
            const float Aspect;

            const float3 CamRi;
            const float3 CamUp;
            const float3 CamFo;

            const float3 CamPos;

            float3 Raycast(float2 uv) {
                float2 p = uv*2.0-1.0;

                float3 rd = normalize(float3(tan(FovY*0.5) * (p * float2(Aspect, 1.0)), 1.0));
                return mul(float3x3(CamRi, CamUp, CamFo), rd);
            }

            //
            // TOOD: put box data (just inv transform 4x4) in here and do a ray box intersection

            fixed4 frag(vOut i) : SV_Target
            {
                float3 rd = Raycast(i.uv);
                //return float4(rd, 1.0);

                return tex2D(_MainTex, i.uv) * float4(1.0, 0.2, 0.2, 1.0);
            }
            ENDCG
        }
    }
}