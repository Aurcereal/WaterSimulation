float sdScene(float3 p) {
    float dObstacle = ObstacleType ? 
        sdBox(ObstacleScale * mul(ObstacleInverseTransform, float4(p, 1.)).xyz, ObstacleScale) : 
        sdSphere(ObstacleScale * mul(ObstacleInverseTransform, float4(p, 1.)).xyz, ObstacleScale.x);
    return dObstacle;
}

// Point -> Material (for now just color)
float3 sampleSceneColor(float3 p) {

    return float3(0.5, 0.1, 0.1);
}