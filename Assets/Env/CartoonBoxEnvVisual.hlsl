const float TimeSinceStart;
const float DebugFloat;

float sdFloor(float3 p) {
    return sdBox(p - float3(0., -5., 0.), float3(2400., 0.1, 2400.));
}

float sdScene(float3 p) {
    float dObstacle = ObstacleType ? 
        sdBox(ObstacleScale * mul(ObstacleInverseTransform, float4(p, 1.)).xyz, ObstacleScale) : 
        sdSphere(ObstacleScale * mul(ObstacleInverseTransform, float4(p, 1.)).xyz, ObstacleScale.x);
    float dFloor = sdFloor(p);

    return min(dObstacle, dFloor);
}

// Point -> Material (for now just color)
float3 sampleSceneColor(float3 p) {
    float dObstacle = ObstacleType ? 
        sdBox(ObstacleScale * mul(ObstacleInverseTransform, float4(p, 1.)).xyz, ObstacleScale) : 
        sdSphere(ObstacleScale * mul(ObstacleInverseTransform, float4(p, 1.)).xyz, ObstacleScale.x);
    float dFloor = sdFloor(p);

    if(dObstacle <= dFloor) {
        return 1.4;//float3(0.5, 0.5, 0.5);
    } else {
        float2 ccp = floor(p.xz*0.125);
        float alt = step(abs(amod(ccp.x + ccp.y, 2.)-1.), 0.5);
        return 1.2-alt*.2;
    }
}