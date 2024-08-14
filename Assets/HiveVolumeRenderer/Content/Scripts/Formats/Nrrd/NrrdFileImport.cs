using HiveVolumeRenderer.Dicom;
using HiveVolumeRenderer.Import;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace HiveVolumeRenderer
{
    public class NrrdFileImport : VolumeImporter
    {
        public override bool CanImport(string[] paths)
        {
            foreach (var path in paths)
                if (Path.GetExtension(path) == ".nrrd")
                    return true;

            return false;
        }

        protected override List<VolumeStream> GetVolumeStreams(string[] paths)
        {
            var streams = new List<VolumeStream>();

            foreach (var path in paths)
            {
                if (!File.Exists(path))
                    continue;

                if (Path.GetExtension(path) != ".nrrd")
                    continue;

                streams.Add(new NrrdFileStream(path));
            }

            return streams;
        }
    }
}
