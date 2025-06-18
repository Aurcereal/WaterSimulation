using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using static Unity.Mathematics.math;

public static class ComputeHelper
{
    public static void Dispatch(ComputeShader computeShader, int itersX, int itersY, int itersZ, int kernelIndex = 0)
    {
        // May have to dispatch more groups than necessary (to achieve desired num iters)
        float3 iterDim = float3(itersX, itersY, itersZ);
        int3 groupCount = (int3)ceil(iterDim / float3(GetGroupSize(computeShader, kernelIndex)));
        computeShader.Dispatch(kernelIndex, groupCount.x, groupCount.y, groupCount.z);
    }

    public static void Dispatch(ComputeShader computeShader, int itersX, int itersY, int itersZ, string kernelName)
    {
        Dispatch(computeShader, itersX, itersY, itersZ, computeShader.FindKernel(kernelName));
    }

    public static uint3 GetGroupSize(ComputeShader computeShader, int kernelIndex = 0)
    {
        uint3 groupSize;
        computeShader.GetKernelThreadGroupSizes(kernelIndex, out groupSize.x, out groupSize.y, out groupSize.z);
        return groupSize;
    }

    public static int SizeOf<T>()
    {
        return Marshal.SizeOf(typeof(T));
    }

    public static ComputeShader FindInResourceFolder(string name)
    {
        // Must be in Resources folder
        return Resources.Load<ComputeShader>(name);
    }

    public static ComputeBuffer CreateBuffer<T>(T[] data)
    {
        ComputeBuffer buffer = CreateBuffer<T>(data.Length);
        buffer.SetData(data);
        return buffer;
    }

    public static ComputeBuffer CreateBuffer<T>(int elemCount)
    {
        return new ComputeBuffer(elemCount, SizeOf<T>());
    }

    public static void DisposeBuffers(params ComputeBuffer[] buffers)
    {
        foreach (var buf in buffers)
            buf.Dispose();
    }

    public static void SetBuffer(this ComputeShader shader, string name, ComputeBuffer buffer, params string[] kernelNames)
    {
        foreach (string kernelName in kernelNames)
        {
            shader.SetBuffer(shader.FindKernel(kernelName), name, buffer);
        }
    }

    public static void SetBuffers(this ComputeShader shader, (string, ComputeBuffer)[] nameBufferPairs, string[] kernelNames)
    {
        foreach (var nbp in nameBufferPairs)
        {
            shader.SetBuffer(nbp.Item1, nbp.Item2, kernelNames);
        }
    }

    public static void SetTexture(this ComputeShader shader, string name, Texture tex, params string[] kernelNames)
    {
        foreach (string kernelName in kernelNames)
        {
            shader.SetTexture(shader.FindKernel(kernelName), name, tex);
        }
    }

    public enum DepthMode
    {
        DepthNone = 0,
        Depth16 = 16,
        Depth24 = 24,
        Depth32 = 32
    }

    public static RenderTexture CreateRenderTexture2D(int2 size, DepthMode depthMode = DepthMode.DepthNone, GraphicsFormat graphicsFormat = GraphicsFormat.R32G32B32A32_SFloat, FilterMode filterMode = FilterMode.Bilinear, string name = "Unnamed", bool useMipMaps = false)
    {
        var texture = new RenderTexture(size.x, size.y, (int)depthMode);
        texture.graphicsFormat = graphicsFormat;
        texture.filterMode = filterMode;
        texture.name = name;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.enableRandomWrite = true;
        texture.autoGenerateMips = false;
        texture.useMipMap = useMipMaps;
        texture.Create();

        return texture;
    }

    public static RenderTexture CreateRenderTexture3D(int3 size, GraphicsFormat format, TextureWrapMode wrapMode = TextureWrapMode.Repeat, string name = "Unnamed", bool useMipmaps = false)
    {
        var texture = new RenderTexture(size.x, size.y, 0);
        texture.graphicsFormat = format;
        texture.volumeDepth = size.z;
        texture.enableRandomWrite = true;
        texture.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
        texture.useMipMap = useMipmaps;
        texture.autoGenerateMips = false;
        texture.wrapMode = wrapMode;
        texture.name = name;
        texture.Create();

        return texture;
    }

    /// <summary>
    /// Same as CreateRenderTexture but may not need to create a new render texture if oldTexture has params satisfied.
    /// </summary>
    public static RenderTexture UpdateRenderTexture3D(RenderTexture oldTexture, int3 size, GraphicsFormat format, TextureWrapMode wrapMode = TextureWrapMode.Repeat, string name = "Untitled", bool useMipmaps = false)
    {
        if (oldTexture == null || !oldTexture.IsCreated() || oldTexture.width != size.x || oldTexture.height != size.y || oldTexture.volumeDepth != size.z || oldTexture.graphicsFormat != format || oldTexture.wrapMode != wrapMode || oldTexture.name != name || oldTexture.useMipMap != useMipmaps)
        {
            if (oldTexture != null) oldTexture.Release();
            return CreateRenderTexture3D(size, format, wrapMode, name, useMipmaps);
        }
        else
        {
            return oldTexture;
        }
    }

    public static ComputeBuffer CreateArgsBuffer(Mesh mesh, int subMeshIndex, uint initialIndexCount)
    {
        uint[] args = {
            mesh.GetIndexCount(subMeshIndex),
            initialIndexCount,
            mesh.GetIndexStart(subMeshIndex),
            mesh.GetBaseVertex(subMeshIndex),
            0
        };

        ComputeBuffer buf = CreateBuffer<uint>(5);
        buf.SetData(args);
        return buf;
    }
}