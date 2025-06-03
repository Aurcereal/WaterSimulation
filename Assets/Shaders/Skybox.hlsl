#define D2R .0174533
#define R2D 57.29578
#define PI 3.14159265
#define TAU 2.0*PI

float2 rot(float2 v, float o) {
    return float2(cos(o), sin(o)) * v.x + float2(-sin(o), cos(o)) * v.y;
}

float2 perp(float2 v) {
    return rot(v, PI*.5);
}

float star(float2 p, float flare)  {
    flare *= 0.7;
    float light = 0.;

    float r = length(p);
    light += .04/r;
    light += flare*max(0., 1.-abs(500.*p.y*p.x));
    p = rot(p, PI*.25);
    light += flare*.6*max(0., 1.-abs(750.*p.y*p.x));
    light *= smoothstep(1., 0.4, r);
    
    return light;
}

float glowLine(float2 p, float2 start, float2 end) {
    float2 mid = .5*(start+end);
    float len = length(end-start);
    float2 dir = normalize(end-start);
    
    p -= mid;
    p = float2(dot(p, dir), dot(p, perp(dir)));
    p.x = abs(p.x) - len*.5;
    p.x = max(0., p.x);
    
    return .001/length(p) * smoothstep(0.4, 0.2, length(p)); //.005
}

float3 hash33m(float3 p3)
{
    p3 += 0.01;
    p3 -= fmod(p3, 0.1);
	p3 = frac(p3 * float3(.1031, .1030, .0973));
    p3 += dot(p3, p3.yxz+33.33);
    return frac((p3.xxy + p3.yxx)*p3.zyx);

}

// float3 hash33m(float3 p) {
//     return frac(float3(104.,510.601.)*(cos(float3(11,19.,31.)+1.) * p));
// }

float2 oscillate(float2 params) {
    return sin(_Time.y*params*.5+frac(params*105.140));
}

float3 stars(float2 p) {
    //
    p += .45*_Time.y;
    p *= 1.5;
    p += 0.1;
    
    float repSize = .8;
    
    float2 lp = fmod(p + repSize*.5, repSize) - repSize*.5;
    float2 id = p-lp;
    float light = 0.;
    
    float2 baseRnd = hash33m(id.xyx).xy*2.-1.;
    float2 baseMove = oscillate(baseRnd*.5+.5)*repSize*.35;
    
    for(float x=-2.; x<=2.; x++) {
        for(float y=-2.; y<=2.; y++) {
            float2 sp = lp;
        
            float2 off = repSize*float2(x, y);
            float2 rnd = hash33m((id+off).xyx).xy*2.-1.;
            float2 move = oscillate(rnd*.5+.5)*repSize*.35;
            float size = frac(rnd.x*4194.+rnd.y*41.);
            
            sp -= (off+move);
            
            light += size*star(sp*2., 1.*smoothstep(.85, .95, size));
            
            float isIntersect = step(hash33m((id+off*.5).xyx).x, 0.15);
            if(abs(x) + abs(y) == 1.) light += isIntersect * glowLine(lp, baseMove, off+move);
        }
    }

    return light;
}


float3 render2D(float2 p) {
    return .8*stars(p) + 0.4*stars(p*2.5);
}

float3 toSpherical(float3 p, float3 up) {
    float3 ri = float3(1., 3., 2.);
    ri = normalize(ri - up * dot(ri, up));
    float3 fo = cross(ri, up);
    p = float3(dot(p, ri), dot(p, up), dot(p, fo));

    return float3(length(p),
                fmod(atan2(p.z, p.x)+TAU+10., TAU),
                PI*.5-acos(p.y/length(p))
                );
}

float3 SampleSpaceSkybox(float3 rd, float2 sp, float3 camFo)
{
    float2 polar = toSpherical(camFo, float3(0.,1.,0.)).yz;
    return smoothstep(0.4, -.8, rd.y) * float3(0.396,0.020,0.267) + render2D(1.5*(sp+polar*1.2*float2(-1.,1.)));
}