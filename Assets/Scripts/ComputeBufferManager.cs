using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Mathematics;
using static Unity.Mathematics.math;

using static SimulationParameters;

public class ComputeBufferManager
{

    // Physics
    public ComputeBuffer positionBuffer;
    public ComputeBuffer predictedPositionBuffer;
    public ComputeBuffer velocityBuffer;

    public ComputeBuffer massBuffer;

    public ComputeBuffer densityBuffer;
    public ComputeBuffer nearDensityBuffer;

    // Spatial Hashing
    public ComputeBuffer particleCellKeyEntryBuffer;
    public ComputeBuffer cellKeyToStartCoordBuffer;

    public ComputeBufferManager()
    {
        positionBuffer = ComputeHelper.CreateBuffer<float2>(ParticleCount);
        velocityBuffer = ComputeHelper.CreateBuffer<float2>(ParticleCount);
    }

    public void Destructor()
    {
        ComputeHelper.DisposeBuffers(
            positionBuffer,
            velocityBuffer,
            densityBuffer,
            nearDensityBuffer,
            particleCellKeyEntryBuffer,
            cellKeyToStartCoordBuffer
        );
    }
}
