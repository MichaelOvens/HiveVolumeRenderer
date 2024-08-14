using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HiveVolumeRenderer.XR.Player
{
    public class ControllerHintRotation : MonoBehaviour
    {
        [SerializeField] private InputAction _axisAction;
        [SerializeField] private AxisVisual[] _axisVisuals;

        private enum Axis
        {
            X = 0,
            Y = 1,
            Z = 2
        }

        [System.Serializable]
        private class AxisVisual
        {
            [Header("Input")]
            public float InputMin;
            public float InputMax;

            [Header("Output")]
            public Axis OutputAxis;
            public float OutputMin;
            public float OutputMax;
        }

        private void Awake()
        {
            _axisAction.Enable();
        }

        private void Update()
        {
            Vector3 eulerAngles = Vector3.zero;

            if (_axisAction.inProgress)
            {
                eulerAngles = _axisVisuals.Length switch
                {
                    1 => ProcessAxis1D(),
                    2 => ProcessAxis2D(),
                    3 => ProcessAxis3D(),
                    _ => throw new IndexOutOfRangeException(_axisVisuals.Length.ToString())
                };
            }

            transform.localRotation = Quaternion.Euler(eulerAngles);
        }

        private Vector3 ProcessAxis1D()
        {
            float input = _axisAction.ReadValue<float>();

            float normalised = (input - _axisVisuals[0].InputMin) / (_axisVisuals[0].InputMax - _axisVisuals[0].InputMin);

            float output = Mathf.Lerp(_axisVisuals[0].OutputMin, _axisVisuals[0].OutputMax, normalised);

            Vector3 euler = Vector3.zero;
            euler[(int)_axisVisuals[0].OutputAxis] = output;

            return euler;
        }

        private Vector3 ProcessAxis2D()
        {
            Vector2 input = _axisAction.ReadValue<Vector2>();

            Vector2 normalised = new Vector2(
                x: (input.x - _axisVisuals[0].InputMin) / (_axisVisuals[0].InputMax - _axisVisuals[0].InputMin), 
                y: (input.y - _axisVisuals[1].InputMin) / (_axisVisuals[1].InputMax - _axisVisuals[1].InputMin)
                );

            Vector2 output = new Vector2(
                x: Mathf.Lerp(_axisVisuals[0].OutputMin, _axisVisuals[0].OutputMax, normalised.x),
                y: Mathf.Lerp(_axisVisuals[1].OutputMin, _axisVisuals[1].OutputMax, normalised.y)
                );

            Vector3 euler = Vector3.zero;

            euler[(int)_axisVisuals[0].OutputAxis] = output[0];
            euler[(int)_axisVisuals[1].OutputAxis] = output[1];

            return euler;
        }

        private Vector3 ProcessAxis3D()
        {
            throw new System.NotImplementedException();
        }
    }
}
