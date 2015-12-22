using System.Collections.Generic;
using Dynamo.PackageManager.Interfaces;

namespace Dynamo.PackageManager
{
    /// <summary>
    ///     An abstract FileSystem for mocking purposes
    /// </summary>
    public interface IFileSystem
    {
        IEnumerable<string> GetFiles(string dir);
        IEnumerable<string> GetDirectories(string dir);

        void CopyFile(string filePath, string destinationPath);
        void DeleteFile(string filePath);
        void DeleteDirectory(string directoryPath);
        IDirectoryInfo TryCreateDirectory(string directoryPath);

        bool DirectoryExists(string directoryPath);
        bool FileExists(string filePath);

        void WriteAllText(string filePath, string content);
    }
}