#define SDNORMEPS 0.001
#define SDEPS 0.005
#define MAXSTEPS 50

#include "../../../Scripts/Sim/Resources/SDFMath3D.hlsl"

float sdScene(float3 p) {
    float dObstacle = ObstacleType ? 
        sdBox(ObstacleScale * mul(ObstacleInverseTransform, float4(p, 1.)).xyz, ObstacleScale) : 
        sdSphere(ObstacleScale * mul(ObstacleInverseTransform, float4(p, 1.)).xyz, ObstacleScale.x);
    float dFloor = sdBox(p - float3(0., -6.5, 0.), float3(50., 0.1, 20.));
    return min(dObstacle, dFloor);
}

// Point -> Material (for now just color)
float3 sampleSceneColor(float3 p) {

    float dObstacle = ObstacleType ? 
        sdBox(ObstacleScale * mul(ObstacleInverseTransform, float4(p, 1.)).xyz, ObstacleScale) : 
        sdSphere(ObstacleScale * mul(ObstacleInverseTransform, float4(p, 1.)).xyz, ObstacleScale.x);
    float dFloor = sdBox(p - float3(0., -6.5, 0.), float3(50., 0.1, 20.));

    if(dObstacle <= dFloor) {
        return float3(0.5, 0.1, 0.1);
    } else {
        float2 cp = floor(p.xz*0.25);
        float alt = step(abs(amod(cp.x + cp.y, 2.)-1.), 0.5);
        return float3(0.3, 0.3, 0.5) + alt * (float3(0.2, 0.8, 0.4) - float3(0.3, 0.3, 0.5));
    }
}

// Material, Hit Params -> Li
float3 shadeScene(float3 normal, float3 color) {
    float diffuse = max(0., dot(normal, -LightDir));
    float3 diffuseCol = color * diffuse;

    float3 ambientCol = float3(0.2, 0.12, 0.12);

    return ambientCol + diffuseCol * 0.7;
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

float3 SampleSkybox(float3 rd);

float3 SampleEnvironment(float3 ro, float3 rd) {
    float dist = RayIntersectScene(ro, rd);
    if(dist >= MAXDIST) return SampleSkybox(rd);

    float3 hitPos = ro+rd*dist;
    float3 norm = normal(hitPos);
    float3 material = sampleSceneColor(hitPos);

    return shadeScene(norm, material);
}