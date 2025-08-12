using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;
using UnityEngine.Rendering;

using static SimulationParameters;

public class ScreenSpaceWaterManager
{

    int ScreenWidth => ResolutionTracker.ScreenWidth;
    int ScreenHeight => ResolutionTracker.ScreenHeight;

    //
    Material particle3DMaterial = new Material(Shader.Find("Unlit/ParticleDebug"));
    public Material particleSphereDepthMaterial = new Material(Shader.Find("Unlit/ParticleSphereDepth"));
    public Material particleAdditiveDensityMaterial = new Material(Shader.Find("Unlit/ParticleAdditiveDensity"));
    // ODOT make material manager that sets all buffers and enable instancing and stuff No it's good to own materials for now

    public Material depthTextureToNormals = new Material(Shader.Find("Unlit/NormalFromDepth"));
    Material compositeIntoWater = new Material(Shader.Find("Unlit/CompositeIntoWater"));

    Material copyDepthMaterial = new Material(Shader.Find("Unlit/CopyDepth"));

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
    float densityTexResolutionPercentage => 1f; // .25

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

        particleAdditiveDensityMaterial.enableInstancing = true;
        particleAdditiveDensityMaterial.SetBuffer("positionBuffer", GameManager.Ins.computeManager.positionBuffer);
        particleAdditiveDensityMaterial.SetBuffer("colorBuffer", GameManager.Ins.computeManager.colorBuffer);

        commandBuffer = new();
        blurManager = new(DepthWorldBlurRadius);

        depthTex = ComputeHelper.CreateRenderTexture2D(int2(Screen.width, Screen.height), ComputeHelper.DepthMode.Depth16, UnityEngine.Experimental.Rendering.GraphicsFormat.R32_SFloat);
        scratchScreenRTex = ComputeHelper.CreateRenderTexture2D(int2(Screen.width, Screen.height), ComputeHelper.DepthMode.Depth16, UnityEngine.Experimental.Rendering.GraphicsFormat.R32_SFloat);
        scratchScreenRGBATex = ComputeHelper.CreateRenderTexture2D(int2(Screen.width, Screen.height), ComputeHelper.DepthMode.Depth16, UnityEngine.Experimental.Rendering.GraphicsFormat.R32G32B32A32_SFloat);
        smoothedDepthTex = ComputeHelper.CreateRenderTexture2D(int2(Screen.width, Screen.height), ComputeHelper.DepthMode.Depth16, UnityEngine.Experimental.Rendering.GraphicsFormat.R32_SFloat);
        normalTex = ComputeHelper.CreateRenderTexture2D(int2(Screen.width, Screen.height), ComputeHelper.DepthMode.Depth16, UnityEngine.Experimental.Rendering.GraphicsFormat.R32G32B32A32_SFloat);

        // TODO: take out depth tex see if it needs it
        densityTex = ComputeHelper.CreateRenderTexture2D((int2)(densityTexResolutionPercentage * float2(Screen.width, Screen.height)), ComputeHelper.DepthMode.Depth16, UnityEngine.Experimental.Rendering.GraphicsFormat.R32_SFloat);

        UniformParametersAndTextures();

        ResolutionTracker.ResolutionChangeEvent += UpdateGlobalScreenSizeUniform;
        UpdateGlobalScreenSizeUniform(int2(ScreenWidth, ScreenHeight));

    }

    public void UniformParametersAndTextures()
    {
        particle3DMaterial.SetFloat("_Radius", ParticleRadius);
        particleSphereDepthMaterial.SetFloat("_Radius", ParticleRadius);
        particleAdditiveDensityMaterial.SetFloat("_Radius", ParticleRadius);


        depthTextureToNormals.SetFloat("DepthDifferenceCutoff", DepthDifferenceCutoffForNormals);

        compositeIntoWater.SetTexture("SmoothedDepthTex", smoothedDepthTex);
        compositeIntoWater.SetTexture("NormalTex", normalTex);
        compositeIntoWater.SetTexture("DensityTex", densityTex);
        compositeIntoWater.SetTexture("FoamTex", GameManager.Ins.simFoamManager.FoamTex);
        compositeIntoWater.SetTexture("EnvironmentMap", EnvironmentMap);

        //
        compositeIntoWater.SetFloat("DensityMultiplier", ScreenSpaceDensityMultiplier);
        compositeIntoWater.SetFloat("LightMultiplier", ScreenSpaceLightMultiplier);
        compositeIntoWater.SetVector("ExtinctionCoefficients", (Vector3)ScreenSpaceExtinctionCoefficients);
        compositeIntoWater.SetFloat("IndexOfRefraction", IndexOfRefraction);
        compositeIntoWater.SetVector("LightDir", (Vector3)LightDir);

        compositeIntoWater.SetInt("ObstacleType", ObstacleType ? 1 : 0);

        //
        compositeIntoWater.SetFloat("UseBillboardFoam", UseBillboardFoam ? 1 : 0);

        // TODO: maybe make it so we can change shadowcam view during sim for demo vid
        compositeIntoWater.SetInteger("UseShadowMapping", UseShadows ? 1 : 0);
        compositeIntoWater.SetMatrix("ShadowCamVP", GameManager.Ins.shadowMapManager.ShadowCamVP);
        compositeIntoWater.SetTexture("DensityFromSunTex", GameManager.Ins.shadowMapManager.DensityFromSunTex);

        // Caustics
        compositeIntoWater.SetInt("UseCaustics", UseCaustics ? 1 : 0);
        compositeIntoWater.SetVector("CausticCamPosition", (Vector3) GameManager.Ins.causticsManager.CausticCamPosition);
        compositeIntoWater.SetMatrix("CausticCamVP", GameManager.Ins.causticsManager.CausticsCamVP);
        compositeIntoWater.SetTexture("DepthFromCausticCam", GameManager.Ins.causticsManager.SmoothedDepthTex);
        compositeIntoWater.SetTexture("NormalFromCausticCam", GameManager.Ins.causticsManager.NormalTex);

        //
        GameManager.Ins.simFoamManager.UniformParameters();

    }

    public void ResetGaussianKernels()
    {
        blurManager.CreateAndSetupGaussianKernel(DepthWorldBlurRadius);
        GameManager.Ins.causticsManager.blurManager.CreateAndSetupGaussianKernel(CausticsDepthWorldBlurRadius);
    }

    public void UpdateObstacleData()
    {
        compositeIntoWater.SetMatrix("ObstacleInverseTransform", ObstacleInverseTransform);
        compositeIntoWater.SetVector("ObstacleScale", (Vector3)ObstacleScale);
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

        // Draw particle depths
        commandBuffer.SetRenderTarget(depthTex);
        commandBuffer.ClearRenderTarget(true, true, Color.white * 100000.0f); // We're using distAlongCam so have to make initial depth very large
        commandBuffer.DrawMeshInstancedProcedural(MeshUtils.QuadMesh, 0, particleSphereDepthMaterial, 0, ParticleCount);

        // Draw foam particles
        if(UseBillboardFoam) GameManager.Ins.simFoamManager.DrawFoamTex(commandBuffer);

        // Draw thickness/density texture
        commandBuffer.SetRenderTarget(densityTex);
        commandBuffer.ClearRenderTarget(true, true, Color.black);
        if(UseBillboardFoam) commandBuffer.Blit(GameManager.Ins.simFoamManager.FoamTex, densityTex, copyDepthMaterial);
        commandBuffer.DrawMeshInstancedProcedural(MeshUtils.QuadMesh, 0, particleAdditiveDensityMaterial, 0, ParticleCount);

        // Blur depth tex
        blurManager.Blur(commandBuffer, depthTex, scratchScreenRTex, smoothedDepthTex, DepthBlurIterationCount);

        // Use smooth depth to get normals
        commandBuffer.Blit(smoothedDepthTex, normalTex, depthTextureToNormals);

        //
        commandBuffer.Blit(null, MainCamera.targetTexture, compositeIntoWater);
    }

    public void DebugDrawSpheres()
    {
        commandBuffer.Clear();

        commandBuffer.SetRenderTarget(MainCamera.targetTexture);
        commandBuffer.ClearRenderTarget(true, true, Color.black);
        commandBuffer.DrawMeshInstancedProcedural(MeshUtils.QuadMesh, 0, particle3DMaterial, 0, ParticleCount);
    }
}
