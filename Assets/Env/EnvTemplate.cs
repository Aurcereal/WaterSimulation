using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "EnvPreset", menuName = "ScriptableObjects/Env/EnvTemplate", order = 1)]
public class EnvTemplate : ScriptableObject
{

    [SerializeField] public string physicsCompileKeyword; // For ParticleSimulator
    [SerializeField] public string visualCompileKeyword; // For SDFScene

    [SerializeField] public bool enableObjectInteraction;
    [SerializeField] public bool enableBoundingBoxInteraction;

}
