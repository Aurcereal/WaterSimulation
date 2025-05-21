// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// using static Unity.Mathematics.math;

// using static SimulationParameters;
// using Unity.Mathematics;
// using System;

// public class SpatialHash
// {

//     public static float GridSize => SmoothingRadius / sqrt(2.0f);
//     const int SpatialLookupSize = 512;

//     static int imod(int i, int m)
//     {
//         if (i >= 0) return i % m;
//         else return m - 1 - ((-i) % m);
//     }

//     int hash21(int2 coord)
//     {
//         return imod(949937 + 119227 * coord.x + 370673 * coord.y, SpatialLookupSize);
//     }

//     int2 posToCell(float2 pos)
//     {
//         return (int2)floor(pos / GridSize);
//     }

//     public struct ParticleEntry
//     {
//         public int particleIndex;
//         public int cellKey;
//         public ParticleEntry(int i, int k) { particleIndex = i; cellKey = k; }
//     }

//     ParticleEntry[] partIDCellKeyPairs;
//     int[] keyToStartCoord;
//     public SpatialHash()
//     {
//         partIDCellKeyPairs = new ParticleEntry[ParticleCount];
//         particleTempColors = new Color[ParticleCount];
//         keyToStartCoord = new int[SpatialLookupSize];
//     }

//     public void UpdateSpatialHash()
//     {
//         //
//         float2[] positions = GameManager.Ins.simUpdater.predictedPositions;

//         // Update Particle Cell Keys
//         for (int i = 0; i < positions.Length; i++)
//         {
//             int2 cell = posToCell(positions[i]);
//             int key = hash21(cell);
//             partIDCellKeyPairs[i] = new(i, key);
//         }

//         // Sort so particles of same key are grouped => particles of same cell are grouped
//         Array.Sort(partIDCellKeyPairs, (kIdPair1, kIdPair2) => kIdPair1.cellKey - kIdPair2.cellKey);

//         // Update spatial lookup table
//         for (int i = 0; i < keyToStartCoord.Length; i++) keyToStartCoord[i] = -1;
//         int prevKey = -1;
//         for (int i = 0; i < partIDCellKeyPairs.Length; i++)
//         {
//             int currKey = partIDCellKeyPairs[i].cellKey;
//             if (currKey != prevKey)
//             {
//                 keyToStartCoord[currKey] = i;
//                 prevKey = currKey;
//             }
//         }
//     }

//     Color[] particleTempColors;
//     public void ForEachParticleWithinSmoothingRadius(float2 pos, Action<int> callback)
//     {
//         int2 centerCell = posToCell(pos);

//         // 3x3 surrounding cells
//         int2 cell;
//         for (int i = -1; i <= 1; i++)
//         {
//             for (int j = -1; j <= 1; j++)
//             {
//                 cell = centerCell + int2(i, j);

//                 int key = hash21(cell);
//                 int currIndex = keyToStartCoord[key];
//                 if (currIndex != -1)
//                 {
//                     while (currIndex < partIDCellKeyPairs.Length && partIDCellKeyPairs[currIndex].cellKey == key)
//                     {
//                         // Debug.Log($"Found {partIDCellKeyPairs[currIndex]}");
//                         callback(partIDCellKeyPairs[currIndex].particleIndex);
//                         currIndex++;
//                     }
//                 }
//             }
//         }
//     }

// }
