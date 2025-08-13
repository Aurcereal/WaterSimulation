using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "FluidRaymarchVisualPreset", menuName = "ScriptableObjects/FluidRaymarchVisualTemplate", order = 1)]
public class RaymarchVisualPreset : ScriptableObject
{
    [Range(0.00005f, 10.0f)][SerializeField] public float densityMultiplier = 1.0f;
    [Range(0.005f, 100.0f)][SerializeField] public float lightMultiplier = 0.5f;
    [SerializeField] public float3 extinctionCoefficients = 1.0f;
    [Range(0.1f, 10.0f)][SerializeField] public float indexOfRefraction = 1.33f;
    [Range(1, 4)][SerializeField] public int numBounces = 2; // TODO: make matter again
    [SerializeField][Tooltip("Traces reflect and refract ray Li on iter 1 and adds them instead of following only refract or reflect.")] public bool traceReflectAndRefract;

    [Header("Advanced")]
    [Range(0.001f, 10.0f)][SerializeField] public float waterExistenceThreshold = 0.1f;
    [SerializeField] public float waterExistenceEps = 0.05f;
    [SerializeField] public float nextRayOffset = 0.0005f;
    [SerializeField] public float densityCacheStepSize = 0.05f;
    [SerializeField] public float densityCacheSampleCount = 128;
    [SerializeField] public bool useDensityStepSize = false;

}
