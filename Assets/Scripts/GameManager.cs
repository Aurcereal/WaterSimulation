using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

using static Unity.Mathematics.math;

using static SimulationParameters;

[RequireComponent(typeof(ResolutionTracker))]
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

    public CameraController camController;

    public ScreenSpaceWaterManager screenSpaceManager;

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

        camController = new(MainCamera.transform.position, float3(0));

        screenSpaceManager = new();

        RaymarchManager.Ins.UniformAllParameters();
        camController.SetGlobalUniformCameraData();
        RaymarchManager.Ins.UpdateContainerData();
        if (RaymarchManager.Ins != null) RaymarchManager.Ins.enabled = EnableRaymarchShader;
    }

    int counter = 1;

    void Update()
    {
        inputManager.Update();

        camController.Update();
        MainCamera.transform.position = camController.Position;
        MainCamera.transform.rotation = camController.Rotation;

        if (MainCamera.transform.hasChanged)
        {
            MainCamera.transform.hasChanged = false;
            camController.SetGlobalUniformCameraData();
        }

        if (inputManager.KeyDownR)
            ResetSimulation();

        simTimeController.UpdateState();

        if (simTimeController.ShouldUpdate())
            simUpdater.Update(simTimeController.GetDeltaTime());

        if (EnableRaymarchShader)
        {
            if (counter >= 1) { RaymarchManager.Ins.CacheDensities(); counter = 0; } // Takes up ton of time . .
            else ++counter;
        }

        if (!EnableRaymarchShader)
            screenSpaceManager.Draw();
    }

    void OnDisable()
    {
        computeManager?.Destructor();
    }

}
