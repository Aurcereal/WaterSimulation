#define NORMEPS 0.0005

float sdSphere(float3 p, float r) {
    return length(p) - r;
}

float sdBox(float3 p, float3 dim) {
    p = abs(p) - dim*.5;
    float s = sqrt(p.x*max(0.,p.x) + p.y*max(0.,p.y) + p.z*max(0., p.z));
    return step(s, 0.) * max(p.x, max(p.y, p.z)) + s;
}

float sdCylinder(float3 p, float2 dim) {
    float2 lp = float2(length(p.xz), p.y);
    lp.y = abs(lp.y);

    lp -= float2(dim.x, dim.y*.5);
    float s = sqrt(lp.x * max(lp.x, 0.) + lp.y * max(lp.y, 0.));
    return s + step(s, 0.) * max(lp.x, lp.y);
}

// float sdCone(float3 p, float2 dim) {
//     float2 lp = float2(length(p.xz), p.y);

//     float2 br = float2(dim.x, -dim.y*.5);
//     float2 top = float2(0., dim.y*.5);
//     float2 outDir = -perp(normalize(br-top));
//     float dSlant = dot(lp - br, outDir);
//     float dBottom = -dim.y*.5 - lp.y;

//     return max(dSlant, dBottom);
// }

float sdCone(float3 p, float2 dim) {
    p.y -= dim.y*.5;

    float2 lp = float2(length(p.xz), p.y);

    float2 br = float2(dim.x, -dim.y);
    float2 fromSlantProj = lp - br*clamp(dot(lp, br)/dot(br, br), 0., 1.);
    float2 fromBotProj = lp-float2(clamp(lp.x, 0., br.x), br.y);
    float dist = min(length(fromSlantProj), length(fromBotProj));

    float insideBottom = step(br.y, lp.y);
    float insideSlant = step(0., lp.x*br.y - br.x*lp.y);
    float inside = insideBottom*insideSlant;

    return -dist*(inside*2.-1.);

}

float sdWasher(float3 p, float3 dim) { // dim = (r, height, cutFac)
    float2 lp = float2(length(p.xz), p.y);
    lp.y = abs(lp.y);

    lp -= float2(dim.x*.5*(dim.z + 1.), dim.y*.5);
    lp.x = abs(lp.x) - dim.x*.5*(1.-dim.z);

    float s = sqrt(lp.x * max(lp.x, 0.) + lp.y * max(lp.y, 0.));
    return s + step(s, 0.) * max(lp.x, lp.y);
}

float sdScene(float3 p); // Implement this wherever you include it

float3 normal(float3 p) {
    float2 eps = float2(NORMEPS, 0.);

    return normalizesafe(
        float3(
            sdScene(p + eps.xyy) - sdScene(p - eps.xyy),
            sdScene(p + eps.yxy) - sdScene(p - eps.yxy),
            sdScene(p + eps.yyx) - sdScene(p - eps.yyx)
        )
    );
}