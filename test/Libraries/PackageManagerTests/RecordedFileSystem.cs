using System;
using System.Collections.Generic;
using System.Linq;
using Dynamo.PackageManager.Interfaces;
using Moq;

namespace Dynamo.PackageManager.Tests
{
    /// <summary>
    ///     Allows mocking of all file system operations while recording all of them
    /// </summary>
    public class RecordedFileSystem : IFileSystem
    {
        private List<string> allFiles = new List<string>();
        private List<string> allDirectories = new List<string>();
        private readonly List<string> deletedFiles = new List<string>();
        private readonly List<string> deletedDirectories = new List<string>();
        private readonly List<IDirectoryInfo> directoriesCreated = new List<IDirectoryInfo>();
        private readonly List<Tuple<string, string>> copiedFiles = new List<Tuple<string, string>>();
        private readonly List<Tuple<string, string>> newFilesWritten = new List<Tuple<string, string>>();

        private readonly Func<string, bool> fileExistsFunc;
        private readonly Func<string, bool> dirExistsFunc;

        public IEnumerable<string> DeletedFiles { get { return deletedFiles; } }
        public IEnumerable<string> DeletedDirectories { get { return deletedDirectories; } }
        public IEnumerable<IDirectoryInfo> DirectoriesCreated { get { return directoriesCreated; } }
        public IEnumerable<Tuple<string, string>> CopiedFiles { get { return copiedFiles; } }
        public IEnumerable<Tuple<string, string>> NewFilesWritten { get { return newFilesWritten; } }

        #region Constructors and initializers

        public RecordedFileSystem(Func<string, bool> fileExistsFunc = null, Func<string, bool> dirExistsFunc = null)
        {
            this.fileExistsFunc = fileExistsFunc ?? ((x) => false);
            this.dirExistsFunc = dirExistsFunc ?? ((x) => false);
        } 

        internal void SetFiles(IEnumerable<string> paths)
        {
            allFiles = paths.ToList();
        }

        internal void SetDirectories(IEnumerable<string> paths)
        {
            allDirectories = paths.ToList();
        }

        #endregion

        #region IFileSystem implementation

        public IEnumerable<string> GetFiles(string dir)
        {
            return allFiles;
        }

        public IEnumerable<string> GetDirectories(string dir)
        {
            return allDirectories;
        }

        public void CopyFile(string filePath, string destinationPath)
        {
            this.copiedFiles.Add(new Tuple<string, string>(filePath, destinationPath));
            this.allFiles.Add(destinationPath);
        }

        public void DeleteFile(string filePath)
        {
            this.deletedFiles.Add(filePath);
        }

        public void DeleteDirectory(string directoryPath)
        {
            this.deletedDirectories.Add(directoryPath);
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

        #endregion
    }
}