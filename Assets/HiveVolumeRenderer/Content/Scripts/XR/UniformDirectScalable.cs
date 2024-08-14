using UnityEngine;
using UnityEngine.InputSystem;

namespace HiveVolumeRenderer.XR
{
    public class UniformDirectScalable : MonoBehaviour
    {
        public float ScaleSpeed = 0.1f;
        public float MinScale, MaxScale;

        [SerializeField] private InputAction _action;

        private void Awake()
        {
            _action.Enable();
        }

        private void Update()
        {
            if (_action.inProgress)
            {
                float delta = _action.ReadValue<Vector2>().y * ScaleSpeed * Time.deltaTime;
                float value = Mathf.Clamp(transform.localScale.x + delta, MinScale, MaxScale);

                transform.localScale = new Vector3(value, value, value);
            }
        }
    }
}
