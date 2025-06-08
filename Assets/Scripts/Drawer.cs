using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Mathematics;
using static Unity.Mathematics.math;
using System.Threading.Tasks;

using static SimulationParameters;
using UnityEngine.Rendering;

public class Drawer
{
    Material particleMaterial;

    Color[] colors;

    public Drawer()
    {
        particleMaterial = new Material(Shader.Find("Unlit/InstancedParticle3D"));
        particleMaterial.enableInstancing = true;

        colors = new Color[SimulationParameters.ParticleCount];

        particleMaterial.SetBuffer("positionBuffer", GameManager.Ins.computeManager.positionBuffer);
        particleMaterial.SetBuffer("colorBuffer", GameManager.Ins.computeManager.colorBuffer);
        UniformParameters();
    }

    public void UniformParameters()
    {
        particleMaterial.SetFloat("_Radius", SimulationParameters.ParticleRadius);
    }

    public void DrawParticles()
    {
        Bounds bounds = new Bounds(Vector3.zero, Vector3.one * 100f);
        Graphics.DrawMeshInstancedProcedural(ParticleMesh, 0, particleMaterial, bounds, ParticleCount);
    }

    public void DrawParticlesOnCommandBuffer(CommandBuffer cmd)
    {
        Bounds bounds = new Bounds(Vector3.zero, Vector3.one * 100f);
        cmd.DrawMeshInstancedProcedural(ParticleMesh, 0, particleMaterial, 0, ParticleCount);
    }
}
