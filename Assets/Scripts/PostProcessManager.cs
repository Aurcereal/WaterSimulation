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
        waterRaymarchMat.SetMatrix("ContainerInverseTransform", ContainerInverseTransform);
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        Graphics.Blit(src, dest, waterRaymarchMat);
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
