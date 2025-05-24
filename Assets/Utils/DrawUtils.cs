using UnityEngine;
using Unity.Mathematics;

using static Unity.Mathematics.math;
using UnityEngine.Diagnostics;

public static class DrawUtils {

    static readonly Material circleMaterial = new(Shader.Find("Unlit/Primitive/Circle"));
    static readonly Material outlineCircleMaterial = new(Shader.Find("Unlit/Primitive/OutlineCircle"));
    static readonly Material outlineBoxMaterial = new(Shader.Find("Unlit/Primitive/OutlineBox"));

    public static void DrawCircle(float2 pos, float radius, Color color, int layer = 1) {
        DrawCircle(float3(pos.x, pos.y, 0.0f), float3(0.0f), radius, color, layer);
    }

    public static void DrawCircle(float3 pos, float3 euler, float radius, Color color, int layer = 1) {
        Matrix4x4 transform = Matrix4x4.Translate((Vector3) pos) * Matrix4x4.Rotate(Quaternion.Euler((Vector3) degrees(euler))) * Matrix4x4.Scale(Vector3.one * radius);

        var materialPropertyBlock = new MaterialPropertyBlock();
        materialPropertyBlock.SetColor("_Color", color);

        Graphics.DrawMesh(MeshUtils.QuadMesh, transform, circleMaterial, layer, null, 0, materialPropertyBlock);
    }
    
    public static void DrawOutlineCircle(float2 pos, float radius, float thickness, Color color, int layer = 1) {
        DrawOutlineCircle(float3(pos.x, pos.y, 0.0f), float3(0.0f), radius, thickness, color, layer);
    }
    
    public static void DrawOutlineCircle(float3 pos, float3 euler, float radius, float thickness, Color color, int layer = 1)
    {
        Matrix4x4 transform = Matrix4x4.Translate((Vector3)pos) * Matrix4x4.Rotate(Quaternion.Euler((Vector3)degrees(euler))) * Matrix4x4.Scale(Vector3.one * radius);

        var materialPropertyBlock = new MaterialPropertyBlock();
        materialPropertyBlock.SetColor("_Color", color);
        materialPropertyBlock.SetFloat("_Radius", radius);
        materialPropertyBlock.SetFloat("_Thickness", thickness);

        Graphics.DrawMesh(MeshUtils.QuadMesh, transform, outlineCircleMaterial, layer, null, 0, materialPropertyBlock);
    }

    public static void DrawOutlineBox(float2 pos, float rotation, float2 dim, float thickness, Color color, int layer = 1)
    {
        DrawOutlineBox(float3(pos.x, pos.y, 0.0f), float3(0.0f, 0.0f, rotation), dim, thickness, color, layer);
    }

    static void DrawOutlineBox(float3 pos, float3 euler, float2 dim, float thickness, Color color, int layer = 1)
    {
        Matrix4x4 transform = Matrix4x4.Translate((Vector3)pos) * Matrix4x4.Rotate(Quaternion.Euler((Vector3)degrees(euler))) * Matrix4x4.Scale(new Vector3(dim.x * 0.5f, dim.y * 0.5f, 1.0f));

        var materialPropertyBlock = new MaterialPropertyBlock();
        materialPropertyBlock.SetColor("_Color", color);
        materialPropertyBlock.SetVector("_Dim", (Vector2)dim);
        materialPropertyBlock.SetFloat("_Thickness", thickness);

        Graphics.DrawMesh(MeshUtils.QuadMesh, transform, outlineBoxMaterial, layer, null, 0, materialPropertyBlock);
    }

}