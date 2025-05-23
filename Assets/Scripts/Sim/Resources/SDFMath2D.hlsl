#define NORMEPS 0.0005

float sdSphere(float2 p, float r) {
    return length(p) - r;
}

float sdBox(float2 p, float2 dim) {
    p = abs(p) - dim*.5;
    float s = sqrt(p.x*max(0.,p.x) + p.y*max(0.,p.y));
    return step(s, 0.) * max(p.x, p.y) + s;
}

float sdScene(float2 p); // Implement this wherever you include it

float2 normal(float2 p) {
    float2 eps = float2(NORMEPS, 0.);

    return normalize(
        float2(
            sdScene(p + eps.xy) - sdScene(p - eps.xy),
            sdScene(p + eps.yx) - sdScene(p - eps.yx)
        )
    );
}