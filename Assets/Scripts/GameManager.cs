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
    public CountSortManager countSorter;
    public OddEvenSortManager oddEvenSorter;

    public CameraController camController;

    public ScreenSpaceWaterManager screenSpaceManager;
    public SimulationFoamParticleManager simFoamManager;
    public ShadowMapManager shadowMapManager;

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

        bitonicSorter = new();
        if (UseOddEvenSort) // OES
            oddEvenSorter = new();
        countSorter = new();

        camController = new(MainCamera.transform.position, float3(0));

        screenSpaceManager = new();

        RaymarchManager.Ins.UniformAllParameters();
        camController.SetGlobalUniformCameraData();
        RaymarchManager.Ins.UpdateContainerData();
        if (RaymarchManager.Ins != null) RaymarchManager.Ins.enabled = EnableRaymarchShader;
        screenSpaceManager.UpdateObstacleData();

        if (EnableRaymarchShader)
        {
            screenSpaceManager.OnDisable();
            RaymarchManager.Ins.OnEnable();
        }
        else
        {
            RaymarchManager.Ins.OnDisable();
            screenSpaceManager.OnEnable();
        }
    }

    int counter = 1;
    bool prevEnableRaymarchShader;

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

        if (EnableRaymarchShader != prevEnableRaymarchShader)
        {
            if (EnableRaymarchShader)
            {
                screenSpaceManager.OnDisable();
                RaymarchManager.Ins.OnEnable();
            }
            else
            {
                RaymarchManager.Ins.OnDisable();
                screenSpaceManager.OnEnable();
            }
        }

        if (EnableRaymarchShader)
        {
            RaymarchManager.Ins.DrawFoam();
        }
        else
        {
            if(UseShadowMapping) shadowMapManager.DrawShadows();
            screenSpaceManager.Draw();
        }

        prevEnableRaymarchShader = EnableRaymarchShader;
    }

    void OnDisable()
    {
        computeManager?.Destructor();
        countSorter?.Destructor();
    }

}
