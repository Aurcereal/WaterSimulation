const float TimeSinceStart;

float sdFunnel(float3 p) {
    //
    p.y -= 14.;

    float sphereAdd = sdCone(-(p - float3(0.,0.,0.)), float2(24., 12.));
    float sphereSub = sdCone(-(p - float3(0., 4.8, 0.)), float2(33.4, 20.));
    
    float cylinderSub = sdCylinder(p, float2(1.0, 50.));

    return max(max(sphereAdd, -sphereSub), -cylinderSub);
}

float sdScene(float3 p) {
    float dObstacle = ObstacleType ? 
        sdBox(ObstacleScale * mul(ObstacleInverseTransform, float4(p, 1.)).xyz, ObstacleScale) : 
        sdSphere(ObstacleScale * mul(ObstacleInverseTransform, float4(p, 1.)).xyz, ObstacleScale.x);
    float dFloor = sdBox(p - float3(0., -1.5, 0.), float3(2400., 0.1, 2400.));

    float dFunnel = sdFunnel(p);

    return min(dFunnel, min(dObstacle, dFloor));
}

// Point -> Material (for now just color)
float3 sampleSceneColor(float3 p) {
    float dObstacle = ObstacleType ? 
        sdBox(ObstacleScale * mul(ObstacleInverseTransform, float4(p, 1.)).xyz, ObstacleScale) : 
        sdSphere(ObstacleScale * mul(ObstacleInverseTransform, float4(p, 1.)).xyz, ObstacleScale.x);
    float dFloor = sdBox(p - float3(0., -1.5, 0.), float3(2400., 0.1, 2400.));

    float dFunnel = sdFunnel(p);

    #define COLOR_COUNT 5
    const float3 cols[COLOR_COUNT] = {
        float3(.5764, .5058, 1.),
        float3(.7215, .7215, 1.),
        float3(.972, .968, 1.),
        float3(1.,.933,.866),
        float3(1.,.847,.745)
    };

    if(dObstacle <= min(dFloor, dFunnel)) {
        return float3(0.5, 0.1, 0.1);
    } else if(dFunnel < dFloor) {
        return 1.2;
    } else {
        float2 ccp = floor(p.xz*0.125);
        float alt = step(abs(amod(ccp.x + ccp.y, 2.)-1.), 0.5);
        return 1.2-alt*.2;
    }
}