#ifndef VOLUMETRIC_CLOUD_SHAPE
#define VOLUMETRIC_CLOUD_SHAPE

#include "Intersection.hlsl"
#include "CloudInput.hlsl"
#include "CloudCommon.hlsl"

// 高度层密度控制（根据云类型参数 b ∈ [0,1]）
// b = 0 → 层云；b = 0.5 → 层积云；b = 1 → 积云
float GetDensityHeightGradientForPoint(float heightFraction, float3 weatherData)
{
    float type = saturate(weatherData.b);

    float stratus        = pow(saturate(1.0 - abs(heightFraction - 0.25) * 2.0), 1.0);   // 层云
    float stratocumulus  = pow(saturate(1.0 - abs(heightFraction - 0.5) * 2.0), 9.0);   // 层积云
    float cumulus        = pow(saturate(1.0 - abs(heightFraction - 0.75) * 1.0), 3.0);  // 积云

    float density =
        lerp(lerp(stratus, stratocumulus, saturate(type * 2.0)),
             cumulus, saturate((type - 0.5) * 2.0));

    return saturate(density);
}

// wrap UV 获取天气数据
float3 GetWeatherData(float3 pos)
{
    // 云图大小（单位 m）
    float2 repeatSize = float2(2048 * 1000.0, 1024 * 1000.0);
    float2 uv = frac(pos.xz / repeatSize);
    return tex2Dlod(_WeatherTex, float4(uv, 0, 0));
}

// 云密度采样主函数
float SampleCloudDensity(float3 p, int mip_level)
{
    float height_fraction = GetHeightFractionFromSphere(p);
    float3 weather_data = GetWeatherData(p);
    float cloud_top_offset = _CloudHeightMax - _CloudHeightMin;

    // 风偏移 + 漂浮
    p += height_fraction * windDirectionWS * cloud_top_offset * 0.1;
    p += (windDirectionWS + float3(0.0, 0.1, 0.0)) * _Time.y * windMainSpeed;

    // curl 噪声滚动
    float2 curl = tex2Dlod(_CurlNoiseTex, float4(p.xz, 0.0, 0.0)).rg;
    p.xz += curl * (1.0 - height_fraction) * 0.1 * 1000.0;

    // 主 shape noise
    float4 noise = tex3Dlod(_ShapeTex, float4(p * baseShapeTiling, mip_level));
    float perlin = noise.r;
    float worley_fmb = (0.625 * noise.g + 0.25 * noise.b + 0.125 * noise.a);
    float worley = pow(1.0 - worley_fmb, 0.3);
    float base_cloud = saturate(pow(perlin, 0.1) * worley);

    // 高度分布梯度
    float density_height_gradient = GetDensityHeightGradientForPoint(height_fraction, weather_data);
    base_cloud = pow(base_cloud, 1.2) * density_height_gradient;

    // 覆盖度遮罩
    float cloud_coverage = mmix(0, weather_data.r, 1, overallCoverage);
    cloud_coverage = pow(cloud_coverage, remap(height_fraction, 0.7, 0.8, 1.0, lerp(1.0, 0.5, anvilBias)));

    // 覆盖 remap（更清晰层次）
    float cloud_with_coverage = saturate((base_cloud - (1.0 - cloud_coverage * 0.85)) * 5.0) * cloud_coverage;

    // 高频蚀刻
    float3 high_noises = tex3Dlod(_DetailTex, float4(p * baseShapeTiling * 0.1, mip_level)).rgb;
    float high_freq_fBm = dot(high_noises.rgb, float3(0.5, 0.35, 0.15));
    float erosionMask = lerp(high_freq_fBm, 1.0 - high_freq_fBm, saturate(height_fraction * 8));
    float erosion = saturate(pow(erosionMask, 1.2) * 1.2 - 0.1);

    // Final cloud density
    float final_cloud = saturate((cloud_with_coverage - erosion));

    return max(0.0, final_cloud);
}

#endif
