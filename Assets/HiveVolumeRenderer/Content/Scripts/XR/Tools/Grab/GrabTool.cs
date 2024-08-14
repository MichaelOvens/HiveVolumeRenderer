using UnityEngine;

namespace HiveVolumeRenderer.XR.Tools.Grab
{
    public class GrabTool : MonoBehaviour
    {
        [field: SerializeField] private GameObject _interactors;

        public void SetActive(bool isActive)
        {
            _interactors.SetActive(isActive);
        }
    }
}
