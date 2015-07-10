using System;
using System.IO;
using System.Reflection;

using NUnit.Framework;

namespace Dynamo
{
    public class UnitTestBase
    {
        private static string executingDirectory;
        protected static string ExecutingDirectory 
        { 
            get 
            {
                if (executingDirectory == null)
                {
                    executingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                }
                return executingDirectory;
            }
        }

        private static string sampleDirectory;
        public static string SampleDirectory 
        { 
            get
            {
                if (sampleDirectory == null)
                {
                    var directory = new FileInfo(ExecutingDirectory);
                    string assemblyDir = directory.DirectoryName;
                    string sampleLocation = Path.Combine(assemblyDir, @"..\..\doc\distrib\Samples\");
                    sampleDirectory = Path.GetFullPath(sampleLocation);
                }
                return sampleDirectory;
            }
        }

        protected string TempFolder { get; private set; }

        private static string testDirectory;
        public static string TestDirectory 
        { 
            get
            {
                if (testDirectory == null)
                {
                    var directory = new DirectoryInfo(ExecutingDirectory);
                    testDirectory = Path.Combine(directory.Parent.Parent.Parent.FullName, "test");
                }
                return testDirectory;
            }
        }

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
            string tempPath = Path.GetTempPath();

            TempFolder = Path.Combine(tempPath, "dynamoTmp\\" + Guid.NewGuid().ToString("N"));
            
            if (!Directory.Exists(TempFolder))
                Directory.CreateDirectory(TempFolder);

            // Setup Temp PreferenceSetting Location for testing
            PreferenceSettings.DynamoTestPath = Path.Combine(TempFolder, "UserPreferenceTest.xml");
        }
    }
}
