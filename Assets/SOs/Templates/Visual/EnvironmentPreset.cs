using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "EnvironmentPreset", menuName = "ScriptableObjects/EnvironmentTemplate", order = 1)]
public class EnvironmentPreset : ScriptableObject
{
    // what is this. maybe remove it and just use an environment override stuff in simulation params
    [Header("Skybox")]
    [SerializeField] public Cubemap environmentMap;

    [Header("Obstacle")]
    [SerializeField] public bool obstacleType; // can later make stuff for objects aside from environment to move and stuff.. and color

    // TODO: connect up floor stuff
    public enum EnvironmentFloorType
    {
        NONE,
        CHECKER,
        SQUIGGLE,
        RADIALCIRCLE
    }
    [Header("Floor")]
    [SerializeField] public EnvironmentFloorType environmentFloorType;
    [SerializeField] public Color floorColor1;
    [SerializeField] public Color floorColor2;
    [SerializeField] public Color floorColor3;

}
