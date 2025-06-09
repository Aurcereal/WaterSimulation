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

    int ScreenWidth => ResolutionTracker.ScreenWidth;
    int ScreenHeight => ResolutionTracker.ScreenHeight;

    RenderTexture testTex;
    CommandBuffer commandBuffer;
    Material testBlit;

    //
    Material particle3DMaterial = new Material(Shader.Find("Unlit/ParticleDebug"));
    Material particleSphereDepthMaterial = new Material(Shader.Find("Unlit/ParticleSphereDepth"));

    Material depthTextureToNormals = new Material(Shader.Find("Unlit/ParticleNormalFromDepth"));

    void UpdateGlobalScreenSizeUniform(int2 newSize)
    {
        Shader.SetGlobalInt("ScreenWidth", ScreenWidth);
        Shader.SetGlobalInt("ScreenHeight", ScreenHeight);
    }

    public ScreenSpaceWaterManager()
    {
        particle3DMaterial.enableInstancing = true;
        particle3DMaterial.SetBuffer("positionBuffer", GameManager.Ins.computeManager.positionBuffer);
        particle3DMaterial.SetBuffer("colorBuffer", GameManager.Ins.computeManager.colorBuffer);

        particleSphereDepthMaterial.enableInstancing = true;
        particleSphereDepthMaterial.SetBuffer("positionBuffer", GameManager.Ins.computeManager.positionBuffer);
        particleSphereDepthMaterial.SetBuffer("colorBuffer", GameManager.Ins.computeManager.colorBuffer);

        commandBuffer = new();
        testTex = ComputeHelper.CreateRenderTexture2D(int2(Screen.width, Screen.height), ComputeHelper.DepthMode.Depth16, UnityEngine.Experimental.Rendering.GraphicsFormat.R32_SFloat);
        testBlit = new Material(Shader.Find("Unlit/TestBlit"));

        UniformParameters();

        ResolutionTracker.ResolutionChangeEvent += UpdateGlobalScreenSizeUniform;
        UpdateGlobalScreenSizeUniform(int2(ScreenWidth, ScreenHeight));
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
        commandBuffer.ClearRenderTarget(true, true, Color.white * 100000.0f); // We're using distAlongCam so have to make initial depth very large
        commandBuffer.DrawMeshInstancedProcedural(MeshUtils.QuadMesh, 0, particleSphereDepthMaterial, 0, ParticleCount);
        commandBuffer.Blit(testTex, MainCamera.targetTexture, depthTextureToNormals);
    }
}
