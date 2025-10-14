using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static SimulationParameters;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using UnityEngine.Rendering;

public class RaymarchManager : MonoBehaviour
{
    public static RaymarchManager Ins { get; private set; }
    Material waterRaymarchMat;

    RenderTexture densityTexture;

    CommandBuffer commandBuffer;

    void Awake()
    {
        Ins = this;
        waterRaymarchMat = new(Shader.Find("Unlit/WaterRaymarch"));

        //
        commandBuffer = new();

    }

    bool? billboardFoamFeature = null;
    bool? raymarchFoamFeature = null;
    bool? causticsFeature = null;
    bool? shadowsFeature = null;
    public void UniformAllParameters()
    {

        if (waterRaymarchMat == null) return;

        //
        waterRaymarchMat.SetFloat("DensityMultiplier", RaymarchDensityMultiplier);
        waterRaymarchMat.SetFloat("LightMultiplier", RaymarchLightMultiplier);
        waterRaymarchMat.SetFloat("SkyboxLightMultiplier", SkyboxLightMultiplier);
        waterRaymarchMat.SetVector("ExtinctionCoefficients", (Vector3)RaymarchExtinctionCoefficients);
        waterRaymarchMat.SetFloat("IndexOfRefraction", RaymarchIndexOfRefraction);
        waterRaymarchMat.SetVector("LightDir", (Vector3)LightDir);
        waterRaymarchMat.SetInt("NumBounces", NumBounces);
        waterRaymarchMat.SetInt("TraceReflectAndRefract", TraceReflectAndRefract ? 1 : 0);
        waterRaymarchMat.SetFloat("WaterExistenceThreshold", WaterExistenceThreshold);
        waterRaymarchMat.SetFloat("WaterExistenceEps", WaterExistenceEps);
        waterRaymarchMat.SetFloat("NextRayOffset", NextRayOffset);

        waterRaymarchMat.SetInt("ObstacleType", ObstacleType ? 1 : 0);

        //
        waterRaymarchMat.SetTexture("EnvironmentMap", EnvironmentMap);
        waterRaymarchMat.SetTexture("FoamTex", GameManager.Ins.simFoamManager.FoamTex);

        //
        waterRaymarchMat.SetInteger("UseShadows", UseShadows ? 1 : 0);

        //
        GameManager.Ins.simFoamManager.UniformParameters();

        ///
        waterRaymarchMat.SetBuffer("cellKeyToStartCoord", GameManager.Ins.computeManager.foamCellKeyToStartCoordBuffer);
        waterRaymarchMat.SetBuffer("foamParticleEntries", GameManager.Ins.computeManager.foamParticleCellKeyEntryBuffer);
        waterRaymarchMat.SetBuffer("updatingFoamParticles", GameManager.Ins.computeManager.updatingFoamParticles);
        waterRaymarchMat.SetBuffer("foamParticleCounts", GameManager.Ins.computeManager.foamParticleCounts);

        waterRaymarchMat.SetFloat("FoamVolumeRadius", FoamVolumeRadius);
        waterRaymarchMat.SetInt("FoamSpatialLookupSize", FoamSpatialLookupSize);
        waterRaymarchMat.SetFloat("FoamGridSize", FoamGridSize);

        waterRaymarchMat.SetInt("UseRaymarchedFoam", UseRaymarchedFoam ? 1 : 0);
        waterRaymarchMat.SetInt("UseBillboardFoam", UseBillboardFoam ? 1 : 0);

        //
        waterRaymarchMat.SetInt("UseCaustics", UseCaustics ? 1 : 0);

        // Sun
        waterRaymarchMat.SetVector("SunDir", SunDir);
        waterRaymarchMat.SetFloat("SunRadius", SunRadius);
        waterRaymarchMat.SetFloat("SunMultiplier", SunMultiplier);

        //
        waterRaymarchMat.SetFloat("DebugFloat", DebugFloat);
        waterRaymarchMat.SetVector("DebugVector", DebugVector);

        //
        if (billboardFoamFeature != UseBillboardFoam) waterRaymarchMat.SetKeywordActive("BILLBOARD_FOAM", UseBillboardFoam);
        if (raymarchFoamFeature != UseRaymarchedFoam) waterRaymarchMat.SetKeywordActive("RAYMARCHED_FOAM", UseBillboardFoam);
        if (causticsFeature != UseCaustics) waterRaymarchMat.SetKeywordActive("CAUSTICS", UseCaustics);
        if (shadowsFeature != UseShadows) waterRaymarchMat.SetKeywordActive("SHADOWS", UseShadows);

        //
        billboardFoamFeature = UseBillboardFoam;
        raymarchFoamFeature = UseRaymarchedFoam;
        causticsFeature = UseCaustics;
        shadowsFeature = UseShadows;

    }

    public void UpdateContainerData()
    {
        Shader.SetGlobalMatrix("ContainerTransform", ContainerTransform);
        Shader.SetGlobalMatrix("ContainerInverseTransform", ContainerInverseTransform);
        Shader.SetGlobalVector("ContainerScale", (Vector3)ContainerScale);
    }

    public void UpdateObstacleData()
    {
        waterRaymarchMat.SetMatrix("ObstacleInverseTransform", ObstacleInverseTransform);
        waterRaymarchMat.SetVector("ObstacleScale", (Vector3)ObstacleScale);
    }

    public void OnEnable()
    {
        MainCamera.RemoveAllCommandBuffers();
        MainCamera.AddCommandBuffer(CameraEvent.AfterEverything, commandBuffer);
        MainCamera.depthTextureMode = DepthTextureMode.Depth;
    }

    public void OnDisable()
    {
        MainCamera.RemoveAllCommandBuffers();
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        Graphics.Blit(null, dest, waterRaymarchMat);
    }

    public void DrawFoam()
    {
        commandBuffer.Clear();

        GameManager.Ins.simFoamManager.DrawFoamTex(commandBuffer);
    }

    public void CacheDensities()
    {
        int3 densitySampleCount = int3(DensityCacheSampleCount); //UseDensityStepSize ? (int3)ceil(ContainerScale / DensityCacheStepSize) : int3(DensityCacheSampleCount);
        densityTexture = ComputeHelper.UpdateRenderTexture3D(densityTexture, densitySampleCount, UnityEngine.Experimental.Rendering.GraphicsFormat.R16_SFloat);

        GameManager.Ins.simUniformer.UniformDensityTexture(densityTexture, densitySampleCount);

        ComputeHelper.Dispatch(GameManager.Ins.computeManager.particleSimulatorShader, densitySampleCount.x, densitySampleCount.y, densitySampleCount.z, "CacheDensities");

        waterRaymarchMat.SetTexture("DensityTexture", densityTexture);
    }

    string currCompileMacro;
    public void HandleNewEnv()
    {
        if (currCompileMacro != null) waterRaymarchMat.DisableKeyword(currCompileMacro);
        waterRaymarchMat.EnableKeyword(EnvPreset.visualCompileKeyword);
    }
}
