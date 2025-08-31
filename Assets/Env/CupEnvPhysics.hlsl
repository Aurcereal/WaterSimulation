float sdEnv(float3 p) {
    float dFloor = sdBox(p - float3(0., -1.5, 0.), float3(120., 0.1, 120.));
    float dWasher = sdWasher(p, float3(9., 30., 0.9));
    return min(dFloor, dWasher);
}