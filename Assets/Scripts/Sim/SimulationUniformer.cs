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

        
        particleSimulatorShader.SetBuffers(
            new (string, ComputeBuffer)[] {
            ("positions", computeManager.positionBuffer),
            ("predictedPositions", computeManager.predictedPositionBuffer),
            ("velocities", computeManager.velocityBuffer),
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
            "UpdateSpatialHashOffsets",
            "CalculateDensities",
            "UpdateParticles",
            "CacheDensities",
            "UpdateFoamParticles"
                });
        

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

    }

    bool? sceneCollisionFeature = null;
    bool? simulateFoamFeature = null;
    bool? pressureForceFeature = null;
    bool? viscosityForceFeature = null;
    bool? stickForceFeature = null;
    bool? springForceFeature = null;
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
        particleSimulatorShader.SetBool("SpawnFoam", SimulateFoam);

        particleSimulatorShader.SetVector("Gravity", (Vector3)Gravity);
        particleSimulatorShader.SetFloat("ParticleRadius", ParticleRadius);
        particleSimulatorShader.SetFloat("SmoothingRadius", SmoothingRadius);
        particleSimulatorShader.SetFloat("SqrSmoothingRadius", SmoothingRadius * SmoothingRadius);
        particleSimulatorShader.SetFloat("InvSmoothingRadius", 1.0f / SmoothingRadius);
        particleSimulatorShader.SetFloat("TargetDensity", TargetDensity);
        particleSimulatorShader.SetFloat("NearDensityPressureMultiplier", NearDensityPressureMultiplier);
        particleSimulatorShader.SetFloat("PressureMultiplier", PressureMultiplier);
        particleSimulatorShader.SetFloat("ViscosityStrength", ViscosityStrength);
        particleSimulatorShader.SetFloat("SpringForceMultiplier", SpringForceMultiplier);
        particleSimulatorShader.SetFloat("SpringRestLenFac", SpringRestLenFac);
        particleSimulatorShader.SetBool("EnableParticleSprings", EnableParticleSprings);
        particleSimulatorShader.SetBool("EnableStickForce", EnableStickForce);
        particleSimulatorShader.SetFloat("MaxStickDistance", MaxStickDistance);
        particleSimulatorShader.SetFloat("StickForceMultiplier", StickForceMultiplier);
        particleSimulatorShader.SetFloat("ForceFieldMultiplier", ForceFieldMultiplier);

        //
        particleSimulatorShader.SetInt("MaxFoamParticleCount", MaxFoamParticleCount);
        particleSimulatorShader.SetFloat("FoamSpawnMultiplier", FoamSpawnMultiplier);
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

        //
        particleSimulatorShader.SetFloat("ParticleLowColorSpeed", ParticleLowColorSpeed);
        particleSimulatorShader.SetFloat("ParticleHighColorSpeed", ParticleHighColorSpeed);
        particleSimulatorShader.SetVector("ParticleLowSpeedColor", ParticleLowSpeedColor);
        particleSimulatorShader.SetVector("ParticleHighSpeedColor", ParticleHighSpeedColor);

        particleSimulatorShader.SetFloat("DebugFloat", DebugFloat);
        particleSimulatorShader.SetVector("DebugVector", DebugVector);
        particleSimulatorShader.SetBool("DebugBool", DebugBool);


        //
        if (EnableSceneCollision != sceneCollisionFeature) particleSimulatorShader.SetKeywordActive("SCENE_COLLISION", EnableSceneCollision);
        if (SimulateFoam != simulateFoamFeature) particleSimulatorShader.SetKeywordActive("SIMULATE_FOAM", SimulateFoam);
        if (pressureForceFeature != EnablePressureForce) particleSimulatorShader.SetKeywordActive("PRESSURE_FORCE", EnablePressureForce);
        if (viscosityForceFeature != EnableViscosityForce) particleSimulatorShader.SetKeywordActive("VISCOSITY_FORCE", EnableViscosityForce);
        if (stickForceFeature != EnableStickForce) particleSimulatorShader.SetKeywordActive("STICK_FORCE", EnableStickForce);
        if (springForceFeature != EnableParticleSprings) particleSimulatorShader.SetKeywordActive("SPRING_FORCE", EnableParticleSprings);

        sceneCollisionFeature = EnableSceneCollision;
        simulateFoamFeature = SimulateFoam;
        pressureForceFeature = EnablePressureForce;
        viscosityForceFeature = EnableViscosityForce;
        stickForceFeature = EnableStickForce;
        springForceFeature = EnableParticleSprings;

    }

    public void UniformDeltaTimeAndCurrentTime(float dt, float timeSinceStart)
    {
        GameManager.Ins.computeManager.particleSimulatorShader.SetFloat("DeltaTime", dt);
        GameManager.Ins.computeManager.particleSimulatorShader.SetFloat("TimeSinceStart", timeSinceStart);
        Shader.SetGlobalFloat("TimeSinceStart", timeSinceStart); // For visual shaders..
    }

    public void UniformDensityTexture(RenderTexture tex, int3 size)
    {
        ComputeHelper.SetTexture(GameManager.Ins.computeManager.particleSimulatorShader, "DensityTexture", tex, "CacheDensities");
        GameManager.Ins.computeManager.particleSimulatorShader.SetVector("DensityTextureSize", (Vector3)(float3)size);
    }

    string? currPhysicsCompileKeyword = null;
    public void HandleNewEnv()
    {
        var particleSimulatorShader = GameManager.Ins.computeManager.particleSimulatorShader;

        //
        if (currPhysicsCompileKeyword != null) particleSimulatorShader.DisableKeyword(currPhysicsCompileKeyword);

        //
        particleSimulatorShader.SetKeywordActive("BBX_COLLISION", EnableBoundingBoxCollisionWithOverride);
        particleSimulatorShader.SetKeywordActive("OBSTACLE_COLLISION", EnableObstacleCollisionWithOverride);

        //
        currPhysicsCompileKeyword = EnvPreset.physicsCompileKeyword;
        particleSimulatorShader.EnableKeyword(currPhysicsCompileKeyword);
    }
}
