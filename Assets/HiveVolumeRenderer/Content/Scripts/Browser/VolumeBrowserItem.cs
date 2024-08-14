using HiveVolumeRenderer.Import;
using HiveVolumeRenderer.Render;
using HiveVolumeRenderer.Render.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HiveVolumeRenderer.Browser
{
    public class VolumeBrowserItem : MonoBehaviour
    {
        public Action<VolumeBrowserItem> OnLayoutDirty;

        public VolumeData Data { get; private set; } = null;
        [field: SerializeField] public SliceRenderer SliceRenderer { get; private set; }
        public VolumeRenderer VolumeRenderer { get; private set; } = null;

        [Header("References")]
        [SerializeField] private VolumeRenderer _prefabVolumeRenderer;

        [Header("Components")]
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private RawImage _textureImage;
        [SerializeField] private AsyncLoadIndicator _loadIndicator;

        private void Start()
        {
            if (Data == null)
            {
                // Set the initial aspect ratio to square
                var group = GetComponentInParent<LayoutGroup>();
                var parent = group.transform as RectTransform;
                UpdateHeight(parent.rect.width, parent.rect.width);
            }
        }

        public async void Inject(VolumeStream stream)
        {
            Log.Message($"Loading data from {stream.GetType().Name}...");

            Data = await stream.LoadVolumeData();

            if (Data == null)
            {
                Log.Error($"No data could be loaded from {stream.GetType().Name}");
                Destroy(gameObject);
                return;
            }

            Log.Message($"{Data.Name} - Volume loaded");

            // Currently create a render as soon as loaded
            CreateVolumeRenderer();

            _nameText.text = Data.Name;
            await SliceRenderer.Render(Data);

            _loadIndicator.SetVisible(false);

            UpdateHeight(_textureImage.texture.width, _textureImage.texture.height);
        }

        public async void CreateVolumeRenderer()
        {
            if (VolumeRenderer != null) return;

            VolumeRenderer = Instantiate(_prefabVolumeRenderer);
            await VolumeRenderer.Render(Data);
        }

        public void DestroyVolumeRenderer()
        {
            if (VolumeRenderer == null) return;

            Destroy(VolumeRenderer.gameObject);
            VolumeRenderer = null;
        }

        private async Task<Texture2D> CreatePreviewTexture()
        {
            Log.Message($"{Data.Name} - Preview creation start");

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

            Log.Message($"{Data.Name} - Preview creation completed");

            return texture;
        }

        private void UpdateHeight(float sourceWidth, float sourceHeight)
        {
            LayoutGroup group = GetComponentInParent<LayoutGroup>();
            RectTransform parent = group.transform as RectTransform;
            float width = parent.rect.width - group.padding.left - group.padding.right;

            float heightRatio = sourceHeight / sourceWidth;
            float height = heightRatio * width;

            var rect = _textureImage.transform as RectTransform;
            rect.sizeDelta = new Vector2(0f, height);

            OnLayoutDirty?.Invoke(this);
        }

        private void OnDestroy()
        {
            if (VolumeRenderer != null)
                DestroyVolumeRenderer();
        }
    }
}
