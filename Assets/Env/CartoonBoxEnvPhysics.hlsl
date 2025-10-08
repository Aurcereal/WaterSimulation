float sdFloor(float3 p) {
    return sdBox(p - float3(0., -5., 0.), float3(2400., 0.1, 2400.));
}

float sdEnv(float3 p) {
    float extraHeight = 0.;// 20.; // 0. for normal cartoon (use .8 sphere)
    float dBound = -sdBox(p-float3(0.,extraHeight*.5-1.,0.), float3(22., 8.+extraHeight, 8.));

    return dBound+100.;
}

#define FORCE_FIELD
float3 sampleForceField(float3 p) {
    float dObstacle = sdObstacle(p);
    float3 norm = normal(p);

    float mag = 400.*DebugVector.y*clamp(1.-dObstacle/(3.+DebugVector.x), 0., 1.);

    return (DebugBool ? -1. : 1.) * (-1.) * norm * mag;
}