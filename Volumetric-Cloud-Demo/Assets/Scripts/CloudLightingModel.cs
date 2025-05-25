using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class CloudLightingModel : MonoBehaviour
{
    public LightingSettings lightingSettings;

    void Update()
    {
        if (null == lightingSettings)
            return;

        Shader.SetGlobalVector(ShaderVariables._VolumetricCloudForwardMieG, new Vector4(1 - (lightingSettings.forwardMieG * lightingSettings.forwardMieG), 1 + (lightingSettings.forwardMieG * lightingSettings.forwardMieG), 2 * lightingSettings.forwardMieG, 1.0f / (4.0f * Mathf.PI)));
        float backwardMieG = - lightingSettings.backwardMieG;
        Shader.SetGlobalVector(ShaderVariables._VolumetricCloudBackwardMieG, new Vector4(1 - (backwardMieG * backwardMieG), 1 + (backwardMieG * backwardMieG), 2 * backwardMieG, 1.0f / (4.0f * Mathf.PI)));
        Shader.SetGlobalVector(ShaderVariables._VolumetricCloudMieScatterParams, new Vector2(lightingSettings.mieScatterBase, lightingSettings.mieScatterMultiply));
    }
}