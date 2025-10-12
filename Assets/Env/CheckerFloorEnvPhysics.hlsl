float sdEnv(float3 p) {
    float dFloor = sdBox(p - float3(0., -3.9, 0.), float3(120., 0.1, 120.));
    return 100.;
}