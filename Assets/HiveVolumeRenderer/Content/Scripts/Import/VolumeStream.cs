using System.Threading.Tasks;

namespace HiveVolumeRenderer.Import
{
    public abstract class VolumeStream
    {
        public VolumeData VolumeData { get; private set; } = null;
        public StreamStatus LoadStatus { get; private set; } = StreamStatus.NotStarted;

        public enum StreamStatus
        {
            NotStarted,
            InProgress,
            Completed
        }

        public async Task<VolumeData> LoadVolumeData()
        {
            if (LoadStatus != StreamStatus.InProgress)
                StartLoadingData();

            while (LoadStatus == StreamStatus.InProgress)
                await Task.Delay(25);

            return VolumeData;
        }

        protected abstract Task<VolumeData> LoadVolumeDataInternal();

        private async void StartLoadingData()
        {
            LoadStatus = StreamStatus.InProgress;

            VolumeData = await LoadVolumeDataInternal();

            LoadStatus = StreamStatus.Completed;
        }
    }
}