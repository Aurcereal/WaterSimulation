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
        ResetSimulation();
    }

    void ResetSimulation()
    {
        computeManager?.Destructor();

        computeManager = new();
        drawer = new();
        particleSimulator = new();
        spatialHash = new();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            ResetSimulation();

        particleSimulator.Update(Time.deltaTime);

        // Color[] colors = new Color[SimulationParameters.ParticleCount];
        // for (int i = 0; i < colors.Length; i++) colors[i] = Color.white;
        // spatialHash.ForEachParticleWithinSmoothingRadius(WorldMousePosition, i => colors[i] = Color.red);
        // computeManager.UpdateColorBuffer(colors);

        computeManager.UpdatePositionBuffer(particleSimulator.positions);
        drawer.UpdateParticleColors();
        drawer.DrawParticles();
        drawer.DrawContainer();
    }
}
