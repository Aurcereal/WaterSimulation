struct FoamParticle {
    float3 position;
    float3 velocity;
    float remainingLifetime;
};

RWStructuredBuffer<FoamParticle> updatingFoamParticles;
RWStructuredBuffer<FoamParticle> survivingFoamParticles;
RWStructuredBuffer<uint> foamParticleCounts; // (updatingFoamParticleCount, survivingFoamParticleCount)
const uint MaxFoamParticleCount;

void SpawnFoamParticle(float3 pos) {
    if(foamParticleCounts[0] >= MaxFoamParticleCount) return; 

    FoamParticle particle;
    particle.position = pos;
    particle.velocity = float3(0.,100.,0.);
    particle.remainingLifetime = 1.;

    int index;
    InterlockedAdd(foamParticleCounts[0], 1, index);
    updatingFoamParticles[index] = particle;

}

void UpdateFoamParticle(int updatingIndex, float dt) {
    FoamParticle particle = updatingFoamParticles[updatingIndex];

    particle.position += particle.velocity * dt;
    particle.remainingLifetime -= dt;

    if(particle.remainingLifetime > 0.) {
        int index;
        InterlockedAdd(foamParticleCounts[1], 1, index);
        survivingFoamParticles[index] = particle;
    }
}
