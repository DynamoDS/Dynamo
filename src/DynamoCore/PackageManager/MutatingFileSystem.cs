using System.IO;
using Dynamo.Annotations;

namespace Dynamo.PackageManager
{
    internal class MutatingFileSystem : IFileSystem
    {
        public void CopyFile([NotNull] string filePath, [NotNull] string destinationPath)
        {
            File.Copy(filePath, destinationPath);
        }

        public void DeleteFile([NotNull] string filePath)
        {
            File.Delete(filePath);
        }

        public  IDirectoryInfo TryCreateDirectory(string path)
        {
            return this.DirectoryExists(path)
                ? new TrueDirectoryInfo(new System.IO.DirectoryInfo(path))
                : new TrueDirectoryInfo(Directory.CreateDirectory(path));
        }

        public bool DirectoryExists([NotNull] string directoryPath)
        {
            return Directory.Exists(directoryPath);
        }

        public bool FileExists([NotNull] string filePath)
        {
            return File.Exists(filePath);
        }

        public void WriteAllText([NotNull] string filePath, [NotNull] string content)
        {
            File.WriteAllText(filePath, content);
        }
    }
}