using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Mathematics;
using static Unity.Mathematics.math;

using static SimulationParameters;
using static ParticlePhysics;
using System.Threading.Tasks;

public class ParticleSimulator
{

    float2 CalculatePredictedPosition(int particleIndex, float lookaheadDT)
    {
        return positions[particleIndex] + velocities[particleIndex] * lookaheadDT;
    }

    float CalculateDensity(float2 pos)
    {
        float totalDensity = 0.0f;
        GameManager.Ins.spatialHash.ForEachParticleWithinSmoothingRadius(pos, i =>
            totalDensity += masses[i] * SmoothingKernelPow2(SmoothingRadius, length(predictedPositions[i] - pos))
            );
        return totalDensity;
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
        float2 pos = predictedPositions[particleIndex];

        float2 totalForce = float2(0.0f);
        GameManager.Ins.spatialHash.ForEachParticleWithinSmoothingRadius(pos, i =>
            {
                if (i != particleIndex)
                {
                    totalForce +=
                        masses[i] *
                        (DensityToPressure(densities[i]) + DensityToPressure(densities[particleIndex])) * 0.5f *
                        (-SmoothingKernelPow2Gradient(SmoothingRadius, predictedPositions[i] - pos))
                        / densities[i];
                }
            }
        );
        return totalForce;
    }

    // Applies force to make velocity more similar to close particles
    float2 CalculateViscosityForce(int particleIndex)
    {
        float2 pos = predictedPositions[particleIndex];
        float2 vel = velocities[particleIndex];

        float2 totalForce = float2(0.0f);
        GameManager.Ins.spatialHash.ForEachParticleWithinSmoothingRadius(pos, i =>
            {
                if (i != particleIndex)
                {
                    totalForce +=
                        masses[i] *
                        (velocities[i] - vel) *
                        SmoothingKernelSmoothTop(SmoothingRadius, length(predictedPositions[i] - pos));
                }
            }
        );
        return totalForce * ViscosityStrength;
    }

    float2 CalculateMouseForce(int particleIndex)
    {
        float forceSign = (GameManager.Ins.inputManager.RightMouseButton ? 1 : 0) + (GameManager.Ins.inputManager.LeftMouseButton ? -1 : 0);
        float2 toParticle = predictedPositions[particleIndex] - GameManager.Ins.inputManager.WorldMousePosition;

        float dist = length(toParticle);
        float2 dir = normalizesafe(toParticle);

        if (forceSign == 0 || dist >= MouseForceRadius) return float2(0f);
        float2 force = MouseForceStrength * forceSign * dir; //* SmoothingKernelSmoothTop(MouseForceRadius, length(toParticle));
        return force;
    }

    public float2[] positions;
    public float2[] predictedPositions;
    public float2[] velocities;
    public float[] masses;
    float[] densities;

    public ParticleSimulator()
    {
        positions = new float2[ParticleCount];
        predictedPositions = new float2[ParticleCount];
        velocities = new float2[ParticleCount];
        masses = new float[ParticleCount];

        densities = new float[ParticleCount];

        for (int i = 0; i < positions.Length; i++)
        {
            float t = (i * 1.0f + 0.5f) / positions.Length;
            positions[i] = float2(
                UnityEngine.Random.Range(-SpawnDimensions.x * 0.5f + BoxThickness + ParticleRadius, SpawnDimensions.x * 0.5f - BoxThickness - ParticleRadius),
                UnityEngine.Random.Range(-SpawnDimensions.y * 0.5f + BoxThickness + ParticleRadius, SpawnDimensions.y * 0.5f - BoxThickness - ParticleRadius)
                );
            velocities[i] = float2(0.0f); //normalize(UnityEngine.Random.insideUnitCircle);
            masses[i] = 1.0f;
        }
    }

    void UpdateParticle(float dt, int index, ref float2 pos, ref float2 vel)
    {
        // Densities being 0 (should never happen) causes positions to become NaN and particles to disappear
        float2 pressureAcceleration = CalculatePressureForce(index) / densities[index];
        float2 mouseAcceleration = CalculateMouseForce(index) / densities[index];
        float2 viscosityAcceleration = CalculateViscosityForce(index) / densities[index];
        float2 a = pressureAcceleration + mouseAcceleration + viscosityAcceleration + Gravity;

        vel += a * dt;
        pos += vel * dt;

        // Collision
        float2 dist = (BoxDimensions * 0.5f - BoxThickness) - abs(pos) - ParticleRadius;
        if (dist.x <= 0.0f)
        {
            pos.x = sign(pos.x) * (BoxDimensions.x * 0.5f - BoxThickness - ParticleRadius);
            vel.x *= -0.9f;
        }
        else if (dist.y <= 0.0f)
        {
            pos.y = sign(pos.y) * (BoxDimensions.y * 0.5f - BoxThickness - ParticleRadius);
            vel.y *= -0.9f;
        }
    }

    public void Update(float dt)
    {
        if (dt > 1.0f / 60.0f)
        {
            Debug.Log("Timestep is too large for an accurate simulation, slowing down time accordingly...");
            dt = 1.0f / 60.0f;
        }

        //
        Parallel.For(0, ParticleCount, i => predictedPositions[i] = CalculatePredictedPosition(i, 1f / 60f));
        GameManager.Ins.spatialHash.UpdateSpatialHash(); // Uses predicted positions
        Parallel.For(0, ParticleCount, i => densities[i] = CalculateDensity(predictedPositions[i]));
        Parallel.For(0, ParticleCount, i => UpdateParticle(dt, i, ref positions[i], ref velocities[i]));

    }
}
