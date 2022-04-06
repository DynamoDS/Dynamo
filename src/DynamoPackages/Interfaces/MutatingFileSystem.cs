using System.Collections.Generic;
using System.IO;
using Dynamo.PackageManager.Interfaces;

namespace Dynamo.PackageManager
{
    /// <summary>
    ///     An IFileSystem that actually mutates the underlying file system
    /// </summary>
    public class MutatingFileSystem : IFileSystem
    {
        public IEnumerable<string> GetFiles(string dir)
        {
            return Directory.GetFiles(dir, "*", SearchOption.AllDirectories);
        }

        public IEnumerable<string> GetDirectories(string dir)
        {
            return Directory.GetDirectories(dir, "*", SearchOption.AllDirectories);
        }

        public void CopyFile(string filePath, string destinationPath)
        {
            File.Copy(filePath, destinationPath);
        }

        public void DeleteFile(string filePath)
        {
            File.Delete(filePath);
        }

        public void DeleteDirectory(string directoryPath)
        {
            Directory.Delete(directoryPath);
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