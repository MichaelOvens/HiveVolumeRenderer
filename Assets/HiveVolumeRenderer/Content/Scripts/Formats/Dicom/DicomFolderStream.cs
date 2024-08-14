using HiveVolumeRenderer.Import;
using itk.simple;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace HiveVolumeRenderer.Dicom
{
    public class DicomFolderStream : VolumeStream
    {
        public readonly string DirectoryPath;
        public readonly string SeriesId;

        public DicomFolderStream(string directoryPath, string seriesId)
        {
            if (!Directory.Exists(directoryPath))
                throw new NullReferenceException($"{directoryPath} does not exist");

            VectorString seriesIds = ImageSeriesReader.GetGDCMSeriesIDs(directoryPath);

            if (!seriesIds.Contains(seriesId))
                throw new NullReferenceException($"{directoryPath} does not contain series {seriesId}");

            DirectoryPath = directoryPath;
            SeriesId = seriesId;
        }

        protected async override Task<VolumeData> LoadVolumeDataInternal()
        {
            // Get a list of all DICOM files from the target series in this folder
            VectorString dicomFileNames = await Task.Run(() => ImageSeriesReader.GetGDCMSeriesFileNames(DirectoryPath, SeriesId));
            if (!dicomFileNames.Any())
                throw new NullReferenceException($"No DICOM files found for series {SeriesId} in {DirectoryPath}");

            // Set up a file reader for meta data
            ImageFileReader fileReader = new ImageFileReader();
            fileReader.SetFileName(dicomFileNames[0]);
            fileReader.LoadPrivateTagsOn();
            fileReader.ReadImageInformation();

            // Set up a series reader for volume data
            ImageSeriesReader seriesReader = new ImageSeriesReader();
            seriesReader.SetFileNames(dicomFileNames);

            // Declare variables
            string name = null;
            float[] values = null;
            Vector3 primaryAxis = Vector3.one;
            Vector3Int dimensions = Vector3Int.one;
            Vector3 size = Vector3.one;

            // Assign variables
            name = ReadVolumeName(fileReader);
            primaryAxis = ReadPrimaryAxis(fileReader);
            (values, dimensions, size) = await ReadVolumeData(seriesReader);

            // Package variables
            return new VolumeData
            (
                name: name,
                values: values,
                primaryAxis: primaryAxis,
                dimensions: dimensions,
                size: size
            );
        }

        private string ReadVolumeName(ImageFileReader fileReader)
        {
            // Read the patient's name
            const string PatientNameDicomKey = "0010|0010";
            string patientName = fileReader.GetMetaData(PatientNameDicomKey);

            // DICOM uses ^ characters instead of whitespace
            patientName = Regex.Replace(patientName, "\\^", " ");

            return patientName;
        }

        private Vector3 ReadPrimaryAxis(ImageFileReader fileReader)
        {
            // Image Orientation (Patient)
            // https://dicom.innolitics.com/ciods/rt-dose/image-plane/00200037

            const string ImageOrientationPatientKey = "0020|0037";
            string imageOrientationPatient = fileReader.GetMetaData(ImageOrientationPatientKey);
            string[] elements = imageOrientationPatient.Split('\\');

            Vector3 firstSliceAxis = new Vector3(
                float.Parse(elements[0]),
                float.Parse(elements[1]),
                float.Parse(elements[2])
                );

            Vector3 secondSliceAxis = new Vector3(
                float.Parse(elements[3]),
                float.Parse(elements[4]),
                float.Parse(elements[5])
                );

            // Axis in DICOM (Left Posterior Superior) space
            Vector3 dicomVolumeAxis = Vector3.Cross(firstSliceAxis, secondSliceAxis);

            // Convert from right-handed system (DICOM) to left-handed system (Unity)
            dicomVolumeAxis = -dicomVolumeAxis;

            // Convert to Unity coordinate system (Right Superior Anterior)
            Vector3 unityVolumeAxis = new Vector3()
            {
                x = -dicomVolumeAxis.x,
                y = dicomVolumeAxis.z,
                z = -dicomVolumeAxis.y
            };

            return unityVolumeAxis;
        }

        private async Task<(float[] values, Vector3Int dimensions, Vector3 size)> ReadVolumeData(ImageSeriesReader seriesReader)
        {
            Image image = await Task.Run(() => seriesReader.Execute());

            // Convert to Unity coordinate system
            image = SimpleITK.DICOMOrient(image, "RSA");

            // Cast to 32-bit float
            image = SimpleITK.Cast(image, PixelIDValueEnum.sitkFloat32);

            VectorUInt32 dicomSize = image.GetSize();

            // Calculate the number of pixels
            int numPixels = 1;
            for (int dim = 0; dim < image.GetDimension(); dim++)
                numPixels *= (int)dicomSize[dim];

            // Read pixel data
            var pixelData = new float[numPixels];
            await Task.Run(() =>
            {
                IntPtr imgBuffer = image.GetBufferAsFloat();
                Marshal.Copy(imgBuffer, pixelData, 0, numPixels);
            });

            // Clamp pixel data to Houndsfield scale
            await Task.Run(() =>
            {
                Parallel.For(0, pixelData.Length, i => {
                    pixelData[i] = Mathf.Clamp(pixelData[i], -1024, 3071);
                });
            });

            // Read spacing to calculate physical size
            VectorDouble spacing = image.GetSpacing();

            // Assign outputs
            var dimensions = new Vector3Int()
            {
                x = (int)dicomSize[0],
                y = (int)dicomSize[1],
                z = (int)dicomSize[2]
            };
            var size = new Vector3()
            {
                x = (float)(spacing[0] * dicomSize[0]) / 1000.0f, // mm to m
                y = (float)(spacing[1] * dicomSize[1]) / 1000.0f, // mm to m
                z = (float)(spacing[2] * dicomSize[2]) / 1000.0f  // mm to m
            };

            return (pixelData, dimensions, size);
        }
    }
}
