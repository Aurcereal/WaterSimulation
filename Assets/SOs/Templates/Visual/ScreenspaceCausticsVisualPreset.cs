using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "ScreenspaceCausticsVisualPreset", menuName = "ScriptableObjects/ScreenspaceCausticsVisualTemplate", order = 1)]
public class ScreenspaceCausticsVisualPreset : ScriptableObject
{
    
    // TODO: add non advanced stuff like multiplier on falloff and constant mult and additional extinction

    [Header("Advanced")]
    [SerializeField] public int causticsDepthNormalResolution = 1000; //
    [Range(1, 1000)] [SerializeField] public float causticsDepthWorldBlurRadius = 300; //
    [Range(1, 5)][SerializeField] public int causticsDepthBlurIterationCount = 2; //
}
