using System;
using System.IO;

namespace A5Soft.CARMA.Application.DataPortal.Internal
{
    /// <summary>
    /// Creates the temp files that uploaded files are spooled to. A spooled file is seekable, has a
    /// known length, and is deleted by the operating system when it is closed. Files of a few
    /// megabytes mostly stay in the OS page cache and rarely reach the disk.
    /// </summary>
    internal static class FileSpool
    {
        public static FileStream Create(string directory, int bufferSize)
        {
            var path = Path.Combine(string.IsNullOrWhiteSpace(directory) ? Path.GetTempPath() : directory,
                "carma-upload-" + Guid.NewGuid().ToString("N") + ".tmp");

            return new FileStream(path, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None,
                Math.Max(4096, Math.Min(bufferSize, 81920)),
                FileOptions.DeleteOnClose | FileOptions.Asynchronous | FileOptions.SequentialScan);
        }
    }
}
