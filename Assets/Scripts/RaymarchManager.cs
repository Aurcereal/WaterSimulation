using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static SimulationParameters;
using Unity.Mathematics;
using static Unity.Mathematics.math;

public class RaymarchManager : MonoBehaviour
{
    public static RaymarchManager Ins { get; private set; }
    Material waterRaymarchMat;
    RenderTexture densityTexture;
    void Awake()
    {
        Ins = this;
        waterRaymarchMat = new(Shader.Find("Unlit/WaterRaymarch"));
    }

    public void UniformAllParameters()
    {

        if (waterRaymarchMat == null) return;

        //
        waterRaymarchMat.SetFloat("FovY", radians(MainCamera.fieldOfView));
        waterRaymarchMat.SetFloat("Aspect", MainCamera.aspect);

        //
        waterRaymarchMat.SetFloat("DensityMultiplier", DensityMultiplier);
        waterRaymarchMat.SetFloat("LightMultiplier", LightMultiplier);
        waterRaymarchMat.SetVector("ExtinctionCoefficients", (Vector3)ExtinctionCoefficients);
        waterRaymarchMat.SetFloat("IndexOfRefraction", IndexOfRefraction);
        waterRaymarchMat.SetVector("LightDir", (Vector3)LightDir);
        waterRaymarchMat.SetInt("NumBounces", NumBounces);
        waterRaymarchMat.SetInt("TraceReflectAndRefract", TraceReflectAndRefract ? 1 : 0);
        waterRaymarchMat.SetFloat("WaterExistenceThreshold", WaterExistenceThreshold);
        waterRaymarchMat.SetFloat("WaterExistenceEps", WaterExistenceEps);
        waterRaymarchMat.SetFloat("NextRayOffset", NextRayOffset);

        waterRaymarchMat.SetInt("ObstacleType", ObstacleType ? 1 : 0);

        //
        waterRaymarchMat.SetTexture("EnvironmentMap", EnvironmentMap);

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
        waterRaymarchMat.SetVector("ContainerScale", (Vector3) ContainerScale);
    }

    public void UpdateObstacleData()
    {
        waterRaymarchMat.SetMatrix("ObstacleInverseTransform", ObstacleInverseTransform);
        waterRaymarchMat.SetVector("ObstacleScale", (Vector3) ObstacleScale);
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        Graphics.Blit(null, dest, waterRaymarchMat);
    }

    public void CacheDensities()
    {
        int3 densitySampleCount = int3(DensityCacheSampleCount); //UseDensityStepSize ? (int3)ceil(ContainerScale / DensityCacheStepSize) : int3(DensityCacheSampleCount);
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
