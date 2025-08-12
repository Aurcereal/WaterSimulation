using UnityEngine;

using static Unity.Mathematics.math;

using static SimulationParameters;

public class OddEvenSortManager
{

    ComputeShader evenPhaseShader;
    ComputeShader oddPhaseShader;
    int entryCount;

    bool currPhase;

    public OddEvenSortManager()
    {
        //
        this.entryCount = ParticleCount;

        evenPhaseShader = ComputeHelper.FindInResourceFolder("OddEvenSortEvenPhase");
        oddPhaseShader = ComputeHelper.FindInResourceFolder("OddEvenSortOddPhase");

        evenPhaseShader.SetBuffer(0, "ParticleEntries", GameManager.Ins.computeManager.particleCellKeyEntryBuffer);
        evenPhaseShader.SetBuffer(0, "ParticleIDToEntryIndex", GameManager.Ins.computeManager.ParticleIDToEntryIndexBuffer);
        oddPhaseShader.SetBuffer(0, "ParticleEntries", GameManager.Ins.computeManager.particleCellKeyEntryBuffer);
        oddPhaseShader.SetBuffer(0, "ParticleIDToEntryIndex", GameManager.Ins.computeManager.ParticleIDToEntryIndexBuffer);

        evenPhaseShader.SetInt("ParticleCount", entryCount); // ODOT: Change this and lots else if particle count changes No, OES is unused since time complexity is too bad
        oddPhaseShader.SetInt("ParticleCount", entryCount);
    }

    public void RunSortPhase()
    {
        // https://www.geeksforgeeks.org/dsa/odd-even-sort-brick-sort/
        ComputeShader currPhaseShader = currPhase ? evenPhaseShader : oddPhaseShader;
        currPhase = !currPhase;

        ComputeHelper.Dispatch(currPhaseShader, ParticleCount, 1, 1, 0);
    }

    public void test()
    {
        ParticleEntry[] entries = new ParticleEntry[ParticleCount];
        GameManager.Ins.computeManager.particleCellKeyEntryBuffer.GetData(entries);

        int sortFails = 0;

        ParticleEntry prev = entries[0];
        ParticleEntry curr;
        for (int i = 1; i < entries.Length; i++)
        {
            curr = entries[i];
            if (curr.cellKey < prev.cellKey) sortFails++;
            prev = curr;
        }

        Debug.Log($"Sort Fails: {sortFails}");
    }

}