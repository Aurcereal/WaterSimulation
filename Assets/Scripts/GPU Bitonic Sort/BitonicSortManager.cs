using UnityEngine;

using static Unity.Mathematics.math;

public class BitonicSortManager
{

    ComputeShader bitonicSortShader;
    int entryCount;
    int nextPowerOf2EntryCount;
    ComputeBuffer tempParticleEntries;

    public BitonicSortManager(ComputeBuffer particleEntries, int entryCount)
    {
        // start with power of 2 entries but make it support arb later
        bitonicSortShader = ComputeHelper.FindInResourceFolder("BitonicSortForward");

        this.tempParticleEntries = particleEntries;
        this.entryCount = entryCount;
        this.nextPowerOf2EntryCount = 1 <<  ((int) ceil(log2(entryCount)));

        bitonicSortShader.SetBuffer(0, "ParticleEntries", particleEntries);
        bitonicSortShader.SetInt("EntryCount", entryCount);
        bitonicSortShader.SetInt("NextPowerOf2EntryCount", nextPowerOf2EntryCount);
        
    }

    public void SortParticleEntries()
    {
        //SpatialHash.ParticleEntry[] entries = new SpatialHash.ParticleEntry[7];
        // Bitonic Sort Resources:
        //  https://developer.nvidia.com/gpugems/gpugems2/part-vi-simulation-and-numerical-algorithms/chapter-46-improved-gpu-sorting
        //  https://www.geeksforgeeks.org/bitonic-sort/
        int stageCount = (int)log2(nextPowerOf2EntryCount);
        Debug.Log($"Expected: 3, Stage Count: {stageCount}");
        for (int stage = 0; stage < stageCount; stage++)
        {
            int alternatorGroupSizePower = stage + 1;
            int alternatorGroupSize = 1 << alternatorGroupSizePower;
            bitonicSortShader.SetInt("AlternatorGroupSize", alternatorGroupSize);

            for (int pass = 0; pass <= stage; pass++)
            {
                int groupSizePower = (stage + 1) - pass;
                int groupSize = 1 << groupSizePower;
                Debug.Log($"Group Size: {groupSize}");

                // CompareSwap first and second half of every group, every stage sorts another level of tree
                bitonicSortShader.SetInt("GroupSize", groupSize);

                // Dispatch
                ComputeHelper.Dispatch(bitonicSortShader, nextPowerOf2EntryCount / 2, 1, 1, 0);

                // tempParticleEntries.GetData(entries);
                // Debug.Log($"After Pass {pass} on stage {stage}");
                // for (int i = 0; i < entries.Length; i++)
                // {
                //     Debug.Log($"Key: {entries[i].cellKey}, Particle Index: {entries[i].particleIndex}");
                // }
            }
        }
    }

}