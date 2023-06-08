using System.IO.Compression;

namespace DynamoPackagesAnalyzer.Helper
{
    /// <summary>
    /// Provides methods to extract zip archives
    /// </summary>
    internal static class ZipArchiveHelper
    {
        /// <summary>
        /// Extract a zip file
        /// </summary>
        /// <param name="file"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        /// <exception cref="IOException"></exception>
        internal static DirectoryInfo Unzip(FileInfo file, DirectoryInfo output = null)
        {
            output ??= new DirectoryInfo(Path.Combine(file.Directory.FullName, Path.GetFileNameWithoutExtension(file.Name)));

            if (output.Exists)
            {
                output.Delete(true);
            }

            using ZipArchive zipArchive = ZipFile.Open(file.FullName, ZipArchiveMode.Read) ?? throw new IOException("Could not open archive at " + file.FullName);
            output.Create();

            foreach (ZipArchiveEntry entry in zipArchive.Entries)
            {
                string fullPath = Path.GetFullPath(Path.Combine(output.FullName, entry.FullName));
                if (fullPath.StartsWith(output.FullName, StringComparison.OrdinalIgnoreCase))
                {
                    if (Path.GetFileName(fullPath)!.Length == 0)
                    {
                        Directory.CreateDirectory(fullPath);
                        continue;
                    }

                    Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                    entry.ExtractToFile(fullPath, overwrite: false);
                }
            }

            return output;
        }
    }
}
