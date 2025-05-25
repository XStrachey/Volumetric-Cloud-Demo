using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Shape Noise Settings", menuName = "JSZX/Volumetric Cloud/Generator/Shape Noise Settings", order = 0)]
public class ShapeNoiseSettings : NoiseSettings
{
    public float perlinPeriod = 4;
    [Range(1, 10)]
    public int perlinOctaves = 7;
    public float layeredWorley1Period = 4;
    public float layeredWorley2Period = 8;
    public float layeredWorley3Period = 16;

    public override System.Array GetDataArray ()
    {
        var data = new DataStruct ()
        {
            perlinPeriod = perlinPeriod,
            perlinOctaves = perlinOctaves,
            layeredWorley1Period = layeredWorley1Period,
            layeredWorley2Period = layeredWorley2Period,
            layeredWorley3Period = layeredWorley3Period
        };

        return new DataStruct[] { data };
    }

    public struct DataStruct
    {
        public float perlinPeriod;
        public int perlinOctaves;
        public float layeredWorley1Period;
        public float layeredWorley2Period;
        public float layeredWorley3Period;
    }

    public override int Stride
    {
        get
        {
            return sizeof (float) * 5;
        }
    }
}