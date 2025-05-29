using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static SimulationParameters;
using Unity.Mathematics;
using static Unity.Mathematics.math;

public class PostProcessManager : MonoBehaviour
{
    public static PostProcessManager Ins { get; private set; }
    Material waterRaymarchMat;
    RenderTexture densityTexture;
    void Awake()
    {
        Ins = this;
        waterRaymarchMat = new(Shader.Find("Unlit/WaterRaymarch"));
    }

    public void SetupShaderUniforms()
    {
        waterRaymarchMat.SetBuffer("positions", GameManager.Ins.computeManager.positionBuffer);
        waterRaymarchMat.SetBuffer("densities", GameManager.Ins.computeManager.densityBuffer);
        waterRaymarchMat.SetBuffer("masses", GameManager.Ins.computeManager.massBuffer);

        waterRaymarchMat.SetBuffer("particleCellKeyEntries", GameManager.Ins.computeManager.particleCellKeyEntryBuffer);
        waterRaymarchMat.SetBuffer("cellKeyToStartCoord", GameManager.Ins.computeManager.cellKeyToStartCoordBuffer);

        waterRaymarchMat.SetInt("ParticleCount", ParticleCount);
        waterRaymarchMat.SetInt("SpatialLookupSize", SpatialLookupSize);
        waterRaymarchMat.SetFloat("GridSize", GridSize);
        waterRaymarchMat.SetFloat("SmoothingRadius", SmoothingRadius);
        waterRaymarchMat.SetFloat("SqrSmoothingRadius", SmoothingRadius * SmoothingRadius);
        waterRaymarchMat.SetFloat("InvSmoothingRadius", 1.0f / SmoothingRadius);

        //
        waterRaymarchMat.SetFloat("FovY", radians(MainCamera.fieldOfView));
        waterRaymarchMat.SetFloat("Aspect", MainCamera.aspect);

        //
        waterRaymarchMat.SetFloat("DensityMultiplier", DensityMultiplier);
        waterRaymarchMat.SetFloat("LightMultiplier", LightMultiplier);
        waterRaymarchMat.SetFloat("ExtinctionMultiplier", ExtinctionMultiplier);
        waterRaymarchMat.SetFloat("LightExtinctionMultiplier", LightExtinctionMultiplier);

    }

    public void UpdateCameraData()
    {
        waterRaymarchMat.SetVector("CamRi", MainCamera.transform.right);
        waterRaymarchMat.SetVector("CamUp", MainCamera.transform.up);
        waterRaymarchMat.SetVector("CamFo", MainCamera.transform.forward);

        waterRaymarchMat.SetVector("CamPos", MainCamera.transform.position);
    }

    public void UpdateContainerData()
    {
        waterRaymarchMat.SetMatrix("ContainerTransform", ContainerTransform);
        waterRaymarchMat.SetMatrix("ContainerInverseTransform", ContainerInverseTransform);
    }

    // void OnRenderImage(RenderTexture src, RenderTexture dest)
    // {
    //     Graphics.Blit(src, dest, waterRaymarchMat);
    // }

    public void CacheDensities()
    {
        int3 densitySampleCount = (int3)ceil(ContainerScale / DensityCacheStepSize);
        densityTexture = ComputeHelper.UpdateRenderTexture3D(densityTexture, densitySampleCount, UnityEngine.Experimental.Rendering.GraphicsFormat.R16_SFloat);

        GameManager.Ins.simUniformer.UniformDensityTexture(densityTexture, densitySampleCount);

        ComputeHelper.Dispatch(GameManager.Ins.computeManager.particleSimulatorShader, densitySampleCount.x, densitySampleCount.y, densitySampleCount.z, "CacheDensities");

        waterRaymarchMat.SetTexture("DensityTexture", densityTexture);
    }

    void Update()
    {
        if (transform.hasChanged)
        {
            transform.hasChanged = false;
            UpdateCameraData();
        }
    }
}
