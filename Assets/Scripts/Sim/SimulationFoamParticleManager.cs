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

    Material foamParticle3DMaterial = new Material(Shader.Find("Unlit/FoamParticleDebug"));
    Material foamParticleBillboardMaterial = new Material(Shader.Find("Unlit/FoamParticle"));

    public RenderTexture FoamTex { get; private set; }

    ComputeShader foamSpatialHashingShader;

    public struct FoamParticle
    {
        float3 position;
        float3 velocity;
        float remainingLifetime;
        int debugType;
    }

    public SimulationFoamParticleManager()
    {
        ComputeManager computeManager = GameManager.Ins.computeManager;

        argsBuffer = ComputeHelper.CreateArgsBuffer(MeshUtils.QuadMesh, 0, 0);

        computeManager.copyFoamParticleCountToArgsBufferShader.SetBuffer("argsBuffer", argsBuffer, "CopyFoamParticleCountToArgsBuffer");
        computeManager.copyFoamParticleCountToArgsBufferShader.SetBuffer("foamParticleCounts", GameManager.Ins.computeManager.foamParticleCounts, "CopyFoamParticleCountToArgsBuffer");

        foamParticle3DMaterial.enableInstancing = true;
        foamParticle3DMaterial.SetBuffer("foamParticleBuffer", computeManager.survivingFoamParticles);

        foamParticleBillboardMaterial.enableInstancing = true;
        foamParticleBillboardMaterial.SetBuffer("foamParticleBuffer", computeManager.survivingFoamParticles);

        FoamTex = ComputeHelper.CreateRenderTexture2D(int2(Screen.width, Screen.height), ComputeHelper.DepthMode.Depth16, UnityEngine.Experimental.Rendering.GraphicsFormat.R32G32B32A32_SFloat);

        //
        foamSpatialHashingShader = ComputeHelper.FindInResourceFolder("FoamSpatialHashing");
        foamSpatialHashingShader.SetBuffers(new[] {
            ("foamCellKeyEntries", computeManager.foamParticleCellKeyEntryBuffer),
            ("cellKeyToStartCoord", computeManager.foamCellKeyToStartCoordBuffer),
            ("updatingFoamParticles", computeManager.updatingFoamParticles),
            ("foamParticleCounts", computeManager.foamParticleCounts)
            },
            "UpdateSpatialHashEntries", "UpdateSpatialHashOffsets");
        foamSpatialHashingShader.SetInt("MaxFoamParticleCount", MaxFoamParticleCount);
        foamSpatialHashingShader.SetInt("FoamSpatialLookupSize", FoamSpatialLookupSize);
        foamSpatialHashingShader.SetFloat("FoamGridSize", FoamGridSize);
    }

    // TODO: mustt be called
    public void UniformParameters()
    {
        foamParticle3DMaterial.SetFloat("_Radius", ParticleRadius); // TODO: make radius global param
        foamParticleBillboardMaterial.SetFloat("FoamScaleMultiplier", FoamScaleMultiplier);
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

    public void RunSpatialHash()
    {
        // 1. Update keys
        ComputeHelper.Dispatch(foamSpatialHashingShader, MaxFoamParticleCount, 1, 1, "UpdateSpatialHashEntries");
        // 2. Sort
        //GameManager.Ins.foamParticleCountSorter.SortParticleEntries(); 
        // 3. Set offsets
        ComputeHelper.Dispatch(foamSpatialHashingShader, MaxFoamParticleCount, 1, 1, "UpdateSpatialHashOffsets");
    }

    public void DrawFoamTex(CommandBuffer cmd)
    {
        cmd.SetRenderTarget(FoamTex);
        cmd.ClearRenderTarget(true, true, new Color(0, 0, 100000)); // ~ 0 color, 0 Unity Depth (i think its reversed), 100000 Linear Depth
        cmd.DrawMeshInstancedIndirect(MeshUtils.QuadMesh, 0, foamParticleBillboardMaterial, 0, argsBuffer);
    }
}
