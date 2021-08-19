using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NodeDocumentationMarkdownGenerator;
using NodeDocumentationMarkdownGenerator.Commands;
using NodeDocumentationMarkdownGenerator.Verbs;
using NUnit.Framework;

namespace NodeDocumentationMarkdownGeneratorTests
{
    [TestFixture]
    public class MarkdownGeneratorTests
    {
        private const string CORENODEMODELS_DLL_NAME = "CoreNodeModels.dll";

        private static readonly string coreDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        private static readonly string toolsTestFilesDirectory = Path.GetFullPath(Path.Combine(coreDirectory, @"..\..\..\docGeneratorTestFiles"));

        private DirectoryInfo tempDirectory = null;

        [SetUp]
        public void SetUp()
        {
            AppDomain.CurrentDomain.AssemblyResolve += Program.CurrentDomain_AssemblyResolve;
        }

        [TearDown]
        public void CleanUp()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= Program.CurrentDomain_AssemblyResolve;

            if (tempDirectory == null || !tempDirectory.Exists) return;

            tempDirectory.Delete(true);
            tempDirectory = null;
        }

        [Test]
        public void ProducesCorrectOutputFromDirectory()
        {
            // Test output is generated with the following args:
            // 
            // NodeDocumentationMarkdownGenerator.exe
            // fromdirectory
            // -i "..\Dynamo\test\Tools\docGeneratorTestFiles"
            // -o "..\Dynamo\test\Tools\docGeneratorTestFiles\TestMdOutput_CoreNodeModels"
            // -f "CoreNodeModels.dll"

            // Arrange
            var testOutputDirName = "TestMdOutput_CoreNodeModels";
            var expectedOutputDirectory = new DirectoryInfo(Path.Combine(toolsTestFilesDirectory, testOutputDirName));
            Assert.That(expectedOutputDirectory.Exists);

            var coreNodeModelsDll = Path.Combine(toolsTestFilesDirectory, CORENODEMODELS_DLL_NAME);
            Assert.That(File.Exists(coreNodeModelsDll));

            // Act
            tempDirectory = CreateTempOutputDirectory();
            Assert.That(tempDirectory.Exists);

            var opts = new FromDirectoryOptions
            {
                InputFolderPath = toolsTestFilesDirectory,
                OutputFolderPath = tempDirectory.FullName,
                Filter = new List<string> { CORENODEMODELS_DLL_NAME },
                ReferencePaths = new List<string>()
            };

            FromDirectoryCommand.HandleDocumentationFromDirectory(opts);

            var generatedFileNames = tempDirectory.GetFiles().Select(x => x.Name);

            // Assert
            CollectionAssert.AreEquivalent(expectedOutputDirectory.GetFiles().Select(x => x.Name), generatedFileNames);
        }


        [Test]
        public void ProducesCorrectOutputFromPackage()
        {
            // Arrange
            var packageName = "Dynamo Samples";
            var packageDirectory = Path.GetFullPath(Path.Combine(toolsTestFilesDirectory, @"..\..\pkgs", packageName));
            var testOutputDirName = "doc";
            tempDirectory = new DirectoryInfo(Path.Combine(packageDirectory, testOutputDirName));
            Assert.IsFalse(tempDirectory.Exists);

            var expectedFileNames = new List<string>
            {
                "Examples.BasicExample.Awesome.md",
                "Examples.BasicExample.Create(point).md",
                "Examples.BasicExample.Create(x, y, z).md",
                "Examples.BasicExample.MultiReturnExample.md",
                "Examples.BasicExample.MultiReturnExample2.md",
                "Examples.BasicExample.Point.md",
                "Examples.CustomRenderExample.Create.md",
                "Examples.PeriodicIncrement.Increment.md",
                "Examples.PeriodicUpdateExample.PointField.md",
                "Examples.TransformableExample.ByGeometry.md",
                "Examples.TransformableExample.Geometry.md",
                "Examples.TransformableExample.TransformableExample.md",
                "Examples.TransformableExample.TransformObject.md",
                "SampleLibraryUI.Examples.ButtonCustomNodeModel.md",
                "SampleLibraryUI.Examples.DropDownExample.md",
                "SampleLibraryUI.Examples.LocalizedCustomNodeModel.md",
                "SampleLibraryUI.Examples.SliderCustomNodeModel.md"
            };
            
            // Act
            var opts = new FromPackageOptions
            {
                InputFolderPath = packageDirectory,
                ReferencePaths = new List<string> { toolsTestFilesDirectory }
            };

            FromPackageFolderCommand.HandlePackageDocumentation(opts);

            tempDirectory.Refresh();

            // Assert
            Assert.IsTrue(tempDirectory.Exists);
            CollectionAssert.AreEquivalent(expectedFileNames, tempDirectory.GetFiles().Select(x => x.Name));
        }


        [Test]
        public void CanOverWriteExistingFiles()
        {
            // Arrange
            var originalOutDirName = "TestMdOutput_CoreNodeModels";
            var originalOutDir = new DirectoryInfo(Path.Combine(toolsTestFilesDirectory, originalOutDirName));

            // Act
            tempDirectory = CreateTempOutputDirectory();
            Assert.That(tempDirectory.Exists);

            CopyFilesRecursively(originalOutDir, tempDirectory);

            var lastWriteTimeBefore = tempDirectory
                .GetFiles()
                .Select(x => x.LastWriteTime)
                .ToList();

            var opts = new FromDirectoryOptions
            {
                InputFolderPath = toolsTestFilesDirectory,
                OutputFolderPath = tempDirectory.FullName,
                Filter = new List<string> { CORENODEMODELS_DLL_NAME },
                ReferencePaths = new List<string>(),
                Overwrite = false
            };

            FromDirectoryCommand.HandleDocumentationFromDirectory(opts);

            tempDirectory.Refresh();
            var lastWriteTimeAfterCommandWithoutOverwrite = tempDirectory
                .GetFiles()
                .Select(x => x.LastWriteTime)
                .ToList();

            opts.Overwrite = true;
            FromDirectoryCommand.HandleDocumentationFromDirectory(opts);

            tempDirectory.Refresh();
            var lastWriteTimeAfterCommandWithOverwrite = tempDirectory
                .GetFiles()
                .Select(x => x.LastWriteTime)
                .ToList();

            // Assert
            Assert.IsTrue(lastWriteTimeAfterCommandWithOverwrite.Count() == lastWriteTimeBefore.Count());

            // Compare last write times on original files
            // and new files without overwrite (-w)
            for (int i = 0; i < lastWriteTimeBefore.Count(); i++)
            {
                // as overwrite has not been specified here
                // it is expected that last write time will
                // be the same for all the files
                Assert.That(DateTime.Compare(lastWriteTimeBefore[i], lastWriteTimeAfterCommandWithoutOverwrite[i]) == 0);
            }

            // Compare last write times on original files
            // and new files with overwrite (-w)
            for (int i = 0; i < lastWriteTimeBefore.Count(); i++)
            {
                // if Compare returns less than 0 first element
                // is earlier then 2nd
                Assert.That(DateTime.Compare(lastWriteTimeBefore[i], lastWriteTimeAfterCommandWithOverwrite[i]) < 0);
            }

            CollectionAssert.AreEquivalent(
                originalOutDir.GetFiles().Select(x => x.Name), 
                tempDirectory.GetFiles().Select(x => x.Name));
        }

        [Test]
        public void CanScanAssemblyFromPath()
        {
            // Arrange
            var assemblyPath = Path.Combine(toolsTestFilesDirectory, CORENODEMODELS_DLL_NAME);
            var coreNodeModelMdFilesDir = new DirectoryInfo(Path.Combine(toolsTestFilesDirectory, "TestMdOutput_CoreNodeModels"));
            var coreNodeModelMdFiles = coreNodeModelMdFilesDir.GetFiles();

            // Act
            var mdFileInfos = AssemblyHandler.ScanAssemblies(new List<string> { assemblyPath });


            // Assert
            Assert.IsTrue(coreNodeModelMdFiles.Count() == mdFileInfos.Count);
            AssertMdFileInfos(mdFileInfos, coreNodeModelMdFiles);
        }

        #region Helpers
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

        private DirectoryInfo CreateTempOutputDirectory()
        {
            string tempDirectoryPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}_markdownGeneratorTestOutput");
            var tempDir = Directory.CreateDirectory(tempDirectoryPath);
            return tempDir;
        }

        private static void CopyFilesRecursively(DirectoryInfo originalDir, DirectoryInfo targetDir)
        {
            foreach (var file in originalDir.GetFiles())
            {
                file.CopyTo(Path.Combine(targetDir.FullName, file.Name));
            }
        }
        #endregion
    }
}
