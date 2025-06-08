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
    CommandBuffer testCommandBuffer;
    Material testBlit;

    public ScreenSpaceWaterManager()
    {
        testCommandBuffer = new();
        testTex = ComputeHelper.CreateRenderTexture2D(int2(Screen.width, Screen.height));
        testBlit = new Material(Shader.Find("Unlit/TestBlit"));
    }

    public void Draw()
    {
        MainCamera.RemoveAllCommandBuffers();
        MainCamera.AddCommandBuffer(CameraEvent.AfterEverything, testCommandBuffer);
        MainCamera.depthTextureMode = DepthTextureMode.Depth;

        testCommandBuffer.Clear();
        testCommandBuffer.SetRenderTarget(testTex);
        testCommandBuffer.ClearRenderTarget(true, true, Color.cyan);
        GameManager.Ins.drawer.DrawParticlesOnCommandBuffer(testCommandBuffer);
        testCommandBuffer.Blit(testTex, MainCamera.targetTexture, testBlit);
    }
}
