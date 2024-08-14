using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HiveVolumeRenderer.XR
{
    public class MixedRealityManager : MonoBehaviour
    {
        public bool PassthroughEnabled { get; private set; } = false;

        [SerializeField] private Camera _mainCamera;
        [SerializeField] private bool _passthroughEnabledOnStart;
        [SerializeField] private GameObject _questPassthrough;

        private void Awake()
        {
            // The Quest passthrough rig has a heavy load on Awake,
            // so we want this to run as the scene loads
            SetPassthrough(true);
        }

        void Start()
        {
            if (!_passthroughEnabledOnStart)
                SetPassthrough(false);
        }

        [Button("Toggle Passthrough")]
        public void TogglePassthrough()
        {
            SetPassthrough(!PassthroughEnabled);
        }

        public void SetPassthrough(bool passthroughEnabled)
        {
            // Passthrough only works in the editor
            if (!Application.isEditor) return;

            PassthroughEnabled = passthroughEnabled;
            _mainCamera.enabled = !passthroughEnabled;
            _questPassthrough.SetActive(passthroughEnabled);
        }
    }
}
