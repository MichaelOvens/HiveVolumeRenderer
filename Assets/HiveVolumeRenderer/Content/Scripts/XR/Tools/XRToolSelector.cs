using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HiveVolumeRenderer.XR.Tools
{
    public class XRToolSelector : MonoBehaviour
    {
        public XRTool ActiveTool { get; private set; } = null;

        [SerializeField] private InputAction _nextToolAction;
        [SerializeField] private InputAction _lastToolAction;
        [SerializeField] private XRToolIcon _icon;
        
        private List<XRTool> _tools = new();

        private void Awake()
        {
            _nextToolAction.performed += OnNextToolAction;
            _nextToolAction.Enable();
            
            _lastToolAction.performed += OnLastToolAction;
            _lastToolAction.Enable();

            _tools.Clear();
            _tools.AddRange(GetComponentsInChildren<XRTool>());
        }

        private void Start()
        {
            foreach (var tool in _tools)
                _icon.RegisterTool(tool);

            if (_tools.Count > 0)
                SetActive(_tools[0], false);
        }

        private void OnNextToolAction(InputAction.CallbackContext context)
        {
            int index = _tools.IndexOf(ActiveTool) + 1;

            if (index >= _tools.Count)
                index = 0;

            SetActive(_tools[index]);
        }

        private void OnLastToolAction(InputAction.CallbackContext context)
        {
            int index = _tools.IndexOf(ActiveTool) - 1;

            if (index < 0)
                index = _tools.Count - 1;

            SetActive(_tools[index]);
        }

        public void SetActive(XRTool activeTool, bool setIconActive = true)
        {
            if (!_tools.Contains(activeTool)) return;

            ActiveTool = activeTool;

            foreach (var tool in _tools)
                tool.SetActive(tool == activeTool);

            _icon.SetActiveTool(activeTool, setIconActive);
        }
    }
}
