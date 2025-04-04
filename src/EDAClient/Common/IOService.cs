using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace EDAClient.Common
{
    internal class IOService
    {
        public string GetDataFilePath(int activityID)
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "EDAClient",
                $"{activityID}.json");
        }

        public string OpenFileDialog(string initialPath)
        {
            var dlg = new OpenFileDialog();
            var res = dlg.ShowDialog();
            if (res.HasValue && res.Value)
                return dlg.FileName;
            return null;
        }

        public void OpenFolderInFS(string folderPath)
        {
            if (!string.IsNullOrWhiteSpace(folderPath)) Process.Start(folderPath);
        }
    }
}