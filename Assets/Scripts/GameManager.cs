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
    public ParticleManager particleManager;

    void Start()
    {
        Ins = this;

        computeManager = new();
        drawer = new();
        particleManager = new();
    }

    void Update()
    {
        particleManager.Update(Time.deltaTime);
        computeManager.UpdatePositionBuffer(particleManager.positions);
        drawer.DrawParticles();
        drawer.DrawContainer();
    }
}
