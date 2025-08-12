using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

using static Unity.Mathematics.math;

using static SimulationParameters;

/// <summary>
/// For Screenspace Rendering only
/// </summary>
public class ShadowMapManager
{
    public RenderTexture DensityFromSunTex { get; private set; }
    public Matrix4x4 ShadowCamVP => ShadowCam.projectionMatrix * ShadowCam.worldToCameraMatrix;

    CommandBuffer cmd;

    public ShadowMapManager()
    {

        cmd = new();
        ShadowCam.RemoveAllCommandBuffers();
        ShadowCam.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, cmd);
        ShadowCam.depthTextureMode = DepthTextureMode.Depth;

        // TODO: see if it functions ok without depth map
        // TODO: make sure shadowcam is disabled completely if it's not drawing shadows
        DensityFromSunTex = ComputeHelper.CreateRenderTexture2D(int2(ShadowMapResolution), ComputeHelper.DepthMode.Depth16, UnityEngine.Experimental.Rendering.GraphicsFormat.R32_SFloat);
    }

    public void OnEnable()
    {
        ShadowCam.RemoveAllCommandBuffers();
        ShadowCam.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, cmd);
        ShadowCam.depthTextureMode = DepthTextureMode.Depth;
    }

    public void OnDisable()
    {
        ShadowCam.RemoveAllCommandBuffers();
    }

    public void DrawShadows()
    {
        cmd.Clear();
        cmd.SetRenderTarget(DensityFromSunTex);
        cmd.ClearRenderTarget(true, true, Color.black);
        cmd.DrawMeshInstancedProcedural(MeshUtils.QuadMesh, 0, GameManager.Ins.screenSpaceManager.particleAdditiveDensityMaterial, 0, ParticleCount);
    }
}