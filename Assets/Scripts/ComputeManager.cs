using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Mathematics;
using static Unity.Mathematics.math;

using static SimulationParameters;

public class ComputeManager
{

    // Shaders
    public ComputeShader particleSimulatorShader;
    public ComputeShader bitonicSortForwardShader;

    // Physics
    public ComputeBuffer positionBuffer;
    public ComputeBuffer predictedPositionBuffer;
    public ComputeBuffer velocityBuffer;

    public ComputeBuffer massBuffer;

    public ComputeBuffer densityBuffer;
    public ComputeBuffer nearDensityBuffer;

    public ComputeBuffer springRestLengthBuffer;

    public ComputeBuffer colorBuffer;

    // Spatial Hashing
    public ComputeBuffer particleCellKeyEntryBuffer;
    public ComputeBuffer cellKeyToStartCoordBuffer;

    public ComputeManager()
    {
        particleSimulatorShader = ComputeHelper.FindInResourceFolder("ParticleSimulator");
        bitonicSortForwardShader = ComputeHelper.FindInResourceFolder("BitonicSortForward");

        positionBuffer = ComputeHelper.CreateBuffer<float2>(ParticleCount);
        predictedPositionBuffer = ComputeHelper.CreateBuffer<float2>(ParticleCount);
        velocityBuffer = ComputeHelper.CreateBuffer<float2>(ParticleCount);
        massBuffer = ComputeHelper.CreateBuffer<float>(ParticleCount);

        densityBuffer = ComputeHelper.CreateBuffer<float>(ParticleCount);
        nearDensityBuffer = ComputeHelper.CreateBuffer<float>(ParticleCount);

        springRestLengthBuffer = ComputeHelper.CreateBuffer<float>(ParticleCount * ParticleCount);

        colorBuffer = ComputeHelper.CreateBuffer<Color>(ParticleCount);

        particleCellKeyEntryBuffer = ComputeHelper.CreateBuffer<ParticleEntry>(ParticleCount);
        cellKeyToStartCoordBuffer = ComputeHelper.CreateBuffer<int>(SpatialLookupSize);
    }

    public void Destructor()
    {
        ComputeHelper.DisposeBuffers(
            positionBuffer,
            predictedPositionBuffer,
            velocityBuffer,
            massBuffer,
            densityBuffer,
            nearDensityBuffer,
            particleCellKeyEntryBuffer,
            cellKeyToStartCoordBuffer,
            colorBuffer
        );
    }
}
