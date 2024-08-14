using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HiveVolumeRenderer.XR.Player
{
    public class ControllerHintColor : MonoBehaviour
    {
        [SerializeField] private InputAction _touchAction;
        [SerializeField] private InputAction _pressAction;

        [SerializeField] private Renderer _renderer;
        [SerializeField] private float _transitionDuration;
        [SerializeField] private Color _idleColor;
        [SerializeField] private Color _touchedColor;
        [SerializeField] private Color _pressedColor;

        private bool _isTouched = false;
        private bool _isPressed = false;

        private void OnEnable()
        {
            _touchAction.performed += OnTouchedPerformed;
            _touchAction.canceled += OnTouchCanceled;
            _touchAction.Enable();

            _pressAction.performed += OnPressPerformed;
            _pressAction.canceled += OnPressCanceled;
            _pressAction.Enable();
        }

        private void OnDisable()
        {
            _touchAction.performed -= OnTouchedPerformed;
            _touchAction.canceled -= OnTouchCanceled;

            _pressAction.performed -= OnPressPerformed;
            _pressAction.canceled -= OnPressCanceled;
        }

        private void OnTouchedPerformed(InputAction.CallbackContext context)
        {
            _isTouched = true;
            UpdateColor();
        }

        private void OnTouchCanceled(InputAction.CallbackContext context)
        {
            _isTouched = false;
            UpdateColor();
        }

        private void OnPressPerformed(InputAction.CallbackContext context)
        {
            _isPressed = true;
            UpdateColor();
        }

        private void OnPressCanceled(InputAction.CallbackContext context)
        {
            _isPressed = false;
            UpdateColor();
        }

        private void UpdateColor()
        {
            Color targetColor = 
                    _isPressed ? _pressedColor
                  : _isTouched ? _touchedColor
                  : _idleColor;

            _renderer.material.DOColor(targetColor, _transitionDuration);
        }
    }
}
