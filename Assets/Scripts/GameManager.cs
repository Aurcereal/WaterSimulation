using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

using static Unity.Mathematics.math;

public class GameManager : MonoBehaviour
{
    public static GameManager Ins {get; private set;}

    public ComputeManager computeManager;
    public Drawer drawer;
    public ParticleSimulator particleSimulator;
    public SpatialHash spatialHash;

    public static float2 WorldMousePosition => (Vector2)Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane));

    void Start()
    {
        Ins = this;

        computeManager = new();
        drawer = new();
        particleSimulator = new();
        spatialHash = new();
    }

    void Update()
    {
        particleSimulator.Update(Time.deltaTime);
        spatialHash.UpdateSpatialHash();
        spatialHash.ForEachParticleWithinSmoothingRadius(WorldMousePosition, null);

        computeManager.UpdatePositionBuffer(particleSimulator.positions);
        drawer.DrawParticles();
        drawer.DrawContainer();
    }
}
