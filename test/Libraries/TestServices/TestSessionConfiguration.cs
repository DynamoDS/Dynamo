using System.Configuration;
using System.IO;
using System.Reflection;

using DynamoShapeManager;

namespace TestServices
{
    /// <summary>
    /// The TestSessionConfiguration class is responsible for 
    /// reading the TestServices configuration file, if provided,
    /// to provide data for configuring a test session from a location
    /// other than Dynamo's core directory.
    /// </summary>
    public class TestSessionConfiguration
    {
        private const string CONFIG_FILE_NAME = "TestServices.dll.config";

        public string DynamoCorePath { get; private set; }
        public LibraryVersion RequestedLibraryVersion { get; private set; }

        public TestSessionConfiguration()
            : this(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)){}

        /// <summary>
        /// A TestSessionConfiguration specifies test session configuration properties
        /// for a test.
        /// </summary>
        /// <param name="dynamoCoreDirectory">The location of Dynamo's core directory. If the config file
        /// contains a value for DynamoBasePath, then this parameter will be ignored.</param>
        /// <param name="configFileDirectory">The location of the directory containing the TestServices.dll.config file.
        /// If this parameter is null, the value of dynamoCoreDirectory will be used.</param>
        public TestSessionConfiguration(string dynamoCoreDirectory, string configFileDirectory = null)
        {
            string configPath;
            if (configFileDirectory == null || !Directory.Exists(configFileDirectory))
            {
                configPath = Path.Combine(dynamoCoreDirectory, CONFIG_FILE_NAME);
            }
            else
            {
                configPath = Path.Combine(configFileDirectory, CONFIG_FILE_NAME);
            }

            // If a config file cannot be found, fall back to 
            // using the executing assembly's directory for core
            // and version 219 for the shape manager.
            if (!File.Exists(configPath))
            {
                DynamoCorePath = dynamoCoreDirectory;
                RequestedLibraryVersion = LibraryVersion.Version219;
                return;
            }

            // Adjust the config file map because the config file
            // might not always be in the same directory as the dll.
            var map = new ExeConfigurationFileMap { ExeConfigFilename = configPath };
            var config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);

            var dir = GetAppSetting(config, "DynamoBasePath");
            DynamoCorePath = string.IsNullOrEmpty(dir) ? dynamoCoreDirectory : dir;

            var versionStr = GetAppSetting(config, "RequestedLibraryVersion");

            LibraryVersion version;
            RequestedLibraryVersion = LibraryVersion.TryParse(versionStr, out version) ? 
                version : LibraryVersion.Version219;
            
        }

        private static string GetAppSetting(Configuration config, string key)
        {
            var element = config.AppSettings.Settings[key];
            if (element == null) return string.Empty;

            var value = element.Value;
            return !string.IsNullOrEmpty(value) ? value : string.Empty;
        }
    }
}
