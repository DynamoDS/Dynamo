using System;
using System.Collections.Generic;
using Dynamo.PackageManager.Interfaces;
using Moq;

namespace Dynamo.PackageManager.Tests
{
    /// <summary>
    ///     Allows mocking of all file system operations while recording all of them
    /// </summary>
    public class RecordedFileSystem : IFileSystem
    {
        private readonly List<string> deletedFiles = new List<string>();
        private readonly List<IDirectoryInfo> directoriesCreated = new List<IDirectoryInfo>();
        private readonly List<Tuple<string, string>> copiedFiles = new List<Tuple<string, string>>();
        private readonly List<Tuple<string, string>> newFilesWritten = new List<Tuple<string, string>>();

        private readonly Func<string, bool> fileExistsFunc;
        private readonly Func<string, bool> dirExistsFunc;

        public IEnumerable<string> DeletedFiles { get { return deletedFiles; } }
        public IEnumerable<IDirectoryInfo> DirectoriesCreated { get { return directoriesCreated; } }
        public IEnumerable<Tuple<string, string>> CopiedFiles { get { return copiedFiles; } }
        public IEnumerable<Tuple<string, string>> NewFilesWritten { get { return newFilesWritten; } }

        public RecordedFileSystem(Func<string, bool> fileExistsFunc = null, Func<string, bool> dirExistsFunc = null)
        {
            this.fileExistsFunc = fileExistsFunc ?? ((x) => false);
            this.dirExistsFunc = dirExistsFunc ?? ((x) => false);
        } 

        public void CopyFile(string filePath, string destinationPath)
        {
            this.copiedFiles.Add(new Tuple<string, string>(filePath, destinationPath));
        }

        public void DeleteFile(string filePath)
        {
            this.deletedFiles.Add(filePath);
        }

        public IDirectoryInfo TryCreateDirectory(string directoryPath)
        {
            var m = new Mock<IDirectoryInfo>();
            m.Setup(x => x.FullName).Returns(directoryPath);
            var mo = m.Object;

            this.directoriesCreated.Add(mo);
            return mo;
        }

        public bool DirectoryExists(string directoryPath)
        {
            return dirExistsFunc(directoryPath);
        }

        public bool FileExists(string filePath)
        {
            return fileExistsFunc(filePath);
        }

        public void WriteAllText(string filePath, string content)
        {
            this.newFilesWritten.Add(new Tuple<string, string>(filePath, content));
        }
    }
}