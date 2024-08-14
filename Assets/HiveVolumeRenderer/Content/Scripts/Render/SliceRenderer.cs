using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace HiveVolumeRenderer.Render
{
    public class SliceRenderer : MonoBehaviour
    {
        public VolumeData Data { get; private set; }
        [field: SerializeField] public RawImage Image { get; private set; }

        public async Task Render(VolumeData data)
        {
            if (data == null) throw new NullReferenceException("VolumeData is null");
            
            Log.Message($"{data.Name} - Slice render creation start");
            
            Data = data;

            var primaryAxisDirection = new List<float> {
                Mathf.Abs(Data.PrimaryAxis[0]),
                Mathf.Abs(Data.PrimaryAxis[1]),
                Mathf.Abs(Data.PrimaryAxis[2]),
            };
            int primaryAxisIndex = primaryAxisDirection.IndexOf(primaryAxisDirection.Max());

            // Mapping of the volume coordinate space to the slice coordinate space
            // Slice coordinate space has the Z axis aligned with the direction of slice,
            // X and Y slices correspond to the X and Y axes of the 2D slice texture
            Vector3Int coordinateMapping = new();

            switch (primaryAxisIndex)
            {
                case 0:
                    coordinateMapping = new Vector3Int(2, 1, 0);
                    break;
                case 1:
                    coordinateMapping = new Vector3Int(0, 2, 1);
                    break;
                case 2:
                    coordinateMapping = new Vector3Int(0, 1, 2);
                    break;
                default:
                    throw new IndexOutOfRangeException();
            }

            int width = Data.Dimensions[coordinateMapping.x];
            int height = Data.Dimensions[coordinateMapping.y];
            float[] values = new float[width * height];

            int zIndex = Mathf.FloorToInt(Data.Dimensions[primaryAxisIndex] / 2f);

            float minValue = float.MaxValue;
            float maxValue = float.MinValue;

            // Unsure why this works, may break
            bool invertXAxis = true;
            bool invertYAxis = false;

            for (int xIndex = 0; xIndex < width; xIndex++)
            {
                for (int yIndex = 0; yIndex < height; yIndex++)
                {
                    Vector3Int sourceCoordinate = Vector3Int.zero;
                    sourceCoordinate[coordinateMapping.x] = xIndex;
                    sourceCoordinate[coordinateMapping.y] = yIndex;
                    sourceCoordinate[coordinateMapping.z] = zIndex;

                    int sourceIndex = sourceCoordinate.x
                        + sourceCoordinate.y * Data.Dimensions.x
                        + sourceCoordinate.z * Data.Dimensions.x * Data.Dimensions.y;

                    float value = Data.Values[sourceIndex];

                    minValue = Mathf.Min(minValue, value);
                    maxValue = Mathf.Max(maxValue, value);

                    int xTarget = invertXAxis ? width - xIndex - 1 : xIndex;
                    int yTarget = invertYAxis ? height - yIndex - 1 : yIndex;

                    values[xTarget + yTarget * width] = value;
                }
            }

            Color[] colors = new Color[values.Length];
            Parallel.For(0, colors.Length, i =>
            {
                float normalisedValue = (values[i] - minValue) / (maxValue - minValue);
                colors[i] = new Color(normalisedValue, normalisedValue, normalisedValue);
            });

            await Task.Yield();

            Texture2D texture = new Texture2D(width, height);
            texture.SetPixels(colors);
            texture.Apply();

            Image.color = Color.white;
            Image.texture = texture;

            Log.Message($"{Data.Name} - Slice render creation completed");
        }
    }
}
