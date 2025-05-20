using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

using static Unity.Mathematics.math;

public class GameManager : MonoBehaviour
{
    public static GameManager Ins {get; private set;}

    public InputManager inputManager;
    public ComputeBufferManager computeManager;
    public Drawer drawer;
    public ParticleSimulator particleSimulator;
    public SpatialHash spatialHash;

    void Start()
    {
        SpatialHash.ParticleEntry[] entries = new SpatialHash.ParticleEntry[] { new(1, 4), new(2, 2), new(3, 1), new(4, 5), new(14, 100), new(5, -10), new(6, 1), new(7, 3), new(8, -4) };
        ComputeBuffer particleEntries = ComputeHelper.CreateBuffer(entries);
        BitonicSortManager bitonicSorter = new(particleEntries, entries.Length);
        bitonicSorter.SortParticleEntries();
        particleEntries.GetData(entries);
        Debug.Log("Final Arr");
        for (int i = 0; i < entries.Length; i++)
        {
            Debug.Log($"Key: {entries[i].cellKey}, Particle Index: {entries[i].particleIndex}");
        }
        // Ins = this;
        // ResetSimulation();
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
        // inputManager.Update();

        // if (inputManager.KeyDownR)
        //     ResetSimulation();

        // particleSimulator.Update(Time.deltaTime);

        // computeManager.UpdatePositionBuffer(particleSimulator.positions);
        // drawer.UpdateParticleColors();
        // drawer.DrawParticles();
        // drawer.DrawContainer();
    }
}
