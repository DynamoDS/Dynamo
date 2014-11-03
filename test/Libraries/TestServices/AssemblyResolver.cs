using System;
using System.Configuration;
using System.IO;
using System.Reflection;

using Dynamo.Utilities;

using DynamoUtilities;

namespace RevitNodesTests
{
    public static class AssemblyResolver
    {
        private static bool resolverSetup;

        /// <summary>
        /// Setup the assembly resolver using the base path 
        /// specified in the config file.
        /// </summary>
        public static void Setup()
        {
            if (resolverSetup) return;

            var basePath = OpenAndReadDynamoBasePath();

            DynamoPathManager.Instance.InitializeCore(basePath);
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyHelper.ResolveAssembly;

            resolverSetup = true;
        }

        /// <summary>
        /// Setup the assembly resolver, specifying a core path.
        /// </summary>
        /// <param name="corePath"></param>
        public static void Setup(string corePath)
        {
            if (resolverSetup) return;

            DynamoPathManager.Instance.InitializeCore(corePath);
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyHelper.ResolveAssembly;

            resolverSetup = true;
        }

        public static string GetDynamoRootDirectory()
        {
            var assemPath = Assembly.GetExecutingAssembly().Location;
            var assemDir = new DirectoryInfo(Path.GetDirectoryName(assemPath));
            return assemDir.Parent.FullName;
        }

        #region private helper methods

        private static string OpenAndReadDynamoBasePath()
        {
            var assDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var configPath = Path.Combine(assDir, "TestServices.dll");
            if (!File.Exists(configPath))
            {
                throw new Exception("The config file for TestServices.dll.config could not be found");
            }

            var config = ConfigurationManager.OpenExeConfiguration(configPath);
            var value = GetAppSetting(config, "DynamoBasePath");

            if (string.IsNullOrEmpty(value))
            {
                throw new Exception("The DynamoBasePath key was not found in the config file, or was invalid.");
            }

            if (!Directory.Exists(value))
            {
                throw new Exception("The DynamoBasePath key specified in the config file does not exist.");
            }

            return value;
        }

        private static string GetAppSetting(Configuration config, string key)
        {
            KeyValueConfigurationElement element = config.AppSettings.Settings[key];
            if (element == null) return string.Empty;

            string value = element.Value;
            return !string.IsNullOrEmpty(value) ? value : string.Empty;
        }

        #endregion

    }
    
}
 