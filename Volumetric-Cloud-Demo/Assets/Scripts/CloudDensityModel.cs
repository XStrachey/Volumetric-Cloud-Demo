using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class CloudDensityModel : MonoBehaviour
{
    public ShapeSettings shapeSettings;

    void Update()
    {
        if (null == shapeSettings)
            return;
        
        Shader.SetGlobalTexture(ShaderVariables._ShapeTex, shapeSettings.shapeTexture);
        Shader.SetGlobalTexture(ShaderVariables._DetailTex, shapeSettings.detailTexture);
        Shader.SetGlobalTexture(ShaderVariables._CurlNoiseTex, shapeSettings.curlNoiseTex);
        
        Shader.SetGlobalVector(ShaderVariables._VolumetricCloudShapeParams, new Vector2(1.0f / shapeSettings.cloudSize, shapeSettings.anvilBias));
    }
}