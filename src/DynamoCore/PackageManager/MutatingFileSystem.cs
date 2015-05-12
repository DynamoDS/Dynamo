using System.IO;
using Dynamo.Annotations;
using Dynamo.PackageManager.Interfaces;

namespace Dynamo.PackageManager
{
    /// <summary>
    ///     An IFileSystem that actually mutates the underlying file system
    /// </summary>
    internal class MutatingFileSystem : IFileSystem
    {
        public void CopyFile(string filePath, string destinationPath)
        {
            File.Copy(filePath, destinationPath);
        }

        public void DeleteFile(string filePath)
        {
            File.Delete(filePath);
        }

        public  IDirectoryInfo TryCreateDirectory(string path)
        {
            return Directory.Exists(path)
                ? new RealDirectoryInfo(new DirectoryInfo(path))
                : new RealDirectoryInfo(Directory.CreateDirectory(path));
        }

        public bool DirectoryExists(string directoryPath)
        {
            return Directory.Exists(directoryPath);
        }

        public bool FileExists(string filePath)
        {
            return File.Exists(filePath);
        }

        public void WriteAllText(string filePath, string content)
        {
            File.WriteAllText(filePath, content);
        }
    }
}