using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

using static Unity.Mathematics.math;

using static SimulationParameters;

public class GameManager : MonoBehaviour
{
    public static GameManager Ins { get; private set; }

    public InputManager inputManager;

    public ComputeManager computeManager;
    public SimulationUniformer simUniformer;
    public SimulationInitializer simInitializer;
    public SimulationUpdater simUpdater;
    public SimulationTimeController simTimeController;

    public BitonicSortManager bitonicSorter;
    public Drawer drawer;

    public CameraController camController;

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
        simTimeController = new();

        bitonicSorter = new();
        drawer = new();

        camController = new(MainCamera.transform.position, float3(0));

        PostProcessManager.Ins.UniformAllParameters();
        PostProcessManager.Ins.UpdateCameraData();
        PostProcessManager.Ins.UpdateContainerData();
    }

    int counter = 1;

    void Update()
    {
        inputManager.Update();

        camController.Update();
        MainCamera.transform.position = camController.Position;
        MainCamera.transform.rotation = camController.Rotation;

        if (inputManager.KeyDownR)
            ResetSimulation();

        simTimeController.UpdateState();

        if (simTimeController.ShouldUpdate())
            simUpdater.Update(simTimeController.GetDeltaTime());

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
