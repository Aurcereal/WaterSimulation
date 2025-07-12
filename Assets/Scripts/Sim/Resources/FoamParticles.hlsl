struct FoamParticle {
    float3 position;
    float3 velocity;
    float remainingLifetime;
    int debugType;
};

RWStructuredBuffer<FoamParticle> updatingFoamParticles;
RWStructuredBuffer<FoamParticle> survivingFoamParticles;
RWStructuredBuffer<uint> foamParticleCounts; // (updatingFoamParticleCount, survivingFoamParticleCount)
const uint MaxFoamParticleCount;

void SpawnFoamParticle(float3 pos, float3 vel, float lifetime) {
    if(foamParticleCounts[0] >= MaxFoamParticleCount) return; 

    FoamParticle particle;
    particle.position = pos;
    particle.velocity = vel;
    particle.remainingLifetime = lifetime; // TODO: parametrizeable or smth idk lifetime has more specifications (prolly still const mabe)
    particle.debugType = 1;

    int index;
    InterlockedAdd(foamParticleCounts[0], 1, index);
    updatingFoamParticles[index] = particle;

}

void CalculateOrthogonalBasis(float3 fo, out float3 ri, out float3 up) {
    if (abs(fo.x) > abs(fo.y))
            ri = float3(-fo.z, 0, fo.x) / sqrt(fo.x * fo.x + fo.z * fo.z);
        else
            ri = float3(0, fo.z, -fo.y) / sqrt(fo.y * fo.y + fo.z * fo.z);
        up = cross(fo, ri);
}

void SpawnFoamParticlesInCylinder(float time, float3 fluidParticlePos, float3 fluidParticleVel, float count, float cylRadius, float cylHeight, float3 fo) {
    // Fractional component of count will be treated as probability of spawning: count = 3.7 means 3 guaranteed particles and .7 chance of 4th particle
    int spawnCount = int(floor(count));
    float fracCount = frac(count);

    float3 randomState = fluidParticlePos + fluidParticleVel + time;

    if(hash31(randomState) < fracCount)
        ++spawnCount;

    //
    float3 ri, up;
    CalculateOrthogonalBasis(fo, ri, up);

    for(int i=0; i<spawnCount; i++) {
        randomState = hash33(randomState*100.);
        
        float3 polar = float3(randomState.x * cylRadius, randomState.y * TAU, randomState.z * cylHeight);
        float3 cylFloorPos = polar.x * (ri * cos(polar.y) + up * sin(polar.y));
        float3 spawnPos = fluidParticlePos + cylFloorPos + polar.z * fo;

        float3 spawnVel = fluidParticleVel + cylFloorPos;

        SpawnFoamParticle(spawnPos, spawnVel, 2. + 2. * randomState.y + count*1.);
    }
}

void UpdateFoamParticle(int updatingIndex, float dt) {
    FoamParticle particle = updatingFoamParticles[updatingIndex];

    // Update velocity
    particle.velocity = EstimateVelocity(particle.position);//particle.velocity += dt * (EstimateVelocity(particle.position) - particle.velocity);//

    particle.position += particle.velocity * dt;
    particle.remainingLifetime -= dt;
    particle.debugType = 0;

    float dScene = sdScene(particle.position) - FoamScaleMultiplier; // We have FoamScaleMultiplier = MaxFoamScale just for keeping all foam frags completely within bounds
    if(dScene < 0.) {
        float3 norm = normal(particle.position);
        particle.velocity -= 2. * norm * dot(norm, particle.velocity);
        particle.position -= norm * dScene;
    }

    if(particle.remainingLifetime > 0.) {
        int index;
        InterlockedAdd(foamParticleCounts[1], 1, index);
        survivingFoamParticles[index] = particle;
    }
}

void UpdateSprayParticle(int updatingIndex, float dt) {
    FoamParticle particle = updatingFoamParticles[updatingIndex];

    // Update velocity
    particle.velocity += dt * (Gravity - dot(particle.velocity, particle.velocity) * normalize(particle.velocity) * SprayAirDragMultiplier); // Can add external forces too

    particle.position += particle.velocity * dt;
    particle.remainingLifetime -= dt;
    particle.debugType = 1;

    float dScene = sdScene(particle.position) - FoamScaleMultiplier; // We have FoamScaleMultiplier = MaxFoamScale just for keeping all foam frags completely within bounds
    if(dScene < 0.) {
        float3 norm = normal(particle.position);
        particle.velocity -= 2. * norm * dot(norm, particle.velocity);
        particle.position -= norm * dScene;
    }

    if(particle.remainingLifetime > 0.) {
        int index;
        InterlockedAdd(foamParticleCounts[1], 1, index);
        survivingFoamParticles[index] = particle;
    }
}

void UpdateBubbleParticle(int updatingIndex, float dt) {
    FoamParticle particle = updatingFoamParticles[updatingIndex];

    // Update velocity
    particle.velocity += dt * BubbleGravityMultiplier * (-Gravity) + BubbleFluidConformingMultiplier * (EstimateVelocity(particle.position) - particle.velocity);

    particle.position += particle.velocity * dt;
    particle.remainingLifetime -= dt;
    particle.debugType = 2;

    float dScene = sdScene(particle.position) - FoamScaleMultiplier; // We have FoamScaleMultiplier = MaxFoamScale just for keeping all foam frags completely within bounds
    if(dScene < 0.) {
        float3 norm = normal(particle.position);
        particle.velocity -= 2. * norm * dot(norm, particle.velocity);
        particle.position -= norm * dScene;
    }

    if(particle.remainingLifetime > 0.) {
        int index;
        InterlockedAdd(foamParticleCounts[1], 1, index);
        survivingFoamParticles[index] = particle;
    }
}
