using System;
using System.Configuration;
using System.IO;
using System.Reflection;

using Dynamo.Utilities;

using DynamoUtilities;

namespace RevitTestServices
{
    public static class AssemblyResolver
    {
        private static bool resolverSetup;

        /// <summary>
        /// Setup the assembly resolver with a default core path.
        /// </summary>
        public static void Setup()
        {
            if (resolverSetup) return;

            var basePath = "";
            if (!OpenAndReadDynamoBasePath(ref basePath))
            {
                basePath = GetDynamoRootDirectory();
            }

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

        internal static string GetDynamoRootDirectory()
        {
            var assemPath = Assembly.GetExecutingAssembly().Location;
            var assemDir = new DirectoryInfo(Path.GetDirectoryName(assemPath));
            return assemDir.Parent.FullName;
        }

        #region private helper methods

        private static bool OpenAndReadDynamoBasePath(ref string basePath)
        {
            var assDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var configPath = Path.Combine(assDir, "RevitTestServices.dll");
            if (!File.Exists(configPath))
            {
                return false;
            }

            try
            {
                var config = ConfigurationManager.OpenExeConfiguration(configPath);
                var value = GetAppSetting(config, "DynamoBasePath");

                if (string.IsNullOrEmpty(value))
                    return false;

                if (!Directory.Exists(value))
                {
                    return false;
                }

                basePath = value;
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
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
 