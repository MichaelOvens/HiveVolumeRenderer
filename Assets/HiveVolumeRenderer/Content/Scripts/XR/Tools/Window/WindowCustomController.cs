using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HiveVolumeRenderer.XR.Tools.Window
{
    public class WindowCustomController : MonoBehaviour
    {
        [field: SerializeField] public float WindowMin { get; private set; }
        [field: SerializeField] public float WindowMax { get; private set; }
        [field: SerializeField] public float WindowLevel { get; private set; }
        [field: SerializeField] public float WindowWidth { get; private set; }

        public float lowerBound { get; private set; }
        public float upperBound { get; private set; }

        [SerializeField] private float _deadZone;
        [SerializeField] private InputAction _inputAction;

        private void Awake()
        {
            _inputAction.Enable();
        }

        private void Update()
        {
            Vector3 inputValue = _inputAction.ReadValue<Vector2>();

            WindowLevel += GetDelta(inputValue.x);
            WindowWidth += GetDelta(inputValue.y);

            WindowLevel = Mathf.Clamp(WindowLevel, WindowMin, WindowMax);
            WindowWidth = Mathf.Clamp(WindowWidth, 0f, WindowMax - WindowMin);

            lowerBound = WindowLevel - (WindowWidth / 2f);
            upperBound = WindowLevel + (WindowWidth / 2f);

            if (lowerBound < WindowMin || upperBound > WindowMax)
            {
                WindowLevel += Mathf.Max(0f, WindowMin - lowerBound);
                WindowLevel += Mathf.Min(0f, WindowMax - upperBound);
                lowerBound = WindowLevel - (WindowWidth / 2f);
                upperBound = WindowLevel + (WindowWidth / 2f);
            }
        }

        private float GetDelta(float inputValue)
        {
            if (Mathf.Abs(inputValue) < _deadZone)
                return 0f;

            return inputValue;
        }
    }
}
