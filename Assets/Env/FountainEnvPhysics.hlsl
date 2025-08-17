float sdEnv(float3 p) {
    float dObstacle = ObstacleType ? 
        sdBox(ObstacleScale * mul(ObstacleInverseTransform, float4(p, 1.)).xyz, ObstacleScale) : 
        sdSphere(ObstacleScale * mul(ObstacleInverseTransform, float4(p, 1.)).xyz, ObstacleScale.x);

    //
    float dFloor = p.y+4.;

    //
    float dFountain = sdWasher(p-float3(0.,-3.,0.), float3(18., 60., 0.9));

    return min(min(dFloor, dFountain), dObstacle);
}

#define FORCE_FIELD
float3 sampleForceField(float3 p) {
    float3 groundVec = float3(p.x,0.,p.z);
    float groundR = length(p.xz);

    float3 inward = -7.*groundVec;
    float3 upward = float3(0.,550.*smoothstep(3., 1.5, groundR)*smoothstep(4.5,2.5, p.y),0.);
    float3 outward = 540. * normalize(groundVec) * smoothstep(5., 7., p.y) * smoothstep(5.5, 4., groundR);

    return inward + upward + outward;
}