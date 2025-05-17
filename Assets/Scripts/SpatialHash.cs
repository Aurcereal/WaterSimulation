using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static Unity.Mathematics.math;

using static SimulationParameters;
using Unity.Mathematics;
using System;

public class SpatialHash
{

    public static float GridSize => SmoothingRadius / sqrt(2.0f);
    const int SpatialLookupSize = 256;

    static int imod(int i, int m)
    {
        if (i >= 0) return i % m;
        else return m - ((-i) % m);
    }

    int hash21(int2 coord)
    {
        return imod(949937 + 119227 * coord.x + 370673 * coord.y, SpatialLookupSize);
    }

    int2 posToCell(float2 pos)
    {
        return (int2)floor(pos / GridSize);
    }

    int2[] partIDCellKeyPairs;
    int[] keyToStartCoord;
    public SpatialHash()
    {
        partIDCellKeyPairs = new int2[ParticleCount];
        particleTempColors = new Color[ParticleCount];
        keyToStartCoord = new int[SpatialLookupSize];
    }

    public void UpdateSpatialHash()
    {
        //
        float2[] positions = GameManager.Ins.particleSimulator.positions;

        // Update Particle Cell Keys
        for (int i = 0; i < positions.Length; i++)
        {
            int2 cell = posToCell(positions[i]);
            int key = hash21(cell);
            partIDCellKeyPairs[i] = int2(i, key);
        }

        // Sort so particles of same key are grouped => particles of same cell are grouped
        Array.Sort(partIDCellKeyPairs, (kIdPair1, kIdPair2) => kIdPair1.y - kIdPair2.y);

        // Update spatial lookup table
        for (int i = 0; i < keyToStartCoord.Length; i++) keyToStartCoord[i] = -1;
        int prevKey = -1;
        for (int i = 0; i < partIDCellKeyPairs.Length; i++)
        {
            int currKey = partIDCellKeyPairs[i].y;
            if (currKey != prevKey)
            {
                //Debug.Log($"Curr Key: {currKey} maps to {i}");
                keyToStartCoord[currKey] = i;
                prevKey = currKey;
            }
        }
    }

    Color[] particleTempColors;
    public void ForEachParticleWithinSmoothingRadius(float2 pos, Action callback)
    {
        for (int i = 0; i < ParticleCount; i++) particleTempColors[i] = Color.white;
        int2 cell = posToCell(pos);

        int key = hash21(cell);
        int currIndex = keyToStartCoord[key];
        Debug.Log($"Key: {key}, Start Index: {currIndex}");
        if (currIndex != -1)
        {
            while (currIndex < partIDCellKeyPairs.Length && partIDCellKeyPairs[currIndex].y == key)
            {
                Debug.Log($"Found {partIDCellKeyPairs[currIndex]}");
                int particleIndex = partIDCellKeyPairs[currIndex].x;
                particleTempColors[particleIndex] = Color.red;
                currIndex++;
            }
        }
        for (int i = 0; i < particleTempColors.Length; i++) if (particleTempColors[i] == Color.red) Debug.Log($"Found red at {i}");
        GameManager.Ins.computeManager.UpdateColorBuffer(particleTempColors);

        // TODO: search 3x3 grid of cells
    }

}
