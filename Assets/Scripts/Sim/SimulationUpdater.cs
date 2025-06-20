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
            // Debug.Log("Timestep is too large for an accurate simulation, slowing down time accordingly...");
            dt = 1.0f / 60.0f;
        }

        GameManager.Ins.simUniformer.UniformDeltaTime(dt);
        GameManager.Ins.simUniformer.UniformMouseInputData();

        ComputeHelper.Dispatch(particleSimulator, ParticleCount, 1, 1, "CalculatePredictedPositions");

        ComputeHelper.Dispatch(particleSimulator, ParticleCount, 1, 1, "UpdateSpatialHashEntries");
        GameManager.Ins.bitonicSorter.SortParticleEntries(); // TODO: optimize with odd even sort (iterative sort) or maybe count sort like seb lague...
        //ComputeHelper.Dispatch(particleSimulator, SpatialLookupSize, 1, 1, "ResetSpatialHashOffsets");
        ComputeHelper.Dispatch(particleSimulator, ParticleCount, 1, 1, "UpdateSpatialHashOffsets");

        ComputeHelper.Dispatch(particleSimulator, ParticleCount, 1, 1, "CalculateDensities");
        if (EnableParticleSprings) ComputeHelper.Dispatch(particleSimulator, ParticleCount, 1, 1, "UpdateSpringLengths");
        ComputeHelper.Dispatch(particleSimulator, ParticleCount, 1, 1, "UpdateParticles");

        GameManager.Ins.simFoamManager.UpdateFoamParticles();
        GameManager.Ins.simFoamManager.MoveSurvivingFoamParticlesToUpdatingBuffer();
        GameManager.Ins.simFoamManager.UpdateFoamArgsBuffer();
    }

}
