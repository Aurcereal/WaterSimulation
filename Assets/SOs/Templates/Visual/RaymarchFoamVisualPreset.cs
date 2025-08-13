using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "RaymarchFoamVisualPreset", menuName = "ScriptableObjects/RaymarchFoamVisualTemplate", order = 1)]
public class RaymarchFoamVisualPreset : ScriptableObject
{

    // TODO: Could parametrize more like foam color
    [SerializeField] public float foamVolumeRadius = 0.01f;

}
