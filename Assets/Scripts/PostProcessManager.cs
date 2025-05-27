using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static SimulationParameters;

public class PostProcessManager : MonoBehaviour
{
    Material waterRaymarchMat;
    void Awake()
    {
        waterRaymarchMat = new(Shader.Find("Unlit/WaterRaymarch"));
    }

    void Start()
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
        waterRaymarchMat.SetFloat("SqrSmoothingRadius", SmoothingRadius*SmoothingRadius);
        waterRaymarchMat.SetFloat("InvSmoothingRadius", 1.0f/SmoothingRadius);
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        Graphics.Blit(src, dest, waterRaymarchMat);
    }
}
