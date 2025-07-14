#define SDNORMEPS 0.001
#define SDEPS 0.005
#define MAXSTEPS 100

#include "../../../Scripts/Sim/Resources/SDFMath3D.hlsl"

float sdScene(float3 p) {
    float dObstacle = ObstacleType ? 
        sdBox(ObstacleScale * mul(ObstacleInverseTransform, float4(p, 1.)).xyz, ObstacleScale) : 
        sdSphere(ObstacleScale * mul(ObstacleInverseTransform, float4(p, 1.)).xyz, ObstacleScale.x);
    float dFloor = sdBox(p - float3(0., -6.5, 0.), float3(120., 0.1, 120.));
    return min(dObstacle, dFloor);
}

// Point -> Material (for now just color)
float3 sampleSceneColor(float3 p) {

    float dObstacle = ObstacleType ? 
        sdBox(ObstacleScale * mul(ObstacleInverseTransform, float4(p, 1.)).xyz, ObstacleScale) : 
        sdSphere(ObstacleScale * mul(ObstacleInverseTransform, float4(p, 1.)).xyz, ObstacleScale.x);
    float dFloor = sdBox(p - float3(0., -6.5, 0.), float3(50., 0.1, 20.));

    const float3 col1 = float3(245.,157.,191.)/255.;
    const float3 col2 = float3(238.,89.,121.)/255.;
    const float3 col3 = float3(249.,154.,93.)/255.;

    if(dObstacle <= dFloor) {
        return float3(0.5, 0.1, 0.1);
    } else {
        float2 cp = floor(p.xz*0.25);
        float alt = step(abs(amod(cp.x + cp.y, 2.)-1.), 0.5);
        return col1 + alt * (col2 - col1);
    }
}

float GetShadowOcclusion(float3 pos);
float3 SampleSkybox(float3 rd);

// Material, Hit Params -> Li
float3 shadeScene(float3 pos, float3 normal, float3 color) {
    float diffuse = max(0., dot(normal, -LightDir));
    float3 diffuseCol = color * diffuse;

    float3 ambientCol = 0.02;

    return ambientCol + GetShadowOcclusion(pos) * diffuseCol * 0.7;
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