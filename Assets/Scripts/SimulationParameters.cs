using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class SimulationParameters : MonoBehaviour
{

    public static int ParticleCount => Ins.particleCount;
    public static float2 BoxDimensions => new(Ins.boxWidth, Ins.boxHeight);
    public static float BoxThickness => Ins.boxThickness;
    public static float Gravity => Ins.gravity;
    public static float ParticleRadius => Ins.particleRadius;

    //
    [Range(10, 500)] [SerializeField] int particleCount = 10;
    [Range(10, 100)] [SerializeField] float boxWidth = 50.0f;
    [Range(10, 100)] [SerializeField] float boxHeight = 40.0f;
    [Range(0.05f, 1.0f)] [SerializeField] float boxThickness = 0.1f;
    [Range(1, 20)] [SerializeField] float gravity = 9.8f;
    [Range(0.025f, 1.0f)] [SerializeField] float particleRadius = 0.05f;

    private static SimulationParameters Ins;
    void Awake() {
        Ins = this;
    }
}
