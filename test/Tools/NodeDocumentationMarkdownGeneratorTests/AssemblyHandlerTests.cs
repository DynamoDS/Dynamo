using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NodeDocumentationMarkdownGenerator;
using NUnit.Framework;

namespace NodeDocumentationMarkdownGeneratorTests
{
    [TestFixture]
    class AssemblyHandlerTests
    {
        private const string CORENODEMODELS_DLL_NAME = "CoreNodeModels.dll";

        private static readonly string coreDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        private static readonly string toolsTestFilesDirectory = Path.GetFullPath(Path.Combine(coreDirectory, @"..\..\..\docGeneratorTestFiles"));

        [Test]
        public void CanScanAssemblyFromPath()
        {
            // Arrange
            var assemblyPath = Path.Combine(toolsTestFilesDirectory, CORENODEMODELS_DLL_NAME);
            var coreNodeModelMdFilesDir = new DirectoryInfo(Path.Combine(toolsTestFilesDirectory, "TestMdOutput_CoreNodeModels"));
            var coreNodeModelMdFiles = coreNodeModelMdFilesDir.GetFiles();

            // Act
            AppDomain.CurrentDomain.AssemblyResolve += Program.CurrentDomain_AssemblyResolve;
            var mdFileInfos = AssemblyHandler.ScanAssemblies(new List<string> { assemblyPath });


            // Assert
            Assert.IsTrue(coreNodeModelMdFiles.Count() == mdFileInfos.Count);
            AssertMdFileInfos(mdFileInfos, coreNodeModelMdFiles);
        }

        private void AssertMdFileInfos(List<MdFileInfo> mdFileInfos, FileInfo[] coreNodeModelMdFiles)
        {
            var expectedFileNames = coreNodeModelMdFiles.Select(x => Path.GetFileNameWithoutExtension(x.FullName));
            var expectedMdFileInfoNamespace = "CoreNodeModels";
            foreach (var info in mdFileInfos)
            {
                Assert.That(expectedFileNames.Contains(info.FileName));
                Assert.IsTrue(info.NodeNamespace.StartsWith(expectedMdFileInfoNamespace));
            }
        }
    }
}
