float sdFunnel(float3 p) {
    //
    float sphereAdd = sdCone(-(p - float3(0.,0.,0.)), float2(24., 12.));
    float sphereSub = sdCone(-(p - float3(0., 4.8, 0.)), float2(33.4, 20.));
    
    float cylinderSub = sdCylinder(p, float2(1.0, 50.));

    return max(max(sphereAdd, -sphereSub), -cylinderSub);
}
float sdEnv(float3 p) {
    float dFloor = sdBox(p - float3(0., -1.5, 0.), float3(120., 0.1, 120.));
    //float dWasher = sdWasher(p, float3(10., 30., 0.9));
    float dBound = 1000.;//-sdBox(p, float3(25., 250., 9.));

    float dFunnel = sdFunnel(p - float3(0.,14.,0.));

    return min(dFunnel, min(dFloor, dBound));
}

#define FORCE_FIELD
float3 sampleForceField(float3 p) {
    float3 groundVec = float3(p.x,0.,p.z);

    float3 inward = -40.*groundVec;

    return inward;
}