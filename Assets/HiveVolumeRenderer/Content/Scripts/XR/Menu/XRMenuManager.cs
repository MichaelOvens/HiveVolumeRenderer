using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace HiveVolumeRenderer.XR.Menu
{
    public class XRMenuManager : MonoBehaviour
    {
        [SerializeField] private InputAction _inputAction;

        [SerializeField] private Transform _head;
        [SerializeField] private Vector2 _offset;

        private void Awake()
        {
            _inputAction.performed += OnToggleMenuManager;
            _inputAction.Enable();
        }

        private void OnToggleMenuManager(InputAction.CallbackContext context)
        {
            Vector3 verticalOffset = Vector3.up;
            Vector3 horizontalOffset = new Vector3()
            {
                x = _head.forward.x,
                y = 0f,
                z = _head.forward.z
            };

            transform.position = _head.position
                + verticalOffset * _offset.y
                + horizontalOffset * _offset.x;

            transform.rotation = Quaternion.LookRotation(transform.position - _head.position);
        }
    }
}
