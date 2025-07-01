using UnityEngine;

using static Unity.Mathematics.math;

using static SimulationParameters;

public class OddEvenSortManager
{

    ComputeShader evenPhaseShader;
    ComputeShader oddPhaseShader;
    int entryCount;

    public OddEvenSortManager()
    {
        //
        evenPhaseShader = ComputeHelper.FindInResourceFolder("OddEvenSortEvenPhase");
        oddPhaseShader = ComputeHelper.FindInResourceFolder("OddEvenSortOddPhase");
        this.entryCount = ParticleCount;
        evenPhaseShader.SetBuffer(0, "ParticleEntries", GameManager.Ins.computeManager.particleCellKeyEntryBuffer);
    }

    public void SortParticleEntries()
    {
        // https://www.geeksforgeeks.org/dsa/odd-even-sort-brick-sort/
    }

}