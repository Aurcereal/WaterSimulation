float sdObstacle(float3 p) {
    float dObstacle = ObstacleType ?
        sdBox(ObstacleScale * mul(ObstacleInverseTransform, float4(p, 1.)).xyz, ObstacleScale-1.5*SmoothingRadius) : 
        sdSphere(ObstacleScale * mul(ObstacleInverseTransform, float4(p, 1.)).xyz, ObstacleScale.x-1.5*SmoothingRadius);
    return dObstacle;
}