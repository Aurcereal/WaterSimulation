// Need to make a 'normalizesafe?'
#define PI 3.14159265
#define IPI 0.318309886

float SmoothingKernelPow2(float smoothingRadius, float dist)
{
    if (dist >= smoothingRadius) return 0.0;

    float vol = PI * smoothingRadius * smoothingRadius * smoothingRadius * 2.0 / 15.0;

    float val = 1.0 - dist / smoothingRadius;
    val = val * val;
    val /= vol;

    return val;
}

// Doesn't check if dist > SmoothingRadius
float SmoothingKernelPow2Unsafe(float dist)
{
    float invVol = IPI * InvSmoothingRadius * InvSmoothingRadius * InvSmoothingRadius * 7.5;

    float val = 1.0 - dist * InvSmoothingRadius;
    val = val * val;
    val *= invVol;

    return val;
}

float3 SmoothingKernelPow2Gradient(float smoothingRadius, float3 fromSample)
{
    float dist = length(fromSample);
    float3 dir = normalizesafe(fromSample);

    if (dist >= smoothingRadius) return 0.0;

    float vol = PI * smoothingRadius * smoothingRadius * smoothingRadius * 2.0 / 15.0;

    float val = 2.0 * (1.0 - dist / smoothingRadius);
    val /= vol;

    return dir * val;
}

float3 SmoothingKernelPow2GradientGolf(float3 fromSample)
{
    float sqrDist = dot(fromSample, fromSample);
    if(sqrDist >= SqrSmoothingRadius) return 0.;

    float4 dirDist = normalizesafegetmag(fromSample);
    float3 dir = dirDist.xyz;
    float dist = dirDist.w;

    float invVol = IPI * InvSmoothingRadius * InvSmoothingRadius * InvSmoothingRadius * 7.5;

    float val = 2.0 * (1.0 - dist * InvSmoothingRadius);
    val *= invVol;

    return dir * val;
}

float SmoothingKernelPow3(float smoothingRadius, float dist)
{
    if (dist >= smoothingRadius) return 0.0;

    float vol = PI * smoothingRadius * smoothingRadius * smoothingRadius / 15.0;

    float val = dist / smoothingRadius;
    val = 1.0 - val;
    val = max(0.0, val * val * val) / vol;

    return val;
}

float SmoothingKernelPow3Unsafe(float dist)
{
    float invVol = IPI * InvSmoothingRadius * InvSmoothingRadius * InvSmoothingRadius * 0.0665;

    float val = dist * InvSmoothingRadius;
    val = 1.0 - val;
    val = val * val * val * invVol;

    return val;
}

float3 SmoothingKernelPow3Gradient(float smoothingRadius, float3 fromSample)
{
    float dist = length(fromSample);
    float3 dir = normalizesafe(fromSample);
    if (dist >= smoothingRadius) return 0.0;

    float vol = PI * smoothingRadius * smoothingRadius * smoothingRadius / 15.0;

    float val = dist / smoothingRadius;
    val = 1.0 - val;
    val = 3.0 * val * val;
    val /= vol * smoothingRadius;
    val = abs(val);

    return val * dir;
}

float3 SmoothingKernelPow3GradientGolf(float3 fromSample)
{
    float sqrDist = dot(fromSample, fromSample);
    if(sqrDist >= SqrSmoothingRadius) return 0.;

    float4 dirDist = normalizesafegetmag(fromSample);
    float dist = dirDist.w;
    float3 dir = dirDist.xyz;

    float invVol = IPI * InvSmoothingRadius * InvSmoothingRadius * InvSmoothingRadius * 0.0665;

    float val = dist * InvSmoothingRadius;
    val = 1.0 - val;
    val = 3.0 * val * val;
    val *= invVol * InvSmoothingRadius;

    return val * dir;
}

float SmoothingKernelSmoothTop(float smoothingRadius, float dist) {
    // https://www.desmos.com/calculator/w1qrbwyhcs
    float vol = PI * smoothingRadius * smoothingRadius * smoothingRadius * 64.0 / 315.0;

    float val = dist / smoothingRadius;
    val = 1.0 - val * val;
    val = max(0.0, val * val * val) / vol;

    return val;
}

float SmoothingKernelSmoothTopUnsafe(float dist) {
    // https://www.desmos.com/calculator/w1qrbwyhcs
    float invVol = IPI * InvSmoothingRadius * InvSmoothingRadius * InvSmoothingRadius * 4.921875;

    float val = dist * InvSmoothingRadius;
    val = 1.0 - val * val;
    val = val * val * val * invVol;

    return val;
}

float3 SmoothingKernelSmoothTopGradient(float smoothingRadius, float3 fromSample)
{
    float dist = length(fromSample);
    float3 dir = normalizesafe(fromSample);
    if (dist >= smoothingRadius) return 0.0;

    // https://www.desmos.com/calculator/s58inuu2pm
    float vol = PI * smoothingRadius * smoothingRadius * smoothingRadius * 64.0 / 315.0;

    float val = dist / smoothingRadius;
    val = 1.0 - val * val;
    val = 6.0 * val * val * dist;
    val /= vol * smoothingRadius * smoothingRadius;

    return val*dir;
}

float3 SmoothingKernelSmoothTopGradientGolf(float3 fromSample)
{
    float sqrDist = dot(fromSample, fromSample);
    if(sqrDist >= SqrSmoothingRadius) return 0.;

    float4 dirDist = normalizesafegetmag(fromSample);
    float3 dir = dirDist.xyz;
    float dist = dirDist.w;

    // https://www.desmos.com/calculator/s58inuu2pm
    float invVol = IPI * InvSmoothingRadius * InvSmoothingRadius * InvSmoothingRadius * 4.921875;

    float val = dist * InvSmoothingRadius;
    val = 1.0 - val * val;
    val = 6.0 * val * val * dist;
    val *= invVol * InvSmoothingRadius * InvSmoothingRadius;

    return val*dir;
}