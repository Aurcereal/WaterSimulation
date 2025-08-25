#define SDNORMEPS 0.001
#define SDEPS 0.005
#define MAXSTEPS 100

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
float3 shadeScene(float3 pos, float3 normal, float3 color) {
    float diffuse = max(0., dot(normal, -LightDir));
    float3 diffuseCol = color * diffuse;

    float3 ambientCol = 0.02;

    #ifdef SHADOWS
    float shadowOcclusion = GetShadowOcclusion(pos);
    #else
    float shadowOcclusion = 1.;
    #endif

    return ambientCol + shadowOcclusion * diffuseCol * 0.7;
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

float3 SampleEnvironment(float3 ro, float3 rd) {
    float dist = RayIntersectScene(ro, rd);
    if(dist >= MAXDIST) return SampleSkybox(rd);

    float3 hitPos = ro+rd*dist;
    float3 norm = normal(hitPos);
    float3 material = sampleSceneColor(hitPos);

    return shadeScene(hitPos, norm, material);
}

float3 SampleEnvironmentDist(float3 ro, float3 rd, out float dist) {
    dist = RayIntersectScene(ro, rd);
    if(dist >= MAXDIST) return SampleSkybox(rd);

    float3 hitPos = ro+rd*dist;
    float3 norm = normal(hitPos);
    float3 material = sampleSceneColor(hitPos);

    return shadeScene(hitPos, norm, material);
}