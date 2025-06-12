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

    //
    Material particle3DMaterial = new Material(Shader.Find("Unlit/ParticleDebug"));
    Material particleSphereDepthMaterial = new Material(Shader.Find("Unlit/ParticleSphereDepth"));
    Material particleAdditiveDensity = new Material(Shader.Find("Unlit/ParticleAdditiveDensity"));

    Material depthTextureToNormals = new Material(Shader.Find("Unlit/NormalFromDepth"));

    //
    public GaussianBlurManager blurManager;

    //
    CommandBuffer commandBuffer;

    //
    RenderTexture depthTex;
    RenderTexture scratchScreenRTex;
    RenderTexture scratchScreenRGBATex;
    RenderTexture smoothedDepthTex;
    RenderTexture normalTex;

    RenderTexture densityTex;
    float densityTexResolutionPercentage => .25f;

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

        particleAdditiveDensity.enableInstancing = true;
        particleAdditiveDensity.SetBuffer("positionBuffer", GameManager.Ins.computeManager.positionBuffer);
        particleAdditiveDensity.SetBuffer("colorBuffer", GameManager.Ins.computeManager.colorBuffer);

        commandBuffer = new();
        blurManager = new();

        depthTex = ComputeHelper.CreateRenderTexture2D(int2(Screen.width, Screen.height), ComputeHelper.DepthMode.Depth16, UnityEngine.Experimental.Rendering.GraphicsFormat.R32_SFloat);
        scratchScreenRTex = ComputeHelper.CreateRenderTexture2D(int2(Screen.width, Screen.height), ComputeHelper.DepthMode.Depth16, UnityEngine.Experimental.Rendering.GraphicsFormat.R32_SFloat);
        scratchScreenRGBATex = ComputeHelper.CreateRenderTexture2D(int2(Screen.width, Screen.height), ComputeHelper.DepthMode.Depth16, UnityEngine.Experimental.Rendering.GraphicsFormat.R32G32B32A32_SFloat);
        smoothedDepthTex = ComputeHelper.CreateRenderTexture2D(int2(Screen.width, Screen.height), ComputeHelper.DepthMode.Depth16, UnityEngine.Experimental.Rendering.GraphicsFormat.R32_SFloat);
        normalTex = ComputeHelper.CreateRenderTexture2D(int2(Screen.width, Screen.height), ComputeHelper.DepthMode.Depth16, UnityEngine.Experimental.Rendering.GraphicsFormat.R32G32B32A32_SFloat);

        // TODO: take out depth tex see if it needs it
        densityTex = ComputeHelper.CreateRenderTexture2D((int2) (densityTexResolutionPercentage * float2(Screen.width, Screen.height)), ComputeHelper.DepthMode.Depth16, UnityEngine.Experimental.Rendering.GraphicsFormat.R32_SFloat);

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

        // Draw particle depths
        commandBuffer.SetRenderTarget(depthTex);
        commandBuffer.ClearRenderTarget(true, true, Color.white * 100000.0f); // We're using distAlongCam so have to make initial depth very large
        commandBuffer.DrawMeshInstancedProcedural(MeshUtils.QuadMesh, 0, particleSphereDepthMaterial, 0, ParticleCount);

        // Draw thickness/density texture
        commandBuffer.SetRenderTarget(densityTex);
        commandBuffer.ClearRenderTarget(true, true, Color.black);
        commandBuffer.DrawMeshInstancedProcedural(MeshUtils.QuadMesh, 0, particleAdditiveDensity, 0, ParticleCount);

        // Blur depth tex
        blurManager.Blur(commandBuffer, depthTex, scratchScreenRTex, smoothedDepthTex);

        // Use smooth depth to get normals
        commandBuffer.Blit(smoothedDepthTex, normalTex, depthTextureToNormals);

        //
        commandBuffer.Blit(densityTex, MainCamera.targetTexture);

    }
}
