using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static SimulationParameters;

public class SimUniformer
{

    public void UniformAllBuffers() {
        var particleSimulatorShader = GameManager.Ins.computeManager.particleSimulatorShader;
        var computeManager = GameManager.Ins.computeManager;

        particleSimulatorShader.SetBuffers(
            new (string, ComputeBuffer)[] {
                ("positions", computeManager.positionBuffer),
                ("predictedPositions", computeManager.predictedPositionBuffer),
                ("velocities", computeManager.velocityBuffer),
                ("masses", computeManager.massBuffer),
                ("densities", computeManager.densityBuffer),
                ("nearDensities", computeManager.nearDensityBuffer),
                ("particleCellKeyEntries", computeManager.particleCellKeyEntryBuffer),
                ("cellKeyToStartCoord", computeManager.cellKeyToStartCoordBuffer),
                ("colors", computeManager.colorBuffer)
                },
            new string[] {
                "CalculatePredictedPositions",
                "UpdateSpatialHashEntries",
                "ResetSpatialHashOffsets",
                "UpdateSpatialHashOffsets",
                "CalculateDensities",
                "UpdateParticles"
                });
    }

    public void UniformAllParameters() {
        var particleSimulatorShader = GameManager.Ins.computeManager.particleSimulatorShader;

        particleSimulatorShader.SetInt("ParticleCount", ParticleCount);
        particleSimulatorShader.SetVector("BoxDimensions", (Vector2)BoxDimensions);
        particleSimulatorShader.SetFloat("BoxThickness", BoxThickness);
        particleSimulatorShader.SetVector("Gravity", (Vector2)Gravity);
        particleSimulatorShader.SetFloat("ParticleRadius", ParticleRadius);
        particleSimulatorShader.SetFloat("SmoothingRadius", SmoothingRadius);
        particleSimulatorShader.SetFloat("MouseForceRadius", MouseForceRadius);
        particleSimulatorShader.SetFloat("MouseForceStrength", MouseForceStrength);
        particleSimulatorShader.SetFloat("TargetDensity", TargetDensity);
        particleSimulatorShader.SetFloat("NearDensityPressureMultiplier", NearDensityPressureMultiplier);
        particleSimulatorShader.SetFloat("PressureMultiplier", PressureMultiplier);
        particleSimulatorShader.SetFloat("ViscosityStrength", ViscosityStrength);

        //
        particleSimulatorShader.SetFloat("GridSize", GridSize);
        particleSimulatorShader.SetInt("SpatialLookupSize", SpatialLookupSize);
    }

    public void UniformDeltaTime(float dt) {
        GameManager.Ins.computeManager.particleSimulatorShader.SetFloat("DeltaTime", dt);
    }
}
