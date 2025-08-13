using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "FluidDebugVisualPreset", menuName = "ScriptableObjects/FluidDebugVisualTemplate", order = 1)]
public class DebugVisualPreset : ScriptableObject
{

    [Range(0.01f, 100f)][SerializeField] public float particleLowColorSpeed = 0.0f;
    [Range(0.01f, 100f)][SerializeField] public float particleHighColorSpeed = 20.0f;

    // TODO: add ball color?

}
