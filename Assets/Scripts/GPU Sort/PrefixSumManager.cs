using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefixSumManager
{

    int length;

    ComputeShader prefixSumShader;

    ComputeBuffer integrateBuffer1;
    ComputeBuffer integrateBuffer2;

    public PrefixSumManager(int length)
    {
        this.length = length;

        prefixSumShader = ComputeHelper.FindInResourceFolder("PrefixSum");

        integrateBuffer1 = ComputeHelper.CreateBuffer<int>(length);
        integrateBuffer2 = ComputeHelper.CreateBuffer<int>(length);

        prefixSumShader.SetInt("Length", length);
    }

    // Assume countBuffer is of ints
    public void IntegrateBuffer(ComputeBuffer countBuffer)
    {
        int windowSize = 1;

        bool swap = false;

        prefixSumShader.SetBuffer("IntegrateBufferSrc", countBuffer, "ExpandWindows");
        prefixSumShader.SetBuffer("IntegrateBufferDst", integrateBuffer1, "ExpandWindows");

        while (windowSize < length)
        {
            prefixSumShader.SetInt("WindowSize", windowSize);

            ComputeHelper.Dispatch(prefixSumShader, length, 1, 1, "ExpandWindows");

            windowSize *= 2;
            prefixSumShader.SetBuffer("IntegrateBufferSrc", swap ? integrateBuffer2 : integrateBuffer1, "ExpandWindows");
            prefixSumShader.SetBuffer("IntegrateBufferDst", swap ? integrateBuffer1 : integrateBuffer2, "ExpandWindows");
            swap = !swap;
        }

        // Make countBuffer the integrated buffer
        prefixSumShader.SetBuffer("IntegrateBufferSrc", swap ? integrateBuffer1 : integrateBuffer2, "CopySrcToDst");
        prefixSumShader.SetBuffer("IntegrateBufferDst", countBuffer, "CopySrcToDst");
        ComputeHelper.Dispatch(prefixSumShader, length, 1, 1, "CopySrcToDst");
    }

    public static void test()
    {
        PrefixSumManager prefixSummer = new(8);

        int[] oldSave = new int[] { 1, 2, 3, 4, 41, 051, -31, 959 };
        int[] toIntegrate = new int[] { 1, 2, 3, 4, 41, 051, -31, 959 };

        ComputeBuffer toIntegrateBuffer = ComputeHelper.CreateBuffer<int>(toIntegrate);
        prefixSummer.IntegrateBuffer(toIntegrateBuffer);
        toIntegrateBuffer.GetData(toIntegrate);

        int sum = 0;
        for (int i = 0; i < toIntegrate.Length; i++)
        {
            sum += oldSave[i];
            Debug.Log($"{i}: {toIntegrate[i]}, sum: {sum}");
        }
    }

}
