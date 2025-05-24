using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static SimulationParameters;

public class SimulationUniformer
{

    public void UniformAllBuffers()
    {
        var particleSimulatorShader = GameManager.Ins.computeManager.particleSimulatorShader;
        var computeManager = GameManager.Ins.computeManager;

        if (EnableParticleSprings)
        {
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
                ("colors", computeManager.colorBuffer),
                ("springRestLengths", computeManager.springRestLengthBuffer)
                    },
                new string[] {
                "CalculatePredictedPositions",
                "UpdateSpatialHashEntries",
                "ResetSpatialHashOffsets",
                "UpdateSpatialHashOffsets",
                "CalculateDensities",
                "UpdateSpringLengths",
                "UpdateParticles"
                    });
        }
        else
        {
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
                ("colors", computeManager.colorBuffer),
                ("springRestLengths", new ComputeBuffer(1, 1)) // Need this since compiler of ParticleSimulator.compute will get scared it doesn't have the buffer (even tho it doesnt use it)
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
    }

    public void UniformAllParameters()
    {
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
        particleSimulatorShader.SetFloat("SurfaceTensionMultiplier", SurfaceTensionMultiplier);
        particleSimulatorShader.SetFloat("SpringForceMultiplier", SpringForceMultiplier);
        particleSimulatorShader.SetFloat("Plasticity", Plasticity);
        particleSimulatorShader.SetFloat("SpringYieldRatio", SpringYieldRatio);
        particleSimulatorShader.SetBool("EnableParticleSprings", EnableParticleSprings);

        particleSimulatorShader.SetVector("ObstacleDimensions", (Vector2) ObstacleDimensions);
        particleSimulatorShader.SetVector("ObstaclePosition", (Vector2) ObstaclePosition);
        particleSimulatorShader.SetFloat("ObstacleRotation", ObstacleRotation);

        //
        particleSimulatorShader.SetFloat("GridSize", GridSize);
        particleSimulatorShader.SetInt("SpatialLookupSize", SpatialLookupSize);
    }

    public void UniformDeltaTime(float dt)
    {
        GameManager.Ins.computeManager.particleSimulatorShader.SetFloat("DeltaTime", dt);
    }

    public void UniformMouseInputData()
    {
        GameManager.Ins.computeManager.particleSimulatorShader.SetFloat("MouseForceSign",
            (GameManager.Ins.inputManager.RightMouseButton ? 1 : 0) +
            (GameManager.Ins.inputManager.LeftMouseButton ? -1 : 0)
            );
        GameManager.Ins.computeManager.particleSimulatorShader.SetVector("MousePosition", (Vector2)GameManager.Ins.inputManager.WorldMousePosition);
    }
}
