using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;

namespace IntegrationTests
{
    [TestFixture]
    class IntegrityTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void AllResxFilesShouldMatchEnUsCounterparts()
        {
            var executingAsmPath = Assembly.GetExecutingAssembly().Location;
            var executingAsmDir = Path.GetDirectoryName(executingAsmPath);
            var dynamoDir = Directory.GetParent(executingAsmDir).Parent.Parent;
            var dynamoSrcDir = Path.Combine(dynamoDir.FullName, "src");

            // Expected to have reached ".../Dynamo/src" folder, bail if not.
            Assert.IsTrue(Directory.Exists(dynamoSrcDir));

            // Get all "Resources.resx" files under the source directory.
            var resxFiles = Directory.GetFiles(dynamoSrcDir,
                "Resources.resx", SearchOption.AllDirectories);

            Assert.IsTrue(resxFiles != null);
            Assert.IsTrue(resxFiles.Length > 0);

            var issues = new List<string>();

            foreach (var resxFile in resxFiles)
            {
                var directory = Path.GetDirectoryName(resxFile);
                var resxEnUsFile = Path.Combine(directory, "Resources.en-US.resx");

                if (!File.Exists(resxEnUsFile))
                {
                    // "Resources.en-US.resx" is missing...
                    issues.Add(string.Format("Missing: {0}", resxEnUsFile));
                }
                else if (!AreFilesIdentical(resxFile, resxEnUsFile))
                {
                    // "Resources.en-US.resx" is different from "Resources.resx"
                    issues.Add(string.Format("Different: {0}", resxEnUsFile));
                }
            }

            // Ensure there is no issue, otherwise outputs the issues.
            Assert.IsTrue(issues.Count == 0, string.Join("\n", issues));
        }

        // Implementation derived from Microsoft knowledge base: 
        // 
        //      https://support.microsoft.com/en-us/kb/320348
        // 
        private bool AreFilesIdentical(string filePathOne, string filePathTwo)
        {
            // Determine if the same file was referenced two times.
            if (filePathOne == filePathTwo)
            {
                return true; // Files are the same.
            }

            // Open the two files.
            var fileOne = new FileStream(filePathOne, FileMode.Open);
            var fileTwo = new FileStream(filePathTwo, FileMode.Open);

            // Check the file sizes. If they are not the same, 
            // the files are not the same.
            if (fileOne.Length != fileTwo.Length)
            {
                fileOne.Close();
                fileTwo.Close();
                return false; // Files are different.
            }

            int file1Byte;
            int file2Byte;

            // Read and compare a byte from each file until either a
            // non-matching set of bytes is found or until the end of
            // file1 is reached.
            do
            {
                // Read one byte from each file.
                file1Byte = fileOne.ReadByte();
                file2Byte = fileTwo.ReadByte();
            }
            while ((file1Byte == file2Byte) && (file1Byte != -1));

            // Close the files.
            fileOne.Close();
            fileTwo.Close();

            // Return the success of the comparison. "file1Byte" is 
            // equal to "file2Byte" at this point only if the files are 
            // the same.
            return ((file1Byte - file2Byte) == 0);
        }
    }
}
