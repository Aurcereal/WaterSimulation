using UnityEngine;

using static Unity.Mathematics.math;

using static SimulationParameters;

public class CountSortManager
{
    PrefixSumManager prefixSumManager;

    ComputeShader countSortShader;

    ComputeBuffer keyCountsBuffer;
    ComputeBuffer particleCellKeyEntrySortedBuffer;

    public CountSortManager()
    {
        prefixSumManager = new(SpatialLookupSize);

        //
        countSortShader = ComputeHelper.FindInResourceFolder("CountSort");

        //
        keyCountsBuffer = ComputeHelper.CreateBuffer<int>(SpatialLookupSize);
        particleCellKeyEntrySortedBuffer = ComputeHelper.CreateBuffer<ParticleEntry>(ParticleCount);

        //
        countSortShader.SetBuffer("KeyCounts", keyCountsBuffer, "ResetCounts", "SetCounts", "SortParticles");
        countSortShader.SetBuffer("ParticleEntries", GameManager.Ins.computeManager.particleCellKeyEntryBuffer, "SetCounts", "SortParticles", "CopyFromSortedBufferToEntryBuffer");
        countSortShader.SetBuffer("ParticleEntriesSorted", particleCellKeyEntrySortedBuffer, "SortParticles", "CopyFromSortedBufferToEntryBuffer");
        countSortShader.SetInt("ParticleCount", ParticleCount);
        countSortShader.SetInt("SpatialLookupSize", SpatialLookupSize);
    }

    public void SortParticleEntries()
    {
        ComputeHelper.Dispatch(countSortShader, SpatialLookupSize, 1, 1, "ResetCounts");
        ComputeHelper.Dispatch(countSortShader, ParticleCount, 1, 1, "SetCounts");
        //logKeyCounts();
        prefixSumManager.IntegrateBuffer(keyCountsBuffer, true);
        ComputeHelper.Dispatch(countSortShader, ParticleCount, 1, 1, "SortParticles");
        ComputeHelper.Dispatch(countSortShader, ParticleCount, 1, 1, "CopyFromSortedBufferToEntryBuffer");
    }

    public void Destructor()
    {
        ComputeHelper.DisposeBuffers(particleCellKeyEntrySortedBuffer, keyCountsBuffer);
    }

    void logKeyCounts()
    {
        int[] keyCounts = new int[10];
        keyCountsBuffer.GetData(keyCounts);
        for (int i = 0; i < keyCounts.Length; i++)
        {
            Debug.Log($"{i}, {keyCounts[i]}");
        }
    }

    // public static void test()
    // {
    //     ParticleEntry[] entries = new ParticleEntry[] {
    //         new(0, 4),
    //         new(1, 3),
    //         new(2, 2),
    //         new(3, 8),
    //         new(4, 9),
    //         new(5, 3),
    //         new(6, 4)
    //     };

    //     ComputeBuffer particleEntries = ComputeHelper.CreateBuffer(entries);
    //     int particleCount = entries.Length;
    //     int spatialLookupSize = 10;

    //     CountSortManager countSorter = new(particleCount, spatialLookupSize, particleEntries);
    //     countSorter.SortParticleEntries();
    //     particleEntries.GetData(entries);

    //     for (int i = 0; i < entries.Length; i++)
    //     {
    //         Debug.Log($"{i}, ID: {entries[i].particleIndex}, Key: {entries[i].cellKey}");
    //     }
    // }
}