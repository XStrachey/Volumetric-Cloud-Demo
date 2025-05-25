using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Detail Noise Settings", menuName = "JSZX/Volumetric Cloud/Generator/Detail Noise Settings", order = 1)]
public class DetailNoiseSettings : NoiseSettings
{
    public float worley1Period = 16;
    public float worley2Period = 32;
    public float worley3Period = 64;

    public override System.Array GetDataArray ()
    {
        var data = new DataStruct ()
        {
            worley1Period = worley1Period,
            worley2Period = worley2Period,
            worley3Period = worley3Period
        };

        return new DataStruct[] { data };
    }

    public struct DataStruct
    {
        public float worley1Period;
        public float worley2Period;
        public float worley3Period;
    }

    public override int Stride
    {
        get
        {
            return sizeof (float) * 3;
        }
    }
}