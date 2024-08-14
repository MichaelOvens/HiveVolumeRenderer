using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HiveVolumeRenderer.XR
{
    public class ResetRenderTransform : MonoBehaviour
    {
        [SerializeField] private InputAction _resetAction;
        [SerializeField] private float _horizontalOffset;
        [SerializeField] private float _verticalOffset;
        [SerializeField] private float _duration;

        private void Awake()
        {
            _resetAction.performed += OnReset;
            _resetAction.Enable();
        }

        private void OnReset(InputAction.CallbackContext context)
        {
            transform.localScale = Vector3.zero;

            Transform _head = Camera.main.transform;

            Vector3 targetPosition = _head.position;

            Vector3 forward = _head.forward;
            forward.y = 0f;
            forward.Normalize();

            targetPosition += forward * _horizontalOffset;
            targetPosition += Vector3.up * _verticalOffset;

            transform.position = targetPosition;
            transform.rotation = Quaternion.LookRotation(-forward, Vector3.up);
            transform.DOScale(1f, _duration);
        }
    }
}
