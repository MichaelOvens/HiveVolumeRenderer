using HiveVolumeRenderer.Import;
using itk.simple;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;

namespace HiveVolumeRenderer.Dicom
{
    public class NrrdFileStream : VolumeStream
    {
        public readonly string FilePath;

        public NrrdFileStream(string filePath)
        {
            FilePath = filePath;
        }

        protected override Task<VolumeData> LoadVolumeDataInternal()
        {
            ImageFileReader reader = new ImageFileReader();

            reader.SetFileName(FilePath);

            Image image = reader.Execute();

            // Convert to LPS coordinate system (may be needed for NRRD and other datasets)
            image = SimpleITK.DICOMOrient(image, "RSA");

            // Cast to 32-bit float
            image = SimpleITK.Cast(image, PixelIDValueEnum.sitkFloat32);

            VectorUInt32 size = image.GetSize();

            int numPixels = 1;
            for (int dim = 0; dim < image.GetDimension(); dim++)
                numPixels *= (int)size[dim];

            // Read pixel data
            float[] pixelData = new float[numPixels];
            IntPtr imgBuffer = image.GetBufferAsFloat();
            Marshal.Copy(imgBuffer, pixelData, 0, numPixels);

            VectorDouble spacing = image.GetSpacing();

            Debug.Log("Name not defined for NRRD");
            Debug.Log("Primary axis not defined for NRRD");

            return Task.FromResult(new VolumeData
            (
                name: Path.GetFileName(FilePath),
                values: pixelData,
                primaryAxis: Vector3.one,
                dimensions: new Vector3Int()
                {
                    x = (int)size[0],
                    y = (int)size[1],
                    z = (int)size[2]
                },
                size: new Vector3()
                {
                    x = (float)(spacing[0] * size[0]) / 1000.0f, // mm to m
                    y = (float)(spacing[1] * size[1]) / 1000.0f, // mm to m
                    z = (float)(spacing[2] * size[2]) / 1000.0f  // mm to m
                }
            ));
        }
    }
}
