struct ParticleEntry {
    int particleIndex;
    int key;
};

struct FoamParticle {
    float3 position;
    float3 velocity;
    float remainingLifetime;
    int debugType;
};

StructuredBuffer<int> cellKeyToStartCoord;
StructuredBuffer<ParticleEntry> foamParticleEntries;
StructuredBuffer<FoamParticle> updatingFoamParticles; // TODO: Would be nice if actual foam particles were sorted not entries,we don't need ids to stay unique anyways, we could keep swapping them doesn't really work with water particles where there's lot sof parallel buffers, well this probably isn't a performance bottleneck
StructuredBuffer<int> foamParticleCounts;

float FoamVolumeRadius; //

///
const int FoamSpatialLookupSize; //
const float FoamGridSize; //

int imod(int i, int m)
{
    if (i >= 0) return uint(i) % uint(m);
    else return m - 1 - (uint(-i) % uint(m));
}

int hash31(int3 coord)
{
    return imod(949937 + 119227 * coord.x + 370673 * coord.y + 440537 * coord.z, FoamSpatialLookupSize);
}

int getStartIndex(int key) {
    return cellKeyToStartCoord[key];
}

int getCellKey(int3 cellPos) {
    return hash31(cellPos); //RVS
}

int3 posToCell(float3 pos) {
    return int3(floor(pos/FoamGridSize));
}
///

// ODOT: Try tex it might be faster
// No, would take way to long to cache at a good res
float CheckFoamInsideVolumeRadius(float3 pos) {
    int3 centerCellPos = posToCell(pos);
    int3 currCellPos;
    int currIndex;
    int key;

    float SqrVolumeRadius = FoamVolumeRadius*FoamVolumeRadius;
    float foundFoam = 0.;

    bool allowContinue = true;

    // for(int x=-1; x<=1; x++) {
    //     for(int y=-1; y<=1; y++) {
    //         for(int z=-1; z<=1; z++) {
                currCellPos = centerCellPos;// + int3(x, y, z);
                key = getCellKey(currCellPos);
                currIndex = getStartIndex(key);
                if(currIndex < 0) return foundFoam;//continue;

                while(allowContinue && currIndex < foamParticleCounts[0] && foamParticleEntries[currIndex].key == key) {
                    float3 oPos = updatingFoamParticles[foamParticleEntries[currIndex].particleIndex].position;
                    float oLife = updatingFoamParticles[foamParticleEntries[currIndex].particleIndex].remainingLifetime;
                    float oScale = min(1., oLife);
                    float3 diff = pos - oPos;
                    if(dot(diff, diff) <= SqrVolumeRadius * oScale * oScale) {
                        foundFoam = 1.; return foundFoam; break; allowContinue = false;
                    }
                    ++currIndex;
                }
    //         }
    //     }
    // }

    return foundFoam;

}