using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;
using UnityEngine.Rendering;

using static SimulationParameters;

public class DebugRenderingManager
{

    //
    Material particle3DMaterial = new Material(Shader.Find("Unlit/ParticleDebug"));
    Material foamParticle3DMaterial = new Material(Shader.Find("Unlit/FoamParticleDebug"));

    //
    CommandBuffer commandBuffer;

    public DebugRenderingManager()
    {
        particle3DMaterial.enableInstancing = true;
        particle3DMaterial.SetBuffer("positionBuffer", GameManager.Ins.computeManager.positionBuffer);
        particle3DMaterial.SetBuffer("colorBuffer", GameManager.Ins.computeManager.colorBuffer);

        foamParticle3DMaterial.enableInstancing = true;
        foamParticle3DMaterial.SetBuffer("foamParticleBuffer", GameManager.Ins.computeManager.survivingFoamParticles);

        commandBuffer = new();

        UniformParametersAndTextures();

    }

    public void UniformParametersAndTextures()
    {
        particle3DMaterial.SetFloat("_Radius", ParticleRadius);
        foamParticle3DMaterial.SetFloat("_FoamRadius", FoamScaleMultiplier);

        //
        GameManager.Ins.simFoamManager.UniformParameters();

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

    public void Draw()
    {
        commandBuffer.Clear();

        commandBuffer.SetRenderTarget(MainCamera.targetTexture);
        commandBuffer.ClearRenderTarget(true, true, Color.black);
        commandBuffer.DrawMeshInstancedProcedural(MeshUtils.QuadMesh, 0, particle3DMaterial, 0, ParticleCount);
        commandBuffer.DrawMeshInstancedIndirect(MeshUtils.QuadMesh, 0, foamParticle3DMaterial, 0, GameManager.Ins.simFoamManager.ArgsBuffer);
    }
}
