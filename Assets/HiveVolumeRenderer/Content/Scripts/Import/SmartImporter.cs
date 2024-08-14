using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace HiveVolumeRenderer.Import
{
    public class SmartImporter : MonoBehaviour
    {
        [SerializeField] private TMP_Text _message;

        private List<VolumeImporter> _importers = new();

        private void Awake()
        {
            _importers.Clear();
            _importers.AddRange(GetComponentsInChildren<VolumeImporter>());
        }

        public async void Import()
        {
            _message.text = "";
            
            var paths = await PathBrowser.GetPaths();

            if (paths != null)
            {
                var validImporters = _importers.Where(i => i.CanImport(paths));

                foreach (var importer in validImporters)
                    importer.Import(paths);

                if (validImporters.Count() == 0)
                    _message.text = "ERROR: No valid files found to import";
            }
        }
    }
}
