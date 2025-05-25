// AtmosphereScattering.hlsl
#ifndef ATMOSPHERE_SCATTERING_INCLUDED
#define ATMOSPHERE_SCATTERING_INCLUDED

#define ATMOSPHERE_HEIGHT 100000.0
#define RAYLEIGH_HEIGHT (ATMOSPHERE_HEIGHT * 0.08)
#define MIE_HEIGHT (ATMOSPHERE_HEIGHT * 0.012)

#define C_RAYLEIGH (float3(5.802, 13.558, 33.1) * 1e-6)
#define C_MIE (float3(3.996, 3.996, 3.996) * 1e-6)
#define C_OZONE (float3(0.650, 1.881, 0.085) * 1e-6)
#define ATMOSPHERE_DENSITY 1.0
#define EXPOSURE 50.0

float2 SphereIntersection(float3 ro, float3 rd, float3 center, float radius)
{
    ro -= center;
    float b = dot(ro, rd);
    float c = dot(ro, ro) - radius * radius;
    float h = b * b - c;
    if (h < 0.0) return float2(-1, -1);
    h = sqrt(h);
    return float2(-b - h, -b + h);
}

float AtmosphereHeight(float3 pos)
{
    return length(pos - _EarthCenter) - _EarthRadius;
}

float DensityRayleigh(float h) { return exp(-max(0, h / RAYLEIGH_HEIGHT)); }
float DensityMie(float h) { return exp(-max(0, h / MIE_HEIGHT)); }
float DensityOzone(float h) { return max(0, 1.0 - abs(h - 25000.0) / 15000.0); }
float3 AtmosphereDensity(float h) { return float3(DensityRayleigh(h), DensityMie(h), DensityOzone(h)); }

float3 Absorb(float3 opticalDepth)
{
    return exp(-(opticalDepth.x * C_RAYLEIGH + opticalDepth.y * C_MIE * 1.1 + opticalDepth.z * C_OZONE) * ATMOSPHERE_DENSITY);
}

float PhaseRayleigh(float costh)
{
    return 3.0 / (16.0 * PI) * (1.0 + costh * costh);
}

float PhaseMie(float costh, float g = 0.85)
{
    float k = 1.55 * g - 0.55 * g * g * g;
    float denom = 1.0 - k * costh;
    return (1.0 - k * k) / (4.0 * PI * denom * denom);
}

float3 IntegrateOpticalDepth(float3 start, float3 dir)
{
    float2 intersection = SphereIntersection(start, dir, _EarthCenter, _EarthRadius + ATMOSPHERE_HEIGHT);
    float rayLength = intersection.y;

    float3 sum = 0;
    const int stepCount = 8;
    float stepSize = rayLength / stepCount;
    
    for (int i = 0; i < stepCount; i++)
    {
        float3 pos = start + dir * (i + 0.5) * stepSize;
        float h = AtmosphereHeight(pos);
        sum += AtmosphereDensity(h) * stepSize;
    }

    return sum;
}

float3 IntegrateScattering(
    float3 rayStart,
    float3 rayDir,
    float rayLength,
    float3 lightDir,
    float3 lightColor,
    out float3 transmittance,
    float mieG
)
{
    float sampleExp = 6.0;

    float2 hit = SphereIntersection(rayStart, rayDir, _EarthCenter, _EarthRadius + ATMOSPHERE_HEIGHT);

    // 如果完全未进入大气层，直接返回
    if (hit.y <= 0.0)
    {
        transmittance = float3(1, 1, 1);
        return float3(0, 0, 0);
    }

    // 计算穿越距离（粗略表示穿透深度）
    float pathLengthInAtmosphere = hit.y - max(hit.x, 0.0);

    // 剪裁 rayStart 到真正进入点
    if (hit.x > 0.0)
    {
        rayStart += rayDir * hit.x;
        rayLength -= hit.x;
    }

    // 限制最大路径长度
    rayLength = min(rayLength, hit.y);

    float3 opticalDepth = 0;
    float3 rayleigh = 0;
    float3 mie = 0;
    
    float costh = dot(rayDir, lightDir);
    float phaseR = PhaseRayleigh(costh);
    float phaseM = PhaseMie(costh, mieG);

    const int sampleCount = 32;
    float lastT = 0.0;

    for (int i = 0; i < sampleCount; i++)
    {
        float t = pow((float)i / (sampleCount - 1), sampleExp) * rayLength;
        float step = t - lastT;
        float3 pos = rayStart + rayDir * t;
        float h = AtmosphereHeight(pos);
        float3 density = AtmosphereDensity(h);

        opticalDepth += density * step;
        float3 viewT = Absorb(opticalDepth);

        float3 lightOD = IntegrateOpticalDepth(pos, lightDir);
        float3 lightT = Absorb(lightOD);

        rayleigh += viewT * lightT * density.x * step * phaseR;
        mie      += viewT * lightT * density.y * step * phaseM;

        lastT = t;
    }

    transmittance = Absorb(opticalDepth);

    float3 scatterColor = (rayleigh * C_RAYLEIGH + mie * C_MIE) * lightColor * EXPOSURE;

    return scatterColor;
}

#endif // ATMOSPHERE_SCATTERING_INCLUDED
