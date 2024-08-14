using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace HiveVolumeRenderer.Render.Utilities
{
    public class AsyncLoadIndicator : MonoBehaviour
    {
        public Vector3 Size = Vector3.one;

        private const float RotationSpeed = 90f;
        private const float ChildSizeRatio = 0.75f;
        private const float ChildPositionOffset = 0.01f;

        public AsyncSpinner[] _childSpinners;
        private float _rotationAroundPrimaryAxis = 0f;

        [System.Serializable]
        public class AsyncSpinner
        {
            public Transform Transform;
            public Quaternion Orientation;
            public Vector3 Direction;
            public int PrimaryAxis;
            public List<int> SecondaryAxes;
        }

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        private void Start()
        {
            _childSpinners = new AsyncSpinner[transform.childCount];

            for (int i = 0; i < transform.childCount; i++)
            {
                Vector3 direction = transform.GetChild(i).localPosition.normalized;

                Quaternion orientation = Quaternion.LookRotation(-direction, Vector3.up);

                var absoluteDirection = new List<float>
                {
                    Mathf.Abs(direction.x),
                    Mathf.Abs(direction.y),
                    Mathf.Abs(direction.z)
                };

                // The axis about which the spinner rotates
                int primaryAxis = absoluteDirection.IndexOf(absoluteDirection.Max());

                var secondaryAxes = new List<int>() { 0, 1, 2 };
                secondaryAxes.Remove(primaryAxis);

                _childSpinners[i] = new AsyncSpinner()
                {
                    Transform = transform.GetChild(i),
                    Orientation = orientation,
                    Direction = direction,
                    PrimaryAxis = primaryAxis,
                    SecondaryAxes = secondaryAxes
                };
            }
        }

        private void Update()
        {
            // Maintain the aspect ratio of the child spinners
            transform.localScale = new Vector3()
            {
                x = 1f / transform.parent.localScale.x,
                y = 1f / transform.parent.localScale.y,
                z = 1f / transform.parent.localScale.z
            };

            _rotationAroundPrimaryAxis += RotationSpeed * Time.deltaTime;
            _rotationAroundPrimaryAxis %= 360f;

            foreach (var spinner in _childSpinners)
            {
                spinner.Transform.localScale = Vector3.one * spinner.SecondaryAxes.Select(i => Size[i]).Min() * ChildSizeRatio;
                spinner.Transform.localPosition = spinner.Direction * (Size[spinner.PrimaryAxis] / 2f + ChildPositionOffset);
                spinner.Transform.localRotation = Quaternion.AngleAxis(_rotationAroundPrimaryAxis, spinner.Direction) * spinner.Orientation;
            }
        }
    }
}
