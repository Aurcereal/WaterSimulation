float sdEnv(float3 p) {
    float dFloor = sdBox(p - float3(0., -3.9, 0.), float3(120., 0.1, 120.));
    float dBound = -sdBox(p-float3(0.,1.,0.), float3(35., 10., 3.));
    return dBound;//min(dFloor, dBound);
}