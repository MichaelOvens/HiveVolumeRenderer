using System;
using UnityEngine;
using UnityEngine.Events;

namespace HiveVolumeRenderer.XR.Tools
{
    public class XRTool : MonoBehaviour
    {
        public UnityEvent<bool> OnToggled;

        [field: SerializeField] public Texture2D Icon { get; private set; }

        public void SetActive(bool isActive)
        {
            OnToggled?.Invoke(isActive);
        }
    }
}