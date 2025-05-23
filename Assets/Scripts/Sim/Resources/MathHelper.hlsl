float3x3 rot2D(float o) {
    return float3x3(
        float3(cos(o), sin(o), 0.),
        float3(-sin(o), cos(o), 0.),
        float3(0.,0.,1.)
    );
}