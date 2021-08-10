using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        [TearDown]
        public void CleanUp()
        {
            if (tempDirectory == null || !tempDirectory.Exists) return;

            tempDirectory.Delete(true);
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

            string[] args = new string[] 
            { 
                "fromdirectory",
                "-i",
                toolsTestFilesDirectory,
                "-o",
                tempDirectory.FullName,
                "-f",
                CORENODEMODELS_DLL_NAME
            };

            NodeDocumentationMarkdownGenerator.Program.Main(args);
            var generatedFileNames = tempDirectory.GetFiles().Select(x => x.Name);

            // Assert
            CollectionAssert.AreEquivalent(expectedOutputDirectory.GetFiles().Select(x => x.Name), generatedFileNames);
        }


        [Test]
        public void ProducesCorrectOutputFromPackage()
        {
            // Arrange
            var packageName = "InspectionTest";
            var testOutputDirName = "doc";
            tempDirectory = new DirectoryInfo(Path.Combine(toolsTestFilesDirectory, packageName, testOutputDirName));
            Assert.IsFalse(tempDirectory.Exists);

            var expectedFileNames = new List<string>
            {
                "Autodesk.Revit.DB.APIObject.Dispose.md",
                "Autodesk.Revit.DB.APIObject.IsReadOnly.md",
                "Autodesk.Revit.DB.ElementReferenceType.REFERENCE_TYPE_CUT_EDGE.md",
                "Autodesk.Revit.DB.ElementReferenceType.REFERENCE_TYPE_FOREIGN.md",
                "Autodesk.Revit.DB.ElementReferenceType.REFERENCE_TYPE_INSTANCE.md",
                "Autodesk.Revit.DB.ElementReferenceType.REFERENCE_TYPE_LINEAR.md",
                "Autodesk.Revit.DB.ElementReferenceType.REFERENCE_TYPE_MESH.md",
                "Autodesk.Revit.DB.ElementReferenceType.REFERENCE_TYPE_NONE.md",
                "Autodesk.Revit.DB.ElementReferenceType.REFERENCE_TYPE_SUBELEMENT.md",
                "Autodesk.Revit.DB.ElementReferenceType.REFERENCE_TYPE_SURFACE.md",
                "Autodesk.Revit.DB.Reference.Contains.md",
                "Autodesk.Revit.DB.Reference.ConvertToStableRepresentation.md",
                "Autodesk.Revit.DB.Reference.CreateLinkReference.md",
                "Autodesk.Revit.DB.Reference.CreateReferenceInLink.md",
                "Autodesk.Revit.DB.Reference.ElementId.md",
                "Autodesk.Revit.DB.Reference.ElementReferenceType.md",
                "Autodesk.Revit.DB.Reference.EqualTo.md",
                "Autodesk.Revit.DB.Reference.GlobalPoint.md",
                "Autodesk.Revit.DB.Reference.LinkedElementId.md",
                "Autodesk.Revit.DB.Reference.ParseFromStableRepresentation.md",
                "Autodesk.Revit.DB.Reference.Reference.md",
                "Autodesk.Revit.DB.Reference.UVPoint.md",
                "InspectionTest.HelloDynamo.HelloDynamo.md"
            };

            // Act
            string[] args = new string[]
            {
                "frompackage",
                "-i",
                Path.Combine(toolsTestFilesDirectory,packageName),
                "-r",
                toolsTestFilesDirectory,
            };

            NodeDocumentationMarkdownGenerator.Program.Main(args);
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

            string[] argsWithOverwrite = new string[]
            {
                "fromdirectory",
                "-i",
                toolsTestFilesDirectory,
                "-o",
                tempDirectory.FullName,
                "-f",
                CORENODEMODELS_DLL_NAME,
                "-w"
            };

            string[] argsWithoutOverwrite = new string[]
            {
                "fromdirectory",
                "-i",
                toolsTestFilesDirectory,
                "-o",
                tempDirectory.FullName,
                "-f",
                CORENODEMODELS_DLL_NAME
            };

            NodeDocumentationMarkdownGenerator.Program.Main(argsWithoutOverwrite);
            tempDirectory.Refresh();
            var lastWriteTimeAfterMainWithoutOverwrite = tempDirectory
                .GetFiles()
                .Select(x => x.LastWriteTime)
                .ToList();

            NodeDocumentationMarkdownGenerator.Program.Main(argsWithOverwrite);
            tempDirectory.Refresh();
            var lastWriteTimeAfterMainWithOverwrite = tempDirectory
                .GetFiles()
                .Select(x => x.LastWriteTime)
                .ToList();

            // Assert
            Assert.IsTrue(lastWriteTimeAfterMainWithOverwrite.Count() == lastWriteTimeBefore.Count());

            // Compare last write times on original files
            // and new files without overwrite (-w)
            for (int i = 0; i < lastWriteTimeBefore.Count(); i++)
            {
                // as overwrite has not been specified here
                // it is expected that last write time will
                // be the same for all the files
                Assert.That(DateTime.Compare(lastWriteTimeBefore[i], lastWriteTimeAfterMainWithoutOverwrite[i]) == 0);
            }

            // Compare last write times on original files
            // and new files with overwrite (-w)
            for (int i = 0; i < lastWriteTimeBefore.Count(); i++)
            {
                // if Compare returns less than 0 first element
                // is earlier then 2nd
                Assert.That(DateTime.Compare(lastWriteTimeBefore[i], lastWriteTimeAfterMainWithOverwrite[i]) < 0);
            }

            CollectionAssert.AreEquivalent(
                originalOutDir.GetFiles().Select(x => x.Name), 
                tempDirectory.GetFiles().Select(x => x.Name));
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
    }
}
