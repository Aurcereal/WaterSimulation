float3 mountainTransform(float3 p) {
    float r = length(p.xz);
    const float2 mountainParam = float2(240., 2.);
    float v = mountainParam.y*max(0., r-mountainParam.x);
    v *= v;
    v *= 30.;

    p.y += v;
    return p;
}

float sdScene(float3 p) {
    //p = mountainTransform(p);

    float dObstacle = ObstacleType ? 
        sdBox(ObstacleScale * mul(ObstacleInverseTransform, float4(p, 1.)).xyz, ObstacleScale) : 
        sdSphere(ObstacleScale * mul(ObstacleInverseTransform, float4(p, 1.)).xyz, ObstacleScale.x);
    float dFloor = sdBox(p - float3(0., -1.5, 0.), float3(2400., 0.1, 2400.));
    return min(dObstacle, dFloor);
}

// Point -> Material (for now just color)
float3 sampleSceneColor(float3 p) {
    //p = mountainTransform(p);

    float dObstacle = ObstacleType ? 
        sdBox(ObstacleScale * mul(ObstacleInverseTransform, float4(p, 1.)).xyz, ObstacleScale) : 
        sdSphere(ObstacleScale * mul(ObstacleInverseTransform, float4(p, 1.)).xyz, ObstacleScale.x);
    float dFloor = sdBox(p - float3(0., -1.5, 0.), float3(2400., 0.1, 2400.));

    // #define COLOR_COUNT 3
    // const float3 cols[COLOR_COUNT] = {
    //     float3(245.,157.,191.)/255.,
    //     float3(238.,89.,121.)/255.,
    //     float3(249.,154.,93.)/255.
    // };

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
        // float2 cp = floor(p.xz*0.25);
        // float alt = step(abs(amod(cp.x + cp.y, 2.)-1.), 0.5);
        //return col1 + alt * (col2 - col1);

        // const float lineThick = 1.8;
        // float2 fp = p.xz;
        // fp = rot2D(fp, PI*.25);
        // float ly = amod(fp.y+sin(fp.x*.5), lineThick*2.)-lineThick;
        // float alt = step(abs(ly), lineThick*.5);
        // return col1 + alt * (col2 - col1);

        const float repLenR = 4.;
        const float repCountTheta = 4.;

        float2 cp = p.xz;
        float2 polar = toPolar(cp);
        polar.y += lerp(0.25, 0.5, (sin(0.015*_Time.y)*.5+.5))*polar.x;

        float lr = fmod(polar.x, repLenR);
        float rID = polar.x-lr+0.001;

        polar.y += rID*13.249;
        polar.y = amod(polar.y, TAU);
        float ltheta = fmod(polar.y, TAU/repCountTheta);
        float thetaID = polar.y-ltheta+0.1;

        float rand = hash21(float2(rID, thetaID));
        rand *= COLOR_COUNT;

        float3 accumCol = 0.;
        for(int i=0; i<COLOR_COUNT; i++) {
            accumCol += cols[i] * step(rand, 1.*(i+1)) * step(1.*i, rand);
        }

        return accumCol;
    }
}