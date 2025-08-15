using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Mathematics;
using static Unity.Mathematics.math;

using static SimulationParameters;
using System.Threading.Tasks;

public class SimulationUpdater
{

    float timeSinceStart;

    ComputeShader particleSimulator;

    public SimulationUpdater()
    {
        particleSimulator = GameManager.Ins.computeManager.particleSimulatorShader;
        timeSinceStart = 0f;
    }

    public void Update(float dt)
    {
        if (dt > 1.0f / 60.0f)
        {
            // Debug.Log("Timestep is too large for an accurate simulation, slowing down time accordingly...");
            dt = 1.0f / 60.0f;
        }

        timeSinceStart += dt;

        GameManager.Ins.simUniformer.UniformDeltaTimeAndCurrentTime(dt, timeSinceStart);
        GameManager.Ins.simUniformer.UniformMouseInputData();

        ComputeHelper.Dispatch(particleSimulator, ParticleCount, 1, 1, "CalculatePredictedPositions");

        ComputeHelper.Dispatch(particleSimulator, ParticleCount, 1, 1, "UpdateSpatialHashEntries");
        GameManager.Ins.waterParticleCountSorter.SortParticleEntries(); // TODO: remove reset spatial offsets code unnecessary (like in compute shader and the kernel)
        ComputeHelper.Dispatch(particleSimulator, ParticleCount, 1, 1, "UpdateSpatialHashOffsets");

        ComputeHelper.Dispatch(particleSimulator, ParticleCount, 1, 1, "CalculateDensities");
        if (EnableParticleSprings) ComputeHelper.Dispatch(particleSimulator, ParticleCount, 1, 1, "UpdateSpringLengths"); // TODO: optimize springs space wise so we can use them
        ComputeHelper.Dispatch(particleSimulator, ParticleCount, 1, 1, "UpdateParticles");

        if (SimulateFoam)
        {
            GameManager.Ins.simFoamManager.UpdateFoamParticles();
            GameManager.Ins.simFoamManager.MoveSurvivingFoamParticlesToUpdatingBuffer();
            GameManager.Ins.simFoamManager.UpdateFoamArgsBuffer();
            if(CurrentVisualMode == VisualMode.Raymarched && UseRaymarchedFoam) GameManager.Ins.simFoamManager.RunSpatialHash();
        }
    }

}
