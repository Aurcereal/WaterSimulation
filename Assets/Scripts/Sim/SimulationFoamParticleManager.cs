using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Mathematics;
using static Unity.Mathematics.math;

using static SimulationParameters;
using UnityEngine.Rendering;

public class SimulationFoamParticleManager
{
    ComputeBuffer argsBuffer;

    public struct FoamParticle
    {
        float3 position;
        float3 velocity;
        float remainingLifetime;
    }

    public SimulationFoamParticleManager()
    {
        argsBuffer = ComputeHelper.CreateArgsBuffer(MeshUtils.QuadMesh, 0, 0);

        GameManager.Ins.computeManager.copyFoamParticleCountToArgsBufferShader.SetBuffer("argsBuffer", argsBuffer, "CopyFoamParticleCountToArgsBuffer");
        GameManager.Ins.computeManager.copyFoamParticleCountToArgsBufferShader.SetBuffer("foamParticleCounts", GameManager.Ins.computeManager.foamParticleCounts, "CopyFoamParticleCountToArgsBuffer");
    }

    public void SpawnFoamParticles()
    {
        ComputeHelper.Dispatch(GameManager.Ins.computeManager.particleSimulatorShader, ParticleCount, 1, 1, "SpawnFoamParticles");
    }

    public void UpdateFoamParticles()
    {
        ComputeHelper.Dispatch(GameManager.Ins.computeManager.particleSimulatorShader, MaxFoamParticleCount, 1, 1, "UpdateFoamParticles");
    }

    public void MoveSurvivingFoamParticlesToUpdatingBuffer()
    {
        ComputeHelper.Dispatch(GameManager.Ins.computeManager.particleSimulatorShader, MaxFoamParticleCount, 1, 1, "MoveSurvivingFoamParticlesToUpdatingBuffer");
    }

    public void UpdateFoamArgsBuffer()
    {
        ComputeHelper.Dispatch(GameManager.Ins.computeManager.copyFoamParticleCountToArgsBufferShader, 1, 1, 1, "CopyFoamParticleCountToArgsBuffer");
    }

    public void Draw(CommandBuffer cmd, Material mat)
    {
        cmd.DrawMeshInstancedIndirect(MeshUtils.QuadMesh, 0, mat, 0, argsBuffer);
    }
}
