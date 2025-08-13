using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "RaymarchCausticsVisualPreset", menuName = "ScriptableObjects/RaymarchCausticsVisualTemplate", order = 1)]
public class RaymarchCausticsVisualPreset : ScriptableObject
{

    [SerializeField] float dummy;
    // TODO: add non advanced stuff like multiplier on falloff and constant mult and additional extinction

}
