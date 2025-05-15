using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Mathematics;
using static Unity.Mathematics.math;

public class ParticleManager
{
    public float2[] positions;
    public float2[] velocities;

    public ParticleManager() {
        positions = new float2[SimulationParameters.ParticleCount];
        velocities = new float2[SimulationParameters.ParticleCount];

        for(int i=0; i<positions.Length; i++) {
            float t = (i*1.0f+0.5f)/positions.Length;
            positions[i] = float2(-2.0f + t*4.0f, 0.0f);
            velocities[i] = normalize(UnityEngine.Random.insideUnitCircle);
        }
    }

    void UpdateParticle(float dt, float2 pos, float2 vel, out float2 outPos, out float2 outVel) {
        outPos = pos; outVel = vel;

        outVel += float2(0, -1) * SimulationParameters.Gravity * dt;
        outPos += vel * dt;

        // Collision
        float2 dist = (SimulationParameters.BoxDimensions * 0.5f - SimulationParameters.BoxThickness) - abs(outPos) - SimulationParameters.ParticleRadius;
        if(dist.x <= 0.0f) {
            outPos.x = sign(outPos.x) * (SimulationParameters.BoxDimensions.x * 0.5f - SimulationParameters.BoxThickness - SimulationParameters.ParticleRadius);
            outVel.x *= -1.0f;
        } else if(dist.y <= 0.0f) {
            outPos.y = sign(outPos.y) * (SimulationParameters.BoxDimensions.y * 0.5f - SimulationParameters.BoxThickness - SimulationParameters.ParticleRadius);
            outVel.y *= -1.0f;
        }
    }

    float SmoothingKernel(float smoothingRadius, float dist) {
        // https://www.desmos.com/calculator/rakgq19me5
        float vol = smoothingRadius*smoothingRadius/8.0f;

        float val = dist/smoothingRadius; 
        val = 1.0f - val*val;
        val = max(0.0f, val*val*val)/vol;
        
        return val;
    }

    public void Update(float dt) {
        for(int i=0; i<SimulationParameters.ParticleCount; i++) {
            float2 pos = positions[i]; float2 vel = velocities[i];

            float2 outPos, outVel;
            UpdateParticle(dt, pos, vel, out outPos, out outVel);

            positions[i] = outPos; velocities[i] = outVel;
        }
    }
}
