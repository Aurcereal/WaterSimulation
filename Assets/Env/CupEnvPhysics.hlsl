float sdCup(float3 p) {
    float yFac = clamp((p.y-(-4.))/8., 0.001, 1.);
    p.xz -= p.xz/4. * 1.25 * sqrt(yFac);

    float washer = sdWasher(p, float3(2.5, 8., 0.94));
    float bottom = sdCylinder(p-float3(0.,-3.5,0.), float2(2.5, 1.));

    return min(bottom, washer)-0.1;
}

float sdPouringCup(float3 p) {
    //
    float t = frac(0.025*TimeSinceStart);
    float pourFac = smoothstep(0.275, 0.4, t);
    float leaveFac = smoothstep(0.44, 0.52, t);

    const float size = 1.75;
    float3 cp = p - float3(4.,14.+leaveFac*30.,0.);
    cp = mul(rotZ(PI*.7*pourFac), cp);
    return sdCup(cp/size)*size;
}

float sdEnv(float3 p) {
    float dFloor = sdBox(p - float3(0., -1.5, 0.), float3(120., 0.1, 120.));
    //float dWasher = sdWasher(p, float3(10., 30., 0.9));
    float dBound = -sdBox(p, float3(25., 250., 9.));

    float dCup = sdPouringCup(p);

    return min(dCup, min(dFloor, dBound));
}