using Dynamo.PackageManager.Interfaces;

namespace Dynamo.PackageManager
{
    /// <summary>
    ///     An abstract FileSystem for mocking purposes
    /// </summary>
    public interface IFileSystem
    {
        void CopyFile(string filePath, string destinationPath);
        void DeleteFile(string filePath);
        IDirectoryInfo TryCreateDirectory(string directoryPath);

        bool DirectoryExists(string directoryPath);
        bool FileExists(string filePath);

        void WriteAllText(string filePath, string content);
    }
}