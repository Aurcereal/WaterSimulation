using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

public class SimulationParameters : MonoBehaviour
{
    public static int ParticleCount => Ins.fluidBehaviorPreset.particleCount;
    public static float3 SpawnDimensions => new(Ins.fluidBehaviorPreset.spawnWidth, Ins.fluidBehaviorPreset.spawnHeight, Ins.fluidBehaviorPreset.spawnDepth);

    public static float SmoothingRadius => Ins.fluidBehaviorPreset.smoothingRadius;
    public static Mesh SphereMesh => Ins.sphereMesh;
    public static bool UseShadows => Ins.useShadows;

    public static Matrix4x4 ContainerTransform => Ins.containerTransform.transform.localToWorldMatrix;
    public static Matrix4x4 ContainerInverseTransform => Ins.containerTransform.transform.worldToLocalMatrix;
    public static float3 ContainerScale => Ins.containerTransform.transform.localScale;

    public static Matrix4x4 ObstacleTransorm => Ins.obstacleTransform.transform.localToWorldMatrix;
    public static Matrix4x4 ObstacleInverseTransform => Ins.obstacleTransform.transform.worldToLocalMatrix;
    public static float3 ObstacleScale => Ins.obstacleTransform.transform.localScale;
    public static bool ObstacleType => Ins.environmentPreset.obstacleType;

    public static float3 Gravity => Ins.fluidBehaviorPreset.gravity;

    public static float TargetDensity => Ins.fluidBehaviorPreset.targetDensity;
    public static float NearDensityPressureMultiplier => Ins.fluidBehaviorPreset.nearDensityPressureMultiplier;
    public static float PressureMultiplier => Ins.fluidBehaviorPreset.pressureMultiplier;

    public static float ViscosityStrength => Ins.fluidBehaviorPreset.viscosityStrength;

    public static bool EnableParticleSprings => Ins.fluidBehaviorPreset.enableSpringForce;
    public static float SpringForceMultiplier => Ins.fluidBehaviorPreset.springForceMultiplier;

    public static bool EnableStickForce => Ins.fluidBehaviorPreset.enableStickForce;
    public static float MaxStickDistance => Ins.fluidBehaviorPreset.maxStickDistance;
    public static float StickForceMultiplier => Ins.fluidBehaviorPreset.stickForceMultiplier;

    public static float ParticleRadius => Ins.particleRadius;
    public static float2 ParticleColorSpeedRange => new(Ins.debugVisualPreset.particleLowColorSpeed, Ins.debugVisualPreset.particleHighColorSpeed);
    public static Camera MainCamera => Ins.mainCamera;

    public const int SpatialLookupSize = 1048576;
    public static float GridSize => SmoothingRadius / sqrt(2);

    public static VisualMode CurrentVisualMode => Ins.currentVisualMode;
    public static bool UseRaymarchedFoam => Ins.useRaymarchedFoamInRaymarchedWater;
    public static bool UseBillboardFoam => Ins.useBillboardFoam;
    public static float DensityCacheStepSize => Ins.raymarchVisualPreset.densityCacheStepSize;
    public static float DensityCacheSampleCount => Ins.raymarchVisualPreset.densityCacheSampleCount;
    public static bool UseDensityStepSize => Ins.raymarchVisualPreset.useDensityStepSize;
    public static float RaymarchDensityMultiplier => Ins.raymarchVisualPreset.densityMultiplier;
    public static float RaymarchLightMultiplier => Ins.raymarchVisualPreset.lightMultiplier;
    public static float3 RaymarchExtinctionCoefficients => Ins.raymarchVisualPreset.extinctionCoefficients;
    public static float RaymarchIndexOfRefraction => Ins.raymarchVisualPreset.indexOfRefraction;
    public static float3 LightDir => Ins.lightTransform.forward;
    public static int NumBounces => Ins.raymarchVisualPreset.numBounces;
    public static bool TraceReflectAndRefract => Ins.raymarchVisualPreset.traceReflectAndRefract;
    public static float WaterExistenceThreshold => Ins.raymarchVisualPreset.waterExistenceThreshold;
    public static float WaterExistenceEps => Ins.raymarchVisualPreset.waterExistenceEps;
    public static float NextRayOffset => Ins.raymarchVisualPreset.nextRayOffset;
    public static Cubemap EnvironmentMap => Ins.environmentPreset.environmentMap;

    public static float2 CameraRotateSpeed => Ins.cameraRotateSpeed;
    public static float2 CameraPanSpeed => Ins.cameraPanSpeed;
    public static float CameraZoomSpeed => Ins.cameraZoomSpeed;

    public static float DepthWorldBlurRadius => Ins.screenspaceVisualPreset.depthWorldBlurRadius;
    public static float DepthBlurBilateralFalloff => Ins.screenspaceVisualPreset.depthBlurBilteralFalloff;
    public static int DepthBlurIterationCount => Ins.screenspaceVisualPreset.depthBlurIterationCount;
    public static float DepthDifferenceCutoffForNormals => Ins.screenspaceVisualPreset.depthDifferenceCutoffForNormals;
    public static float ScreenSpaceDensityMultiplier => Ins.screenspaceVisualPreset.screenSpaceDensityMultiplier;
    public static float3 ScreenSpaceExtinctionCoefficients => Ins.screenspaceVisualPreset.screenSpaceExtinctionCoefficients;
    public static float ScreenSpaceLightMultiplier => Ins.screenspaceVisualPreset.screenSpaceLightMultiplier;
    public static int ShadowMapResolution => Ins.screenspaceVisualPreset.shadowMapResolution;
    public static Camera ShadowCam => Ins.shadowCam;
    public static float ScreenspaceIndexOfRefraction => Ins.screenspaceVisualPreset.indexOfRefraction;

    public static bool SimulateFoam => Ins.simulateFoam;
    public const int FoamSpatialLookupSize = 1048576;
    public static float FoamGridSize => 5f*FoamVolumeRadius;
    public static float FoamVolumeRadius => Ins.raymarchFoamVisualPreset.foamVolumeRadius;
    public static int MaxFoamParticleCount => Ins.foamBehaviorPreset.maxFoamParticleCount;
    public static float TrappedAirPotentialRemapLow => Ins.foamBehaviorPreset.trappedAirPotentialRemapLow;
    public static float TrappedAirPotentialRemapHigh => Ins.foamBehaviorPreset.trappedAirPotentialRemapHigh;
    public static float TrappedAirMultiplier => Ins.foamBehaviorPreset.trappedAirMultiplier;
    public static float KineticPotentialRemapLow => Ins.foamBehaviorPreset.kineticPotentialRemapLow;
    public static float KineticPotentialRemapHigh => Ins.foamBehaviorPreset.kineticPotentialRemapHigh;
    public static float FoamScaleMultiplier => Ins.screenspaceFoamVisualPreset.foamScaleMultiplier;
    public static float HighestSprayDensity => Ins.foamBehaviorPreset.highestSprayDensity;
    public static float LowestBubbleDensity => Ins.foamBehaviorPreset.lowestBubbleDensity;
    public static float BubbleGravityMultiplier => Ins.foamBehaviorPreset.bubbleGravityMultiplier;
    public static float BubbleFluidConformingMultiplier => Ins.foamBehaviorPreset.bubbleFluidConformingMultiplier;
    public static float SprayAirDragMultiplier => Ins.foamBehaviorPreset.sprayAirDragMultiplier;

    public static bool UseCaustics => Ins.useCaustics;

    public static Camera CausticsVerticalCamera => Ins.causticsVerticalCamera;
    public static int CausticsDepthNormalResolution => Ins.screenspaceCausticsVisualPreset.causticsDepthNormalResolution;
    public static float CausticsDepthWorldBlurRadius => Ins.screenspaceCausticsVisualPreset.causticsDepthWorldBlurRadius;
    public static int CausticsDepthBlurIterationCount => Ins.screenspaceCausticsVisualPreset.causticsDepthBlurIterationCount;

    public static float ParticleLowColorSpeed => Ins.debugVisualPreset.particleLowColorSpeed;
    public static float ParticleHighColorSpeed => Ins.debugVisualPreset.particleHighColorSpeed;
    public static Color ParticleLowSpeedColor => Ins.debugVisualPreset.particleLowSpeedColor;
    public static Color ParticleHighSpeedColor => Ins.debugVisualPreset.particleHighSpeedColor;

    public static EnvTemplate EnvPreset => Ins.envPreset;
    public static bool EnableBoundingBoxCollisionWithOverride => OverrideEnvPreset ? OverrideEnableBoundingBoxCollision : EnvPreset.enableBoundingBoxInteraction;
    public static bool EnableObstacleCollisionWithOverride => OverrideEnvPreset ? OverrideEnableObstacleCollision : EnvPreset.enableObjectInteraction;

    public static bool OverrideEnvPreset => Ins.overrideEnvPreset;
    public static bool OverrideEnableBoundingBoxCollision => Ins.overrideEnableBoundingBoxCollision;
    public static bool OverrideEnableObstacleCollision => Ins.overrideEnableObstacleCollision;

    public enum VisualMode
    {
        DebugSpheres,
        Raymarched,
        Screenspace
    }
    [Header("Visual Toggles")]
    [SerializeField] VisualMode currentVisualMode; /// TODO: make enum btwn raymarch, screenspace, debug
    [SerializeField] bool useRaymarchedFoamInRaymarchedWater = true; ///
    [SerializeField] bool useBillboardFoam = true; ///
    [SerializeField] bool useShadows = false; ///
    [SerializeField] bool useCaustics; ///

    [Header("Other Toggles")]
    [SerializeField] bool simulateFoam = true;

    [Header("Visual Presets")]
    [SerializeField] DebugVisualPreset debugVisualPreset;
    [SerializeField] EnvironmentPreset environmentPreset;
    [SerializeField] RaymarchCausticsVisualPreset raymarchCausticsVisualPreset;
    [SerializeField] RaymarchFoamVisualPreset raymarchFoamVisualPreset;
    [SerializeField] RaymarchVisualPreset raymarchVisualPreset;
    [SerializeField] ScreenspaceCausticsVisualPreset screenspaceCausticsVisualPreset;
    [SerializeField] ScreenspaceFoamVisualPreset screenspaceFoamVisualPreset;
    [SerializeField] ScreenspaceVisualPreset screenspaceVisualPreset;

    [Header("Behavior Presets")]
    [SerializeField] FluidBehaviorPreset fluidBehaviorPreset;
    [SerializeField] FoamBehaviorPreset foamBehaviorPreset;

    [Header("Environment Preset")]
    [SerializeField] EnvTemplate envPreset;
    [SerializeField] bool overrideEnvPreset = false;
    [SerializeField] bool overrideEnableBoundingBoxCollision = false;
    [SerializeField] bool overrideEnableObstacleCollision = false;

    [Header("Unity References")]
    [SerializeField] Transform obstacleTransform; ///
    [SerializeField] Transform containerTransform; ///
    [SerializeField] Transform lightTransform; /// make this stuff unity references header in simulationparams can't be turned into SO
    [SerializeField] Camera mainCamera; ///
    [SerializeField] Camera shadowCam; ///
    [SerializeField] Camera causticsVerticalCamera; ///
    [SerializeField] Mesh sphereMesh; ///

    [Header("Camera Controller Parameters")]
    [SerializeField] float2 cameraRotateSpeed; ///
    [SerializeField] float2 cameraPanSpeed; ///
    [SerializeField] float cameraZoomSpeed; ///

    [Header("Advanced")]
    [SerializeField] float particleRadius = 0.05f;

    private static SimulationParameters Ins;
    void Awake()
    {
        Ins = this;
    }

    bool prevOverrideEnvPreset = false;
    void OnValidate()
    {
        // This Monobehavior func is called when a value changes
        GameManager.Ins?.simUniformer.UniformAllParameters();
        RaymarchManager.Ins?.UniformAllParameters();
        GameManager.Ins?.screenSpaceManager.UniformParametersAndTextures();
        GameManager.Ins?.causticsManager.UniformParameters();
        if (RaymarchManager.Ins != null) RaymarchManager.Ins.enabled = CurrentVisualMode == VisualMode.Raymarched;
        GameManager.Ins?.screenSpaceManager.ResetGaussianKernels();
        GameManager.Ins?.screenSpaceManager.blurManager.UniformAllParameters();

        if (prevOverrideEnvPreset || OverrideEnvPreset)
        {
            GameManager.Ins.HandleNewEnv();
        }
        prevOverrideEnvPreset = OverrideEnvPreset;
    }

    void Update()
    {
        if (obstacleTransform.hasChanged || containerTransform.hasChanged || lightTransform.hasChanged)
        {
            obstacleTransform.hasChanged = false; containerTransform.hasChanged = false; lightTransform.hasChanged = false;
            GameManager.Ins?.simUniformer.UniformAllParameters();
            RaymarchManager.Ins.UpdateContainerData();
            RaymarchManager.Ins.UpdateObstacleData();
            GameManager.Ins.screenSpaceManager.UpdateObstacleData();
            RaymarchManager.Ins.UniformAllParameters();
        }
    }
}
