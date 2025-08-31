const float FovY;
const float Aspect;

const float3 CamRi;
const float3 CamUp;
const float3 CamFo;

const float3 CamPos;

#define SDNORMEPS 0.001
#define SDEPS 0.005
#define MAXSTEPS 100
#define CAMERAMAXSTEPS 350

#include "../../../Scripts/Sim/Resources/SDFMath3D.hlsl"

#ifdef CHECKERFLOOR_ENV
#include "../../../Env/CheckerFloorEnvVisual.hlsl"
#elif defined(EMPTY_ENV)
#include "../../../Env/EmptyEnvVisual.hlsl"
#elif defined(FOUNTAIN_ENV)
#include "../../../Env/FountainEnvVisual.hlsl"
#elif defined(FALL_ENV)
#include "../../../Env/FallEnvVisual.hlsl"
#endif

float GetShadowOcclusion(float3 pos);
float3 SampleSkybox(float3 rd);
float3 SampleCameraSkybox(float3 rd);

const float3 SunDir;
const float SunRadius;
const float SunMultiplier;
float3 SampleSun(float3 rd) {
    float sunDot = dot(rd, -SunDir);
    float sunAngle = acos(sunDot);

    sunAngle /= SunRadius;
    float energy = 1./(sunAngle*sunAngle);

    return SunMultiplier*energy;
}

// Material, Hit Params -> Li
float3 shadeScene(float3 pos, float3 normal, float3 color, float3 rd) {
    float diffuse = max(0., dot(normal, -LightDir));
    float3 diffuseCol = color * diffuse;

    float3 ambientCol = 0.02;

    #ifdef SHADOWS
    float shadowOcclusion = GetShadowOcclusion(pos);
    #else
    float shadowOcclusion = 1.;
    #endif

    float3 solidCol = ambientCol + shadowOcclusion * diffuseCol * 0.7;

    //
    const float2 fogFalloff = float2(80., 0.025);
    float dist = length(pos-CamPos); // dot(pos - CamPos, CamFo);
    float fog = 1.-min(1., exp(-fogFalloff.y*(dist-fogFalloff.x)));
    float3 skyCol = SampleCameraSkybox(rd); // Technically should be SampleSkybox(rd) when indirect, but won't matter much in practice

    //
    float3 finalCol = lerp(solidCol, skyCol, fog);
    return finalCol;
}

float RayIntersectScene(float3 ro, float3 rd) {
    float d, sd = 0.;
    for(int i=0; i<MAXSTEPS; i++) {
        sd = sdScene(ro + rd*d);
        d += sd;

        if(abs(sd) <= SDEPS) {
            return d;
        }

        if(d >= MAXDIST) {
            return MAXDIST;
        }
    }
    
    return MAXDIST;
}

float RayIntersectExpensiveScene(float3 ro, float3 rd) {
    float d, sd = 0.;
    for(int i=0; i<CAMERAMAXSTEPS; i++) {
        sd = sdScene(ro + rd*d);
        d += sd;

        if(abs(sd) <= SDEPS) {
            return d;
        }

        if(d >= MAXDIST) {
            return MAXDIST;
        }
    }
    
    return MAXDIST;
}

float3 SampleEnvironment(float3 ro, float3 rd) {
    float dist = RayIntersectScene(ro, rd);
    if(dist >= MAXDIST) return SampleSkybox(rd);

    float3 hitPos = ro+rd*dist;
    float3 norm = normal(hitPos);
    float3 material = sampleSceneColor(hitPos);

    return shadeScene(hitPos, norm, material, rd);
}

float3 SampleCameraEnvironment(float3 ro, float3 rd) {
    float dist = RayIntersectExpensiveScene(ro, rd);
    if(dist >= MAXDIST) return SampleCameraSkybox(rd);

    float3 hitPos = ro+rd*dist;
    float3 norm = normal(hitPos);
    float3 material = sampleSceneColor(hitPos);

    return shadeScene(hitPos, norm, material, rd);
}

float3 SampleEnvironmentDist(float3 ro, float3 rd, out float dist) {
    dist = RayIntersectScene(ro, rd);
    if(dist >= MAXDIST) return SampleSkybox(rd);

    float3 hitPos = ro+rd*dist;
    float3 norm = normal(hitPos);
    float3 material = sampleSceneColor(hitPos);

    return shadeScene(hitPos, norm, material, rd);
}