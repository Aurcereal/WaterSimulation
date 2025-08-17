float sdScene(float3 p) {
    float dObstacle = ObstacleType ? 
        sdBox(ObstacleScale * mul(ObstacleInverseTransform, float4(p, 1.)).xyz, ObstacleScale) : 
        sdSphere(ObstacleScale * mul(ObstacleInverseTransform, float4(p, 1.)).xyz, ObstacleScale.x);

    //
    float dFloor = sdCylinder(p-float3(0.,-4.,0.), float2(24., 0.1));

    //
    float dFountain = sdWasher(p-float3(0.,-3.,0.), float3(18., 2., 0.9));

    return min(min(dFloor, dFountain), dObstacle);
}

// Point -> Material (for now just color)
float3 sampleSceneColor(float3 p) {
    float dObstacle = ObstacleType ? 
        sdBox(ObstacleScale * mul(ObstacleInverseTransform, float4(p, 1.)).xyz, ObstacleScale) : 
        sdSphere(ObstacleScale * mul(ObstacleInverseTransform, float4(p, 1.)).xyz, ObstacleScale.x);

    //
    float dFloor = sdCylinder(p-float3(0.,-4.,0.), float2(24., 0.1));

    //
    float dFountain = sdWasher(p-float3(0.,-3.,0.), float3(18., 2., 0.9));

    float d = min(min(dObstacle, dFloor), dFountain);

    //
    float isFountain = step(dFountain, d+0.001);
    float isFloor = step(dFloor, d+0.001);
    float isObstacle = step(dObstacle, d+0.001);

    //
    float3 fountainCol = float3(0.5,0.5,0.5);
    float3 floorCol = float3(0.2,0.6,0.4);
    float3 obstacleCol = float3(0.5,0.1,0.1);

    //
    return isFloor*floorCol + isFountain *fountainCol + isObstacle * obstacleCol;//isFountain*fountainCol + isFloor * floorCol * isObstacle*obstacleCol;

}