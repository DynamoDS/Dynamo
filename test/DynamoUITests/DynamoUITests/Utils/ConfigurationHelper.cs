using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace DynamoTests.Utils
{
    public static class ConfigurationHelper
    {

        public static IConfigurationRoot GetConfiguration()
        {
            return GetConfiguration("appsettings");
        }

        public static T GetTestConfiguration<T>(string testFile, string section, string path = "")
        {
            return GetConfiguration(testFile, path).GetSection(section).Get<T>();
        }

        private static readonly string GetAppDataRoute = Path.GetDirectoryName(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));

        public static readonly string DynamoDataRoute = Path.Combine(GetAppDataRoute ,"Roaming","Dynamo");

        private static readonly string GetAppRoute = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

        public static readonly string GetStaticFilesRoute = Path.Combine(GetAppRoute, "StaticFiles");

        public static readonly string GetDynamoFilesRoute = Path.Combine(GetStaticFilesRoute,"DynamoFiles");

        public static readonly string GetTestPackagesRoute = Path.Combine(GetStaticFilesRoute ,"testPackages");


        private static IConfigurationRoot GetConfiguration(string configurationFile, string path = "")
        {
            return new ConfigurationBuilder()
                .SetBasePath(Path.Combine(GetStaticFilesRoute,path))
                .AddJsonFile($"{configurationFile}.json")
                .Build();
        }

        //https://stackoverflow.com/questions/677221/copy-folders-in-c-sharp-using-system-io
        /// <summary>
        /// copies directories recursively from source to dest
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        public static void CopyDirectory(DirectoryInfo source, DirectoryInfo destination)
        {
            try
            {
                if (!destination.Exists)
                {
                    destination.Create();
                }

                // Copy all files.
                FileInfo[] files = source.GetFiles();
                foreach (FileInfo file in files)
                {
                    file.CopyTo(Path.Combine(destination.FullName,
                        file.Name));
                }

                // Process subdirectories.
                DirectoryInfo[] dirs = source.GetDirectories();
                foreach (DirectoryInfo dir in dirs)
                {
                    // Get destination directory.
                    string destinationDir = Path.Combine(destination.FullName, dir.Name);

                    // Call CopyDirectory() recursively.
                    CopyDirectory(dir, new DirectoryInfo(destinationDir));
                }
            }
            catch {}
        }
    }
}
