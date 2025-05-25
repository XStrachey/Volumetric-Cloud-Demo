using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CloudNoiseType { Shape, Detail }

public class CloudNoiseGenerator
{
    public ComputeShader noiseComputeCS;

    const int computeThreadGroupSize = 8;
    public const string detailNoiseName = "DetailNoise";
    public const string shapeNoiseName = "ShapeNoise";

    [Header ("Editor Settings")]
    public CloudNoiseType activeCloudNoiseType = CloudNoiseType.Shape;

    [Header ("Noise Settings")]
    public int shapeResolution = 64;
    public int detailResolution = 32;

    [Header ("Shape Settings")]
    public ShapeNoiseSettings shapeNoiseSettings;

    [Header ("Detail Settings")]
    public DetailNoiseSettings detailNoiseSettings;

    [HideInInspector]
    public bool showSettingsEditor = true;
    [SerializeField, HideInInspector]
    public RenderTexture shapeTexture;
    [SerializeField, HideInInspector]
    public RenderTexture detailTexture;

    public NoiseSettings ActiveSettings
    {
        get
        {
            switch (activeCloudNoiseType)
            {
                case CloudNoiseType.Shape:
                return shapeNoiseSettings;
                case CloudNoiseType.Detail:
                return detailNoiseSettings;
                default:
                return null;
            }
        }
    }

    public CloudNoiseGenerator (ComputeShader noiseComputeCS)
    {
        this.noiseComputeCS = noiseComputeCS;
    }

    public void UpdateShape ()
    {
        if (!noiseComputeCS)
            return;

        ValidateParamaters ();
        CreateTexture (ref shapeTexture, shapeResolution, shapeNoiseName);

        // Set noise gen kernel data:
        noiseComputeCS.SetTexture (0, "ShapeResult", shapeTexture);
        noiseComputeCS.SetInt     ("ShapeResolution", shapeResolution);

        int numThreadGroups = Mathf.CeilToInt (shapeResolution / (float) computeThreadGroupSize);

        // Shape

        noiseComputeCS.SetFloat ("perlinPeriod", shapeNoiseSettings.perlinPeriod);
        noiseComputeCS.SetInt ("perlinOctaves", shapeNoiseSettings.perlinOctaves);
        noiseComputeCS.SetFloat ("layeredWorley1Period", shapeNoiseSettings.layeredWorley1Period);
        noiseComputeCS.SetFloat ("layeredWorley2Period", shapeNoiseSettings.layeredWorley2Period);
        noiseComputeCS.SetFloat ("layeredWorley3Period", shapeNoiseSettings.layeredWorley3Period);

        noiseComputeCS.Dispatch  (0, numThreadGroups, numThreadGroups, numThreadGroups);
    }

    public void UpdateDetail ()
    {
        if (!noiseComputeCS)
            return;

        ValidateParamaters ();
        CreateTexture (ref detailTexture, detailResolution, detailNoiseName);

        // Set noise gen kernel data:
        noiseComputeCS.SetTexture (1, "DetailResult", detailTexture);
        noiseComputeCS.SetInt     ("DetailResolution", detailResolution);

        int numThreadGroups = Mathf.CeilToInt (detailResolution / (float) computeThreadGroupSize);

        // Detail

        noiseComputeCS.SetFloat ("worley1Period", detailNoiseSettings.worley1Period);
        noiseComputeCS.SetFloat ("worley2Period", detailNoiseSettings.worley2Period);
        noiseComputeCS.SetFloat ("worley3Period", detailNoiseSettings.worley3Period);

        noiseComputeCS.Dispatch  (1, numThreadGroups, numThreadGroups, numThreadGroups);
    }

    public void UpdateNoise ()
    {
        UpdateShape ();
        UpdateDetail ();
    }

    void CreateTexture (ref RenderTexture texture, int resolution, string name)
    {
        var format = UnityEngine.Experimental.Rendering.GraphicsFormat.R16G16B16A16_UNorm;
        if (texture == null || !texture.IsCreated () || texture.width != resolution || texture.height != resolution || texture.volumeDepth != resolution || texture.graphicsFormat != format)
        {
            if (texture != null)
            {
                texture.Release ();
            }
            texture = new RenderTexture (resolution, resolution, 0);
            texture.graphicsFormat = format;
            texture.volumeDepth = resolution;
            texture.enableRandomWrite = true;
            texture.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
            texture.name = name;

            texture.Create ();
        }
        texture.wrapMode = TextureWrapMode.Repeat;
        texture.filterMode = FilterMode.Bilinear;
    }

    void OnValidate () { }

    void ValidateParamaters ()
    {
        detailResolution = Mathf.Min(Mathf.Max (1, detailResolution), 128);
        shapeResolution = Mathf.Min(Mathf.Max (1, shapeResolution), 256);
    }
}
