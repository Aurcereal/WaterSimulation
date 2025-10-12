using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

[CreateAssetMenu(fileName = "EnvPreset", menuName = "ScriptableObjects/Env/EnvTemplate", order = 1)]
public class EnvTemplate : ScriptableObject
{

    // TODO:  make it an in-editor box instead of 3 dimensions here
    [Header("Initialization")]
    [Range(1, 200000)][SerializeField] public int particleCount = 10;
    [Range(0.05f, 100)][SerializeField] public float spawnWidth = 50.0f;
    [Range(0.05f, 100)][SerializeField] public float spawnHeight = 40.0f;
    [Range(0.05f, 100)][SerializeField] public float spawnDepth = 40.0f;
    [SerializeField] public float3 spawnPosition = 0f;

    [Header("Keywords")]
    [SerializeField] public string physicsCompileKeyword; // For ParticleSimulator
    [SerializeField] public string visualCompileKeyword; // For SDFScene

    [Header("Physics Toggles")]
    [SerializeField] public bool enableObjectInteraction;
    [SerializeField] public bool enableBoundingBoxInteraction;

    [Header("Sun")]
    [SerializeField] public float sunRadius = 0.1f;
    [SerializeField] public float sunMultiplier = 1f;

    [Header("Bounding Box")]
    public float3 boundingBoxPosition;
    public float3 boundingBoxDimensions = float3(1f);

    [Header("Object")]
    public bool obstacleType;

    [Header("Fade Ins")]
    public float2 pressureFadeIn = float2(0.8f, 2.0f);
    public float2 foamFadeIn = float2(2.5f, 3.5f);

}
