#ifndef VOLUMETRIC_CLOUD_SHADE
#define VOLUMETRIC_CLOUD_SHADE

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "CloudCommon.hlsl"

float3 ApproxAtmosphere(float heightFraction, float cosSunViewAngle)
{
    // 模拟日落红光和顶部蓝光
    float sunset = pow(saturate(1.0 - cosSunViewAngle), 3.0); // 黄红
    float altitude = pow(saturate(heightFraction), 2.0);      // 高处偏冷色
    
    float3 warmColor = float3(1.0, 0.4, 0.3);
    float3 coolColor = float3(0.2, 0.8, 1.0);

    return lerp(coolColor, warmColor, altitude) * lerp(1.0, 0.3, sunset);
}

// ----------------------------
// 光照能量采样函数
// ----------------------------

// dl：沿光线方向累积密度（light cone）
// ds：当前点的密度
// phase：Mie 散射相函数结果
// brightness：人为亮度参数（一般为常数）
float3 GetLightEnergy(
    float height_fraction,
    float dl,
    float ds,
    float phase,
    float cosTheta,
    float brightness,
    float3 lightColor,
    float3 viewDir,
    float3 lightDir
)
{
    float T = BeerPowderLaw(dl); 
    float scatterWeight = pow(ds, 1.5) * remap(height_fraction, 0.2, 0.8, 0.5, 1.0);
    float phaseWeight = saturate(remap(cosTheta, -0.2, 1.0, 0.5, 1.0));

    float cosSunView = dot(viewDir, lightDir);
    float3 atmoColor = ApproxAtmosphere(height_fraction, cosSunView);

    float3 energy = T * scatterWeight * phase * phaseWeight * brightness * lightColor * atmoColor;

    return max(0.005, energy);
}

// ----------------------------
// 随机单位向量内核，用于锥形采样
// ----------------------------

static float2 noise_kernel[6] = 
{
    float2(0.0673, -0.0556),
    float2(0.0325, -0.1716), 
    float2(-0.1277, 0.2287), 
    float2(-0.2737, -0.2171), 
    float2(0.0030, 0.4367), 
    float2(0.2009, -0.4840)
};

// 可选权重（越远越低）
static float weights[6] = 
{
    2.0,
    1.0,
    0.5,
    0.25,
    0.125,
    0.0625
};

// ----------------------------
// 沿光锥采样云密度（用于模拟入射光强）
// ----------------------------

float SampleCloudDensityAlongCone(float3 p, float3 rayDir)
{
    float2 cloudDst = RayCloudLayerDst(_EarthCenter, _EarthRadius, _CloudHeightMin, _CloudHeightMax, p, rayDir, false);
    float dstInCloud = cloudDst.y;

    if (cloudDst.y <= 0.01) return 0.0;

    float stepLen = dstInCloud / 6.0;
    float3 light_step = rayDir * stepLen;
    float height_fraction = GetHeightFractionFromSphere(p);
    float coneRadius = stepLen * lerp(0.4, 1.0, height_fraction);

    float3 localX = normalize(cross(float3(0, 1, 0), rayDir));
    float3 localZ = normalize(cross(localX, rayDir));

    float densitySum = 0.0;
    float weightSum = 0.001;

    [unroll]
    for (int i = 0; i < 6; i++)
    {
        float2 offset = coneRadius * noise_kernel[i] * i;
        float3 offsetDir = offset.x * localX + offset.y * localZ;
        float3 samplePos = p + light_step * i + offsetDir;

        height_fraction = GetHeightFractionFromSphere(samplePos);
        if (height_fraction < 0 || height_fraction > 1)
            continue;

        int mip = int(i * 0.5);
        float density = SampleCloudDensity(samplePos, mip);
        float weight = weights[i];

        densitySum += density * weight;
        weightSum += weight;
    }

    return densitySum / weightSum;
}

#endif
