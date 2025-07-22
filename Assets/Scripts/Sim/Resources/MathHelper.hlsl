#define PI 3.141592
#define TAU 6.283185

float amod(float v, float m) {
    float s = fmod(abs(v), m);
    return s + (m - 2. * s) * step(v, 0.);
}

float3x3 rot2D(float o) {
    return float3x3(
        float3(cos(o), sin(o), 0.),
        float3(-sin(o), cos(o), 0.),
        float3(0.,0.,1.)
    );
}

float2 rot2D(float2 v, float o) {
    return mul(float2x2(cos(o), sin(o), -sin(o), cos(o)), v);
}

// float2 rot(float2 v, float o) {
//     return mul(float2x2(cos(o), sin(o), -sin(o), cos(o)), v);
// }

float3 normalizesafe(float3 v) {
    float sqrDist = dot(v, v);
    if(sqrDist < 0.0000001) {
        return float3(1.0,0.0,0.0);
    } else {
        return v / sqrt(sqrDist);
    }
}

float4 normalizesafegetmag(float3 v) {
    float sqrDist = dot(v, v);
    if(sqrDist < 0.0000001) {
        return float4(1.0,0.0,0.0, 0.);
    } else {
        float len = sqrt(sqrDist);
        return float4(v / len, len);
    }
}

// https://www.shadertoy.com/view/4djSRW
float3 hash33(float3 p3) {
    p3 = frac(p3 * float3(.1031, .1030, .0973));
    p3 += dot(p3, p3.yxz+33.33);
    return frac((p3.xxy + p3.yxx)*p3.zyx);
}

float hash31(float3 p3)
{
    //return frac(410.*sin(dot(p3, float3(145., 15., 98.))));
	p3  = frac(p3 * .1031);
    p3 += dot(p3, p3.zyx + 31.32);
    return frac((p3.x + p3.y) * p3.z);
}