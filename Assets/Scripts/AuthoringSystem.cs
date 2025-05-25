using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class AuthoringSystem : MonoBehaviour
{
    public bool isShowGizmos = true;

    public WeatherSettings weatherSettings = new WeatherSettings();

    void Update()
    {
        Shader.SetGlobalVector(ShaderVariables._WindParams, new Vector4(weatherSettings.windDirection.x, weatherSettings.windDirection.y, weatherSettings.windDirection.z, weatherSettings.windMainSpeed));
        Shader.SetGlobalVector(ShaderVariables._OverallParams, new Vector4(weatherSettings.coverage, 0, 0, 0));
    }

    void OnEnable()
    {
        Shader.SetGlobalTexture(ShaderVariables._WeatherTex, weatherSettings.weatherTexture);
    }
}
