using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

using static Unity.Mathematics.math;

public class GameManager : MonoBehaviour
{
    public static GameManager Ins {get; private set;}

    public InputManager inputManager;
    public ComputeManager computeManager;
    public Drawer drawer;
    public ParticleSimulator particleSimulator;
    public SpatialHash spatialHash;

    void Start()
    {
        Ins = this;
        ResetSimulation();
    }

    void ResetSimulation()
    {
        computeManager?.Destructor();

        inputManager = new();
        computeManager = new();
        drawer = new();
        particleSimulator = new();
        spatialHash = new();
    }

    void Update()
    {
        inputManager.Update();

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
