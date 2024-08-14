using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HiveVolumeRenderer.XR.Tools.Window
{
    public class WindowTool : MonoBehaviour
    {
        [field: SerializeField] private GameObject _interactors;

        public void SetActive(bool isActive)
        {
            _interactors.SetActive(isActive);
        }
    }
}
