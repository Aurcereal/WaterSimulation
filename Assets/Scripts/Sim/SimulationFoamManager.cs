using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Mathematics;
using static Unity.Mathematics.math;

using static SimulationParameters;
using UnityEngine.Rendering;

public class SimulationFoamManager
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

    public SimulationFoamManager()
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

    int a = 0;
    public void RunSpatialHash()
    {
        // 1. Update keys
        ComputeHelper.Dispatch(foamSpatialHashingShader, MaxFoamParticleCount, 1, 1, "UpdateSpatialHashEntries");

        // 2. Sort
        GameManager.Ins.foamParticleCountSorter.SortParticleEntries();
        // 3. Set offsets
        ComputeHelper.Dispatch(foamSpatialHashingShader, MaxFoamParticleCount, 1, 1, "UpdateSpatialHashOffsets");
        ++a;
        if (a == 30) { loggspatialhashhh(); a = 0; }
        // check if the sorting and spatial offsets are valid and stuff
    }

    public void DrawFoamTex(CommandBuffer cmd)
    {
        cmd.SetRenderTarget(FoamTex);
        cmd.ClearRenderTarget(true, true, new Color(0, 0, 100000)); // ~ 0 color, 0 Unity Depth (i think its reversed), 100000 Linear Depth
        cmd.DrawMeshInstancedIndirect(MeshUtils.QuadMesh, 0, foamParticleBillboardMaterial, 0, argsBuffer);
    }

    void logggAHH()
    {
        ParticleEntry[] entries = new ParticleEntry[MaxFoamParticleCount];
        GameManager.Ins.computeManager.foamParticleCellKeyEntryBuffer.GetData(entries);
        for (int i = 0; i < 10; i++)
        {
            Debug.Log($"{i}, Index: {entries[i].particleIndex}, Key: {entries[i].cellKey}");
        }
    }

    void logSort()
    {
        ParticleEntry[] entries = new ParticleEntry[MaxFoamParticleCount];
        GameManager.Ins.computeManager.foamParticleCellKeyEntryBuffer.GetData(entries);

        int[] counts = new int[2];
        GameManager.Ins.computeManager.foamParticleCounts.GetData(counts);

        int sortFails = 0;

        for (int i = 1; i < counts[0]; i++)
        {
            if (entries[i - 1].cellKey > entries[i].cellKey)
            {
                Debug.Log($"Old: ({entries[i - 1].particleIndex}, {entries[i - 1].cellKey}), New: ({entries[i].particleIndex}, {entries[i].cellKey})");
                ++sortFails;
            }
        }

        Debug.Log($"Sort Fails: {sortFails} Sort success: {counts[0]-sortFails}");
    }

    void loggspatialhashhh()
    {
        int[] offsets = new int[FoamSpatialLookupSize];
        GameManager.Ins.computeManager.foamCellKeyToStartCoordBuffer.GetData(offsets);
        ParticleEntry[] entries = new ParticleEntry[MaxFoamParticleCount];
        GameManager.Ins.computeManager.foamParticleCellKeyEntryBuffer.GetData(entries);

        int[] counts = new int[2];
        GameManager.Ins.computeManager.foamParticleCounts.GetData(counts);

        int found = 0;

        for (int key = 0; key < FoamSpatialLookupSize; key++)
        {
            int currIndex = offsets[key];
            while (currIndex >= 0 && currIndex < counts[0] && entries[currIndex].cellKey == key)
            {
                ++found;
                ++currIndex;
            }
        }

        Debug.Log($"Found: {found}, Count: {counts[0]}");

    }
}
