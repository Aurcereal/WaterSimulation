float3x3 rot2D(float o) {
    return float3x3(
        float3(cos(o), sin(o), 0.),
        float3(-sin(o), cos(o), 0.),
        float3(0.,0.,1.)
    );
}

float2 rot(float2 v, float o) {
    return mul(float2x2(cos(o), sin(o), -sin(o), cos(o)), v);
}

float3 normalizesafe(float3 v) {
    if(length(v) < 0.00001) {
        return float3(1.0,0.0,0.0);
    } else {
        return normalize(v);
    }
}