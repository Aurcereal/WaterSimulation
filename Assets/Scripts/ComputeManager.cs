using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Mathematics;
using static Unity.Mathematics.math;

public class ComputeManager
{
    public ComputeBuffer positionBuffer;

    public ComputeManager() {
        positionBuffer = new ComputeBuffer(SimulationParameters.ParticleCount, sizeof(float) * 2, ComputeBufferType.Default);
    }

    public void UpdatePositionBuffer(float2[] posititions) {
        positionBuffer.SetData(posititions);
    }

    void OnDisable() {
        positionBuffer.Dispose();
    }
}
