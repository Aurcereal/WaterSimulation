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
    public CountSortManager waterParticleCountSorter;
    public CountSortManager foamParticleCountSorter;

    public CameraController camController;

    public SimulationFoamManager simFoamManager;
    public ShadowMapManager shadowMapManager;
    public CausticsManager causticsManager;

    public ScreenSpaceWaterManager screenSpaceManager;
    public DebugRenderingManager debugRenderingManager;
    public RaymarchManager raymarchManager => RaymarchManager.Ins;

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
        simFoamManager = new();
        shadowMapManager = new();
        causticsManager = new();

        bitonicSorter = new();

        waterParticleCountSorter = new(ParticleCount, SpatialLookupSize, computeManager.particleCellKeyEntryBuffer);
        foamParticleCountSorter = new(MaxFoamParticleCount, FoamSpatialLookupSize, computeManager.foamParticleCellKeyEntryBuffer, true, computeManager.foamParticleCounts);

        camController = new(MainCamera.transform.position, float3(0));

        screenSpaceManager = new();
        debugRenderingManager = new();

        camController.SetGlobalUniformCameraData();
        raymarchManager.UniformAllParameters();
        raymarchManager.UpdateContainerData();
        if (raymarchManager != null) raymarchManager.enabled = CurrentVisualMode == VisualMode.Raymarched;
        screenSpaceManager.UpdateObstacleData();

        prevVisualMode = null;
        HandleVisualModeSwitching();

        if (UseCaustics)
        {
            // Since it's on startup right now will only change on restart, can add listener
            causticsManager.OnEnable();
        }
        else
        {
            causticsManager.OnDisable();
        }

        if (UseShadows)
        {
            // Since it's on startup right now will only change on restart, can add listener
            shadowMapManager.OnEnable();
        }
        else
        {
            shadowMapManager.OnDisable();
        }

        HandleNewEnv();
    }

    int counter = 1;
    VisualMode? prevVisualMode;

    void HandleNewEnv()
    {
        screenSpaceManager.HandleNewEnv();
        raymarchManager.HandleNewEnv();
        simUniformer.HandleNewEnv();
    }

    void HandleVisualModeSwitching()
    {
        if (prevVisualMode != CurrentVisualMode)
        {
            switch (prevVisualMode)
            {
                case VisualMode.DebugSpheres:
                    debugRenderingManager.OnDisable();
                    break;
                case VisualMode.Raymarched:
                    RaymarchManager.Ins.OnDisable();
                    break;
                case VisualMode.Screenspace:
                    screenSpaceManager.OnDisable();
                    break;
            }

            switch (CurrentVisualMode)
            {
                case VisualMode.DebugSpheres:
                    debugRenderingManager.OnEnable();
                    break;
                case VisualMode.Raymarched:
                    RaymarchManager.Ins.OnEnable();
                    break;
                case VisualMode.Screenspace:
                    screenSpaceManager.OnEnable();
                    break;
            }
        }
    }

    void Update()
    {
        #region Input and Camera
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
        #endregion

        #region Update Sim
        simTimeController.UpdateState();

        if (simTimeController.ShouldUpdate())
            simUpdater.Update(simTimeController.GetDeltaTime());
        #endregion

        HandleVisualModeSwitching();

        if (CurrentVisualMode == VisualMode.Raymarched)
        {
            if (counter >= 1) { RaymarchManager.Ins.CacheDensities(); counter = 0; } // Takes up ton of time . .
            else ++counter;
        }

        switch (CurrentVisualMode)
        {
            case VisualMode.DebugSpheres:
                // Debug Draw Spheres using ScreenSpaceManager CMD and stuff
                debugRenderingManager.Draw();
                break;
            case VisualMode.Raymarched:
                if (UseBillboardFoam) RaymarchManager.Ins.DrawFoam();
                // The actual drawing to screen will happen in RaymarchManager.OnRenderImage
                break;
            case VisualMode.Screenspace:
                // Put shadowmap and caustics in screenspace
                if (UseCaustics) causticsManager.DrawTextures();
                if (UseShadows) shadowMapManager.DrawShadows();
                screenSpaceManager.Draw();
                break;
        }

        prevVisualMode = CurrentVisualMode;
    }

    void OnDisable()
    {
        computeManager?.Destructor();
        waterParticleCountSorter?.Destructor();
    }

}
