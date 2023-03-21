using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using Dynamo.Configuration;
using Dynamo.Utilities;
using NUnit.Framework;

namespace Dynamo
{
    public class UnitTestBase
    {
        private AssemblyHelper assemblyHelper;
        private static string alternativeSampleDirectory = string.Empty;

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
                if (!string.IsNullOrEmpty(alternativeSampleDirectory))
                    return alternativeSampleDirectory;

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
#if NETFRAMEWORK
            
            DSOffice.ExcelInterop.ShowOnStartup = false;
#endif

            if (assemblyHelper == null)
            {
                var assemblyPath = Assembly.GetExecutingAssembly().Location;
                var moduleRootFolder = Path.GetDirectoryName(assemblyPath);
                var resolutionPaths = new[]
                {
                    // These tests need "CoreNodeModels.dll" under "nodes" folder.
                    Path.Combine(moduleRootFolder, "nodes")
                };
                assemblyHelper = new AssemblyHelper(moduleRootFolder, resolutionPaths);
                AppDomain.CurrentDomain.AssemblyResolve += assemblyHelper.ResolveAssembly;
            }
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

            if (assemblyHelper != null)
            {
                AppDomain.CurrentDomain.AssemblyResolve -= assemblyHelper.ResolveAssembly;
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
            var path = this.GetType().Assembly.Location;
            var config = ConfigurationManager.OpenExeConfiguration(path);
            if (config!=null)
            {
                var key = config.AppSettings.Settings["alternativeSampleDirectory"];
                alternativeSampleDirectory = key == null ? string.Empty : key.Value;
                
            }
            
            string tempPath = Path.GetTempPath();

            TempFolder = Path.Combine(tempPath, "dynamoTmp\\" + Guid.NewGuid().ToString("N"));

            if (!Directory.Exists(TempFolder))
                Directory.CreateDirectory(TempFolder);

            // Setup Temp PreferenceSetting Location for testing
            PreferenceSettings.DynamoTestPath = Path.Combine(TempFolder, "UserPreferenceTest.xml");
        }
    }
}
