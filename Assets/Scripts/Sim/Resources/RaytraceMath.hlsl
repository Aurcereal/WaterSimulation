float2 rayBoxIntersect(float3 ro, float3 rd) {

    const float3 lb = -.5; // Left bottom
    const float3 rt = .5;  // Right top

    float t1x = (lb - ro.x) / rd.x;
    float t2x = (rt - ro.x) / rd.x;
    float t1y = (lb - ro.y) / rd.y;
    float t2y = (rt - ro.y) / rd.y;
    float t1z = (lb - ro.z) / rd.z;
    float t2z = (rt - ro.z) / rd.z;

    float tMin = max(max(min(t1x, t2x), min(t1y, t2y)), min(t1z, t2z));
    float tMax = min(min(max(t1x, t2x), max(t1y, t2y)), max(t1z, t2z));

    return float2(tMin, tMax);

}