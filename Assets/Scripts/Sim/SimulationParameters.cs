using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

public class SimulationParameters : MonoBehaviour
{
    // TODO: remove box thickness and use only container transform (and a float3 box dimensions for accurate sdf) for sdf and particle simulator
    // sdf: inverse transform to unit box, transform the box dimensions, use sdf of box with box dimensions
    public static int ParticleCount => Ins.particleCount;
    public static float3 SpawnDimensions => new(Ins.spawnWidth, Ins.spawnHeight, Ins.spawnDepth);

    public static float SmoothingRadius => Ins.smoothingRadius;

    public static Matrix4x4 ContainerTransform => Ins.containerTransform.transform.localToWorldMatrix;
    public static Matrix4x4 ContainerInverseTransform => Ins.containerTransform.transform.worldToLocalMatrix;
    public static float3 ContainerScale => Ins.containerTransform.transform.localScale;

    public static Matrix4x4 ObstacleTransorm => Ins.obstacleTransform.transform.localToWorldMatrix;
    public static Matrix4x4 ObstacleInverseTransform => Ins.obstacleTransform.transform.worldToLocalMatrix;
    public static float3 ObstacleScale => Ins.obstacleTransform.transform.localScale;
    public static bool ObstacleType => Ins.obstacleType;
    public static bool ObstacleSimInteraction => Ins.obstacleSimInteraction;

    public static float3 Gravity => Ins.gravity;

    public static float MouseForceRadius => Ins.mouseForceRadius;
    public static float MouseForceStrength => Ins.mouseForceStrength;

    public static float TargetDensity => Ins.targetDensity;
    public static float NearDensityPressureMultiplier => Ins.nearDensityPressureMultiplier;
    public static float PressureMultiplier => Ins.pressureMultiplier;

    public static float ViscosityStrength => Ins.viscosityStrength;

    public static float SurfaceTensionMultiplier => Ins.surfaceTensionMultiplier;

    public static bool EnableParticleSprings => Ins.enableParticleSprings;
    public static float SpringForceMultiplier => Ins.springForceMultiplier;
    public static float Plasticity => Ins.plasticity;
    public static float SpringYieldRatio => Ins.springYieldRatio;

    public static bool EnableStickForce => Ins.enableStickForce;
    public static float MaxStickDistance => Ins.maxStickDistance;
    public static float StickForceMultiplier => Ins.stickForceMultiplier;

    public static float ParticleRadius => Ins.particleRadius;
    public static Gradient ParticleSpeedGradient => Ins.particleSpeedGradient;
    public static float2 ParticleColorSpeedRange => new(Ins.particleLowColorSpeed, Ins.particleHighColorSpeed);
    public static Mesh ParticleMesh => Ins.particleMesh;
    public static Camera MainCamera => Ins.mainCamera;

    public const int SpatialLookupSize = 1048576;
    public static float GridSize => SmoothingRadius/sqrt(2);

    public static bool EnableRaymarchShader => Ins.enableRaymarchShader;
    public static float DensityCacheStepSize => Ins.densityCacheStepSize;
    public static float DensityCacheSampleCount => Ins.densityCacheSampleCount;
    public static bool UseDensityStepSize => Ins.useDensityStepSize;
    public static float DensityMultiplier => Ins.densityMultiplier;
    public static float LightMultiplier => Ins.lightMultiplier;
    public static float3 ExtinctionCoefficients => Ins.extinctionCoefficients;
    public static float IndexOfRefraction => Ins.indexOfRefraction;
    public static float3 LightDir => Ins.lightTransform.forward;
    public static int NumBounces => Ins.numBounces;
    public static bool TraceReflectAndRefract => Ins.traceReflectAndRefract;
    public static float WaterExistenceThreshold => Ins.waterExistenceThreshold;
    public static float WaterExistenceEps => Ins.waterExistenceEps;
    public static float NextRayOffset => Ins.nextRayOffset;
    public static Cubemap EnvironmentMap => Ins.environmentMap;

    public static float2 CameraRotateSpeed => Ins.cameraRotateSpeed;
    public static float2 CameraPanSpeed => Ins.cameraPanSpeed;
    public static float CameraZoomSpeed => Ins.cameraZoomSpeed;

    [Header("Initialization Parameters")]
    [Range(1, 200000)][SerializeField] int particleCount = 10;
    [Range(0.05f, 100)][SerializeField] float spawnWidth = 50.0f;
    [Range(0.05f, 100)][SerializeField] float spawnHeight = 40.0f;
    [Range(0.05f, 100)][SerializeField] float spawnDepth = 40.0f;

    [Header("Misc Parameters")]
    [Range(0.005f, 10.0f)][SerializeField] float smoothingRadius = 0.1f;

    [Header("Box/Obstacle Parameters")]
    [SerializeField] Transform obstacleTransform;
    [SerializeField] bool obstacleType;
    [SerializeField] bool obstacleSimInteraction = true;
    [SerializeField] Transform containerTransform;

    [Header("Gravity Force")]
    [SerializeField] float3 gravity = new float3(0.0f, -9.8f, 0.0f);

    [Header("Mouse Force")]
    [Range(0.005f, 10.0f)][SerializeField] float mouseForceRadius = 4.0f;
    [Range(0.0f, 5000.0f)][SerializeField] float mouseForceStrength = 100.0f;

    [Header("Pressure Force")]
    [Range(0.0f, 500.0f)][SerializeField] float targetDensity = 10.0f;
    [Range(0.0f, 500.0f)][SerializeField] float nearDensityPressureMultiplier = 1.0f;
    [Range(0.05f, 500.0f)][SerializeField] float pressureMultiplier = 1.0f;

    [Header("Viscosity Force")]
    [Range(0.0f, 750.0f)][SerializeField] float viscosityStrength = 4.0f;

    [Header("Surface Tension Force")]
    [Range(0.0f, 500.0f)][SerializeField] float surfaceTensionMultiplier = 5.0f;

    [Header("Spring Force")]
    [Tooltip("Requires Restart")] [SerializeField] bool enableParticleSprings = false;
    [Range(0.0f, 200000.0f)][SerializeField] float springForceMultiplier = 50000f;
    [Range(0.0f, 10.0f)][SerializeField] float plasticity = 0f;
    [Range(0.0f, 10.0f)][SerializeField] float springYieldRatio = 0.1f;

    [Header("Stickiness Force")]
    [SerializeField] bool enableStickForce = true;
    [Range(0.0f, 2.0f)][SerializeField] float maxStickDistance = 0.07f;
    [Range(0.0f, 10000.0f)][SerializeField] float stickForceMultiplier = 10.0f;

    [Header("Mostly Visual")]
    [Range(0.025f, 1.0f)][SerializeField] float particleRadius = 0.05f;
    [SerializeField] Gradient particleSpeedGradient;
    [Range(0.01f, 100f)][SerializeField] float particleLowColorSpeed = 0.0f;
    [Range(0.01f, 100f)][SerializeField] float particleHighColorSpeed = 20.0f;
    [SerializeField] Mesh particleMesh;
    [SerializeField] Camera mainCamera;

    [Header("Raymarched Rendering")]
    [SerializeField] bool enableRaymarchShader = true;
    [SerializeField] float densityCacheStepSize = 0.05f;
    [SerializeField] float densityCacheSampleCount = 128;
    [SerializeField] bool useDensityStepSize = false;
    [Range(0.00005f, 10.0f)] [SerializeField] float densityMultiplier = 1.0f;
    [Range(0.005f, 100.0f)] [SerializeField] float lightMultiplier = 0.5f;
    [SerializeField] float3 extinctionCoefficients = 1.0f;
    [Range(0.1f, 10.0f)][SerializeField] float indexOfRefraction = 1.33f;
    [SerializeField] Transform lightTransform;
    [Range(1, 4)] [SerializeField] int numBounces = 2;
    [SerializeField] [Tooltip("Traces reflect and refract ray Li on iter 1 and adds them instead of following only refract or reflect.")] bool traceReflectAndRefract;
    [Range(0.001f, 10.0f)][SerializeField] float waterExistenceThreshold = 0.1f;
    [SerializeField] float waterExistenceEps = 0.05f;
    [SerializeField] float nextRayOffset = 0.0005f;
    [SerializeField] Cubemap environmentMap;

    [Header("Camera Controller Parameters")]
    [SerializeField] float2 cameraRotateSpeed;
    [SerializeField] float2 cameraPanSpeed;
    [SerializeField] float cameraZoomSpeed;

    private static SimulationParameters Ins;
    void Awake()
    {
        Ins = this;
    }

    void OnValidate()
    {
        // This Monobehavior func is called when a value changes
        GameManager.Ins?.simUniformer.UniformAllParameters();
        RaymarchManager.Ins?.UniformAllParameters();
        GameManager.Ins?.drawer.UniformParameters();
        if (RaymarchManager.Ins != null) RaymarchManager.Ins.enabled = EnableRaymarchShader;
    }

    void Update()
    {
        if (obstacleTransform.hasChanged || containerTransform.hasChanged || lightTransform.hasChanged)
        {
            obstacleTransform.hasChanged = false; containerTransform.hasChanged = false; lightTransform.hasChanged = false;
            GameManager.Ins?.simUniformer.UniformAllParameters();
            RaymarchManager.Ins.UpdateContainerData();
            RaymarchManager.Ins.UpdateObstacleData();
            RaymarchManager.Ins.UniformAllParameters();
        }
    }
}
