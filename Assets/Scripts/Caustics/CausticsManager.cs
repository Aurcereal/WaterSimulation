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

    CommandBuffer cmd;

    RenderTexture depthTex;
    RenderTexture scratchRTex;
    public RenderTexture SmoothedDepthTex { get; private set; }
    public RenderTexture NormalTex { get; private set; }

    GaussianBlurManager blurManager;

    public CausticsManager()
    {
        cmd = new();

        depthTex = ComputeHelper.CreateRenderTexture2D(int2((int)(CausticsDepthNormalResolution * CausticsVerticalCamera.aspect), CausticsDepthNormalResolution), ComputeHelper.DepthMode.Depth16, UnityEngine.Experimental.Rendering.GraphicsFormat.R32_SFloat);
        scratchRTex = ComputeHelper.CreateRenderTexture2D(int2((int)(CausticsDepthNormalResolution * CausticsVerticalCamera.aspect), CausticsDepthNormalResolution), ComputeHelper.DepthMode.Depth16, UnityEngine.Experimental.Rendering.GraphicsFormat.R32_SFloat);
        SmoothedDepthTex = ComputeHelper.CreateRenderTexture2D(int2((int)(CausticsDepthNormalResolution * CausticsVerticalCamera.aspect), CausticsDepthNormalResolution), ComputeHelper.DepthMode.Depth16, UnityEngine.Experimental.Rendering.GraphicsFormat.R32_SFloat);
        NormalTex = ComputeHelper.CreateRenderTexture2D(int2((int)(CausticsDepthNormalResolution * CausticsVerticalCamera.aspect), CausticsDepthNormalResolution), ComputeHelper.DepthMode.Depth16, UnityEngine.Experimental.Rendering.GraphicsFormat.R32G32B32A32_SFloat);

        blurManager = new();
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

    public void DrawTextures()
    {
        cmd.Clear();

        // Draw particle depths
        cmd.SetRenderTarget(depthTex);
        cmd.ClearRenderTarget(true, true, Color.white * 100000.0f); // We're using distAlongCam not normalized depth so have to make initial depth very large
        cmd.DrawMeshInstancedProcedural(MeshUtils.QuadMesh, 0, GameManager.Ins.screenSpaceManager.particleSphereDepthMaterial, 0, ParticleCount);

        // Blur depth tex
        blurManager.Blur(cmd, depthTex, scratchRTex, SmoothedDepthTex);

        // Use smooth depth to get normals
        cmd.Blit(SmoothedDepthTex, NormalTex, GameManager.Ins.screenSpaceManager.depthTextureToNormals);
    }
}
