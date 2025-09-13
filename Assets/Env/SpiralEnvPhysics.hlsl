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
    
    lowSample = float3(xSample-sampleXLen, ySample-spiralHeight, zSample);
    highSample = float3(xSample+sampleXLen, ySample+spiralHeight, zSample); 
    
    bbxDist = spiralHeight*.505-abs(lp.y-spiralHeight*.5);
    
    return samp;
}

float sdSpiraledScene(float3 p) {
    const float width = 1.4;
    const float height = .15;
    const float2 railDim = float2(.15,9.35);

    p.zy = rot2D(p.zy, 0.25);
    p.z = abs(p.z);
    return min(
        sdBox(p, float3(22.42,height,width)),
        sdBox(p-float3(0.,height*.5+railDim.y*.5, width*.5-railDim.x*.5), float3(22.42,railDim.y,railDim.x))
        );
}

float sdEndRail(float3 p) {
    return 1000.;
    const float width = 1.4;
    const float height = .15;
    const float2 railDim = float2(.15,4.35);

    p.zy = rot2D(p.zy, smoothstep(-15., 9., p.x) * 0.25);
    p.z = abs(p.z);
    return min(
        sdBox(p, float3(22.42,height,width)),
        sdBox(p-float3(0.,height*.5+railDim.y*.5, width*.5-railDim.x*.5), float3(22.42,railDim.y,railDim.x))
        );
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

    float spiralD = 0.75*min(min(1000.+bbxDist, sdSpiraledScene(sp)), min(sdSpiraledScene(lowSample), sdSpiraledScene(highSample)));
    float endD = sdEndRail((p-float3(-11., -spiralHeight*.25, -spiralOffset-spiralThickness*.5))*float3(1.,1.,-1.));

    return sca*min(spiralD, endD);
}

float sdFloor(float3 p) {
    return sdBox(p - float3(0., -100.5, 0.), float3(2400., 0.1, 2400.));
}

float sdEnv(float3 p) {
    float dFloor = sdFloor(p);
    float dBound = 1000.;//-sdBox(p, float3(25., 250., 9.));

    float dSpiral = sdSpiral(p);

    return min(dSpiral, min(dFloor, dBound));
}

//#define FORCE_FIELD
float3 sampleForceField(float3 p) {
    float3 groundVec = float3(p.x,0.,p.z);
    float groundR = length(p.xz);

    float3 inward = smoothstep(15., 13., p.y) * (-20.) * groundVec;

    return inward;
}