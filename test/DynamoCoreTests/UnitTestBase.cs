using System;
using System.IO;
using System.Reflection;

using NUnit.Framework;

namespace Dynamo
{
    public class UnitTestBase
    {
        protected string ExecutingDirectory { get; set; }
        protected string TempFolder { get; private set; }
        public string SampleDirectory { get; private set; }
        public string TestDirectory { get; private set; }

        [SetUp]
        public virtual void Setup()
        {
            SetupDirectories();
        }

        [TearDown]
        public virtual void Cleanup()
        {
            try
            {
                var directory = new DirectoryInfo(TempFolder);
                directory.Delete(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }

        public string GetNewFileNameOnTempPath(string fileExtension = "dyn")
        {
            var guid = Guid.NewGuid().ToString();
            return Path.Combine(
                TempFolder,
                string.IsNullOrWhiteSpace(fileExtension) 
                    ? guid 
                    : Path.ChangeExtension(guid, fileExtension));
        }

        protected void SetupDirectories()
        {
            ExecutingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string tempPath = Path.GetTempPath();

            TempFolder = Path.Combine(tempPath, "dynamoTmp\\" + Guid.NewGuid().ToString("N"));
            
            if (!Directory.Exists(TempFolder))
                Directory.CreateDirectory(TempFolder);

            // Setup Temp PreferenceSetting Location for testing
            PreferenceSettings.DynamoTestPath = Path.Combine(TempFolder, "UserPreferenceTest.xml");

            SampleDirectory = GetSampleDirectory();

            TestDirectory = GetTestDirectory();
        }

        private string GetSampleDirectory()
        {
            var directory = new FileInfo(ExecutingDirectory);
            string assemblyDir = directory.DirectoryName;
            string sampleLocation = Path.Combine(assemblyDir, @"..\..\doc\distrib\Samples\");
            string samplePath = Path.GetFullPath(sampleLocation);
            return samplePath;
        }

        private string GetTestDirectory()
        {
            var directory = new DirectoryInfo(ExecutingDirectory);
            return Path.Combine(directory.Parent.Parent.Parent.FullName, "test");
        }
    }
}
