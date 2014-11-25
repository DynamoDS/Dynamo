using System;
using System.IO;
using System.Reflection;
using Dynamo.Utilities;
using NUnit.Framework;

namespace Dynamo
{
    public class UnitTestBase
    {

        protected string ExecutingDirectory { get; set; }
        protected string TempFolder { get; private set; }
        protected TestResourceConfig testResourceConfiguration;

        [SetUp]
        public virtual void Init()
        {
            testResourceConfiguration = TestResourceConfig.GetSettings();
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

        public string GetTestDirectory()
        {
            var pathToTestFolder = testResourceConfiguration.DynamoCoreTestPath;
            return pathToTestFolder;
        }

        public string GetSampleDirectory()
        {
            string samplePath = testResourceConfiguration.SamplePath;

            return samplePath;

        }

        protected void SetupDirectories()
        {
            ExecutingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string tempPath = Path.GetTempPath();

            TempFolder = Path.Combine(tempPath, "dynamoTmp\\" + Guid.NewGuid().ToString("N"));

            if (!Directory.Exists(TempFolder))
                Directory.CreateDirectory(TempFolder);
        }
    }
}
