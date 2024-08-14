using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace HiveVolumeRenderer.Render
{
    using System;
    using Utilities;

    public class VolumeRenderer : MonoBehaviour
    {
        public VolumeData Data { get; private set; }

        [field: SerializeField] public TransferFunction TransferFunction { get; private set; }
        [field: SerializeField] public Renderer Renderer { get; private set; }

        public async Task Render(VolumeData data)
        {
            if (data == null) throw new NullReferenceException("VolumeData is null");

            Log.Message($"{data.Name} - Volume render creation started");

            Data = data;
            
            Renderer.transform.localScale = data.Size;

            Color[] colors = new Color[data.Values.Length];

            await Task.Run(() => { // Start as a task so it doesn't lock the main thread
                Parallel.For(0, colors.Length, i => {
                    colors[i] = new Color(data.Values[i], data.Values[i], data.Values[i]);
                });
            });

            // Start min/max tasks on side threads while the texture load happens on the main thread
            var defaultWindowMinTask = Task.Run(() => data.Values.AsParallel().Min());
            var defaultWindowMaxTask = Task.Run(() => data.Values.AsParallel().Max());

            Texture3D texture = new Texture3D(
                data.Dimensions.x,
                data.Dimensions.y,
                data.Dimensions.z, 
                TextureFormat.RFloat, 
                false);

            texture.wrapMode = TextureWrapMode.Clamp;
            texture.SetPixels(colors);
            texture.Apply();

            Renderer.material.SetTexture("_DataTex", texture);
            Renderer.material.SetTexture("_TFTex", TransferFunction.GenerateTexture());
            Renderer.material.SetFloatArray("_Dimensions", new float[]
            {
                data.Dimensions.x,
                data.Dimensions.y,
                data.Dimensions.z
            });

            await Task.WhenAll(defaultWindowMinTask, defaultWindowMaxTask);
            SetWindow(defaultWindowMinTask.Result, defaultWindowMaxTask.Result);
            SetCutoff(defaultWindowMinTask.Result, defaultWindowMaxTask.Result);

            Log.Message($"{Data.Name} - Volume render creation completed");
        }

        public void SetWindow(float min, float max)
        {
            Renderer.material.SetFloat("_WindowMin", min);
            Renderer.material.SetFloat("_WindowMax", max);
        }

        public void SetCutoff(float min, float max)
        {
            Renderer.material.SetFloat("_CutMin", min);
            Renderer.material.SetFloat("_CutMax", max);
        }
    }
}
