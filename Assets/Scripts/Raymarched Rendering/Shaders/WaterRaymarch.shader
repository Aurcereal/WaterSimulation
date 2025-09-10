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
            #pragma multi_compile CHECKERFLOOR_ENV EMPTY_ENV FOUNTAIN_ENV FALL_ENV CUP_ENV HONEY_ENV
            #pragma multi_compile BILLBOARD_FOAM __
            #pragma multi_compile RAYMARCHED_FOAM __
            #pragma multi_compile CAUSTICS __
            #pragma multi_compile SHADOWS __

            #pragma editor_sync_compilation
            #pragma vertex vert
			#pragma fragment frag
			#pragma target 4.5

            #include "../../../Scripts/Sim/Resources/MathHelper.hlsl"
            #include "../../../Scripts/Sim/Resources/RaytraceMath.hlsl"
            #include "./Skybox.hlsl"

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

            // Cam Params in SDFScene.hlsl

            //
            const float4x4 ContainerInverseTransform;
            const float4x4 ContainerTransform;
            const float3 ContainerScale;

            //
            const float4x4 ObstacleInverseTransform;
            const float3 ObstacleScale;
            const bool ObstacleType;

            //
            Texture3D<float> DensityTexture;
            SamplerState _LinearClamp; // https://docs.unity3d.com/Manual/SL-SamplerStates.html

            //
            const float DensityMultiplier;
            const float LightMultiplier;
            const float3 ExtinctionCoefficients;
            const float IndexOfRefraction;

            #define MAXBOUNCECOUNT 4
            const bool TraceReflectAndRefract;
            const int NumBounces;
            const float WaterExistenceThreshold;
            const float WaterExistenceEps;
            const float NextRayOffset;

            //
            const bool UseShadows;
            const bool UseCaustics;
            const bool UseRaymarchedFoam;
            const bool UseBillboardFoam;

            //
            sampler2D FoamTex;

            //
            const float3 LightDir;

            float SampleDensity(float3 p) {
                float3 lp = mul(ContainerInverseTransform, float4(p, 1.0));
                
                float3 alp = abs(lp);
                if(max(alp.x, max(alp.y, alp.z)) > .5) return 0.;
                
                float3 uv = lp+0.5;
                return DensityMultiplier*DensityTexture.SampleLevel(_LinearClamp, uv, 0.0, 0).r;
            }

            // Same as SampleDensity but slightly better performance (no transformation)
            float SampleLocalDensity(float3 lp) {
                lp /= ContainerScale;
                
                float3 alp = abs(lp);
                if(max(alp.x, max(alp.y, alp.z)) > .5) return 0.;
                
                float3 uv = lp+0.5;
                return DensityMultiplier*DensityTexture.SampleLevel(_LinearClamp, uv, 0.0, 0).r;//DensityTexture[uv*float3(30.,20.,10.)];
            }

            #define STEPSIZE 0.01
            #define BIGSTEPSIZE 0.1 // 0.1
            #define WATERNORMEPS 0.1 // 0.001
            #define MAXDIST 1000.0 // 250.0

            #include "./SDFScene.hlsl"

            float3 Raycast(float2 uv) {
                float2 p = uv*2.0-1.0;

                float3 rd = normalize(float3(tan(FovY*0.5) * (p * float2(Aspect, 1.0)), 1.0));
                return CamRi * rd.x + CamUp * rd.y + CamFo * rd.z; // float3x3(row, row, row) not column..
            }

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

                bool foundFirstWater = false;

                while(tCurr <= tEnd) {
                    float3 lpos = lro + lrd * tCurr;
                    float dens = SampleLocalDensity(lpos);
                    accumDensity += BIGSTEPSIZE * dens;
                    tCurr += BIGSTEPSIZE;

                    #if 1
                    // Only go through the first 'patch' of water optimization, could lead to errors when multiple patches along ray
                    if(!foundFirstWater) {
                        if(dens >= WaterExistenceThreshold + WaterExistenceEps) foundFirstWater = true;
                    } else {
                        if(dens < WaterExistenceThreshold - WaterExistenceEps) break;
                    }
                    #endif
                }

                return accumDensity;
            }

            float CalculateDensityAlongRayStopAtObject(float3 ro, float3 rd) {
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
                float tEnd = min(boxTs.y, RayIntersectScene(ro, rd));

                float accumDensity = 0.;

                while(tCurr <= tEnd) {
                    float3 lpos = lro + lrd * tCurr;
                    accumDensity += BIGSTEPSIZE * SampleLocalDensity(lpos);
                    tCurr += BIGSTEPSIZE;
                }

                return accumDensity;
            }

            #include "./FoamSpace.hlsl"

            samplerCUBE EnvironmentMap;

            float3 SampleSkybox(float3 rd) {
                return LightMultiplier*texCUBE(EnvironmentMap, rd) + SampleSun(rd);
            }

            float3 SampleCameraSkybox(float3 rd) {
                return texCUBE(EnvironmentMap, rd);// + SampleSun(rd);
            }

            // ior is Index of Medium we're in div by Index of Medium we're entering (this divided by that)
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

            float3 CalculateWaterNormal(float3 pos) {
                // -Gradient of Density
                const float2 eps = float2(WATERNORMEPS, 0.);

                // Can turn it to SampleLocalDensity later by transforming the normal direction and stuff
                return -normalize(float3(
                    SampleDensity(pos+eps.xyy) - SampleDensity(pos-eps.xyy),
                    SampleDensity(pos+eps.yxy) - SampleDensity(pos-eps.yxy),
                    SampleDensity(pos+eps.yyx) - SampleDensity(pos-eps.yyx)
                ));
            }

            float3 CalculateWaterNormal(float3 pos, float epsDist) {
                // -Gradient of Density
                const float2 eps = float2(epsDist, 0.);

                // Can turn it to SampleLocalDensity later by transforming the normal direction and stuff
                return -normalize(float3(
                    SampleDensity(pos+eps.xyy) - SampleDensity(pos-eps.xyy),
                    SampleDensity(pos+eps.yxy) - SampleDensity(pos-eps.yxy),
                    SampleDensity(pos+eps.yyx) - SampleDensity(pos-eps.yyx)
                ));
            }

            float2 RayIntersectWater(float3 ro, float3 rd, float objDist, bool isInsideLiquid) { // ~ (t, densityAlongRay)
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
                float tEnd = min(objDist, boxTs.y);

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
                        if(dens > WaterExistenceThreshold + WaterExistenceEps) {
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

            float3 RayIntersectWaterFoam(float3 ro, float3 rd, float objDist, bool isInsideLiquid) { // ~ (t, densityAlongRay, foamAlongRay)
                // First do bounding box, then step for water existence threshold, possibly doing binary search later
                // keep in mind that we can do larger steps if density is small, we wanna do much smaller steps when we're in water than when we're in air

                // Bounding Box Intersection
                float3 lro = mul(ContainerInverseTransform, float4(ro, 1.));
                float3 lrd = mul(ContainerInverseTransform, float4(rd, 0.));

                float2 boxTs = rayBoxIntersect(lro, lrd);
                if(boxTs.x > boxTs.y) return float3(MAXDIST, 0., 1.);

                // Bring to Local Rot Trans space
                lro *= ContainerScale;
                lrd *= ContainerScale; 

                //
                float tStart = max(0., boxTs.x);
                float tCurr = tStart;
                float tEnd = min(objDist, boxTs.y);

                float accumDensity = 0.;
                float transmittanceThroughFoam = 1.;

                //
                float foamSamplePeriod = 40.; float currFoamSampleTime = 0.;
                float outsideWaterFoamSamplePeriod = 4.;
                if(isInsideLiquid) {
                    while(tCurr <= tEnd) {
                        // CheckFoamInsideVolumeRadius is an expensive call so DO IT LESS larger step size skipping areas youknowwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwyeahhhhhhhh
                        if(currFoamSampleTime >= foamSamplePeriod) {
                            float foamSample = CheckFoamInsideVolumeRadius(ro+rd*tCurr);
                            //foamSamplePeriod += -4. * (foamSample*2.-1.); foamSamplePeriod = max(foamSamplePeriod, 10.);
                            transmittanceThroughFoam *= exp(-foamSample * 80. * 0.1 * STEPSIZE * (foamSamplePeriod+1.) / (1.+0.1*accumDensity));
                            currFoamSampleTime = 0.;
                        } else {
                            currFoamSampleTime += 1.;
                        }
                        float3 lpos = lro + lrd * tCurr;
                        float dens = SampleLocalDensity(lpos);
                        if(dens < WaterExistenceThreshold - WaterExistenceEps) {
                            return float3(tCurr, accumDensity, transmittanceThroughFoam);
                        }
                        accumDensity += STEPSIZE * dens;
                        tCurr += STEPSIZE;
                    }
                } else {
                    while(tCurr <= tEnd) {
                        float3 lpos = lro + lrd * tCurr;

                        float dens = SampleLocalDensity(lpos);
                        if(dens > WaterExistenceThreshold + WaterExistenceEps) {
                            while(dens >= WaterExistenceThreshold + WaterExistenceEps && tCurr >= tStart) {
                                tCurr -= STEPSIZE;
                                float3 lpos = lro + lrd * tCurr;
                                dens = SampleLocalDensity(lpos);
                            }
                            tCurr += STEPSIZE;
                            return float3(tCurr, accumDensity, transmittanceThroughFoam);
                        }

                        accumDensity += BIGSTEPSIZE * dens;
                        tCurr += BIGSTEPSIZE;
                    }
                }
                
                return float3(MAXDIST, accumDensity, transmittanceThroughFoam);
            }

            #define INTERTYPE_OBJECT 0
            #define INTERTYPE_WATER 1
            #define INTERTYPE_SKYBOX 2

            float3 RayIntersectEnvironment(float3 ro, float3 rd, bool isInsideLiquid, out int intersectionType) {
                float objDist = RayIntersectScene(ro, rd);
                float2 waterInter = RayIntersectWater(ro, rd, objDist, isInsideLiquid);
                float waterDist = waterInter.x; float accumDensity = waterInter.y;

                if(objDist + waterDist >= 2. * MAXDIST) {
                    intersectionType = INTERTYPE_SKYBOX;
                    return float3(MAXDIST, 0., 1.);
                }

                if(objDist <= waterDist) {
                    intersectionType = INTERTYPE_OBJECT;
                    return float3(objDist, accumDensity, 1.);
                } else {
                    intersectionType = INTERTYPE_WATER;
                    return float3(waterInter.x, waterInter.y, 1.);
                }
            }

            float3 RayIntersectEnvironmentFoam(float3 ro, float3 rd, bool isInsideLiquid, out int intersectionType) {
                float objDist = RayIntersectScene(ro, rd);
                float3 waterInter = RayIntersectWaterFoam(ro, rd, objDist, isInsideLiquid);
                float waterDist = waterInter.x; float accumDensity = waterInter.y;

                if(objDist + waterDist >= 2. * MAXDIST) {
                    intersectionType = INTERTYPE_SKYBOX;
                    return float3(MAXDIST, 0., waterInter.z); // RVS last component should be able to always be 1
                }

                if(objDist <= waterDist) {
                    intersectionType = INTERTYPE_OBJECT;
                    return float3(objDist, accumDensity, waterInter.z);
                } else {
                    intersectionType = INTERTYPE_WATER;
                    return waterInter;
                }
            }

            bool IsInsideLiquid(float3 pos) {
                return SampleDensity(pos) >= WaterExistenceThreshold;
            }

            //#define SDF_SHADOWS // Make feature

            float GetShadowOcclusion(float3 pos) {
                if(!UseShadows) return 1.;

                pos += normal(pos)*0.07;

                // Occluded by object
                #ifdef SDF_SHADOWS
                float objDist = RayIntersectScene(pos, -LightDir);
                if(objDist < MAXDIST-0.001) return 0.;
                #endif

                // Get to the liquid
                if(!IsInsideLiquid(pos)) {
                    float2 waterInter = RayIntersectWater(pos, -LightDir, MAXDIST, false);
                    if(waterInter.x >= MAXDIST) return 1.;
                    pos += waterInter.x * (-LightDir);
                }

                // TODO: can edit shadow extinction coefficients (scalar is fine but vec y not)
                float densAlongRay = RayIntersectWater(pos, -LightDir, MAXDIST, true);
                float transmittance = exp(-1. * ExtinctionCoefficients * densAlongRay);

                return transmittance;
            }

            float3 TraceWaterRayOverride(float3 ro, float3 rd, float2 sp, bool firstFollowReflect, float foamT) {
                float3 transmittance = 1.;
                float3 li = 0.;

                bool isInsideLiquid = IsInsideLiquid(ro);
                
                float ft = foamT;
                float foamTransmittance = 1.;

                for(int i=0; i<2; i++) { //min(NumBounces, MAXBOUNCECOUNT); i++) {

                    int interType;
                    #ifdef RAYMARCHED_FOAM
                    float3 inter = !firstFollowReflect ? RayIntersectEnvironmentFoam(ro, rd, isInsideLiquid, interType) : RayIntersectEnvironment(ro, rd, isInsideLiquid, interType);
                    #else
                    float3 inter = RayIntersectEnvironment(ro, rd, isInsideLiquid, interType);
                    #endif
                    float t = inter.x; float densityAlongRay = inter.y; foamTransmittance *= inter.z;
                    float3 hitPos = ro + rd*t;
                    float3 norm = CalculateWaterNormal(hitPos);
                    if(isInsideLiquid) norm *= -1.0;

                    // Moved this above inter break
                    transmittance *= exp(- ExtinctionCoefficients * densityAlongRay);

                    #ifdef BILLBOARD_FOAM
                    if(!firstFollowReflect && i <= 1 && ft <= t) {
                        const float3 FoamColor = 1.; // TODO: temp, expose it to editor
                        return li + 0.9*FoamColor;
                    }
                    ft -= t;
                    #endif

                    if(interType != INTERTYPE_WATER) {
                        if(i==0) return 0.5*SampleCameraEnvironment(ro, rd);

                        #ifdef CAUSTICS
                        if(interType == INTERTYPE_OBJECT && i==1 && !firstFollowReflect) {
                            // Caustics
                            float3 floorPoint = hitPos + normal(hitPos)*.02; // Escape SDEps
                            float distUpToScene = RayIntersectScene(floorPoint+float3(0.,1.,0.), float3(0.,1.,0.));
                            if(distUpToScene >= MAXDIST) { // No object blocking sun.. could soften caustics? TODO ?
                                float2 causticWaterInter = RayIntersectWater(floorPoint, float3(0.,1.,0.), MAXDIST, true);

                                float3 waterExitPos = floorPoint + float3(0.,causticWaterInter.x, 0.);
                                float3 waterExitNormal = CalculateWaterNormal(waterExitPos, .3);
                                float3 transmittanceToWaterExit = exp(-ExtinctionCoefficients * float3(1.35, 1.1, 1.) * max(1.8, causticWaterInter.y) * 3.);

                                float3 causticRefractRay = Refract(float3(0.,-1.,0.), -waterExitNormal, IndexOfRefraction);
                                float sunAlignment = max(causticRefractRay.y, 0.);

                                // Fade out caustics near borders cuz density gets too low there so we get bright edges
                                float3 lp = ContainerScale * mul(ContainerInverseTransform, float4(floorPoint, 1.)).xyz;
                                float2 dist2D = ContainerScale.xz*.5 - abs(lp.xz);
                                float minDist = max(0., min(dist2D.x, dist2D.y));
                                float causticsBorderFadeout = smoothstep(0.2, .8, minDist);

                                li += causticsBorderFadeout * transmittance * transmittanceToWaterExit * exp(-ExtinctionCoefficients * float3(1.35, 1.1, 1.) * 1.4) * ( 
                                    smoothstep(0.97, 1.0, pow(sunAlignment, 4.)) +
                                    smoothstep(0.98, 1.0, pow(sunAlignment, 8.)) +
                                    smoothstep(0.992, 1.0, pow(sunAlignment, 16.))
                                );
                            }
                        }
                        #endif
                        break;
                    }

                    float ior = isInsideLiquid ? IndexOfRefraction : 1./IndexOfRefraction;

                    float f = Fresnel(-rd, norm, ior);

                    float3 reflectRay = Reflect(-rd, norm);
                    float3 refractRay = Refract(-rd, norm, ior);

                    // Optimize, shouldn't have these 2 calculate densities and not stop at water when we use same intersection info in next loop
                    float densAlongReflect = CalculateDensityAlongRayStopAtObject(hitPos+reflectRay*0.0005, reflectRay);
                    float densAlongRefract = CalculateDensityAlongRayStopAtObject(hitPos+refractRay*0.0005, refractRay);

                    float reflectTransmittance = f * exp(-ExtinctionCoefficients * densAlongReflect);
                    float refractTransmittance = (1.0-f) * exp(-ExtinctionCoefficients * densAlongRefract);

                    if((i == 0 && firstFollowReflect) || (i != 0 && reflectTransmittance >= refractTransmittance)) { // f >= 0.5
                        rd = reflectRay;
                        ro = hitPos + (norm+rd)*NextRayOffset;
                        transmittance *= f;

                        if(i != 0) li += transmittance * refractTransmittance * SampleEnvironment(ro, refractRay);
                    } else {
                        rd = refractRay;
                        ro = hitPos + (norm+rd)*NextRayOffset;
                        transmittance *= 1.-f;

                        isInsideLiquid = !isInsideLiquid;

                        if(i != 0) li += transmittance * reflectTransmittance * SampleEnvironment(ro, reflectRay);
                    }

                }

                // We already calculate this density in case of t >= MAXDIST..
                float3 transmittanceToLight = exp(-ExtinctionCoefficients * CalculateDensityAlongRay(ro, rd)); // Can use big step sizefor this one
                li += lerp(transmittance * transmittanceToLight * SampleEnvironment(ro, rd), float3(1.,1.,1.), .9*(1.-foamTransmittance));

                return li;
            }

            fixed4 frag(vOut i) : SV_Target
            {
                float3 ro = CamPos;
                float3 rd = Raycast(i.uv);

                //
                float2 sp = (i.uv*2.0-1.0)*float2(1.3, 1.0);
                
                //
                #ifdef BILLBOARD_FOAM
                float distAlongRayToFoam = tex2D(FoamTex, i.uv).b/dot(rd, CamFo); // RVS
                #else
                float distAlongRayToFoam = 100000.0;
                #endif

                //
                float3 accumLight;

                if(!TraceReflectAndRefract) {
                    accumLight = 0.;//TraceWaterRay(ro, rd, sp); TODO: add this back in after u separate tracewaterrayoverride into 2 funcs
                } else {
                    // TODO: Separate reflect and refract into different funcs for performance and THEN
                    // TODO: Can optimize more like so many repeated intersection and density marching calculations idk if they're bottleneck tho, reflect and refract rays have exact same sequence up until first intersection so can avoid doing same thing twice
                    float3 accumReflectLight = TraceWaterRayOverride(ro, rd, sp, true, distAlongRayToFoam);
                    float3 accumRefractLight = TraceWaterRayOverride(ro, rd, sp, false, distAlongRayToFoam);
                    accumLight = accumReflectLight + accumRefractLight;
                }

                float3 col = pow(accumLight/(1.+accumLight),1./2.2);

                return float4(accumLight, 1.);
            }
            ENDCG
        }
    }
}