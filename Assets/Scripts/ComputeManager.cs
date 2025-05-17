using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Mathematics;
using static Unity.Mathematics.math;

public class ComputeManager
{
    public ComputeBuffer positionBuffer;
    public ComputeBuffer colorBuffer;

    public ComputeManager()
    {
        positionBuffer = new ComputeBuffer(SimulationParameters.ParticleCount, sizeof(float) * 2, ComputeBufferType.Default);
        colorBuffer = new ComputeBuffer(SimulationParameters.ParticleCount, sizeof(float) * 4, ComputeBufferType.Default);
    }

    public void UpdatePositionBuffer(float2[] posititions) {
        positionBuffer.SetData(posititions);
    }

    public void UpdateColorBuffer(Color[] colors)
    {
        colorBuffer.SetData(colors);
    }

    public void Destructor()
    {
        positionBuffer.Dispose();
        colorBuffer.Dispose();
    }
}
