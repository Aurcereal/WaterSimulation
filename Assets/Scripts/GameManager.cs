using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

using static Unity.Mathematics.math;

public class GameManager : MonoBehaviour
{
    public static GameManager Ins { get; private set; }

    public InputManager inputManager;

    public ComputeManager computeManager;
    public SimulationUniformer simUniformer;
    public SimulationInitializer simInitializer;
    public SimulationUpdater simUpdater;

    public BitonicSortManager bitonicSorter;
    public Drawer drawer;

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

        simUniformer = new();
        simUniformer.UniformAllBuffers();
        simUniformer.UniformAllParameters();

        simInitializer = new();
        simInitializer.InitializeSimulation();

        simUpdater = new();

        bitonicSorter = new();
        drawer = new();

        PostProcessManager.Ins.SetupShaderUniforms();
        PostProcessManager.Ins.UpdateCameraData();
        PostProcessManager.Ins.UpdateContainerData();
    }

    int counter = 1;

    void Update()
    {
        inputManager.Update();

        if (inputManager.KeyDownR)
            ResetSimulation();

        simUpdater.Update(Time.deltaTime);
        if (counter >= 1) { PostProcessManager.Ins.CacheDensities(); counter = 0; } // Takes up ton of time . .
        else ++counter;

        drawer.DrawParticles();
        drawer.DrawBoxAndObstacle();
    }

    void OnDisable()
    {
        computeManager?.Destructor();
    }

}
