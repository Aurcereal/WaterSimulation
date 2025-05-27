using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Mathematics;
using static Unity.Mathematics.math;

using static SimulationParameters;
using System.Threading.Tasks;

public class SimulationUpdater
{

    ComputeShader particleSimulator;

    public SimulationUpdater()
    {
        particleSimulator = GameManager.Ins.computeManager.particleSimulatorShader;
    }

    public void Update(float dt)
    {
        if (dt > 1.0f / 60.0f)
        {
            Debug.Log("Timestep is too large for an accurate simulation, slowing down time accordingly...");
            dt = 1.0f / 60.0f;
        }

        GameManager.Ins.simUniformer.UniformDeltaTime(dt);
        GameManager.Ins.simUniformer.UniformMouseInputData();

        ComputeHelper.Dispatch(particleSimulator, ParticleCount, 1, 1, "CalculatePredictedPositions");

        ComputeHelper.Dispatch(particleSimulator, ParticleCount, 1, 1, "UpdateSpatialHashEntries");
        GameManager.Ins.bitonicSorter.SortParticleEntries();
        ComputeHelper.Dispatch(particleSimulator, SpatialLookupSize, 1, 1, "ResetSpatialHashOffsets");
        ComputeHelper.Dispatch(particleSimulator, ParticleCount, 1, 1, "UpdateSpatialHashOffsets");

        ComputeHelper.Dispatch(particleSimulator, ParticleCount, 1, 1, "CalculateDensities");
        // if(EnableParticleSprings) ComputeHelper.Dispatch(particleSimulator, ParticleCount, 1, 1, "UpdateSpringLengths");
        ComputeHelper.Dispatch(particleSimulator, ParticleCount, 1, 1, "UpdateParticles");

        //testspatialhash();
        //testdensities();
    }

    void testdensities()
    {
        var densities = new float[ParticleCount];
        GameManager.Ins.computeManager.densityBuffer.GetData(densities);
        for (int i = 0; i < densities.Length; i++)
        {
            if (densities[i] <= 0.0001f)
            {
                Debug.Log("Very low density");
                break;
            }
        }
    }

    void testspatialhash()
    {
        var positions = new float3[ParticleCount];
        var predictedPositions = new float3[ParticleCount];
        var particleEntries = new ParticleEntry[ParticleCount];
        var spatialOffsets = new int[SpatialLookupSize];

        GameManager.Ins.computeManager.positionBuffer.GetData(positions);
        GameManager.Ins.computeManager.predictedPositionBuffer.GetData(predictedPositions);
        GameManager.Ins.computeManager.particleCellKeyEntryBuffer.GetData(particleEntries);
        GameManager.Ins.computeManager.cellKeyToStartCoordBuffer.GetData(spatialOffsets);

        int imod(int i, int m)
        {
            if (i >= 0) return (int) (((uint)i) % ((uint)m));
            else return m - 1 - (int)(((uint)(-i) % (uint)m));
        }

        int hash21(int2 coord)
        {
            return imod(949937 + 119227 * coord.x + 370673 * coord.y, SpatialLookupSize);
        }

        int hash31(int3 coord)
        {
            return imod(949937 + 119227 * coord.x + 370673 * coord.y + 440537 * coord.z, SpatialLookupSize);
        }

        int getStartIndex(int key)
        {
            return spatialOffsets[key];
        }

        int getCellKey(int3 cellPos)
        {
            return hash31(cellPos);
        }

        int3 posToCell(float3 pos)
        {
            return int3(floor(pos / GridSize));
        }

        Color[] colors = new Color[ParticleCount];
        GameManager.Ins.computeManager.colorBuffer.GetData(colors);
        for (int i = 0; i < ParticleCount; i++) colors[i] = Color.white;

        for (int i = 0; i < particleEntries.Length; i++)
        {
            int particleIndex = particleEntries[i].particleIndex;
            if (colors[particleIndex] == Color.red) continue;
            Debug.Log($"{i} Key: {particleEntries[i].cellKey}, Particle Index: {particleIndex}, Position: {positions[particleIndex]}, Cell Position: {posToCell(positions[particleIndex])}");
        }

        for (int i = 0; i < ParticleCount; i++)
        {
            float3 pos = predictedPositions[i];
            int3 cell = posToCell(pos);
            int3 currCell;

            bool sw = false;
            for (int x = 0; x <= 0; x++)
            {
                for (int y = 0; y <= 0; y++)
                {
                    for (int z = 0; z <= 0; z++)
                    {
                        currCell = cell + int3(x, y, z);

                        int key = getCellKey(currCell);
                        int currIndex = getStartIndex(key);

                        //Debug.Log($"Cell {currCell}, Key {key}, Start Index: {currIndex}");

                        if (currIndex != -1)
                        {
                            while (currIndex < ParticleCount && particleEntries[currIndex].cellKey == key)
                            {

                                int particleIndex = particleEntries[currIndex].particleIndex;
                                colors[particleIndex] = Color.red;
                                sw = true;

                                currIndex++;
                            }
                        }
                    }
                }
            }
            if (!sw) Debug.Log("WHAT");
        }
        

        GameManager.Ins.computeManager.colorBuffer.SetData(colors);

    }
    
}
