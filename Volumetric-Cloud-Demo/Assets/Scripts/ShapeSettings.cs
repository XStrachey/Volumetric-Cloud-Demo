using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Shape Settings", menuName = "Volumetric Cloud/Shape Settings", order = 0)]
public class ShapeSettings : ScriptableObject
{
    public Texture3D shapeTexture;
    public Texture3D detailTexture;
    public Texture2D curlNoiseTex;

    [Min(10f)]
    public float cloudSize = 1000;

    [Range(0, 1)]
    public float anvilBias = 0.5f;
}
