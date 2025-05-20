struct ParticleEntry {
    int particleIndex;
    int key;
};

RWStructuredBuffer<ParticleEntry> particleCellKeyEntries;
RWStructuredBuffer<int> cellKeyToStartCoord;

// is this weird uint stuff ok?
static uint imod(int i, uint m)
{
    if (i >= 0) return i % m;
    else return m - 1 - ((-i) % m);
}

int hash21(int2 coord)
{
    return imod(949937 + 119227 * coord.x + 370673 * coord.y, SpatialLookupSize);
}

int getStartIndex(int key) {
    return cellKeyToStartCoord[key];
}

int getCellKey(int2 cellPos) {
    return hash21(cellPos);
}

int2 posToCell(float2 pos) {
    return int2(floor(pos/GridSize));
}

