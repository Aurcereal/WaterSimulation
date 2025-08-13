using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "ScreenspaceFoamVisualPreset", menuName = "ScriptableObjects/ScreenspaceFoamVisualTemplate", order = 1)]
public class ScreenspaceFoamVisualPreset : ScriptableObject
{

    // TODO: foam color
    [Range(0.0f, 200.0f)][SerializeField] public float foamScaleMultiplier = 1.0f;

}
