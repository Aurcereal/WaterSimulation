float sdFloor(float3 p) {
    return sdBox(p - float3(0., -5., 0.), float3(2400., 0.1, 2400.));
}

float sdEnv(float3 p) {
    float dBound = -sdBox(p, float3(22., 8., 8.));

    return dBound;
}