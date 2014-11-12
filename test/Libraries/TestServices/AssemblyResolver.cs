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

        /// <summary>
        /// Read the Dynamo base directory from a config file. If the 
        /// path is empty in the config file, then use the directory
        /// of the executing assembly.
        /// </summary>
        /// <returns></returns>
        private static string OpenAndReadDynamoBasePath()
        {
            var assDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var configPath = Path.Combine(assDir, "TestServices.dll");
            if (!File.Exists(configPath))
            {
                return assDir;
            }

            var config = ConfigurationManager.OpenExeConfiguration(configPath);
            var dir = GetAppSetting(config, "DynamoBasePath");

            if (string.IsNullOrEmpty(dir))
            {
                return assDir;
            }

            if (!Directory.Exists(dir))
            {
                throw new Exception("The DynamoBasePath key specified in the config file does not exist.");
            }

            return dir;
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
 