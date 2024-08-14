using SimpleFileBrowser;
using System.Threading.Tasks;

namespace HiveVolumeRenderer.Import
{
    public static class PathBrowser
    {
        public static async Task<string[]> GetPaths()
        {
            return await GetPaths(FileBrowser.PickMode.FilesAndFolders);
        }

        public static async Task<string[]> GetFilePaths()
        {
            return await GetPaths(FileBrowser.PickMode.Files);
        }

        public static async Task<string[]> GetFolderPaths()
        {
            return await GetPaths(FileBrowser.PickMode.Folders);
        }

        private static async Task<string[]> GetPaths(FileBrowser.PickMode pickMode)
        {
            bool windowOpen = true;
            string[] outPaths = null;

            FileBrowser.ShowLoadDialog(
                onSuccess: (paths) =>
                {
                    outPaths = paths;
                    windowOpen = false;
                },
                onCancel: () =>
                {
                    outPaths = null;
                    windowOpen = false;
                },
                pickMode: pickMode
                );

            while (windowOpen)
                await Task.Delay(100);

            return outPaths;
        }
    }
}
