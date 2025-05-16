using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Mathematics;
using static Unity.Mathematics.math;

using static SimulationParameters;

public class ParticleManager
{

    float SmoothingKernelSpiky(float smoothingRadius, float dist)
    {
        // https://www.desmos.com/calculator/7fyumtu2rw
        float vol = PI * smoothingRadius * smoothingRadius / 10.0f;

        float val = dist / smoothingRadius;
        val = 1.0f - val;
        val = max(0.0f, val * val * val) / vol;

        return val;
    }

    float2 SmoothingKernelSpikyGradient(float smoothingRadius, float2 toSample)
    {
        float dist = length(toSample);
        float2 dir = normalizesafe(toSample);
        if (dist >= smoothingRadius) return float2(0.0f);

        // https://www.desmos.com/calculator/ligsrccvda
        float vol = PI * smoothingRadius * smoothingRadius / 10.0f;

        float val = dist / smoothingRadius;
        val = 1.0f - val;
        val = 3.0f * val * val;
        val /= vol * smoothingRadius;

        return val*dir;
    }

    float SmoothingKernel(float smoothingRadius, float dist) {
        // https://www.desmos.com/calculator/w1qrbwyhcs
        float vol = PI * smoothingRadius * smoothingRadius / 4.0f;

        float val = dist / smoothingRadius;
        val = 1.0f - val * val;
        val = max(0.0f, val * val * val) / vol;

        return val;
    }

    float2 SmoothingKernelGradient(float smoothingRadius, float2 toSample)
    {
        float dist = length(toSample);
        float2 dir = normalizesafe(toSample);
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
            totalDensity += masses[i] * SmoothingKernel(SmoothingRadius, length(positions[i] - pos));
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
        return PressureMultiplier * (TargetDensity - density);
    }

    // Pressure force is -PressureGradient since we'll flow from high pressure to low pressure
    float2 CalculatePressureForce(float2 pos)
    {
        float2 totalForce = float2(0.0f);
        for (int i = 0; i < ParticleCount; i++)
        {
            totalForce +=
                masses[i] *
                DensityToPressure(densities[i]) *
                -SmoothingKernelGradient(SmoothingRadius, pos - positions[i])
                / densities[i];
        }
        return totalForce;
    }

    public float2[] positions;
    public float2[] velocities;
    public float[] masses;

    public ParticleManager()
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

    void UpdateParticle(float dt, int index, float2 pos, float2 vel, out float2 outPos, out float2 outVel) {
        outPos = pos; outVel = vel;

        float2 a = CalculatePressureForce(pos) / densities[index];
        outVel += a * dt; //outVel += float2(0, -1) * SimulationParameters.Gravity * dt;
        outPos += vel * dt;

        // Collision
        float2 dist = (SimulationParameters.BoxDimensions * 0.5f - SimulationParameters.BoxThickness) - abs(outPos) - SimulationParameters.ParticleRadius;
        if (dist.x <= 0.0f) {
            outPos.x = sign(outPos.x) * (SimulationParameters.BoxDimensions.x * 0.5f - SimulationParameters.BoxThickness - SimulationParameters.ParticleRadius);
            outVel.x *= -1.0f;
        } else if (dist.y <= 0.0f) {
            outPos.y = sign(outPos.y) * (SimulationParameters.BoxDimensions.y * 0.5f - SimulationParameters.BoxThickness - SimulationParameters.ParticleRadius);
            outVel.y *= -1.0f;
        }
    }

    public void Update(float dt) {
        UpdateParticleDensities();

        for (int i = 0; i < SimulationParameters.ParticleCount; i++)
        {
            float2 pos = positions[i]; float2 vel = velocities[i];

            float2 outPos, outVel;
            UpdateParticle(dt, i, pos, vel, out outPos, out outVel);

            positions[i] = outPos; velocities[i] = outVel;
        }
    }
}
