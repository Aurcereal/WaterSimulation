// Need to make a 'normalizesafe?'
#define PI 3.14159265

float SmoothingKernelPow2(float smoothingRadius, float dist)
{
    if (dist >= smoothingRadius) return 0.0;

    float vol = PI * smoothingRadius * smoothingRadius / 6.0;

    float val = 1.0 - dist / smoothingRadius;
    val = val * val;
    val /= vol;

    return val;
}

float2 SmoothingKernelPow2Gradient(float smoothingRadius, float2 fromSample)
{
    float dist = length(fromSample);
    float2 dir = normalize(fromSample);

    if (dist >= smoothingRadius) return 0.0;

    float vol = PI * smoothingRadius * smoothingRadius / 6.0;

    float val = 2.0 * (1.0 - dist / smoothingRadius);
    val /= vol;

    return dir * val;
}

float SmoothingKernelPow3(float smoothingRadius, float dist)
{
    if (dist >= smoothingRadius) return 0.0;

    float vol = PI * smoothingRadius * smoothingRadius / 10.0;

    float val = dist / smoothingRadius;
    val = 1.0 - val;
    val = max(0.0, val * val * val) / vol;

    return val;
}

float2 SmoothingKernelPow3Gradient(float smoothingRadius, float2 fromSample)
{
    float dist = length(fromSample);
    float2 dir = normalize(fromSample);
    if (dist >= smoothingRadius) return 0.0;

    float vol = PI * smoothingRadius * smoothingRadius / 10.0;

    float val = dist / smoothingRadius;
    val = 1.0 - val;
    val = 3.0 * val * val;
    val /= vol * smoothingRadius;
    val = abs(val);

    return val * dir;
}

float SmoothingKernelSmoothTop(float smoothingRadius, float dist) {
    // https://www.desmos.com/calculator/w1qrbwyhcs
    float vol = PI * smoothingRadius * smoothingRadius / 4.0;

    float val = dist / smoothingRadius;
    val = 1.0 - val * val;
    val = max(0.0, val * val * val) / vol;

    return val;
}

float2 SmoothingKernelSmoothTopGradient(float smoothingRadius, float2 fromSample)
{
    float dist = length(fromSample);
    float2 dir = normalize(fromSample);
    if (dist >= smoothingRadius) return 0.0;

    // https://www.desmos.com/calculator/s58inuu2pm
    float vol = PI * smoothingRadius * smoothingRadius / 4.0;

    float val = dist / smoothingRadius;
    val = 1.0 - val * val;
    val = 6.0 * val * val * dist;
    val /= vol * smoothingRadius * smoothingRadius;

    return val*dir;
}