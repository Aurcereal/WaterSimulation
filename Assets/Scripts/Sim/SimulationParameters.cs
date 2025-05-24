using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

public class SimulationParameters : MonoBehaviour
{

    public static int ParticleCount => Ins.particleCount;
    public static float2 BoxDimensions => new(Ins.boxWidth, Ins.boxHeight);
    public static float2 SpawnDimensions => new(Ins.spawnWidth, Ins.spawnHeight);
    public static float BoxThickness => Ins.boxThickness;
    public static float2 Gravity => Ins.gravity;
    public static float ParticleRadius => Ins.particleRadius;
    public static float SmoothingRadius => Ins.smoothingRadius;
    public static float MouseForceRadius => Ins.mouseForceRadius;
    public static float MouseForceStrength => Ins.mouseForceStrength;
    public static float TargetDensity => Ins.targetDensity;
    public static float NearDensityPressureMultiplier => Ins.nearDensityPressureMultiplier;
    public static float PressureMultiplier => Ins.pressureMultiplier;
    public static Gradient ParticleSpeedGradient => Ins.particleSpeedGradient;
    public static float2 ParticleColorSpeedRange => new(Ins.particleLowColorSpeed, Ins.particleHighColorSpeed);
    public static float ViscosityStrength => Ins.viscosityStrength;
    public static float SurfaceTensionMultiplier => Ins.surfaceTensionMultiplier;
    public static bool EnableParticleSprings => Ins.enableParticleSprings;
    public static float SpringForceMultiplier => Ins.springForceMultiplier;
    public static float Plasticity => Ins.plasticity;
    public static float SpringYieldRatio => Ins.springYieldRatio;
    public static float MaxStickDistance => Ins.maxStickDistance;
    public static float StickForceMultiplier => Ins.stickForceMultiplier;
    public static float2 ObstacleDimensions => float2(Ins.obstacleTransform.localScale.x, Ins.obstacleTransform.localScale.y);
    public static float2 ObstaclePosition => float2(Ins.obstacleTransform.position.x, Ins.obstacleTransform.position.y);
    public static float ObstacleRotation => radians(Ins.obstacleTransform.localRotation.eulerAngles.z);
    public static bool IsObstacleBox => Ins.obstacleType;

    public const int SpatialLookupSize = 512;
    public static float GridSize => SmoothingRadius;

    [Header("Initialization Parameters")]
    [Range(1, 40000)][SerializeField] int particleCount = 10;
    [Range(0.05f, 100)][SerializeField] float spawnWidth = 50.0f;
    [Range(0.05f, 100)][SerializeField] float spawnHeight = 40.0f;

    [Header("Misc Parameters")]
    [Range(0.005f, 10.0f)][SerializeField] float smoothingRadius = 0.1f;

    [Header("Box/Obstacle Parameters")]
    [Range(2, 200)][SerializeField] float boxWidth = 50.0f;
    [Range(2, 100)][SerializeField] float boxHeight = 40.0f;
    [SerializeField] Transform obstacleTransform;
    [SerializeField] bool obstacleType;

    [Header("Gravity Force")]
    [SerializeField] float2 gravity = new float2(0.0f, -9.8f);

    [Header("Mouse Force")]
    [Range(0.005f, 10.0f)][SerializeField] float mouseForceRadius = 4.0f;
    [Range(0.0f, 5000.0f)][SerializeField] float mouseForceStrength = 100.0f;

    [Header("Pressure Force")]
    [Range(0.0f, 100.0f)][SerializeField] float targetDensity = 10.0f;
    [Range(0.0f, 500.0f)][SerializeField] float nearDensityPressureMultiplier = 1.0f;
    [Range(0.05f, 500.0f)][SerializeField] float pressureMultiplier = 1.0f;

    [Header("Viscosity Force")]
    [Range(0.0f, 500.0f)][SerializeField] float viscosityStrength = 4.0f;

    [Header("Surface Tension Force")]
    [Range(0.0f, 500.0f)][SerializeField] float surfaceTensionMultiplier = 5.0f;

    [Header("Spring Force")]
    [Tooltip("Requires Restart")] [SerializeField] bool enableParticleSprings = false;
    [Range(0.0f, 200000.0f)][SerializeField] float springForceMultiplier = 50000f;
    [Range(0.0f, 10.0f)][SerializeField] float plasticity = 0f;
    [Range(0.0f, 10.0f)][SerializeField] float springYieldRatio = 0.1f;

    [Header("Stickiness Force")]
    [Range(0.0f, 2.0f)][SerializeField] float maxStickDistance = 0.07f;
    [Range(0.0f, 10000.0f)][SerializeField] float stickForceMultiplier = 10.0f;

    [Header("Mostly Visual")]
    [Range(0.025f, 1.0f)][SerializeField] float particleRadius = 0.05f;
    [Range(0.05f, 1.0f)][SerializeField] float boxThickness = 0.1f;
    [SerializeField] Gradient particleSpeedGradient;
    [Range(0.01f, 100f)][SerializeField] float particleLowColorSpeed = 0.0f;
    [Range(0.01f, 100f)][SerializeField] float particleHighColorSpeed = 20.0f;

    private static SimulationParameters Ins;
    void Awake()
    {
        Ins = this;
    }

    void OnValidate()
    {
        // This Monobehavior func is called when a value changes
        GameManager.Ins?.simUniformer.UniformAllParameters();
    }

    void Update()
    {
        if (obstacleTransform.hasChanged)
        {
            obstacleTransform.hasChanged = false;
            GameManager.Ins?.simUniformer.UniformAllParameters();
        }
    }
}
