using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class WeatherMapGen
{
    const int computeThreadGroupSize = 16;

    public ComputeShader noiseComputeCS;

    public int resolution = 512;
    public WeatherNoiseSettings settings;

    public RenderTexture weatherMap;
    public Texture2D weatherMapTex;
    public string name;

    List<ComputeBuffer> buffersToRelease;

    [HideInInspector]
    public bool showSettingsEditor = true;

    public WeatherMapGen(ComputeShader noiseComputeCS)
    {
        this.noiseComputeCS = noiseComputeCS;
    }

    public void UpdateMap ()
    {
        CreateTexture (ref weatherMap, resolution);

        if (noiseComputeCS == null)
        {
            return;
        }
        buffersToRelease = new List<ComputeBuffer> ();

        noiseComputeCS.SetTexture (0, "Result", weatherMap);
        noiseComputeCS.SetInt ("resolution", resolution);

        noiseComputeCS.SetVector ("randomness", settings.randomness);

        int numThreadGroups = Mathf.CeilToInt (resolution / (float) computeThreadGroupSize);
        noiseComputeCS.Dispatch (0, numThreadGroups, numThreadGroups, 1);

        // Release buffers
        foreach (var buffer in buffersToRelease)
        {
            buffer.Release ();
        }
    }

    // Create buffer with some data, and set in shader. Also add to list of buffers to be released
    ComputeBuffer CreateBuffer (System.Array data, int stride, string bufferName, int kernel = 0)
    {
        var buffer = new ComputeBuffer (data.Length, stride, ComputeBufferType.Raw);
        buffersToRelease.Add (buffer);
        buffer.SetData (data);
        noiseComputeCS.SetBuffer (kernel, bufferName, buffer);
        return buffer;
    }

    void CreateTexture (ref RenderTexture texture, int resolution)
    {
        var format = UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm;
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
            texture.name = this.name;

            texture.Create ();
        }
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;
    }

    public Texture2D ConvertFromRenderTexture (RenderTexture rt)
    {
        Texture2D output = new Texture2D (rt.width, rt.height);
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = rt;
        output.ReadPixels (new Rect (0, 0, rt.width, rt.height), 0, 0);
        output.Apply ();
        RenderTexture.active = currentRT;
        return output;
    }

    void SaveToTexture(string name, RenderTexture renderTexture, bool alphaIsTransparency)
    {
#if UNITY_EDITOR
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = renderTexture;
        Texture2D texture2D = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
        texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        RenderTexture.active = currentRT;

        if (!File.Exists(string.Concat(Application.dataPath, "/Resources/")))
        {
            Directory.CreateDirectory(string.Concat(Application.dataPath, "/Resources/"));
        }

        byte[] bytes = texture2D.EncodeToPNG();
        string path = "Assets/Resources/" + name + ".png";
        System.IO.File.WriteAllBytes(path, bytes);

        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
        if (importer != null)
        {
            importer.alphaIsTransparency = alphaIsTransparency;
            importer.sRGBTexture = false;
            importer.mipmapEnabled = false;
            AssetDatabase.ImportAsset(path);
        }

        Debug.Log("Saved to " + path);
        AssetDatabase.Refresh();
#endif
    }

    public void Save()
    {
        SaveToTexture(name, weatherMap, false);
    }
}
