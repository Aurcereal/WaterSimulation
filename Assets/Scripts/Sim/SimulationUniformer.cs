using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
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
                "UpdateSpatialHashEntriesOES",
                "ResetSpatialHashOffsets",
                "UpdateSpatialHashOffsets",
                "CalculateDensities",
                "UpdateSpringLengths",
                "UpdateParticles",
                "CacheDensities"
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
                ("springRestLengths", computeManager.colorBuffer) // Need this since compiler of ParticleSimulator.compute will get scared it doesn't have the buffer (even tho it doesnt use it) so I give it a random buffer
                    },
                new string[] {
                "CalculatePredictedPositions",
                "UpdateSpatialHashEntries",
                "UpdateSpatialHashEntriesOES",
                "ResetSpatialHashOffsets",
                "UpdateSpatialHashOffsets",
                "CalculateDensities",
                "UpdateParticles",
                "CacheDensities",
                "UpdateFoamParticles"
                    });
        }

        particleSimulatorShader.SetBuffers(new (string, ComputeBuffer)[] {
            ("updatingFoamParticles", computeManager.updatingFoamParticles),
            ("survivingFoamParticles", computeManager.survivingFoamParticles),
            ("foamParticleCounts", computeManager.foamParticleCounts)
        },
            new string[] {
            "UpdateParticles",
            "UpdateFoamParticles",
            "MoveSurvivingFoamParticlesToUpdatingBuffer"
            });

        // OES
        if (UseOddEvenSort)
        {
            particleSimulatorShader.SetBuffers(new (string, ComputeBuffer)[] {
                ("ParticleIDToEntryIndex", computeManager.ParticleIDToEntryIndexBuffer)
            }, new string[] {
                "UpdateSpatialHashEntriesOES"
            });
        }
    }

    public void UniformAllParameters()
    {
        var particleSimulatorShader = GameManager.Ins.computeManager.particleSimulatorShader;

        particleSimulatorShader.SetInt("ParticleCount", ParticleCount);

        particleSimulatorShader.SetMatrix("ContainerTransform", ContainerTransform);
        particleSimulatorShader.SetMatrix("ContainerInverseTransform", ContainerInverseTransform);
        particleSimulatorShader.SetVector("ContainerScale", (Vector3)ContainerScale);

        particleSimulatorShader.SetMatrix("ObstacleInverseTransform", ObstacleInverseTransform);
        particleSimulatorShader.SetVector("ObstacleScale", (Vector3)ObstacleScale);
        particleSimulatorShader.SetBool("ObstacleType", ObstacleType);
        particleSimulatorShader.SetBool("ObstacleSimInteraction", ObstacleSimInteraction);

        particleSimulatorShader.SetVector("Gravity", (Vector3)Gravity);
        particleSimulatorShader.SetFloat("ParticleRadius", ParticleRadius);
        particleSimulatorShader.SetFloat("SmoothingRadius", SmoothingRadius);
        particleSimulatorShader.SetFloat("SqrSmoothingRadius", SmoothingRadius * SmoothingRadius);
        particleSimulatorShader.SetFloat("InvSmoothingRadius", 1.0f / SmoothingRadius);
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
        particleSimulatorShader.SetBool("EnableStickForce", EnableStickForce);
        particleSimulatorShader.SetFloat("MaxStickDistance", MaxStickDistance);
        particleSimulatorShader.SetFloat("StickForceMultiplier", StickForceMultiplier);

        //
        particleSimulatorShader.SetInt("MaxFoamParticleCount", MaxFoamParticleCount);
        particleSimulatorShader.SetFloat("FoamScaleMultiplier", FoamScaleMultiplier);
        particleSimulatorShader.SetFloat("TrappedAirPotentialRemapLow", TrappedAirPotentialRemapLow);
        particleSimulatorShader.SetFloat("TrappedAirPotentialRemapHigh", TrappedAirPotentialRemapHigh);
        particleSimulatorShader.SetFloat("TrappedAirMultiplier", TrappedAirMultiplier);
        particleSimulatorShader.SetFloat("KineticPotentialRemapLow", KineticPotentialRemapLow);
        particleSimulatorShader.SetFloat("KineticPotentialRemapHigh", KineticPotentialRemapHigh);
        particleSimulatorShader.SetFloat("HighestSprayDensity", HighestSprayDensity);
        particleSimulatorShader.SetFloat("LowestBubbleDensity", LowestBubbleDensity);
        particleSimulatorShader.SetFloat("BubbleGravityMultiplier", BubbleGravityMultiplier);
        particleSimulatorShader.SetFloat("BubbleFluidConformingMultiplier", BubbleFluidConformingMultiplier);
        particleSimulatorShader.SetFloat("SprayAirDragMultiplier", SprayAirDragMultiplier);

        //
        particleSimulatorShader.SetFloat("GridSize", GridSize);
        particleSimulatorShader.SetInt("SpatialLookupSize", SpatialLookupSize);

    }

    public void UniformDeltaTimeAndCurrentTime(float dt, float timeSinceStart)
    {
        GameManager.Ins.computeManager.particleSimulatorShader.SetFloat("DeltaTime", dt);
        GameManager.Ins.computeManager.particleSimulatorShader.SetFloat("TimeSinceStart", timeSinceStart);
    }

    public void UniformMouseInputData()
    {
        GameManager.Ins.computeManager.particleSimulatorShader.SetFloat("MouseForceSign",
            (GameManager.Ins.inputManager.RightMouseButton ? 1 : 0) +
            (GameManager.Ins.inputManager.LeftMouseButton ? -1 : 0)
            );
        GameManager.Ins.computeManager.particleSimulatorShader.SetVector("MousePosition", (Vector2)GameManager.Ins.inputManager.WorldMousePosition);
    }

    public void UniformDensityTexture(RenderTexture tex, int3 size)
    {
        ComputeHelper.SetTexture(GameManager.Ins.computeManager.particleSimulatorShader, "DensityTexture", tex, "CacheDensities");
        GameManager.Ins.computeManager.particleSimulatorShader.SetVector("DensityTextureSize", (Vector3) (float3) size);
    }
}
