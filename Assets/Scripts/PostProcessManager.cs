using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PostProcessManager : MonoBehaviour
{
    Material waterRaymarchMat;
    void Awake()
    {
        waterRaymarchMat = new(Shader.Find("Unlit/WaterRaymarch"));
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        Graphics.Blit(src, dest, waterRaymarchMat);
    }
}
