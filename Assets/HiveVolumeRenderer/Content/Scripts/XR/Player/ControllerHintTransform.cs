using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HiveVolumeRenderer.XR.Player
{
    public class ControllerHintTransform : MonoBehaviour
    {
        [SerializeField] private InputAction _axisAction;

        [Header("Input")]
        public float InputMin;
        public float InputMax;

        [Header("Output Position")]
        public bool ControlPosition;
        public Vector3 OutputPositionMin;
        public Vector3 OutputPositionMax;

        [Header("Output Rotation")]
        public bool ControlRotation;
        public Vector3 OutputRotationMin;
        public Vector3 OutputRotationMax;

        private void Awake()
        {
            _axisAction.Enable();
        }

        private void Update()
        {
            float inputValue = _axisAction.ReadValue<float>();

            float normalisedValue = (inputValue - InputMin) / (InputMax - InputMin);

            if (ControlPosition)
                transform.localPosition = Vector3.Lerp(OutputPositionMin, OutputPositionMax, normalisedValue);

            if (ControlRotation)
                transform.localRotation = Quaternion.Euler(Vector3.Lerp(OutputRotationMin, OutputRotationMax, normalisedValue));
        }
    }
}
