using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Weather Noise Settings", menuName = "JSZX/Volumetric Cloud/Generator/Weather Noise Settings", order = 2)]
public class WeatherNoiseSettings : NoiseSettings
{
    public Vector3 randomness = new Vector3(-344, 12, 0.23f);

    public override System.Array GetDataArray ()
    {
        var data = new DataStruct ()
        {
            randomness = randomness
        };

        return new DataStruct[] { data };
    }

    public struct DataStruct
    {
        public Vector3 randomness;
    }

    public override int Stride
    {
        get
        {
            return sizeof (float) * 3;
        }
    }
}