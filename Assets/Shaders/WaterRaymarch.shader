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
			#pragma target 4.5

            StructuredBuffer<float3> positions;
            StructuredBuffer<float> masses;
            StructuredBuffer<float> densities;
            //StructuredBuffer<float4> colors;

            sampler2D _MainTex;

            const int ParticleCount;
            const float SmoothingRadius;
            const float SqrSmoothingRadius;
            const float InvSmoothingRadius;

            #include "../Scripts/Sim/Resources/MathHelper.hlsl"
            #include "../Scripts/Sim/Resources/RaytraceMath.hlsl"
            #include "../Scripts/Sim/Resources/ParticleMath3D.hlsl"
            #include "../Scripts/Sim/Resources/SpatialHash3DWater.hlsl"

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
                float totalDensity = 0.0;

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

            //
            const float4x4 ContainerInverseTransform;
            const float4x4 ContainerTransform;

            //
            Texture3D<float> DensityTexture;
            SamplerState _LinearClamp;

            //
            const float DensityMultiplier;
            const float LightMultiplier;
            const float ExtinctionMultiplier;
            const float LightExtinctionMultiplier;

            float3 Raycast(float2 uv) {
                float2 p = uv*2.0-1.0;

                float3 rd = normalize(float3(tan(FovY*0.5) * (p * float2(Aspect, 1.0)), 1.0));
                return CamRi * rd.x + CamUp * rd.y + CamFo * rd.z; // float3x3(row, row, row) not column..
            }

            float3 SampleSkybox(float3 rd) {
                return float3(1., 0., 0.);
            }

            // maybe should march in local space or smth.. or at least local rotation and position, maybe not scale (cuz transforming takes a lot of time)
            float SampleDensity(float3 p) {
                float3 lp = mul(ContainerInverseTransform, float4(p, 1.0));
                
                float3 alp = abs(lp);
                if(max(alp.x, max(alp.y, alp.z)) > .5) return 0.;
                
                float3 uv = lp+0.5;
                return DensityMultiplier * DensityMultiplier*DensityTexture.SampleLevel(_LinearClamp, uv, 0.0, 0).r;//DensityTexture[uv*float3(30.,20.,10.)];
            }

            #define STEPSIZE 0.01
            #define BIGSTEPSIZE 0.5

            float CalculateDensityAlongRay(float3 ro, float3 rd) {
                // Bounding Box Intersection
                float3 lro = mul(ContainerInverseTransform, float4(ro, 1.));
                float3 lrd = mul(ContainerInverseTransform, float4(rd, 0.));

                float2 boxTs = rayBoxIntersect(lro, lrd);
                if(boxTs.x > boxTs.y) return 0.;
                // boxTs.x += 0.0001;
                // boxTs.y -= 0.0001;

                //
                float tCurr = max(0., boxTs.x);
                float tEnd = boxTs.y;

                float accumDensity = 0.;

                while(tCurr <= tEnd) {
                    float3 pos = ro + rd * tCurr;
                    accumDensity += BIGSTEPSIZE * SampleDensity(pos);
                    tCurr += BIGSTEPSIZE;
                }

                return accumDensity;
            }

            float3 AccumLightAlongRay(float3 ro, float3 rd) {
                // temp
                float3 lightDir = float3(1.,-1.,1.)/sqrt(3.);
                
                // Bounding Box Intersection
                float3 lro = mul(ContainerInverseTransform, float4(ro, 1.));
                float3 lrd = mul(ContainerInverseTransform, float4(rd, 0.));

                float2 boxTs = rayBoxIntersect(lro, lrd);
                if(boxTs.x > boxTs.y) return 0.;
                // boxTs.x += 0.0001;
                // boxTs.y -= 0.0001;

                //
                float tCurr = max(0., boxTs.x);
                float tEnd = boxTs.y;

                float3 accumLight = 0.;
                float transmittance = 1.;

                while(tCurr <= tEnd) {
                    float3 pos = ro + rd * tCurr;

                    float density = SampleDensity(pos);
                    transmittance *= exp(-STEPSIZE * ExtinctionMultiplier * density);
                    accumLight += STEPSIZE * transmittance * LightMultiplier * density * float3(1., 1., 1.) * float3(0.2, 0.4, 1.0) * exp(- LightExtinctionMultiplier * CalculateDensityAlongRay(pos, -lightDir));

                    tCurr += STEPSIZE;
                }

                return accumLight;
            }

            fixed4 frag(vOut i) : SV_Target
            {
                float3 ro = CamPos;
                float3 rd = Raycast(i.uv);
                
                //
                float3 accumLight = AccumLightAlongRay(ro, rd);
                // gamma reinhards?

                return float4(accumLight, 1.);//tex2D(_MainTex, i.uv) * float4(1.0, 0.2, 0.2, 1.0);
            }
            ENDCG
        }
    }
}