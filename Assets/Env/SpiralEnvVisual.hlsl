const float TimeSinceStart;
const float DebugFloat;

float3 spiralify(float3 p, float sampleZLen, float sampleXLen, float spiralOffset, float spiralThickness, float spiralHeight, out float3 lowSample, out float3 highSample, out float bbxDist) {
    
    float3 lp = p;
    lp.y = amod(lp.y, spiralHeight);
    float spiralID = (p.y-lp.y)/spiralHeight;
    float angleFac = amod(atan2(lp.z, lp.x), TAU)/TAU;
    float currSpiralHeight = spiralHeight*angleFac;
    
    float xSample = sampleXLen*(spiralID+angleFac);
    float zSample = sampleZLen*((length(lp.xz)-spiralOffset)/spiralThickness*2.-1.);
    float ySample = lp.y-currSpiralHeight;
    
    float3 samp = float3(xSample, ySample, zSample);
    
    lowSample = float3(xSample-sampleXLen, ySample+spiralHeight, zSample);
    highSample = float3(xSample+sampleXLen, ySample-spiralHeight, zSample); 
    
    bbxDist = spiralHeight*.505-abs(lp.y-spiralHeight*.5);
    
    return samp;
}

float sdSpiraledScene(float3 p) {
    const float width = 1.2-.2*smoothstep(25.6-30.-1.8,24.-30.-1.8,p.x);
    const float height = .15;
    const float2 railDim = float2(.15,.35);

    p.zy = rot2D(p.zy, 0.25);
    p.z = abs(p.z);
    return min(
        sdBox(p, float3(22.42,height,width)),
        sdBox(p-float3(0.,height*.5+railDim.y*.5, width*.5-railDim.x*.5), float3(22.42,railDim.y,railDim.x))
        )-.03;
}

float sdEndRail(float3 p) {
    return 1000.;
    const float width = 1.4;
    const float height = .15;
    const float2 railDim = float2(.15,.35);

    p.zy = rot2D(p.zy, smoothstep(-15., 9., p.x) * 0.25);
    p.z = abs(p.z);
    return min(
        sdBox(p, float3(22.42,height,width)),
        sdBox(p-float3(0.,height*.5+railDim.y*.5, width*.5-railDim.x*.5), float3(22.42,railDim.y,railDim.x))
        )-.03;
}

float sdSpiral(float3 p)
{
    float sca = 4.;
    p /= sca;

    const float spiralOffset = 1.;
    const float spiralThickness = 8.;
    const float spiralHeight = 5.;
    float3 lowSample, highSample;
    float bbxDist;
    float3 sp = spiralify(p-float3(0.,spiralHeight*2.,0.), 4., 5., spiralOffset, spiralThickness, spiralHeight, lowSample, highSample, bbxDist);

    float spiralD = 0.75*min(min(bbxDist, sdSpiraledScene(sp)), min(sdSpiraledScene(lowSample), sdSpiraledScene(highSample)));
    float endD = sdEndRail((p-float3(-11., -spiralHeight*.25, -spiralOffset-spiralThickness*.5))*float3(1.,1.,-1.));

    return sca*smin(spiralD, endD, 0.01);
}

float sdFloor(float3 p) {
    return sdBox(p - float3(0., -100.5, 0.), float3(2400., 0.1, 2400.));
}

float sdScene(float3 p) {
    float dObstacle = ObstacleType ? 
        sdBox(ObstacleScale * mul(ObstacleInverseTransform, float4(p, 1.)).xyz, ObstacleScale) : 
        sdSphere(ObstacleScale * mul(ObstacleInverseTransform, float4(p, 1.)).xyz, ObstacleScale.x);
    float dFloor = sdFloor(p);

    float dSpiral = sdSpiral(p);

    return min(dSpiral, min(dObstacle, dFloor));
}

// Point -> Material (for now just color)
float3 sampleSceneColor(float3 p) {
    float dObstacle = ObstacleType ? 
        sdBox(ObstacleScale * mul(ObstacleInverseTransform, float4(p, 1.)).xyz, ObstacleScale) : 
        sdSphere(ObstacleScale * mul(ObstacleInverseTransform, float4(p, 1.)).xyz, ObstacleScale.x);
    float dFloor = sdFloor(p);

    #define COLOR_COUNT 5
    const float3 cols[COLOR_COUNT] = {
        float3(.5764, .5058, 1.),
        float3(.7215, .7215, 1.),
        float3(.972, .968, 1.),
        float3(1.,.933,.866),
        float3(1.,.847,.745)
    };

    float dSpiral = sdSpiral(p);

    if(dObstacle <= min(dFloor, dSpiral)) {
        return float3(0.5, 0.1, 0.1);
    } else if(dSpiral <= dFloor) {
        return 1.2;
    } else {
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

        return accumCol;//lerp(accumCol, 1., 0.2);
    }
}