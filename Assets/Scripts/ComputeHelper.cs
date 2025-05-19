using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

public class ComputeHelper
{
    public static void Dispatch(ComputeShader computeShader, int itersX, int itersY, int itersZ, int kernelIndex = 0)
    {
        // May have to dispatch more groups than necessary (to achieve desired num iters)
        float3 iterDim = float3(itersX, itersY, itersZ);
        int3 groupCount = (int3)ceil(iterDim / float3(GetGroupSize(computeShader, kernelIndex)));
        computeShader.Dispatch(kernelIndex, groupCount.x, groupCount.y, groupCount.z);
    }

    public static uint3 GetGroupSize(ComputeShader computeShader, int kernelIndex = 0)
    {
        uint3 groupSize;
        computeShader.GetKernelThreadGroupSizes(kernelIndex, out groupSize.x, out groupSize.y, out groupSize.z);
        //Debug.Log($"Delete this later but GroupSize: {groupSize}");
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

    public static ComputeBuffer CreateBuffer<T>(int elemCount) {
        return new ComputeBuffer(elemCount, SizeOf<T>());
    }
}