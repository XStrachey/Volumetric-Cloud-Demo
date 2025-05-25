#ifndef VOLUMETRIC_CLOUD_COMMON
#define VOLUMETRIC_CLOUD_COMMON

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

#ifndef FOUR_PI
#define FOUR_PI (12.5663706143591729538)
#endif

/* description: expands lerp to linearly lerp more than two values */
float   mmix(in float a, in float b, in float c) { return lerp(a, b, c); }

float mmix(float a , float b, float c, float pct) {
    return mmix(
        mmix(a, b, 2. * pct),
        mmix(b, c, 2. * (max(pct, .5) - .5)),
        step(.5, pct)
    );
}

float remap(in float value, in float inMin, in float inMax, in float outMin, in float outMax) {
  return outMin + (outMax - outMin) * (value - inMin) / (inMax - inMin);
}

// Beer衰减
float BeerLaw(float density, float absorptivity = 1)
{
    return exp(-density * absorptivity);
}

// 粉糖效应，模拟云的内散射影响
float BeerPowderLaw(float density, float absorptivity = 1)
{
    return 2.0 * exp(-density * absorptivity) * (1.0 - exp(-2.0 * density * absorptivity));
}

float BeerPowderLawStable(float d)
{
    d = saturate(min(d, 5.0));
    float a = exp(-d);
    float b = exp(-2.0 * d);
    return saturate(1.0 - a + b); // 输出稳定在 [0, 1] 范围
}

// 参数g用于控制散射的分布形状
// 当g小于0时，散射更多的是逆向的
// 等于0的时候是各向同性的，也就是在各个方向上散射的光能量是相等的
// 如果大于0就表示光更多的能量会向前传播
float HenyeyGreensteinScattering(float cosTheta, float g)
{
    float g2 = g * g;
    return (1 - g2) / (FOUR_PI * pow((1 + g2 - 2 * g * cosTheta), 1.5));
}

// 4 分量 g 值版本是 1 分量 g 值版本的部分 CPU 预计算版本
// g.x (1 - g * g)
// g.y (1 + g * g)
// g.z (2 * g)
// g.w (1.0 / (4 * PI))
float HenyeyGreensteinScattering(float cosTheta, float4 g)
{
    return g.w * g.x / pow(g.y - g.z * cosTheta, 1.5);
}

/////////////////////////////////////////////////////////云光照计算帮助函数/////////////////////////////////////////////////////////

// 两层Henyey-Greenstein散射，使用Max混合。同时兼顾向前 向后散射
float HGScatterMax(float cosTheta, float4 g_1, float4 g_2)
{
    return max(HenyeyGreensteinScattering(cosTheta, g_1), HenyeyGreensteinScattering(cosTheta, g_2));
}

float HGScatterLerp(float cosTheta, float4 g_1, float4 g_2)
{
    return lerp(HenyeyGreensteinScattering(cosTheta, g_1), HenyeyGreensteinScattering(cosTheta, g_2), 0.5);
}

// 返回点到地心的物理高度（单位：米），用于逻辑判断
float GetHeightFromEarthSurface(float3 position)
{
    return length(position - _EarthCenter);
}

// 返回云壳内 [0,1] 的归一化高度，用于密度/光照渐变
float GetHeightFractionFromSphere(float3 position)
{
    float height = length(position - _EarthCenter);
    float heightRange = _CloudHeightMax - _CloudHeightMin;
    heightRange = max(heightRange, 1e-3); // 防止除以0

    return saturate((height - (_EarthRadius + _CloudHeightMin)) / heightRange);
}

#endif