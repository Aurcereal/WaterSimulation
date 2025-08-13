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
    public ComputeShader copyFoamParticleCountToArgsBufferShader;

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

    // Foam
    public ComputeBuffer updatingFoamParticles;
    public ComputeBuffer survivingFoamParticles;
    public ComputeBuffer foamParticleCounts;

    public ComputeBuffer foamParticleCellKeyEntryBuffer;
    public ComputeBuffer foamCellKeyToStartCoordBuffer;

    public ComputeManager()
    {
        particleSimulatorShader = ComputeHelper.FindInResourceFolder("ParticleSimulator");
        bitonicSortForwardShader = ComputeHelper.FindInResourceFolder("BitonicSortForward");
        copyFoamParticleCountToArgsBufferShader = ComputeHelper.FindInResourceFolder("CopyFoamParticleCountToArgsBuffer");

        positionBuffer = ComputeHelper.CreateBuffer<float3>(ParticleCount);
        predictedPositionBuffer = ComputeHelper.CreateBuffer<float3>(ParticleCount);
        velocityBuffer = ComputeHelper.CreateBuffer<float3>(ParticleCount);

        massBuffer = ComputeHelper.CreateBuffer<float>(ParticleCount);
        densityBuffer = ComputeHelper.CreateBuffer<float>(ParticleCount);
        nearDensityBuffer = ComputeHelper.CreateBuffer<float>(ParticleCount);

        if (EnableParticleSprings) springRestLengthBuffer = ComputeHelper.CreateBuffer<float>(ParticleCount * ParticleCount);

        colorBuffer = ComputeHelper.CreateBuffer<Color>(ParticleCount);

        particleCellKeyEntryBuffer = ComputeHelper.CreateBuffer<ParticleEntry>(ParticleCount);
        cellKeyToStartCoordBuffer = ComputeHelper.CreateBuffer<int>(SpatialLookupSize);

        //
        updatingFoamParticles = ComputeHelper.CreateBuffer<SimulationFoamManager.FoamParticle>(MaxFoamParticleCount);
        survivingFoamParticles = ComputeHelper.CreateBuffer<SimulationFoamManager.FoamParticle>(MaxFoamParticleCount);
        foamParticleCounts = ComputeHelper.CreateBuffer<uint>(2);

        foamParticleCellKeyEntryBuffer = ComputeHelper.CreateBuffer<ParticleEntry>(MaxFoamParticleCount);
        foamCellKeyToStartCoordBuffer = ComputeHelper.CreateBuffer<int>(FoamSpatialLookupSize);

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
            colorBuffer,
            particleCellKeyEntryBuffer,
            cellKeyToStartCoordBuffer,
            updatingFoamParticles,
            survivingFoamParticles,
            foamParticleCounts
        );

        springRestLengthBuffer?.Dispose();
    }
}
