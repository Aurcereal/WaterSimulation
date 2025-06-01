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

            #include "../Scripts/Sim/Resources/MathHelper.hlsl"
            #include "../Scripts/Sim/Resources/RaytraceMath.hlsl"

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
            const float3 ContainerScale;

            //
            Texture3D<float> DensityTexture;
            SamplerState _LinearClamp; // https://docs.unity3d.com/Manual/SL-SamplerStates.html

            //
            const float DensityMultiplier;
            const float LightMultiplier;
            const float3 ExtinctionCoefficients;

            //
            const float3 LightDir;

            float3 Raycast(float2 uv) {
                float2 p = uv*2.0-1.0;

                float3 rd = normalize(float3(tan(FovY*0.5) * (p * float2(Aspect, 1.0)), 1.0));
                return CamRi * rd.x + CamUp * rd.y + CamFo * rd.z; // float3x3(row, row, row) not column..
            }

            // maybe should march in local space or smth.. or at least local rotation and position, maybe not scale (cuz transforming takes a lot of time)
            float SampleDensity(float3 p) {
                float3 lp = mul(ContainerInverseTransform, float4(p, 1.0));
                
                float3 alp = abs(lp);
                if(max(alp.x, max(alp.y, alp.z)) > .5) return 0.;
                
                float3 uv = lp+0.5;
                return DensityMultiplier*DensityTexture.SampleLevel(_LinearClamp, uv, 0.0, 0).r;//DensityTexture[uv*float3(30.,20.,10.)];
            }

            float SampleLocalDensity(float3 lp) {
                lp /= ContainerScale;
                
                float3 alp = abs(lp);
                if(max(alp.x, max(alp.y, alp.z)) > .5) return 0.;
                
                float3 uv = lp+0.5;
                return DensityMultiplier*DensityTexture.SampleLevel(_LinearClamp, uv, 0.0, 0).r;//DensityTexture[uv*float3(30.,20.,10.)];
            }

            const int NumBounces;
            const float WaterExistenceThreshold;
            const float WaterExistenceEps;

            #define STEPSIZE 0.01
            #define BIGSTEPSIZE 0.5
            #define NORMEPS 0.001
            #define MAXDIST 1000.0

            float CalculateDensityAlongRay(float3 ro, float3 rd) {
                // Bounding Box Intersection
                float3 lro = mul(ContainerInverseTransform, float4(ro, 1.));
                float3 lrd = mul(ContainerInverseTransform, float4(rd, 0.));

                float2 boxTs = rayBoxIntersect(lro, lrd);
                if(boxTs.x > boxTs.y) return 0.;

                // Bring to Local Rot Trans space
                lro *= ContainerScale;
                lrd *= ContainerScale; 

                //
                float tCurr = max(0., boxTs.x);
                float tEnd = boxTs.y;

                float accumDensity = 0.;

                while(tCurr <= tEnd) {
                    // TODO: Optimize and make it so once we find the first thing over existence threshold, we stop when we find the first thing not over existence threshold.
                    // Can also optimize with larger step size and then a binary search once we find wall
                    float3 lpos = lro + lrd * tCurr;
                    accumDensity += BIGSTEPSIZE * SampleLocalDensity(lpos);
                    tCurr += BIGSTEPSIZE;
                }

                return accumDensity;
            }

            float3 AccumLightAlongRay(float3 ro, float3 rd) {
                
                // Bounding Box Intersection
                float3 lro = mul(ContainerInverseTransform, float4(ro, 1.));
                float3 lrd = mul(ContainerInverseTransform, float4(rd, 0.));

                float2 boxTs = rayBoxIntersect(lro, lrd);
                if(boxTs.x > boxTs.y) return 0.;

                //
                float tCurr = max(0., boxTs.x);
                float tEnd = boxTs.y;

                float3 accumLight = 0.;
                float3 transmittance = 1.;

                while(tCurr <= tEnd) {
                    float3 pos = ro + rd * tCurr;

                    float densityAlongStep = STEPSIZE * SampleDensity(pos);
                    transmittance *= exp(-densityAlongStep * ExtinctionCoefficients);
                    float3 Li = exp(- ExtinctionCoefficients * CalculateDensityAlongRay(pos, -LightDir)); // Directional light
                    accumLight += transmittance * densityAlongStep * ExtinctionCoefficients * Li;

                    tCurr += STEPSIZE;
                }

                return LightMultiplier * accumLight;
            }

            float3 SampleEnvironment(float3 rd) {
                return LightMultiplier*1.;//float3(10., 0., 0.);
            }

            // ior is Index of Medium we're in div by Index of Medium we're entering
            float3 Refract(float3 wo, float3 norm, float ior) {
                float cosThetaI = dot(wo, norm);
                float sinThetaT2 = ior * ior * (1.0 - cosThetaI*cosThetaI);
                if(sinThetaT2 > 1.0) return float3(0.,1.,0.); // TIR
                float cosThetaT = sqrt(1.0-sinThetaT2);
                return - ior * wo + (ior * cosThetaI - cosThetaT) * norm;
            }

            float3 Reflect(float3 wo, float3 norm) {
                return -(wo - 2.0 * norm * dot(norm, wo));
            }

            // Water rSchlick2
            float Fresnel(float3 wo, float3 norm, float ior) {
                float baseReflectance = (ior - 1.0)/(ior + 1.0);
                baseReflectance *= baseReflectance;
                float cosThetaI = dot(wo, norm);
                float cosTheta;

                if(ior > 1.0) {
                    float sinThetaT2 = ior * ior * (1.0 - cosThetaI*cosThetaI);
                    if(sinThetaT2 > 1.0) return 1.0; // TIR
                    float cosThetaT = sqrt(1.0-sinThetaT2);

                    // Use the transmitted angle
                    cosTheta = cosThetaT;
                } else {
                    // Use the incident angle
                    cosTheta = cosThetaI;
                }

                float x = 1.0 - cosTheta;
                return baseReflectance + (1.0 - baseReflectance) * x*x*x*x*x;
            }

            float3 CalculateNormal(float3 pos) {
                // -Gradient of Density
                const float2 eps = float2(NORMEPS, 0.);

                // Can turn it to SampleLocalDensity later by transforming the normal direction and stuff
                return -normalize(float3(
                    SampleDensity(pos+eps.xyy) - SampleDensity(pos-eps.xyy),
                    SampleDensity(pos+eps.yxy) - SampleDensity(pos-eps.yxy),
                    SampleDensity(pos+eps.yyx) - SampleDensity(pos-eps.yyx)
                ));
            }

            float2 RayIntersectWater(float3 ro, float3 rd, bool isInsideLiquid) { // ~ (t, densityAlongRay)
                // First do bounding box, then step for water existence threshold, possibly doing binary search later
                // keep in mind that we can do larger steps if density is small, we wanna do much smaller steps when we're in water than when we're in air

                // Bounding Box Intersection
                float3 lro = mul(ContainerInverseTransform, float4(ro, 1.));
                float3 lrd = mul(ContainerInverseTransform, float4(rd, 0.));

                float2 boxTs = rayBoxIntersect(lro, lrd);
                if(boxTs.x > boxTs.y) return float2(MAXDIST, 0.);

                // Bring to Local Rot Trans space
                lro *= ContainerScale;
                lrd *= ContainerScale; 

                //
                float tStart = max(0., boxTs.x);
                float tCurr = tStart;
                float tEnd = boxTs.y;

                float accumDensity = 0.;

                //
                if(isInsideLiquid) {
                    while(tCurr <= tEnd) {
                    float3 lpos = lro + lrd * tCurr;
                    float dens = SampleLocalDensity(lpos);
                    if(dens < WaterExistenceThreshold - WaterExistenceEps) {
                        return float2(tCurr, accumDensity);
                    }
                    accumDensity += STEPSIZE * dens;
                    tCurr += STEPSIZE;
                }
                } else {
                    while(tCurr <= tEnd) {
                        float3 lpos = lro + lrd * tCurr;

                        float dens = SampleLocalDensity(lpos);
                        if(dens >= WaterExistenceThreshold + WaterExistenceEps) {
                            while(dens >= WaterExistenceThreshold + WaterExistenceEps && tCurr >= tStart) {
                                tCurr -= STEPSIZE;
                                float3 lpos = lro + lrd * tCurr;
                                dens = SampleLocalDensity(lpos);
                            }
                            tCurr += STEPSIZE;
                            return float2(tCurr, accumDensity);
                        }

                        accumDensity += BIGSTEPSIZE * dens;
                        tCurr += BIGSTEPSIZE;
                    }
                }
                
                return float2(MAXDIST, accumDensity);
            }

            bool IsInsideLiquid(float3 pos) {
                return SampleDensity(pos) >= WaterExistenceThreshold;
            }

            float3 TraceWaterRay(float3 ro, float3 rd) {
                float3 transmittance = 1.;
                float3 li = 0.;

                for(int i=0; i<NumBounces; i++) {
                    bool isInsideLiquid = IsInsideLiquid(ro);

                    float2 inter = RayIntersectWater(ro, rd, isInsideLiquid);
                    float t = inter.x; float densityAlongRay = inter.y;

                    float3 hitPos = ro + rd*t;
                    float3 norm = CalculateNormal(hitPos);
                    if(isInsideLiquid) norm *= -1.0;

                    if(t >= MAXDIST) {
                        if(i==0) return 0.; // Just want the background to be black
                        break;
                    }

                    transmittance *= exp(- ExtinctionCoefficients * densityAlongRay);

                    float ior = isInsideLiquid ? 1.33 : 1./1.33;

                    float f = Fresnel(-rd, norm, ior);
                    float kReflect = f;
                    float kRefract = 1.-f;

                    float3 reflectRay = Reflect(-rd, norm);
                    float3 refractRay = Refract(-rd, norm, ior);

                    // Optimize, shouldn't have these 2 calculate densities and not stop at water when we use same intersection info in next loop
                    float densAlongReflect = CalculateDensityAlongRay(hitPos+reflectRay*0.0005, reflectRay);
                    float densAlongRefract = CalculateDensityAlongRay(hitPos+refractRay*0.0005, refractRay);

                    float reflectTransmittance = f * exp(-ExtinctionCoefficients * densAlongReflect);
                    float refractTransmittance = (1.0-f) * exp(-ExtinctionCoefficients * densAlongRefract);

                    if(reflectTransmittance >= refractTransmittance) { // f >= 0.5
                        rd = reflectRay;
                        ro = hitPos + rd*0.0005;
                        transmittance *= f;

                        li += transmittance * refractTransmittance * SampleEnvironment(refractRay);
                    } else {
                        rd = refractRay;
                        ro = hitPos + rd*0.0005;
                        transmittance *= 1.-f;

                        li += transmittance * reflectTransmittance * SampleEnvironment(reflectRay);
                    }
                }

                float3 transmittanceToLight = exp(-ExtinctionCoefficients * CalculateDensityAlongRay(ro, rd));
                li += transmittance * transmittanceToLight * SampleEnvironment(rd);

                return li;
            }

            fixed4 frag(vOut i) : SV_Target
            {
                float3 ro = CamPos;
                float3 rd = Raycast(i.uv);
                
                //
                float3 accumLight = TraceWaterRay(ro, rd);
                float3 col = pow(accumLight/(1.+accumLight),1./2.2);

                return float4(accumLight, 1.);
            }
            ENDCG
        }
    }
}