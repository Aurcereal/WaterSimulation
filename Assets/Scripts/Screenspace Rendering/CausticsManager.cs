using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using static SimulationParameters;
using static Unity.Mathematics.math;

public class CausticsManager
{
    // TODO: rn relies on screenspacemanager being initialized since it has all materials we need so make sure that's the case
    public Matrix4x4 CausticsCamVP => CausticsVerticalCamera.projectionMatrix * CausticsVerticalCamera.worldToCameraMatrix;
    public float3 CausticCamPosition => CausticsVerticalCamera.transform.position;
    public float CausticCamSize => CausticsVerticalCamera.orthographicSize;

    CommandBuffer cmd;

    RenderTexture depthTex;
    RenderTexture scratchRTex;
    public RenderTexture SmoothedDepthTex { get; private set; }
    public RenderTexture NormalTex { get; private set; }

    public GaussianBlurManager blurManager;

    Material depthTextureToNormalsOrthoCamera;

    int2 texDimensions;

    public CausticsManager()
    {
        cmd = new();

        // TODO: replace .cameraAspect with script controlled aspect
        texDimensions = int2((int)(CausticsDepthNormalResolution * CausticsVerticalCamera.aspect), CausticsDepthNormalResolution);

        depthTex = ComputeHelper.CreateRenderTexture2D(texDimensions, ComputeHelper.DepthMode.Depth16, UnityEngine.Experimental.Rendering.GraphicsFormat.R32_SFloat);
        scratchRTex = ComputeHelper.CreateRenderTexture2D(texDimensions, ComputeHelper.DepthMode.Depth16, UnityEngine.Experimental.Rendering.GraphicsFormat.R32_SFloat);
        SmoothedDepthTex = ComputeHelper.CreateRenderTexture2D(texDimensions, ComputeHelper.DepthMode.Depth16, UnityEngine.Experimental.Rendering.GraphicsFormat.R32_SFloat);
        NormalTex = ComputeHelper.CreateRenderTexture2D(texDimensions, ComputeHelper.DepthMode.Depth16, UnityEngine.Experimental.Rendering.GraphicsFormat.R32G32B32A32_SFloat);

        blurManager = new(CausticsDepthWorldBlurRadius);

        depthTextureToNormalsOrthoCamera = new(Shader.Find("Unlit/NormalFromDepthOrthoCam"));

        UniformParameters();
    }

    // TODO: gotta do the ball and blur thing it's pretty much same as screenspace from diff angle and less
    public void OnEnable()
    {
        CausticsVerticalCamera.RemoveAllCommandBuffers();
        CausticsVerticalCamera.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, cmd);
        CausticsVerticalCamera.depthTextureMode = DepthTextureMode.Depth;
    }

    public void OnDisable()
    {
        CausticsVerticalCamera.RemoveAllCommandBuffers();
    }

    public void UniformParameters()
    {
        depthTextureToNormalsOrthoCamera.SetFloat("Size", CausticCamSize);
        depthTextureToNormalsOrthoCamera.SetFloat("CausticAspect", CausticsVerticalCamera.aspect);

        depthTextureToNormalsOrthoCamera.SetVector("CausticCamRi", CausticsVerticalCamera.transform.right);
        depthTextureToNormalsOrthoCamera.SetVector("CausticCamUp", CausticsVerticalCamera.transform.up);
        depthTextureToNormalsOrthoCamera.SetVector("CausticCamFo", CausticsVerticalCamera.transform.forward);

        depthTextureToNormalsOrthoCamera.SetVector("CausticCamPos", CausticsVerticalCamera.transform.position);

        depthTextureToNormalsOrthoCamera.SetInt("TextureWidth", texDimensions.x);
        depthTextureToNormalsOrthoCamera.SetInt("TextureHeight", texDimensions.y);

        depthTextureToNormalsOrthoCamera.SetFloat("DepthDifferenceCutoff", DepthDifferenceCutoffForNormals);
    }

    public void DrawTextures()
    {
        cmd.Clear();

        // Draw particle depths
        cmd.SetRenderTarget(depthTex);
        cmd.ClearRenderTarget(true, true, Color.white * 100000.0f); // We're using distAlongCam not normalized depth so have to make initial depth very large
        cmd.DrawMeshInstancedProcedural(MeshUtils.QuadMesh, 0, GameManager.Ins.screenSpaceManager.particleSphereDepthMaterial, 0, ParticleCount);

        // Blur depth tex
        blurManager.Blur(cmd, depthTex, scratchRTex, SmoothedDepthTex, CausticsDepthBlurIterationCount);

        // Use smooth depth to get normals
        cmd.Blit(SmoothedDepthTex, NormalTex, depthTextureToNormalsOrthoCamera);
    }
}
