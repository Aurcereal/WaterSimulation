using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

using Unity.Mathematics;
using static Unity.Mathematics.math;
using UnityEngine.Rendering;

using static SimulationParameters;

public class ScreenSpaceWaterManager
{

    int ScreenWidth => Screen.width;
    int ScreenHeight => Screen.height;

    RenderTexture testTex;
    CommandBuffer commandBuffer;
    Material testBlit;

    //
    Material particle3DMaterial = new Material(Shader.Find("Unlit/ParticleDebug"));
    Material particleSphereDepthMaterial = new Material(Shader.Find("Unlit/ParticleSphereDepth"));

    public ScreenSpaceWaterManager()
    {
        particle3DMaterial.enableInstancing = true;
        particle3DMaterial.SetBuffer("positionBuffer", GameManager.Ins.computeManager.positionBuffer);
        particle3DMaterial.SetBuffer("colorBuffer", GameManager.Ins.computeManager.colorBuffer);

        particleSphereDepthMaterial.enableInstancing = true;
        particleSphereDepthMaterial.SetBuffer("positionBuffer", GameManager.Ins.computeManager.positionBuffer);
        particleSphereDepthMaterial.SetBuffer("colorBuffer", GameManager.Ins.computeManager.colorBuffer);

        commandBuffer = new();
        testTex = ComputeHelper.CreateRenderTexture2D(int2(Screen.width, Screen.height), ComputeHelper.DepthMode.Depth16);
        testBlit = new Material(Shader.Find("Unlit/TestBlit"));

        UniformParameters();
    }

    public void UniformParameters()
    {
        particle3DMaterial.SetFloat("_Radius", ParticleRadius);
        particleSphereDepthMaterial.SetFloat("_Radius", ParticleRadius);
    }

    public void Draw()
    {
        MainCamera.RemoveAllCommandBuffers();
        MainCamera.AddCommandBuffer(CameraEvent.AfterEverything, commandBuffer);
        MainCamera.depthTextureMode = DepthTextureMode.Depth;

        commandBuffer.Clear();
        commandBuffer.SetRenderTarget(testTex);
        commandBuffer.ClearRenderTarget(true, true, Color.black);
        commandBuffer.DrawMeshInstancedProcedural(MeshUtils.QuadMesh, 0, particleSphereDepthMaterial, 0, ParticleCount);
        commandBuffer.Blit(testTex, MainCamera.targetTexture, testBlit);
    }
}
