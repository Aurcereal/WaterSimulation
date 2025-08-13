using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "FluidScreenspaceVisualPreset", menuName = "ScriptableObjects/FluidScreenspaceVisualTemplate", order = 1)]
public class ScreenspaceVisualPreset : ScriptableObject
{

    [Range(0.00005f, 10.0f)][SerializeField] public float screenSpaceDensityMultiplier = 0.118f;
    [SerializeField] public float3 screenSpaceExtinctionCoefficients = 1.0f;
    [Range(0.005f, 100.0f)][SerializeField] public float screenSpaceLightMultiplier = 1f;
    [Range(0.1f, 10.0f)][SerializeField] public float indexOfRefraction = 1.33f;

    [Header("Advanced")]
    [SerializeField] public int shadowMapResolution = 1000;
    [Range(1, 1000)][SerializeField] public float depthWorldBlurRadius = 10;
    [Range(0.0f, 100.0f)][SerializeField] public float depthBlurBilteralFalloff = 1.0f;
    [Range(1, 5)][SerializeField] public int depthBlurIterationCount;
    [SerializeField] public float depthDifferenceCutoffForNormals = 0.5f;

}
