using UnityEngine;

using static Unity.Mathematics.math;

using static SimulationParameters;

public class CountSortManager
{
    PrefixSumManager prefixSumManager;

    ComputeShader countSortShader;

    ComputeBuffer keyCountsBuffer;
    ComputeBuffer particleCellKeyEntrySortedBuffer;

    int n, k;

    public CountSortManager(int n, int k, ComputeBuffer entryBuffer, bool bufferCount = false, ComputeBuffer countBuffer = null)
    {
        this.n = n; this.k = k;

        prefixSumManager = new(k);

        //
        countSortShader = ComputeHelper.FindInResourceFolder("CountSort"); //bufferCount ? "CountSort" : "CountSortTempCopy");

        //
        keyCountsBuffer = ComputeHelper.CreateBuffer<int>(k);
        particleCellKeyEntrySortedBuffer = ComputeHelper.CreateBuffer<ParticleEntry>(n);

        //
        countSortShader.SetBuffer("KeyCounts", keyCountsBuffer, "ResetCounts", "SetCounts", "SortParticles");
        countSortShader.SetBuffer("ParticleEntries", entryBuffer, "SetCounts", "SortParticles", "CopyFromSortedBufferToEntryBuffer");
        countSortShader.SetBuffer("ParticleEntriesSorted", particleCellKeyEntrySortedBuffer, "SortParticles", "CopyFromSortedBufferToEntryBuffer");
        countSortShader.SetInt("SpatialLookupSize", k);

        if (bufferCount)
        {
            if (countBuffer == null) Debug.LogError("Count Sort: Count Buffer is Null when Buffer Count is on");
            countSortShader.SetBuffer("CountBuffer", countBuffer, 0, 1, 2, 3);
            countSortShader.EnableKeyword("BUFFER_COUNT"); //RVS
        }
        else
        {
            countSortShader.SetInt("ParticleCount", n);
            countSortShader.DisableKeyword("BUFFER_COUNT");
        }
    }

    public void SortParticleEntries()
    {
        ComputeHelper.Dispatch(countSortShader, k, 1, 1, "ResetCounts");
        ComputeHelper.Dispatch(countSortShader, n, 1, 1, "SetCounts");
        //logKeyCounts();
        prefixSumManager.IntegrateBuffer(keyCountsBuffer, true);
        ComputeHelper.Dispatch(countSortShader, n, 1, 1, "SortParticles");
        ComputeHelper.Dispatch(countSortShader, n, 1, 1, "CopyFromSortedBufferToEntryBuffer");
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