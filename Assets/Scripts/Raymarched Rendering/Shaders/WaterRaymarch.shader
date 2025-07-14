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
            sampler2D FoamTex;

            //
            const float3 LightDir;

            float3 Raycast(float2 uv) {
                float2 p = uv*2.0-1.0;

                float3 rd = normalize(float3(tan(FovY*0.5) * (p * float2(Aspect, 1.0)), 1.0));
                return CamRi * rd.x + CamUp * rd.y + CamFo * rd.z; // float3x3(row, row, row) not column..
            }

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

            const bool UseShadowMapping;

            float GetShadowOcclusion(float3 pos) {
                if(!UseShadowMapping) return 1.;
                return 1.;
                // TODO: implement
            }

            samplerCUBE EnvironmentMap;

            float3 SampleSkybox(float3 rd) {
                return LightMultiplier*texCUBE(EnvironmentMap, rd);//SampleSpaceSkybox(rd, float2(rd.x, rd.y), CamFo);////;//1.;//;//
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

            float3 CalculateNormal(float3 pos) {
                // -Gradient of Density
                const float2 eps = float2(WATERNORMEPS, 0.);

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

            #define INTERTYPE_OBJECT 0
            #define INTERTYPE_WATER 1
            #define INTERTYPE_SKYBOX 2

            float2 RayIntersectEnvironment(float3 ro, float3 rd, bool isInsideLiquid, out int intersectionType) {
                float objDist = RayIntersectScene(ro, rd);
                float2 waterInter = RayIntersectWater(ro, rd, objDist, isInsideLiquid);
                float waterDist = waterInter.x; float accumDensity = waterInter.y;

                if(objDist + waterDist >= 2. * MAXDIST) {
                    intersectionType = INTERTYPE_SKYBOX;
                    return float2(MAXDIST, 0.);
                }

                if(objDist <= waterDist) {
                    intersectionType = INTERTYPE_OBJECT;
                    return float2(objDist, accumDensity);
                } else {
                    intersectionType = INTERTYPE_WATER;
                    return waterInter;
                }
            }

            bool IsInsideLiquid(float3 pos) {
                return SampleDensity(pos) >= WaterExistenceThreshold;
            }

            float3 TraceWaterRayOverride(float3 ro, float3 rd, float2 sp, bool firstFollowReflect, float foamT) {
                float3 transmittance = 1.;
                float3 li = 0.;

                bool isInsideLiquid = IsInsideLiquid(ro);
                
                float ft = foamT;

                for(int i=0; i<2; i++) { //min(NumBounces, MAXBOUNCECOUNT); i++) {

                    int interType;
                    float2 inter = RayIntersectEnvironment(ro, rd, isInsideLiquid, interType);
                    float t = inter.x; float densityAlongRay = inter.y;
                    float3 hitPos = ro + rd*t;
                    float3 norm = CalculateNormal(hitPos);
                    if(isInsideLiquid) norm *= -1.0;

                    // Moved this above inter break
                    transmittance *= exp(- ExtinctionCoefficients * densityAlongRay);

                    //
                    if(!firstFollowReflect && i <= 1 && ft <= t) {
                        const float3 FoamColor = 1.; // TODO: temp
                        return li + FoamColor;// * transmittance
                    }
                    ft -= t;

                    if(interType != INTERTYPE_WATER) {
                        if(i==0) return 0.5*SampleEnvironment(hitPos, rd)/(interType == INTERTYPE_OBJECT ? 1.0 : LightMultiplier);
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
                li += transmittance * transmittanceToLight * SampleEnvironment(ro, rd);

                return li;
            }

            fixed4 frag(vOut i) : SV_Target
            {
                float3 ro = CamPos;
                float3 rd = Raycast(i.uv);

                //
                float2 sp = (i.uv*2.0-1.0)*float2(1.3, 1.0);
                
                //
                float distAlongRayToFoam = tex2D(FoamTex, i.uv).b/dot(rd, CamFo);

                //
                float3 accumLight;

                if(!TraceReflectAndRefract) {
                    accumLight = 0.;//TraceWaterRay(ro, rd, sp);
                } else {
                    // TODO: Foam partial transparency? Well more like just the raymarched foam I can do later with spatial hashing and better sort
                    // TODO: Separate reflect and refract into different funcs for performance
                    // TODO: Can optimize more like so many repeated intersection and density marching calculations idk if they're bottleneck tho
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