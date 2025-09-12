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
    const float width = 2.8;
    const float height = .15;
    const float2 railDim = float2(.15,.35);

    p.z = abs(p.z);
    return min(
        sdBox(p, float3(60.,height,width)),
        sdBox(p-float3(0.,height*.5+railDim.y*.5, width*.5-railDim.x*.5), float3(60.,railDim.y,railDim.x))
        );
}

float sdSpiral(float3 p)
{
    float sca = 4.;
    p /= sca;

    //return sdSpiraledScene(p);
    float3 lowSample, highSample;
    float bbxDist;
    p = spiralify(p, 4., 5., 1., 8., 4., lowSample, highSample, bbxDist);

    return sca*0.75*min(min(bbxDist+1000., sdSpiraledScene(p)), min(sdSpiraledScene(lowSample), sdSpiraledScene(highSample)));
}

float sdFloor(float3 p) {
    return sdBox(p - float3(0., -10.5, 0.), float3(2400., 0.1, 2400.));
}

float sdEnv(float3 p) {
    float dFloor = sdFloor(p);
    float dBound = 1000.;//-sdBox(p, float3(25., 250., 9.));

    float dSpiral = sdSpiral(p);

    return min(dSpiral, min(dFloor, dBound));
}