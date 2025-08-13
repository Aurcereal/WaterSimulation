float sdScene(float3 p) {
    float dObstacle = ObstacleType ? 
        sdBox(ObstacleScale * mul(ObstacleInverseTransform, float4(p, 1.)).xyz, ObstacleScale) : 
        sdSphere(ObstacleScale * mul(ObstacleInverseTransform, float4(p, 1.)).xyz, ObstacleScale.x);
    float dFloor = sdBox(p - float3(0., -6.5, 0.), float3(120., 0.1, 120.));
    return min(dObstacle, dFloor);
}

// Point -> Material (for now just color)
float3 sampleSceneColor(float3 p) {

    float dObstacle = ObstacleType ? 
        sdBox(ObstacleScale * mul(ObstacleInverseTransform, float4(p, 1.)).xyz, ObstacleScale) : 
        sdSphere(ObstacleScale * mul(ObstacleInverseTransform, float4(p, 1.)).xyz, ObstacleScale.x);
    float dFloor = sdBox(p - float3(0., -6.5, 0.), float3(50., 0.1, 20.));

    const float3 col1 = float3(245.,157.,191.)/255.;
    const float3 col2 = float3(238.,89.,121.)/255.;
    const float3 col3 = float3(249.,154.,93.)/255.;

    if(dObstacle <= dFloor) {
        return float3(0.5, 0.1, 0.1);
    } else {
        // float2 cp = floor(p.xz*0.25);
        // float alt = step(abs(amod(cp.x + cp.y, 2.)-1.), 0.5);
        //return col1 + alt * (col2 - col1);

        const float lineThick = 1.8;
        float2 fp = p.xz;
        fp = rot2D(fp, PI*.25);
        float ly = amod(fp.y+sin(fp.x*.5), lineThick*2.)-lineThick;
        float alt = step(abs(ly), lineThick*.5);
        return col1 + alt * (col2 - col1);
    }
}