using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;

using DynamoShapeManager;

namespace TestServices
{
    /// <summary>
    /// The RemoteTestSessionConfig class is responsible for 
    /// reading the TestServices configuration file, if provided,
    /// to provide data for configuring a test session from a location
    /// other than Dynamo's core directory.
    /// </summary>
    public class RemoteTestSessionConfig
    {
        public string DynamoCorePath { get; private set; }
        public LibraryVersion RequestedLibraryVersion { get; private set; }

        public RemoteTestSessionConfig()
            : this(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)){}

        public RemoteTestSessionConfig(string fallBackCoreDirectory)
        {
            var execAssemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var configPath = Path.Combine(execAssemblyDir, "TestServices.dll");

            // If a config file cannot be found, fall back to 
            // using the executing assembly's directory for core
            // and version 219 for the shape manager.
            if (!File.Exists(configPath))
            {
                DynamoCorePath = fallBackCoreDirectory;
                RequestedLibraryVersion = LibraryVersion.Version219;
                return;
            }

            var config = ConfigurationManager.OpenExeConfiguration(configPath);

            var dir = GetAppSetting(config, "DynamoBasePath");
            DynamoCorePath = string.IsNullOrEmpty(dir) ? execAssemblyDir : dir;

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
