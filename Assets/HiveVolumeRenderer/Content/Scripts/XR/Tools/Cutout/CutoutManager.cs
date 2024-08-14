using System.Collections.Generic;
using UnityEngine;

namespace HiveVolumeRenderer.XR.Tools.Cutout
{
    public class CutoutManager : MonoBehaviour
    {
        [SerializeField] private Renderer _renderer;

        // Derived from shader
        private const int MaxToolCount = 8;

        private List<CutoutTool> _tools = new();
        private float[] _types = new float[MaxToolCount];
        private Matrix4x4[] _matrices = new Matrix4x4[MaxToolCount];

        public void AddTool(CutoutTool tool)
        {
            if (_tools.Contains(tool)) return;

            _tools.Add(tool);
            if (_tools.Count > 0)
                _renderer.material.EnableKeyword("CUTOUT_ON");
        }

        public void RemoveTool(CutoutTool tool)
        {
            if (!_tools.Contains(tool)) return;

            _tools.Remove(tool);
            if (_tools.Count == 0)
                _renderer.material.DisableKeyword("CUTOUT_ON");
        }

        private void Update()
        {
            if (_tools.Count == 0) return;

            for (int i = 0; i < Mathf.Min(_tools.Count, MaxToolCount); i++)
            {
                _types[i] = (float)_tools[i].CutoutType;
                _matrices[i] = _tools[i].GetVolumeToToolMatrix(_renderer.transform);
            }

            _renderer.material.SetFloatArray("_CutoutTypes", _types);
            _renderer.material.SetMatrixArray("_CutoutMatrices", _matrices);
            _renderer.material.SetInt("_CutoutToolCount", _tools.Count);
        }
    }
}
