float sdEnv(float3 p) {
    float dFloor = sdBox(p - float3(0., -6.5, 0.), float3(120., 0.1, 120.));
    return dFloor;
}