using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodeDocumentationMarkdownGenerator;
using NUnit.Framework;

namespace NodeDocumentationMarkdownGeneratorTests
{
    public class MarkdownGeneratorTests
    {
        private static string coreDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        private static string toolsTestFilesDirectory = Path.GetFullPath(Path.Combine(coreDirectory, @"..\..\..\docGeneratorTestFiles"));

        [Test]
        public void ProducesCorrectOutputFromDirectory()
        {
            // Arrange
            var expectedOutputDirectory = Path.Combine(toolsTestFilesDirectory, "TestMdOutput_CoreNodeModels");
            Assert.That(Directory.Exists(expectedOutputDirectory));

            var coreNodeModelsDll = Path.Combine(toolsTestFilesDirectory, "CoreNodeModels.dll");
            Assert.That(File.Exists(coreNodeModelsDll));

            // Act
            //CommandHandler.HandleFromDirectory(NodeDocumentationMarkdownGeneratorTests)

            // Assert
        }
    }
}
