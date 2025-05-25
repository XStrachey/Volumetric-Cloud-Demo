using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Channel { R, G, B, A }

public class Split2D
{
    const int threadGroupSize = 32;

    public Channel channel = Channel.R;

    public ComputeShader spliterCS;

    public RenderTexture resultRT;

    Vector4[] channelMasks = new Vector4[4] {new Vector4(1, 0, 0, 0), new Vector4(0, 1, 0, 0), new Vector4(0, 0, 1, 0), new Vector4(0, 0, 0, 1)};

    public Split2D(ComputeShader spliterCS)
    {
        this.spliterCS = spliterCS;
    }

    public void Split(RenderTexture input)
    {
        int resolution = input.width;

        CreateTexture (ref resultRT, resolution);

        spliterCS.SetTexture (0, "Input", input);
        spliterCS.SetTexture (0, "Result", resultRT);
        spliterCS.SetVector ("ChannelMask", channelMasks[(int)channel]);
        int numThreadGroups = Mathf.CeilToInt (resolution / (float) threadGroupSize);
        spliterCS.Dispatch (0, numThreadGroups, numThreadGroups, 1);
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
            texture.name = "Split Channel";

            texture.Create ();
        }
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;
    }
}
