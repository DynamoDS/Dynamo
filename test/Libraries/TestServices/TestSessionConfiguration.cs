using System.Configuration;
using System.IO;
using System.Reflection;

using DynamoShapeManager;
using System.Collections.Generic;
using System;

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
        [Obsolete("Please use the Version2 Property instead.")]
        public LibraryVersion RequestedLibraryVersion { get { return (LibraryVersion)this.RequestedLibraryVersion2.Major; }}
        /// <summary>
        /// The requested libG library version as a full system.version string.
        /// If the key is not present in the config file a default value will be selected.
        /// If the key is set to 'HostDefault' - the version will be set to null, and
        /// Hosts will be responsible for loading libG and providing a version to load.
        /// </summary>
        public Version RequestedLibraryVersion2 { get; private set; }

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

                var versions = new List<Version>
                {
                    new Version(224,4,0),
                    new Version(224,0,1),
                    new Version(223,0,1),
                    new Version(222,0,0),
                    new Version(221,0,0)
                };
                var shapeManagerPath = string.Empty;
                RequestedLibraryVersion2 = Utilities.GetInstalledAsmVersion2(versions, ref shapeManagerPath, dynamoCoreDirectory);
                return;
            }

            // Adjust the config file map because the config file
            // might not always be in the same directory as the dll.
            var map = new ExeConfigurationFileMap { ExeConfigFilename = configPath };
            var config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);

            var dir = GetAppSetting(config, "DynamoBasePath");
            DynamoCorePath = string.IsNullOrEmpty(dir) ? dynamoCoreDirectory : dir;

            //if an old client supplies an older format test config we should still load it.
            var versionStrOld = GetAppSetting(config, "RequestedLibraryVersion");
            var versionStr = GetAppSetting(config, "RequestedLibraryVersion2");

            Version version;
            LibraryVersion libVersion;
            // first try to load the requested library in the more precise format
            if (Version.TryParse(versionStr, out version))
            {
                RequestedLibraryVersion2 = version;
            }
            // else try to load the older one and convert it to a known precise version
            else if (Enum.TryParse<LibraryVersion>(versionStrOld, out libVersion))
            {
                var realVersion = Preloader.MapLibGVersionEnumToFullVersion(libVersion);
                RequestedLibraryVersion2 = realVersion;

            }
            //if the config is set to HostDefault
            else if(versionStr.ToLower().Contains("hostdefault"))
            {
                RequestedLibraryVersion2 = null;
            }
            // fallback to a mid range version
            else
            {
                RequestedLibraryVersion2 = new Version(223, 0, 1);
            } 
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
