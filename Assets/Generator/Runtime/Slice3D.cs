using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slice3D
{
    public Slice3D (ComputeShader slicerCS)
    {
        this.slicerCS = slicerCS;
    }

    const int threadGroupSize = 32;

    public ComputeShader slicerCS;
    public int layer;

    [HideInInspector]
    public RenderTexture slicedLayer;

    public void Slice (Texture3D volumeTexture)
    {
        int resolution = volumeTexture.width;

        CreateTexture (ref slicedLayer, resolution);

        slicerCS.SetTexture (0, "volumeTexture", volumeTexture);
        slicerCS.SetTexture (0, "slice", slicedLayer);
        slicerCS.SetInt ("layer", layer);
        int numThreadGroups = Mathf.CeilToInt (resolution / (float) threadGroupSize);
        slicerCS.Dispatch (0, numThreadGroups, numThreadGroups, 1);
    }

    void CreateTexture (ref RenderTexture texture, int resolution)
    {
        var format = UnityEngine.Experimental.Rendering.GraphicsFormat.R16G16B16A16_UNorm;
        if (texture == null || !texture.IsCreated () || texture.width != resolution || texture.height != resolution || texture.graphicsFormat != format)
        {
            if (texture != null)
            {
                texture.Release ();
            }
            texture = new RenderTexture (resolution, resolution, 0);
            texture.graphicsFormat = format;
            texture.volumeDepth = resolution;
            texture.enableRandomWrite = true;
            texture.dimension = UnityEngine.Rendering.TextureDimension.Tex2D;
            texture.name = "Sliced Layer";

            texture.Create ();
        }
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;
    }
}
