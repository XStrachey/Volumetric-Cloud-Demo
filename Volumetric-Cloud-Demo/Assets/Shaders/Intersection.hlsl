#ifndef VOLUMETRIC_CLOUD_INTERSECTION
#define VOLUMETRIC_CLOUD_INTERSECTION

#include "CloudCommon.hlsl"

// https://www.iquilezles.org/www/articles/intersectors/intersectors.htm

struct Ray
{
    float3 origin;
    float3 direction;
};

// 射线与球体相交, x 到球体最近的距离， y 穿过球体的距离
// 原理是将射线方程(x = o + dl)带入球面方程求解(|x - c|^2 = r^2)
float2 RaySphereDst(float3 sphereCenter, float sphereRadius, float3 pos, float3 rayDir)
{
    float3 oc = pos - sphereCenter;
    float b = dot(rayDir, oc);
    float c = dot(oc, oc) - sphereRadius * sphereRadius;
    float t = b * b - c;//t > 0有两个交点, = 0 相切， < 0 不相交
    
    float delta = sqrt(max(t, 0));
    float dstToSphere = max(-b - delta, 0);
    float dstInSphere = max(-b + delta - dstToSphere, 0);
    return float2(dstToSphere, dstInSphere);
}

// 射线与云层相交, x到云层的最近距离, y穿过云层的距离
// 通过两个射线与球体相交进行计算
float2 RayCloudLayerDst(float3 sphereCenter, float earthRadius, float heightMin, float heightMax, float3 pos, float3 rayDir, bool isShape = true)
{
    float2 cloudDstMin = RaySphereDst(sphereCenter, earthRadius + heightMin, pos, rayDir);
    float2 cloudDstMax = RaySphereDst(sphereCenter, earthRadius + heightMax, pos, rayDir);

    float dstToCloudLayer = 0;
    float dstInCloudLayer = 0;

    float height = GetHeightFromEarthSurface(pos);
    float surface = earthRadius;
    float cloudMin = earthRadius + heightMin;
    float cloudMax = earthRadius + heightMax;

    if (cloudDstMin.y <= 0 && cloudDstMax.y <= 0)
        return float2(0, 0);

    if (isShape)
    {
        if (height <= cloudMin) // 在云面以下
        {
            float3 startPos = pos + rayDir * cloudDstMin.y;
            if (length(startPos - sphereCenter) >= surface)
            {
                dstToCloudLayer = cloudDstMin.y;
                dstInCloudLayer = cloudDstMax.y - cloudDstMin.y;
            }
            return float2(dstToCloudLayer, dstInCloudLayer);
        }

        if (height > cloudMin && height <= cloudMax) // 在云层内
        {
            dstToCloudLayer = 0;
            dstInCloudLayer = cloudDstMin.y > 0 ? cloudDstMin.x : cloudDstMax.y;
            return float2(dstToCloudLayer, dstInCloudLayer);
        }

        // 在云层之外
        dstToCloudLayer = cloudDstMax.x;
        dstInCloudLayer = cloudDstMin.y > 0 ? cloudDstMin.x - dstToCloudLayer : cloudDstMax.y;
    }
    else // 光照采样，从云内开始
    {
        dstToCloudLayer = 0;
        dstInCloudLayer = cloudDstMin.y > 0 ? cloudDstMin.x : cloudDstMax.y;
    }

    return float2(dstToCloudLayer, dstInCloudLayer);
}
#endif