using HiveVolumeRenderer.Import;
using itk.simple;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;

namespace HiveVolumeRenderer.Dicom
{
    public class DicomFolderImporter : VolumeImporter
    {
        public override bool CanImport(string[] paths)
        {
            bool canImportAtLeastOnePath = false;

            foreach (var path in paths)
            {
                if (!Directory.Exists(path))
                    continue;

                VectorString seriesIds = ImageSeriesReader.GetGDCMSeriesIDs(path);

                if (!seriesIds.Any())
                    continue;
                
                canImportAtLeastOnePath = true;
                break;
            }

            return canImportAtLeastOnePath;
        }

        protected override List<VolumeStream> GetVolumeStreams(string[] paths)
        {
            var streams = new List<VolumeStream>();

            foreach(var path in paths)
            {
                if (!Directory.Exists(path))
                    continue;

                VectorString seriesIds = ImageSeriesReader.GetGDCMSeriesIDs(path);

                if (!seriesIds.Any())
                    continue;

                foreach (var seriesId in seriesIds)
                    streams.Add(new DicomFolderStream(path, seriesId));
            }

            return streams;
        }
    }
}
