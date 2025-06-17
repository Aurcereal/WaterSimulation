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

            const float FovY;
            const float Aspect;

            const float3 CamRi;
            const float3 CamUp;
            const float3 CamFo;

            const float3 CamPos;

            const int ScreenWidth;
            const int ScreenHeight;

            //
            const float DensityMultiplier;
            const float LightMultiplier;
            const float3 ExtinctionCoefficients;
            const float IndexOfRefraction;

            float3 Raycast(float2 uv) {
                float2 p = uv*2.0-1.0;

                float3 rd = normalize(float3(tan(FovY*0.5) * (p * float2(Aspect, 1.0)), 1.0));
                return CamRi * rd.x + CamUp * rd.y + CamFo * rd.z; // float3x3(row, row, row) not column..
            }

            float3 GetPosFromDepthTexture(float2 uv) {
                float3 rd = Raycast(uv);
                float distAlongCam = tex2D(SmoothedDepthTex, uv).r;
                float distAlongRay = distAlongCam / dot(rd, CamFo);
                float3 pos = CamPos + rd*distAlongRay;
                return pos;
            }

            inline bool InvalidDepth(float depth) {
                return depth >= 100000.0;
            }

            samplerCUBE EnvironmentMap;

            float3 SampleSkybox(float3 rd) {
                return LightMultiplier*texCUBE(EnvironmentMap, rd).rgb;
            }

            float3 SampleEnvironment(float3 ro, float3 rd) {
                return SampleSkybox(rd); // temp, bring in sdf later
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

            float3 ShadeWater(float3 rd, float3 pos, float3 norm, float densityAlongRd) {
                float ior = 1./IndexOfRefraction; // Screen Space Technique would only work outside water I think

                float3 reflectRay = Reflect(-rd, norm);
                float3 refractRay = Refract(-rd, norm, ior);

                float densityAlongRefractRay = densityAlongRd; 

                float f = Fresnel(-rd, norm, ior);

                const float densAlongReflect = 0.;
                float densAlongRefract = densityAlongRd; // Approximation since refracted ray is bent

                float3 reflectTransmittance = f * exp(-ExtinctionCoefficients * densAlongReflect);
                float3 refractTransmittance = (1.0-f) * exp(-ExtinctionCoefficients * densAlongRefract);

                float3 reflectExitPoint = pos + norm*0.0005;
                float3 refractExitPoint = pos + refractRay * densityAlongRd; // Bad approx, can use a multiplier

                float3 reflectLi = SampleEnvironment(reflectExitPoint, reflectRay); // Env is Scene and Skybox
                float3 refractLi = SampleEnvironment(refractExitPoint, refractRay);

                float3 lo = reflectTransmittance * reflectLi +
                            refractTransmittance * refractLi;

                return lo;
            }

            fixed4 frag(vOut i) : SV_Target
            {

                float3 rd = Raycast(i.uv);
                float3 pos = GetPosFromDepthTexture(i.uv);
                float4 norm = tex2D(NormalTex, i.uv);
                float accumDensityAlongRay = tex2D(DensityTex, i.uv).x;
                if(norm.a == 0.) return float4(SampleSkybox(rd), 1.);

                float3 accumLight = ShadeWater(rd, pos, normalize(norm.xyz), DensityMultiplier * accumDensityAlongRay);
                float3 col = accumLight;//pow(accumLight/(1.+accumLight),1./2.2);

                return float4(col, 1.0);
            }
            ENDCG
        }
    }
}