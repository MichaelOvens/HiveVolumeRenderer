using HiveVolumeRenderer.Browser;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace HiveVolumeRenderer.Import
{
    public abstract class VolumeImporter : MonoBehaviour
    {
        [SerializeField] private VolumeBrowser _volumeBrowser;

        public abstract bool CanImport(string[] paths);

        public void Import(string[] paths)
        {
            var streams = GetVolumeStreams(paths);

            foreach(var stream in streams)
                _volumeBrowser.AddVolume(stream);
        }

        protected abstract List<VolumeStream> GetVolumeStreams(string[] paths);
    }
}
