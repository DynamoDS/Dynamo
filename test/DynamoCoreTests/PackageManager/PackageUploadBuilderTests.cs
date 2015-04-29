using System;
using System.Collections.Generic;
using Dynamo.PackageManager;
using Moq;
using NUnit.Framework;

namespace Dynamo.Tests
{
    class PackageUploadBuilderTests
    {
        #region Mocks

        private class MockFileInfo : IFileInfo
        {
            private readonly Func<string> nameFunc;
            private readonly Func<long> lengthFunc;

            public MockFileInfo(Func<string> nameFunc, Func<long> lengthFunc)
            {
                this.nameFunc = nameFunc;
                this.lengthFunc = lengthFunc;
            }

            public string Name
            {
                get { return nameFunc(); }
            }

            public long Length
            {
                get { return lengthFunc(); }
            }
        }

        private class MockFileCompressor : ICompressor
        {
            private readonly Func<string, IFileInfo> zipFunc;

            public IFileInfo Zip(string directoryPath)
            {
                return zipFunc(directoryPath);
            }
        }

        private class MockDirectoryInfo : IDirectoryInfo
        {
            private readonly Func<string> nameFunc;

            public MockDirectoryInfo(Func<string> nameFunc)
            {
                this.nameFunc = nameFunc;
            }

            public string FullName
            {
                get { return nameFunc(); }
            }
        }

        private class RecordedFileSystemMock : IFileSystem
        {
            private readonly List<string> deletedFiles = new List<string>();
            private readonly List<IDirectoryInfo> directoriesCreated = new List<IDirectoryInfo>();
            private readonly List<Tuple<string, string>> copiedFiles = new List<Tuple<string, string>>();
            private readonly List<Tuple<string, string>> filesWritten = new List<Tuple<string, string>>();

            public IEnumerable<string> DeletedFiles { get {  return deletedFiles; } }
            public IEnumerable<IDirectoryInfo> DirectoriesCreated { get { return directoriesCreated; } }
            public IEnumerable<Tuple<string, string>> CopiedFiles { get { return copiedFiles; } }
            public IEnumerable<Tuple<string, string>> FilesWritten { get { return filesWritten; } } 

            public void CopyFile(string filePath, string destinationPath)
            {
                this.copiedFiles.Add(new Tuple<string, string>(filePath, filePath));
            }

            public void DeleteFile(string filePath)
            {
                this.deletedFiles.Add(filePath);
            }

            public IDirectoryInfo TryCreateDirectory(string directoryPath)
            {
                var m = new MockDirectoryInfo(directoryPath);
                this.directoriesCreated.Add(m);
                return m;
            }

            public bool DirectoryExists(string directoryPath)
            {
                return false;
            }

            public bool FileExists(string filePath)
            {
                return false;
            }

            public void WriteAllText(string filePath, string content)
            {
                this.filesWritten.Add(new Tuple<string, string>(filePath, content));
            }
        }

        #endregion

        [Test]
        public void FormPackageDirectory_BuildsDirectoryForValidDirectory()
        {
            Assert.Inconclusive("Finish me");
        }

        [Test]
        public void FormPackageDirectory_DoesNothingForInvalidDirectory()
        {
            Assert.Inconclusive("Finish me");
        }

        [Test]
        public void WritePackageHeader_WritesValidPackageHeader()
        {
            Assert.Inconclusive("Finish me");
        }

        [Test]
        public void CopyFilesIntoPackageDirectory_SucceedsForInvalidTargetDirectory()
        {
            Assert.Inconclusive("Finish me");
        }

        [Test]
        public void CopyFilesIntoPackageDirectory_FailsForInvalidTargetDirectory()
        {
            Assert.Inconclusive("Finish me");
        }

        [Test]
        public void NewPackage_FailsToZipForOverlyLargeFile()
        {
            var fs = new RecordedFileSystemMock();

            var bigFile = new Mock<IFileInfo>();
            bigFile.Setup(x => x.Length).Returns(PackageUploadBuilder.MaximumPackageSize + 1);
            bigFile.Setup(x => x.Name).Returns("Foo");
            
            var zip = new Mock<ICompressor>();
            zip.Setup((x) => x.Zip("directory")).Returns(bigFile.Object);

            var pub = new PackageUploadBuilder(fs, zip.Object);

            pub.NewPackage(new PackageUploadBuilder.UploadParams()
            {
                
            });

        }

        [Test]
        public void RemoveDyfFiles_SucceedsForValidFiles()
        {
            Assert.Inconclusive("Finish me");
        }

        [Test]
        public void RemoveDyfFiles_FailsForInvalidFiles()
        {
            Assert.Inconclusive("Finish me");
        }

        [Test]
        public void RemapCustomNodeFilePaths_SucceedsForValidFiles()
        {
            Assert.Inconclusive("Finish me");
        }

        [Test]
        public void RemapCustomNodeFilePaths_FailsForInvalidFiles()
        {
            Assert.Inconclusive("Finish me");
        }

    }
}
