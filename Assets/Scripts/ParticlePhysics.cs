using Unity.Mathematics;
using static Unity.Mathematics.math;

public static class ParticlePhysics
{
    public static float SmoothingKernelPow2(float smoothingRadius, float dist)
    {
        if (dist >= smoothingRadius) return 0.0f;

        float vol = PI * smoothingRadius * smoothingRadius / 6.0f;

        float val = 1.0f - dist / smoothingRadius;
        val = val * val;
        val /= vol;

        return val;
    }

    public static float2 SmoothingKernelPow2Gradient(float smoothingRadius, float2 fromSample)
    {
        float dist = length(fromSample);
        float2 dir = normalizesafe(fromSample, normalize(UnityEngine.Random.insideUnitCircle));

        if (dist >= smoothingRadius) return float2(0.0f);

        float vol = PI * smoothingRadius * smoothingRadius / 6.0f;

        float val = 2.0f * (1.0f - dist / smoothingRadius);
        val /= vol;

        return dir * val;
    }

    public static float SmoothingKernelPow3(float smoothingRadius, float dist)
    {
        if (dist >= smoothingRadius) return 0.0f;

        float vol = PI * smoothingRadius * smoothingRadius / 10.0f;

        float val = dist / smoothingRadius;
        val = 1.0f - val;
        val = max(0.0f, val * val * val) / vol;

        return val;
    }

    public static float2 SmoothingKernelPow3Gradient(float smoothingRadius, float2 fromSample)
    {
        float dist = length(fromSample);
        float2 dir = normalizesafe(fromSample, normalize(UnityEngine.Random.insideUnitCircle));
        if (dist >= smoothingRadius) return float2(0.0f);

        float vol = PI * smoothingRadius * smoothingRadius / 10.0f;

        float val = dist / smoothingRadius;
        val = 1.0f - val;
        val = 3.0f * val * val;
        val /= vol * smoothingRadius;
        val = abs(val);

        return val * dir;
    }

    public static float SmoothingKernelSmoothTop(float smoothingRadius, float dist) {
        // https://www.desmos.com/calculator/w1qrbwyhcs
        float vol = PI * smoothingRadius * smoothingRadius / 4.0f;

        float val = dist / smoothingRadius;
        val = 1.0f - val * val;
        val = max(0.0f, val * val * val) / vol;

        return val;
    }

    public static float2 SmoothingKernelSmoothTopGradient(float smoothingRadius, float2 fromSample)
    {
        float dist = length(fromSample);
        float2 dir = normalizesafe(fromSample, normalize(UnityEngine.Random.insideUnitCircle));
        if (dist >= smoothingRadius) return float2(0.0f);

        // https://www.desmos.com/calculator/s58inuu2pm
        float vol = PI * smoothingRadius * smoothingRadius / 4.0f;

        float val = dist / smoothingRadius;
        val = 1.0f - val * val;
        val = 6.0f * val * val * dist;
        val /= vol * smoothingRadius * smoothingRadius;

        return val*dir;
    }
}