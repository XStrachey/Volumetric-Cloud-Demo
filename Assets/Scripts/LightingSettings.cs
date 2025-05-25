using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Lighting Settings", menuName = "Volumetric Cloud/Lighting Settings", order = 1)]
public class LightingSettings : ScriptableObject
{
    [Range(0.0f, 1.0f)]
    public float forwardMieG = 0.78f;
    [Range(0.0f, 1.0f)]
    public float backwardMieG = 0.25f;
    [Range(0.0f, 1.0f)]
    public float mieScatterBase = 0.29f;
    [Range(0.0f, 1.0f)]
    public float mieScatterMultiply = 0.60f;
}
