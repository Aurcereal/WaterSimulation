using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static SimulationParameters;
using Unity.Mathematics;
using static Unity.Mathematics.math;

// may separate into classes based on simulation purpose and then have them each implement uniform methods and update methods and initialize methods...
public struct ParticleEntry
{
    public int particleIndex;
    public int cellKey;
    public ParticleEntry(int i, int k) { particleIndex = i; cellKey = k; }
}

public class SimulationInitializer
{

    ComputeShader particleSimulatorShader;
    public SimulationInitializer()
    {
        particleSimulatorShader = GameManager.Ins.computeManager.particleSimulatorShader;
    }

    // Could make this run on compute shader later if you want
    public void InitializeSimulation()
    {
        var positions = new float2[ParticleCount];
        var predictedPositions = new float2[ParticleCount];
        var velocities = new float2[ParticleCount];
        var masses = new float[ParticleCount];

        var densities = new float[ParticleCount];
        var nearDensities = new float[ParticleCount];
        
        var colors = new Color[ParticleCount];

        for (int i = 0; i < positions.Length; i++)
        {
            float t = (i * 1.0f + 0.5f) / positions.Length;
            positions[i] = float2(
                UnityEngine.Random.Range(-SpawnDimensions.x * 0.5f + BoxThickness + ParticleRadius, SpawnDimensions.x * 0.5f - BoxThickness - ParticleRadius),
                UnityEngine.Random.Range(-SpawnDimensions.y * 0.5f + BoxThickness + ParticleRadius, SpawnDimensions.y * 0.5f - BoxThickness - ParticleRadius)
                );
            velocities[i] = float2(0.0f);
            masses[i] = 1.0f;
            colors[i] = Color.white;
        }

        var partIDCellKeyPairs = new ParticleEntry[ParticleCount];
        var keyToStartCoord = new int[SpatialLookupSize];

        GameManager.Ins.computeManager.positionBuffer.SetData(positions);
        GameManager.Ins.computeManager.predictedPositionBuffer.SetData(predictedPositions);
        GameManager.Ins.computeManager.velocityBuffer.SetData(velocities);
        GameManager.Ins.computeManager.massBuffer.SetData(masses);

        GameManager.Ins.computeManager.densityBuffer.SetData(densities);
        GameManager.Ins.computeManager.nearDensityBuffer.SetData(nearDensities);

        if (EnableParticleSprings)
        {
            var springRests = new float[ParticleCount * ParticleCount];
            for (int i = 0; i < springRests.Length; i++) springRests[i] = -1.0f;
            GameManager.Ins.computeManager.springRestLengthBuffer.SetData(springRests);
        }

        GameManager.Ins.computeManager.colorBuffer.SetData(colors);

        GameManager.Ins.computeManager.particleCellKeyEntryBuffer.SetData(partIDCellKeyPairs);
        GameManager.Ins.computeManager.cellKeyToStartCoordBuffer.SetData(keyToStartCoord);

    }

}
