float sdScene(float3 p) {
    float dObstacle = ObstacleType ? 
        sdBox(ObstacleScale * mul(ObstacleInverseTransform, float4(p, 1.)).xyz, ObstacleScale) : 
        sdSphere(ObstacleScale * mul(ObstacleInverseTransform, float4(p, 1.)).xyz, ObstacleScale.x);
    float dFloor = sdBox(p - float3(0., -3.9, 0.), float3(2400., 0.1, 2400.));
    return min(dObstacle, dFloor);
}

// Point -> Material (for now just color)
float3 sampleSceneColor(float3 p) {

    float dObstacle = ObstacleType ? 
        sdBox(ObstacleScale * mul(ObstacleInverseTransform, float4(p, 1.)).xyz, ObstacleScale) : 
        sdSphere(ObstacleScale * mul(ObstacleInverseTransform, float4(p, 1.)).xyz, ObstacleScale.x);
    float dFloor = sdBox(p - float3(0., -3.9, 0.), float3(2400., 0.1, 2400.));

    #define COLOR_COUNT 5
    const float3 cols[COLOR_COUNT] = {
        float3(.5764, .5058, 1.),
        float3(.7215, .7215, 1.),
        float3(.972, .968, 1.),
        float3(1.,.933,.866),
        float3(1.,.847,.745)
    };

    if(dObstacle <= dFloor) {
        return float3(0.5, 0.1, 0.1);
    } else {
        float2 cp = floor(p.xz*0.125);
        float alt = step(abs(amod(cp.x + cp.y, 2.)-1.), 0.5);
        return 1.2-alt*.2;
    }
}