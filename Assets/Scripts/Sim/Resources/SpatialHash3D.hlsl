struct ParticleEntry {
    int particleIndex;
    int key;
};

RWStructuredBuffer<ParticleEntry> particleCellKeyEntries;
RWStructuredBuffer<int> cellKeyToStartCoord;

// uint try
int imod(int i, int m)
{
    if (i >= 0) return uint(i) % uint(m);
    else return m - 1 - (uint(-i) % uint(m));
}

int hash21(int2 coord)
{
    return imod(949937 + 119227 * coord.x + 370673 * coord.y, SpatialLookupSize);
}

int hash31(int3 coord)
{
    return imod(949937 + 119227 * coord.x + 370673 * coord.y + 440537 * coord.z, SpatialLookupSize);
}

int getStartIndex(int key) {
    return cellKeyToStartCoord[key];
}

int getCellKey(int3 cellPos) {
    return hash31(cellPos);
}

int3 posToCell(float3 pos) {
    return int3(floor(pos/GridSize));
}

