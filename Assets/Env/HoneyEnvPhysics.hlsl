float sdFunnel(float3 p) {
    //
    float sphereAdd = sdCone(-(p - float3(0.,15.,0.)), float2(12., 6.));//length(p - float3(0., 10., 0.)) - 4.;
    float sphereSub = sdCone(-(p - float3(0., 15.4, 0.)), float2(11.7, 6.));//length(p - float3(0., 12.5, 0.)) - 3.5;
    float cylinderSub = sdCylinder(p, float2(1.2, 50.));

    return max(max(sphereAdd, -sphereSub), -cylinderSub);
}

float sdEnv(float3 p) {
    float dFloor = sdBox(p - float3(0., -1.5, 0.), float3(120., 0.1, 120.));
    //float dWasher = sdWasher(p, float3(10., 30., 0.9));
    float dBound = -sdBox(p, float3(25., 250., 9.));

    float dFunnel = sdFunnel(p);

    return min(dFunnel, min(dFloor, dBound));
}