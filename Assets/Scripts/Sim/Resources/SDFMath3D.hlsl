#define NORMEPS 0.0005

float sdSphere(float3 p, float r) {
    return length(p) - r;
}

float sdBox(float3 p, float3 dim) {
    p = abs(p) - dim*.5;
    float s = sqrt(p.x*max(0.,p.x) + p.y*max(0.,p.y) + p.z*max(0., p.z));
    return step(s, 0.) * max(p.x, max(p.y, p.z)) + s;
}

float sdScene(float3 p); // Implement this wherever you include it

float3 normal(float3 p) {
    float2 eps = float2(NORMEPS, 0.);

    return normalizesafe(
        float3(
            sdScene(p + eps.xyy) - sdScene(p - eps.xyy),
            sdScene(p + eps.yxy) - sdScene(p - eps.yxy),
            sdScene(p + eps.yyx) - sdScene(p - eps.yyx)
        )
    );
}