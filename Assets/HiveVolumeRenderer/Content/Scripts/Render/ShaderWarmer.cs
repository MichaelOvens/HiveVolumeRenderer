using System.Collections.Generic;
using UnityEngine;

namespace HiveVolumeRenderer.Render
{
    public class ShaderWarmer : MonoBehaviour
    {
        [SerializeField] private List<ShaderVariantCollection> _shaders;

        private void Awake()
        {
            foreach (var shader in _shaders)
                shader.WarmUp();
        }

        private void Start()
        {
            Destroy(gameObject);
        }
    }
}
