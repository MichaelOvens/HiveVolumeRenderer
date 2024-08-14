using System.Collections.Generic;
using UnityEngine;

namespace HiveVolumeRenderer.XR.Tools.Cutout
{
    public class CutoutTool : MonoBehaviour
    {
        [field: SerializeField] public CutoutType CutoutType { get; private set; }
        [field: SerializeField] public Collider Collider { get; private set; }
        
        private List<CutoutManager> _activeCutouts = new();

        public Matrix4x4 GetVolumeToToolMatrix(Transform volume)
        {
            return Collider.transform.worldToLocalMatrix * volume.localToWorldMatrix;
        }

        public void SetActive(bool isActive)
        {
            Collider.gameObject.SetActive(isActive);

            if (!isActive)
                ClearActiveCutouts();
        }

        private void OnDisable()
        {
            ClearActiveCutouts();
        }

        private void ClearActiveCutouts()
        {
            foreach (var cutout in _activeCutouts)
                cutout.RemoveTool(this);

            _activeCutouts.Clear();
        }

        private void OnTriggerEnter(Collider other)
        {
            var manager = other.GetComponentInParent<CutoutManager>();

            if (manager != null && !_activeCutouts.Contains(manager))
            {
                manager.AddTool(this);
                _activeCutouts.Add(manager);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            var manager = other.GetComponentInParent<CutoutManager>();

            if (manager != null && _activeCutouts.Contains(manager))
            {
                manager.RemoveTool(this);
                _activeCutouts.Remove(manager);
            }
        }
    }
}
