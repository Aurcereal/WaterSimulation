using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Mathematics;
using static Unity.Mathematics.math;
using System.Threading.Tasks;

using static SimulationParameters;

public class Drawer
{
    Material particleMaterial;

    Color[] colors;

    public Drawer()
    {
        particleMaterial = new Material(Shader.Find("Unlit/InstancedParticle"));
        particleMaterial.enableInstancing = true;

        colors = new Color[SimulationParameters.ParticleCount];

        particleMaterial.SetBuffer("positionBuffer", GameManager.Ins.computeManager.positionBuffer);
        particleMaterial.SetBuffer("colorBuffer", GameManager.Ins.computeManager.colorBuffer);
        particleMaterial.SetFloat("_Radius", SimulationParameters.ParticleRadius);
    }

    public void DrawContainer() {
        DrawUtils.DrawOutlineBox(float2(0.0f), 0.0f, SimulationParameters.BoxDimensions, 0.1f, Color.white);
    }

    // public void UpdateParticleColors()
    // {
    //     Parallel.For(0, ParticleCount,
    //         i => colors[i] = ParticleSpeedGradient.Evaluate(
    //             smoothstep(ParticleColorSpeedRange.x, ParticleColorSpeedRange.y, length(GameManager.Ins.simUpdater.velocities[i]))
    //             )
    //         );
    //     GameManager.Ins.computeManager.UpdateColorBuffer(colors);
    // }

    public void DrawParticles()
    {
        particleMaterial.SetFloat("_Radius", SimulationParameters.ParticleRadius); // TODO: take out in update I put it in so it changes as user updates
        Bounds bounds = new Bounds(Vector3.zero, Vector3.one * 100f);
        Graphics.DrawMeshInstancedProcedural(MeshUtils.QuadMesh, 0, particleMaterial, bounds, SimulationParameters.ParticleCount);
    }
}
