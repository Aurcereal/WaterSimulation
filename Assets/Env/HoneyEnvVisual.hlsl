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

        // const float repLenR = 4.;
        // const float repCountTheta = 4.;

        // float2 cp = p.xz;
        // float2 polar = toPolar(cp);
        // polar.y += lerp(0.25, 0.5, (sin(0.015*_Time.y)*.5+.5))*polar.x;

        // float lr = fmod(polar.x, repLenR);
        // float rID = polar.x-lr+0.001;

        // polar.y += rID*13.249;
        // polar.y = amod(polar.y, TAU);
        // float ltheta = fmod(polar.y, TAU/repCountTheta);
        // float thetaID = polar.y-ltheta+0.1;

        // float rand = hash21(float2(rID, thetaID));
        // rand *= COLOR_COUNT;

        // float3 accumCol = 0.;
        // for(int i=0; i<COLOR_COUNT; i++) {
        //     accumCol += cols[i] * step(rand, 1.*(i+1)) * step(1.*i, rand);
        // }

        return 1.2-alt*.2;//accumCol;//lerp(accumCol, 1., 0.2);
    }
}