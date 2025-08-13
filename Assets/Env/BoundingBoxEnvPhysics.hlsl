float sdBoundingBox(float3 p) {
    float dBox = -sdBox(ContainerScale * mul(ContainerInverseTransform, float4(p, 1.)).xyz, ContainerScale);
    return dBox;
}