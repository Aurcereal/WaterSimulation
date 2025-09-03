using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "FluidBehaviorPreset", menuName = "ScriptableObjects/FluidBehaviorTemplate", order = 1)]
public class FluidBehaviorPreset : ScriptableObject
{

    [Header("Force Toggles")]
    [SerializeField] public bool enableGravityForce = true;
    [SerializeField] public bool enablePressureForce = true;
    [SerializeField] public bool enableViscosityForce = true;
    [SerializeField] public bool enableSpringForce = true;
    [SerializeField] public bool enableStickForce = true;

    [Header("Gravity Force")]
    [SerializeField] public float3 gravity = new float3(0.0f, -9.8f, 0.0f);

    [Header("Pressure Force")]
    [Range(0.0f, 500.0f)][SerializeField] public float targetDensity = 10.0f;
    [Range(0.0f, 500.0f)][SerializeField] public float nearDensityPressureMultiplier = 1.0f;
    [Range(0.05f, 500.0f)][SerializeField] public float pressureMultiplier = 1.0f;

    [Header("Viscosity Force")]
    [Range(0.0f, 750.0f)][SerializeField] public float viscosityStrength = 4.0f;

    [Header("Spring Force")]
    [Range(0.0f, 1000000.0f)][SerializeField] public float springForceMultiplier = 50000f;
    [Range(0.0f, 1.0f)][SerializeField] public float springRestLenFac = 0.5f;

    [Header("Stickiness Force")]
    [Range(0.0f, 2.0f)][SerializeField] public float maxStickDistance = 0.07f;
    [Range(0.0f, 10000.0f)][SerializeField] public float stickForceMultiplier = 10.0f;

    [Header("Force Field")]
    public float forceFieldMultiplier = 1.0f;

    [Header("Advanced")]
    [Range(0.005f, 10.0f)][SerializeField] public float smoothingRadius = 0.1f;
}
