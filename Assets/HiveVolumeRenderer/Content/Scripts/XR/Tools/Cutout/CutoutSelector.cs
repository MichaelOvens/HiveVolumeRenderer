using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HiveVolumeRenderer.XR.Tools.Cutout
{
    public class CutoutSelector : MonoBehaviour
    {
        [SerializeField] private InputAction _selectAxis;
        [SerializeField] private float _selectThreshold;

        private int _selectedIndex = 0;
        private List<CutoutTool> _tools = new();

        private enum AxisState
        {
            DeadZone,
            AboveThreshold,
            BelowThreshold
        }

        private AxisState _state = AxisState.DeadZone;

        private void Awake()
        {
            _selectAxis.Enable();

            _tools.Clear();
            _tools.AddRange(GetComponentsInChildren<CutoutTool>());
            
            SetActiveTool(_tools[0]);
        }

        private void Update()
        {
            float value = _selectAxis.ReadValue<Vector2>().x;

            if (_state == AxisState.DeadZone)
            {
                if (value > _selectThreshold)
                {
                    SelectNextTool();
                }
                else if (value < -_selectThreshold)
                {
                    SelectLastTool();
                }
            }
            else if (_state == AxisState.AboveThreshold)
            {
                if (value < _selectThreshold)
                {
                    _state = AxisState.DeadZone;
                }
            }
            else if (_state == AxisState.BelowThreshold)
            {
                if (value > -_selectThreshold)
                {
                    _state = AxisState.DeadZone;
                }
            }
        }

        private void SelectNextTool()
        {
            _state = AxisState.AboveThreshold;

            _selectedIndex++;
            if (_selectedIndex >= _tools.Count)
                _selectedIndex = 0;

            SetActiveTool(_tools[_selectedIndex]);
        }

        private void SelectLastTool()
        {
            _state = AxisState.BelowThreshold;

            _selectedIndex--;
            if (_selectedIndex < 0)
                _selectedIndex = _tools.Count - 1;

            SetActiveTool(_tools[_selectedIndex]);
        }

        private void SetActiveTool(CutoutTool activeTool)
        {
            foreach (var tool in _tools)
                tool.SetActive(tool == activeTool);
        }
    }
}
