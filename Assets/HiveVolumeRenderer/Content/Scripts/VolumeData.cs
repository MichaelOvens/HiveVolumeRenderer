using UnityEngine;

namespace HiveVolumeRenderer
{
    public class VolumeData
    {
        public string Name;
        public float[] Values;
        public Vector3 PrimaryAxis;
        public Vector3Int Dimensions;
        public Vector3 Size;

        public VolumeData(string name, float[] values, Vector3 primaryAxis, Vector3Int dimensions, Vector3 size)
        {
            Name = name;
            Values = values;
            PrimaryAxis = primaryAxis;
            Dimensions = dimensions;
            Size = size;
        }
    }
}
