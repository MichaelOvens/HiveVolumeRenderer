using HiveVolumeRenderer.Import;
using UnityEngine;
using UnityEngine.UI;

namespace HiveVolumeRenderer.Browser
{
    public class VolumeBrowser : MonoBehaviour
    {
        [SerializeField] private VolumeBrowserItem _prefab;
        [SerializeField] private RectTransform _parent;

        private VolumeBrowserItem _item = null;
        
        private bool _layoutIsDirty = false;

        public void AddVolume(VolumeStream stream)
        {
            Clear(); // Currently only support one volume at a time

            _item = Instantiate(_prefab, _parent);
            _item.OnLayoutDirty += (x) => _layoutIsDirty = true;
            _item.Inject(stream);
        }

        public void Clear()
        {
            if (_item != null)
                Destroy(_item.gameObject);

            _item = null;
        }

        private void LateUpdate()
        {
            if (_layoutIsDirty)
            {
                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(_parent);

                _layoutIsDirty = false;
            }
        }
    }
}