using System.Configuration;
using System.IO;
using System.Reflection;

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
        public string RequestedLibraryVersion { get; private set; }

        public RemoteTestSessionConfig()
        {
            var assDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var configPath = Path.Combine(assDir, "TestServices.dll");
            if (!File.Exists(configPath))
            {
                DynamoCorePath = assDir;
                RequestedLibraryVersion = "Version219";
                return;
            }

            var config = ConfigurationManager.OpenExeConfiguration(configPath);

            var dir = GetAppSetting(config, "DynamoBasePath");
            DynamoCorePath = string.IsNullOrEmpty(dir) ? assDir : dir;

            var asm = GetAppSetting(config, "RequestedLibraryVersion");
            RequestedLibraryVersion = string.IsNullOrEmpty(dir) ? "Version219" : asm;
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
