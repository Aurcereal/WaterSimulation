#define SDNORMEPS 0.001
#define SDEPS 0.005
#define MAXSTEPS 50

#include "../Scripts/Sim/Resources/SDFMath3D.hlsl"

float sdScene(float3 p) {
    // Uniform obstacle transform and type from Simulation Params and an Object Transform (and make it interactable in sim)
    float dBox = sdBox(p - float3(0., 0., 0.), float3(2., 15., 2.));
    return dBox;
}

// Point -> Material (for now just color)
float3 sampleSceneColor(float3 p) {
    return float3(0.5,0.1,0.1);
}

// Material, Hit Params -> Li
float3 shadeObject(float3 normal, float3 color) {
    float diffuse = max(0., dot(normal, -LightDir));
    float3 diffuseCol = color * diffuse;

    float3 ambientCol = float3(0.2, 0.12, 0.12);

    return ambientCol + diffuseCol * 0.7;
}

float RayIntersectObjects(float3 ro, float3 rd) {
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

float3 SampleEnvironment(float3 rd);

float3 SampleObjectScene(float3 ro, float3 rd) {
    float dist = RayIntersectObjects(ro, rd);
    if(dist >= MAXDIST) return SampleEnvironment(rd);

    float3 hitPos = ro+rd*dist;
    float3 norm = normal(hitPos);
    float3 material = sampleSceneColor(hitPos);

    return shadeObject(norm, material);
}