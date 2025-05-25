#ifndef VOLUMETRIC_CLOUD_INPUT
#define VOLUMETRIC_CLOUD_INPUT

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);

TEXTURE2D(_CameraDepthTexture);
SAMPLER(sampler_CameraDepthTexture);

TEXTURE2D(_AtmosphereBackgroundTex);
SAMPLER(sampler_AtmosphereBackgroundTex);

sampler3D _ShapeTex;
sampler3D _DetailTex;
sampler2D _CurlNoiseTex;

sampler2D _WeatherTex;

#define windDirectionWS _WindParams.xyz //风向
#define windMainSpeed _WindParams.w //风速

#define overallCoverage _OverallParams.x

#define baseShapeTiling _VolumetricCloudShapeParams.x //基础形状平铺
#define anvilBias _VolumetricCloudShapeParams.y

#define mieScatterBase _VolumetricCloudMieScatterParams.x
#define mieScatterMultiply _VolumetricCloudMieScatterParams.y

float4 _WindParams;
float4 _OverallParams;

// 蓝噪声
float3 _BlueNoiseParams;

float2 _VolumetricCloudShapeParams;

float4 _VolumetricCloudForwardMieG;
float4 _VolumetricCloudBackwardMieG;
float2 _VolumetricCloudMieScatterParams;

// 地球球心和半径
#define _EarthCenter float3(0, -6371000, 0) // 通常为 float3(0, -R, 0)
#define _EarthRadius 6371000  // 如 6371000（单位：米）
#define _CloudHeightMin 1500 // 如 1500m
#define _CloudHeightMax 9000 // 如 9000m
#endif