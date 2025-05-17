using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Mathematics;
using static Unity.Mathematics.math;

using static SimulationParameters;

public class ParticleSimulator
{

    static float SmoothingKernelCPY(float radius, float dst)
    {
        if (dst >= radius) return 0;
        float volume = (PI * pow(radius, 4)) / 6;
        return (radius - dst) * (radius - dst) / volume;
    }

    static float2 SmoothingKernelGradientCPY(float radius, float2 toSample)
    {
        float dst = length(toSample);

        if (dst >= radius) return 0;

        float scale = 12 / (pow(radius, 4) * PI);
        return normalizesafe(-toSample) * (radius-dst) * scale;
    }

    static float SmoothingKernelPow2(float smoothingRadius, float dist)
    {
        if (dist >= smoothingRadius) return 0.0f;

        float vol = PI * smoothingRadius * smoothingRadius / 6.0f;

        float val = 1.0f - dist / smoothingRadius;
        val = val * val;
        val /= vol;

        return val;
    }

    static float2 SmoothingKernelPow2Gradient(float smoothingRadius, float2 fromSample)
    {
        float dist = length(fromSample);
        float2 dir = normalizesafe(fromSample);//, normalize(UnityEngine.Random.insideUnitCircle));

        if (dist >= smoothingRadius) return float2(0.0f);

        float vol = PI * smoothingRadius * smoothingRadius / 6.0f;

        float val = 2.0f * (1.0f - dist / smoothingRadius);
        val /= vol;

        return dir * val;
    }

    static float SmoothingKernelPow3(float smoothingRadius, float dist)
    {
        if (dist >= smoothingRadius) return 0.0f;

        // https://www.desmos.com/calculator/7fyumtu2rw
        float vol = PI * smoothingRadius * smoothingRadius / 10.0f;

        float val = dist / smoothingRadius;
        val = 1.0f - val;
        val = max(0.0f, val * val * val) / vol;

        return val;
    }

    static float2 SmoothingKernelPow3Gradient(float smoothingRadius, float2 toSample)
    {
        float dist = length(toSample);
        float2 dir = normalizesafe(-toSample);
        if (dist >= smoothingRadius) return float2(0.0f);

        // https://www.desmos.com/calculator/ligsrccvda
        float vol = PI * smoothingRadius * smoothingRadius / 10.0f;

        float val = dist / smoothingRadius;
        val = 1.0f - val;
        val = 3.0f * val * val;
        val /= vol * smoothingRadius;
        val = abs(val);

        return val * dir;
    }

    static float SmoothingKernel(float smoothingRadius, float dist) {
        // https://www.desmos.com/calculator/w1qrbwyhcs
        float vol = PI * smoothingRadius * smoothingRadius / 4.0f;

        float val = dist / smoothingRadius;
        val = 1.0f - val * val;
        val = max(0.0f, val * val * val) / vol;

        return val;
    }

    static float2 SmoothingKernelGradient(float smoothingRadius, float2 toSample)
    {
        float dist = length(toSample);
        float2 dir = normalizesafe(toSample, normalize(UnityEngine.Random.insideUnitCircle));
        if (dist >= smoothingRadius) return float2(0.0f);

        // https://www.desmos.com/calculator/s58inuu2pm
        float vol = PI * smoothingRadius * smoothingRadius / 4.0f;

        float val = dist / smoothingRadius;
        val = 1.0f - val * val;
        val = 6.0f * val * val * dist;
        val /= vol * smoothingRadius * smoothingRadius;

        return val*dir;
    }

    float CalculateDensity(float2 pos)
    {
        float totalDensity = 0.0f;
        for (int i = 0; i < ParticleCount; i++)
        {
            totalDensity += masses[i] * SmoothingKernelCPY(SmoothingRadius, length(positions[i] - pos));
        }
        return totalDensity;
    }

    float[] densities;
    void UpdateParticleDensities()
    {
        for (int i = 0; i < ParticleCount; i++)
        {
            densities[i] = CalculateDensity(positions[i]);
        }
    }

    float DensityToPressure(float density)
    {
        // TargetDensity kind of offsets pressure to determine what happens with empty space: 
        // if the result is high, our gradient will go from empty to wherever this is, 
        // if our result is low, our gradient will go from wherever this is to empty
        return PressureMultiplier * (density - TargetDensity);
    }

    // Pressure force is -PressureGradient since we'll flow from high pressure to low pressure
    float2 CalculatePressureForce(int particleIndex)
    {
        float2 pos = positions[particleIndex];

        float2 totalForce = float2(0.0f);
        for (int i = 0; i < ParticleCount; i++)
        {
            if (i == particleIndex) continue;

            totalForce +=
                masses[i] * 
                (DensityToPressure(densities[i]) + DensityToPressure(densities[particleIndex])) * 0.5f *
                (-SmoothingKernelPow2Gradient(SmoothingRadius, positions[i] - pos))
                / densities[i];
        }
        return totalForce;
    }

    public float2[] positions;
    public float2[] velocities;
    public float[] masses;

    public ParticleSimulator()
    {
        positions = new float2[ParticleCount];
        velocities = new float2[ParticleCount];
        masses = new float[ParticleCount];

        densities = new float[ParticleCount];

        for (int i = 0; i < positions.Length; i++)
        {
            float t = (i * 1.0f + 0.5f) / positions.Length;
            positions[i] = float2(
                UnityEngine.Random.Range(-BoxDimensions.x * 0.5f + BoxThickness + ParticleRadius, BoxDimensions.x * 0.5f - BoxThickness - ParticleRadius),
                UnityEngine.Random.Range(-BoxDimensions.y * 0.5f + BoxThickness + ParticleRadius, BoxDimensions.y * 0.5f - BoxThickness - ParticleRadius)
                );
            velocities[i] = float2(0.0f); //normalize(UnityEngine.Random.insideUnitCircle);
            masses[i] = 1.0f;
        }
    }

    void UpdateParticle(float dt, int index, ref float2 pos, ref float2 vel) {
        float2 pressureAcceleration = CalculatePressureForce(index) / densities[index];
        float2 a = pressureAcceleration + Gravity;
        vel += a * dt; //outVel += float2(0, -1) * SimulationParameters.Gravity * dt;
        pos += vel * dt;

        // Collision
        float2 dist = (SimulationParameters.BoxDimensions * 0.5f - SimulationParameters.BoxThickness) - abs(pos) - SimulationParameters.ParticleRadius;
        if (dist.x <= 0.0f) {
            pos.x = sign(pos.x) * (SimulationParameters.BoxDimensions.x * 0.5f - SimulationParameters.BoxThickness - SimulationParameters.ParticleRadius);
            vel.x *= -0.9f;
        } else if (dist.y <= 0.0f) {
            pos.y = sign(pos.y) * (SimulationParameters.BoxDimensions.y * 0.5f - SimulationParameters.BoxThickness - SimulationParameters.ParticleRadius);
            vel.y *= -0.9f;
        }
    }

    public void Update(float dt) {
        // Timestep is SO IMPORTANT TO A GOOD SIM IF ITS BAD WE'LL DIVERGE
        UpdateParticleDensities();

        for (int i = 0; i < SimulationParameters.ParticleCount; i++)
        {
            UpdateParticle(1f/60f, i, ref positions[i], ref velocities[i]);
        }
    }
}
