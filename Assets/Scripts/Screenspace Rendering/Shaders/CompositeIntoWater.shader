// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Unlit/CompositeIntoWater"
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
            #pragma multi_compile CHECKERFLOOR_ENV EMPTY_ENV FOUNTAIN_ENV FALL_ENV
            #pragma multi_compile BILLBOARD_FOAM __
            #pragma multi_compile CAUSTICS __
            #pragma multi_compile SHADOWS __
            #pragma vertex vert
			#pragma fragment frag
			#pragma target 4.5

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

            sampler2D SmoothedDepthTex;
            sampler2D NormalTex;
            sampler2D DensityTex;
            sampler2D FoamTex;

            const int ScreenWidth;
            const int ScreenHeight;

            //
            const float DensityMultiplier;
            const float LightMultiplier;
            const float3 ExtinctionCoefficients;
            const float IndexOfRefraction;

            //
            const float3 LightDir;

            //
            const bool UseBillboardFoam;
            const bool UseShadowMapping;

            //
            const float4x4 ObstacleInverseTransform;
            const float3 ObstacleScale;
            const bool ObstacleType;

            // Shadow Mapping
            const float4x4 ShadowCamVP;
            sampler2D DensityFromSunTex;
            
            // Caustics
            const bool UseCaustics;
            const float3 CausticCamPosition;
            const float4x4 CausticCamVP;
            sampler2D DepthFromCausticCam;
            sampler2D NormalFromCausticCam;

            #define MAXDIST 1000.0
            #include "../../../Scripts/Sim/Resources/MathHelper.hlsl"
            #include "../../Raymarched Rendering/Shaders/SDFScene.hlsl"

            float GetShadowOcclusion(float3 pos) {
                if(!UseShadowMapping) return 1.;

                float4 clipSpacePos = mul(ShadowCamVP, float4(pos, 1.));
                clipSpacePos /= clipSpacePos.w;

                float2 uv = clipSpacePos.xy*0.5+0.5;
                if(uv.x < 0. || uv.x > 1. || uv.y < 0. || uv.y > 1.) return 1.;

                // TODO: Use a very low res depth tex from shadow cam to check whether we're being occluded at all
                float fragDepth = dot(pos - CamPos, CamFo);

                //
                float densityAlongSunRay = tex2D(DensityFromSunTex, uv).r;
                float transmittance = exp(-0.05 * densityAlongSunRay * ExtinctionCoefficients);

                return transmittance;
            }

            float3 Raycast(float2 uv) {
                float2 p = uv*2.0-1.0;

                float3 rd = normalize(float3(tan(FovY*0.5) * (p * float2(Aspect, 1.0)), 1.0));
                return CamRi * rd.x + CamUp * rd.y + CamFo * rd.z; // float3x3(row, row, row) not column..
            }

            float3 GetPosFromDepthTexture(float2 uv, out float distAlongRay) {
                float3 rd = Raycast(uv);
                float distAlongCam = tex2D(SmoothedDepthTex, uv).r;
                distAlongRay = distAlongCam / dot(rd, CamFo);
                float3 pos = CamPos + rd*distAlongRay;
                return pos;
            }

            inline bool InvalidDepth(float depth) {
                return depth >= 100000.0;
            }

            samplerCUBE EnvironmentMap;

            float3 SampleSkybox(float3 rd) {
                return LightMultiplier*texCUBE(EnvironmentMap, rd).rgb + SampleSun(rd);
            }

            float3 SampleCameraSkybox(float3 rd) {
                return texCUBE(EnvironmentMap, rd).rgb;// + SampleSun(rd);
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

            float AccountForSDFInDensityAlongRay(float densAlongRayWithFoam, float distFromWaterToSDFAlongRefract, float distFromWaterToEndAlongRefract) {
                return min(1., distFromWaterToSDFAlongRefract / distFromWaterToEndAlongRefract) * densAlongRayWithFoam;
            }

            float3 ShadeWater(float3 rd, float distAlongRayToWater, float3 pos, float3 norm, float densityAlongRd, float distAlongRayToSDF) {
                if(distAlongRayToSDF < distAlongRayToWater) {
                    // SDF Before Water
                    return SampleCameraEnvironment(CamPos, rd);
                }

                float ior = 1./IndexOfRefraction; // Screen Space Technique would only work outside water I think

                float3 reflectRay = Reflect(-rd, norm);
                float3 refractRay = Refract(-rd, norm, ior);

                float densityAlongRefractRay = densityAlongRd; 

                float f = Fresnel(-rd, norm, ior);

                const float densAlongReflect = 0.;
                float densAlongRefract = densityAlongRd; // Approximation since refracted ray is bent

                float3 reflectTransmittance = f * exp(-ExtinctionCoefficients * densAlongReflect);

                float3 reflectExitPoint = pos + norm*0.0005;
                float3 reflectLo = SampleEnvironment(reflectExitPoint, reflectRay); // Env is Scene and Skybox

                float distFromWaterToEnd = 6.*densityAlongRd;
                float distFromWaterToSDF = RayIntersectScene(pos, refractRay);
                
                densAlongRefract = AccountForSDFInDensityAlongRay(densAlongRefract, distFromWaterToSDF, distFromWaterToEnd);
                float3 refractTransmittance = (1.0-f) * exp(-ExtinctionCoefficients * densAlongRefract);

                float3 refractExitPoint = pos + refractRay * min(distFromWaterToSDF, distFromWaterToEnd);
                float3 refractLo = SampleEnvironment(refractExitPoint, refractRay);

                // Caustics
                #ifdef CAUSTICS
                float3 causticLo = 0.;
                if(distFromWaterToSDF <= distFromWaterToEnd) {
                    float3 floorPoint = refractExitPoint;
                    const float3 causticWi = float3(0.,1.,0.);

                    float4 clipSpace = mul(CausticCamVP, float4(floorPoint, 1.));
                    float2 causticUV = (clipSpace.xy / clipSpace.w)*0.5+0.5;

                    if(!(causticUV.x < 0. || causticUV.x > 1. || causticUV.y < 0. || causticUV.y > 1.)) {
                        float causticDepth = tex2D(DepthFromCausticCam, causticUV).r;
                        float waterExitY = CausticCamPosition.y - causticDepth;

                        float lightTravelDist = max(0., waterExitY - floorPoint.y);
                        float3 transmittanceToWaterExit = exp(-ExtinctionCoefficients * lightTravelDist * 0.05);

                        float3 waterExitPosition = float3(floorPoint.x, waterExitY, floorPoint.z);
                        float3 waterExitNormal = tex2D(NormalFromCausticCam, causticUV).xyz;

                        float3 causticRefractRay = Refract(float3(0.,-1.,0.), -waterExitNormal, IndexOfRefraction);
                        float causticRefractSceneDist = RayIntersectScene(waterExitPosition, causticRefractRay);
                        if(causticRefractSceneDist >= MAXDIST) {
                            //return waterExitNormal*.5+.5;
                            causticLo = transmittanceToWaterExit * exp(-ExtinctionCoefficients * 1.5) *( // * .2 instead of exp
                                smoothstep(0.987, 1.0, pow(causticRefractRay.y, 16.)) +
                                smoothstep(0.997, 1.0, pow(causticRefractRay.y, 32.)) +
                                smoothstep(0.9979, 1.0, pow(causticRefractRay.y, 64.))
                            );
                        }
                    }
                }
                refractLo += causticLo;
                #endif

                float3 li = reflectTransmittance * reflectLo +
                            refractTransmittance * refractLo;

                return li;
            }

            float3 ShadeWaterFoam(float3 rd, float distAlongRayToWater, float3 pos, float3 norm, float densityAlongRd, float distAlongRayToFoam, float distAlongRayToSDF, float3 foamCol) {
                if(min(distAlongRayToFoam, distAlongRayToSDF) < distAlongRayToWater) {
                    // Foam or SDF Before Water
                    return distAlongRayToFoam < distAlongRayToSDF ? foamCol : SampleCameraEnvironment(CamPos, rd);
                }

                float ior = 1./IndexOfRefraction; // Screen Space Technique would only work outside water I think

                float3 reflectRay = Reflect(-rd, norm);
                float3 refractRay = Refract(-rd, norm, ior);

                float densityAlongRefractRay = densityAlongRd; 

                float f = Fresnel(-rd, norm, ior);

                const float densAlongReflect = 0.;
                float densAlongRefract = densityAlongRd; // Approximation since refracted ray is bent

                float3 reflectTransmittance = f * exp(-ExtinctionCoefficients * densAlongReflect);

                float3 reflectExitPoint = pos + norm*0.0005;
                float3 reflectLo = SampleEnvironment(reflectExitPoint, reflectRay); // Env is Scene and Skybox

                float distFromWaterToEnd = 6.*densityAlongRd;
                float distFromWaterToFoam = distAlongRayToFoam - distAlongRayToWater;
                float distFromWaterToSDF = RayIntersectScene(pos, refractRay);
                
                densAlongRefract = AccountForSDFInDensityAlongRay(densAlongRefract, distFromWaterToSDF, min(distFromWaterToEnd, distFromWaterToFoam));
                float3 refractTransmittance = (1.0-f) * exp(-ExtinctionCoefficients * densAlongRefract);

                float3 refractExitPoint = pos + refractRay * min(distFromWaterToSDF, min(distFromWaterToEnd, distFromWaterToFoam));
                float3 refractLo = 
                    distFromWaterToFoam <= distFromWaterToSDF ? foamCol : SampleEnvironment(refractExitPoint, refractRay);

                // Caustics
                #ifdef CAUSTICS
                float3 causticLo = 0.;
                if(distFromWaterToSDF <= min(distFromWaterToEnd, distFromWaterToFoam)) {
                    float3 floorPoint = refractExitPoint;
                    const float3 causticWi = float3(0.,1.,0.);

                    float4 clipSpace = mul(CausticCamVP, float4(floorPoint, 1.));
                    float2 causticUV = (clipSpace.xy / clipSpace.w)*0.5+0.5;

                    if(!(causticUV.x < 0. || causticUV.x > 1. || causticUV.y < 0. || causticUV.y > 1.)) {
                        float causticDepth = tex2D(DepthFromCausticCam, causticUV).r;
                        float waterExitY = CausticCamPosition.y - causticDepth;

                        float lightTravelDist = max(0., waterExitY - floorPoint.y);
                        float3 transmittanceToWaterExit = exp(-ExtinctionCoefficients * lightTravelDist * 0.05);

                        float3 waterExitPosition = float3(floorPoint.x, waterExitY, floorPoint.z);
                        float3 waterExitNormal = tex2D(NormalFromCausticCam, causticUV).xyz;

                        float3 causticRefractRay = Refract(float3(0.,-1.,0.), -waterExitNormal, IndexOfRefraction);
                        float causticRefractSceneDist = RayIntersectScene(waterExitPosition, causticRefractRay);
                        if(causticRefractSceneDist >= MAXDIST) {
                            //return waterExitNormal*.5+.5;
                            causticLo = transmittanceToWaterExit * exp(-ExtinctionCoefficients * 1.5) *( // * .2 instead of exp
                                smoothstep(0.987, 1.0, pow(causticRefractRay.y, 16.)) +
                                smoothstep(0.997, 1.0, pow(causticRefractRay.y, 32.)) +
                                smoothstep(0.9979, 1.0, pow(causticRefractRay.y, 64.))
                            );
                        }
                    }
                }
                refractLo += causticLo;
                #endif

                float3 li = reflectTransmittance * reflectLo +
                            refractTransmittance * refractLo;

                return li;
            }

            fixed4 frag(vOut i) : SV_Target
            {
                float3 FoamColor = 1.;//1. * (tex2D(FoamTex, i.uv).r < 0.3 ? float3(1.,1.,1.) : (tex2D(FoamTex, i.uv).r < 0.6 ? float3(1.,0.,0.) : float3(0.,1.,0.))); // temp TODO take out this and foam debug attrib

                float3 rd = Raycast(i.uv);
                float distAlongRay;
                float3 pos = GetPosFromDepthTexture(i.uv, distAlongRay);
                float4 norm = tex2D(NormalTex, i.uv);
                float accumDensityAlongRay = tex2D(DensityTex, i.uv).x;
                float distAlongRayToFoam = tex2D(FoamTex, i.uv).b/dot(rd, CamFo);
                float distAlongRayToSDF = RayIntersectScene(CamPos, rd);
                if(norm.a == 0.) {
                    return float4(!UseBillboardFoam || distAlongRayToFoam >= 100000.0 || distAlongRayToSDF <= distAlongRayToFoam ? 
                        SampleCameraEnvironment(CamPos, rd) : FoamColor, 1.);
                }

                #ifdef BILLBOARD_FOAM
                float3 accumLight = ShadeWaterFoam(rd, distAlongRay, pos, normalize(norm.xyz), DensityMultiplier * accumDensityAlongRay, distAlongRayToFoam, distAlongRayToSDF, FoamColor);
                #else
                float3 accumLight = ShadeWater(rd, distAlongRay, pos, normalize(norm.xyz), DensityMultiplier * accumDensityAlongRay, distAlongRayToSDF);
                #endif
                float3 col = accumLight;//pow(accumLight/(1.+accumLight),1./2.2);

                return float4(col, 1.0);
            }
            ENDCG
        }
    }
}