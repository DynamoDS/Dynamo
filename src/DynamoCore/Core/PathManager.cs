using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using ProtoCore.AST;

namespace Dynamo.Core
{
    public interface IPathManager
    {
        /// <summary>
        /// The local directory that contains custom nodes created by the user.
        /// </summary>
        string UserDefinitions { get; }

        /// <summary>
        /// The local directory that contains custom nodes created by all users.
        /// </summary>
        string CommonDefinitions { get; }

        IEnumerable<string> NodeDirectories { get; }
    }

    class PathManager : IPathManager
    {
        private readonly HashSet<string> nodeDirectories;
        private readonly string dynamoCoreDir;
        private readonly string userDataDir;
        private readonly string commonDataDir;

        private readonly string userDefinitions;
        private readonly string commonDefinitions;

        internal PathManager()
        {
            // This method is invoked in DynamoCore.dll, dynamoCorePath 
            // represents the directory that contains DynamoCore.dll.
            var dynamoCorePath = Assembly.GetExecutingAssembly().Location;
            dynamoCoreDir = Path.GetDirectoryName(dynamoCorePath);

            if (!File.Exists(Path.Combine(dynamoCoreDir, "DynamoCore.dll")))
            {
                throw new Exception("Dynamo's core path could not be found. " +
                    "If you are running Dynamo from a test, try specifying the " +
                    "Dynamo core location in the DynamoBasePath variable in " +
                    "TestServices.dll.config.");
            }

            userDataDir = CreateFolder(GetUserDataFolder());
            commonDataDir = CreateFolder(GetCommonDataFolder());

            userDefinitions = CreateFolder(Path.Combine(userDataDir, "definitions"));
            commonDefinitions = CreateFolder(Path.Combine(commonDataDir, "definitions"));

            nodeDirectories = new HashSet<string>
            {
                Path.Combine(dynamoCoreDir, "nodes")
            };
        }

        public string UserDefinitions
        {
            get { return userDefinitions; }
        }

        public string CommonDefinitions
        {
            get { return commonDefinitions; }
        }

        public IEnumerable<string> NodeDirectories
        {
            get { return nodeDirectories; }
        }

        private string GetUserDataFolder()
        {
            var folder = Environment.SpecialFolder.ApplicationData;
            return GetDynamoDataFolder(folder);
        }

        private string GetCommonDataFolder()
        {
            var folder = Environment.SpecialFolder.CommonApplicationData;
            return GetDynamoDataFolder(folder);
        }

        private string GetDynamoDataFolder(Environment.SpecialFolder folder)
        {
            var assemblyPath = Path.Combine(dynamoCoreDir, "DynamoCore.dll");
            var v = FileVersionInfo.GetVersionInfo(assemblyPath);
            return Path.Combine(Environment.GetFolderPath(folder), "Dynamo",
                String.Format("{0}.{1}", v.FileMajorPart, v.FileMinorPart));
        }

        private static string CreateFolder(string folderPath)
        {
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            return folderPath;
        }
    }
}
