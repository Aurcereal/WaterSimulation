using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

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
    public static float PressureMultiplier => Ins.pressureMultiplier;
    public static Gradient ParticleSpeedGradient => Ins.particleSpeedGradient;
    public static float2 ParticleColorSpeedRange => new(Ins.particleLowColorSpeed, Ins.particleHighColorSpeed);
    public static float ViscosityStrength => Ins.viscosityStrength;

    [Header("Physics Parameters")]
    [Range(1, 2000)] [SerializeField] int particleCount = 10;
    [Range(2, 100)] [SerializeField] float boxWidth = 50.0f;
    [Range(2, 100)] [SerializeField] float boxHeight = 40.0f;
    [Range(2, 100)] [SerializeField] float spawnWidth = 50.0f;
    [Range(2, 100)] [SerializeField] float spawnHeight = 40.0f;
    [SerializeField] float2 gravity = new float2(0.0f, -9.8f);
    [Range(0.005f, 10.0f)] [SerializeField] float smoothingRadius = 0.1f;
    [Range(0.005f, 10.0f)][SerializeField] float mouseForceRadius = 4.0f;
    [Range(0.0f, 1000.0f)][SerializeField] float mouseForceStrength = 100.0f;
    [Range(0.0f, 100.0f)] [SerializeField] float targetDensity = 10.0f;
    [Range(0.05f, 500.0f)] [SerializeField] float pressureMultiplier = 1.0f;
    [Range(0.0f, 100.0f)][SerializeField] float viscosityStrength = 4.0f;

    [Header("Mostly Visual")]
    [Range(0.025f, 1.0f)] [SerializeField] float particleRadius = 0.05f;
    [Range(0.05f, 1.0f)] [SerializeField] float boxThickness = 0.1f;
    [SerializeField] Gradient particleSpeedGradient;
    [Range(0.01f, 100f)][SerializeField] float particleLowColorSpeed = 0.0f;
    [Range(0.01f, 100f)][SerializeField] float particleHighColorSpeed = 20.0f;

    private static SimulationParameters Ins;
    void Awake() {
        Ins = this;
    }
}
