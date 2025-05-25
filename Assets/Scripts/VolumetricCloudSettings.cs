using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WeatherSettings
{
    public Texture2D weatherTexture;
    public Vector3 windDirection;
    public float windMainSpeed;

    [Range(0, 1)]
    public float coverage = 0.5f;
}

public class ShaderVariables
{
    internal readonly static int _FrameCount = Shader.PropertyToID("_FrameCount");
    internal readonly static int _RenderTargetSize = Shader.PropertyToID("_RenderTargetSize");

    internal readonly static int _WeatherTex = Shader.PropertyToID("_WeatherTex");
    internal readonly static int _WindParams = Shader.PropertyToID("_WindParams");

    internal readonly static int _ShapeTex = Shader.PropertyToID("_ShapeTex");
    internal readonly static int _DetailTex = Shader.PropertyToID("_DetailTex");
    internal readonly static int _CurlNoiseTex = Shader.PropertyToID("_CurlNoiseTex");
    internal readonly static int _VolumetricCloudShapeParams = Shader.PropertyToID("_VolumetricCloudShapeParams");
    internal readonly static int _StratusParams = Shader.PropertyToID("_StratusParams");
    internal readonly static int _StratocumulusParams = Shader.PropertyToID("_StratocumulusParams");
    internal readonly static int _CumulusParams = Shader.PropertyToID("_CumulusParams");

    internal readonly static int _VolumetricCloudForwardMieG = Shader.PropertyToID("_VolumetricCloudForwardMieG");
    internal readonly static int _VolumetricCloudBackwardMieG = Shader.PropertyToID("_VolumetricCloudBackwardMieG");
    internal readonly static int _VolumetricCloudMieScatterParams = Shader.PropertyToID("_VolumetricCloudMieScatterParams");

    internal readonly static int _OverallParams = Shader.PropertyToID("_OverallParams");
}