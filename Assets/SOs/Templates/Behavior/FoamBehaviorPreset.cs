using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "FoamBehaviorPreset", menuName = "ScriptableObjects/FoamBehaviorTemplate", order = 1)]
public class FoamBehaviorPreset : ScriptableObject
{
    
    [SerializeField] public int maxFoamParticleCount = 1048576;

    [Header("Spawning")]
    [Range(0.0f, 200.0f)] [SerializeField] public float trappedAirPotentialRemapLow = 1.0f;
    [Range(0.0f, 200.0f)] [SerializeField] public float trappedAirPotentialRemapHigh = 4.0f;
    [Range(0.0f, 200.0f)] [SerializeField] public float trappedAirMultiplier = 1.0f;
    [Range(0.0f, 200.0f)] [SerializeField] public float kineticPotentialRemapLow = 1.0f;
    [Range(0.0f, 200.0f)] [SerializeField] public float kineticPotentialRemapHigh = 4.0f;

    [Header("Spray/Foam/Bubble Classification")]
    [Range(0.0f, 50.0f)][SerializeField] public float highestSprayDensity = 0.8f;
    [Range(0.0f, 50.0f)][SerializeField] public float lowestBubbleDensity = 2.0f;

    [Header("Physical Behavior")]
    [Range(0.0f, 200.0f)][SerializeField] public float bubbleGravityMultiplier = 1.0f;
    [Range(0.0f, 200.0f)][SerializeField] public float bubbleFluidConformingMultiplier = 1.0f;
    [Range(0.0f, 200.0f)][SerializeField] public float sprayAirDragMultiplier = 1.0f;
}
