using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Dynamo.Interfaces;

namespace Dynamo.Core
{
    class PathManager : IPathManager
    {
        #region Class Data Members and Properties

        private readonly string dynamoCoreDir;
        private readonly string userDataDir;
        private readonly string commonDataDir;

        private readonly string userDefinitions;
        private readonly string commonDefinitions;
        private readonly string logDirectory;

        private readonly HashSet<string> nodeDirectories;
        private readonly List<string> additionalResolutionPaths;

        public string UserDefinitions
        {
            get { return userDefinitions; }
        }

        public string CommonDefinitions
        {
            get { return commonDefinitions; }
        }

        public string LogDirectory
        {
            get { return logDirectory; }
        }

        public IEnumerable<string> NodeDirectories
        {
            get { return nodeDirectories; }
        }

        #endregion

        #region Public Class Operational Methods

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
            logDirectory = CreateFolder(Path.Combine(userDataDir, "Logs"));

            nodeDirectories = new HashSet<string>
            {
                Path.Combine(dynamoCoreDir, "nodes")
            };

            additionalResolutionPaths = new List<string>();
        }

        /// <summary>
        /// Given an initial file path with the file name, resolve the full path
        /// to the target file. The search happens in the following order:
        /// 
        /// 1. If the provided file path is valid and points to an existing file, 
        ///    the file path is return as-is.
        /// 2. The file is searched alongside DynamoCore.dll for a match.
        /// 3. The file is searched within AdditionalResolutionPaths.
        /// 4. The search is left to system path resolution.
        /// 
        /// </summary>
        /// <param name="library">The initial library file path.</param>
        /// <returns>Returns true if the requested file can be located, or false
        /// otherwise.</returns>
        /// 
        internal bool ResolveLibraryPath(ref string library)
        {
            if (File.Exists(library)) // Absolute path, we're done here.
                return true;

            library = LibrarySearchPaths(library).FirstOrDefault(File.Exists);
            return library != default(string);
        }

        #endregion

        #region Private Class Helper Methods

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

        private IEnumerable<string> LibrarySearchPaths(string library)
        {
            // Strip out possible directory from library path.
            string assemblyName = Path.GetFileName(library);
            if (assemblyName == null)
                yield break;

            var assemPath = Path.Combine(dynamoCoreDir, assemblyName);
            yield return assemPath;

            var p = additionalResolutionPaths.Select(
                dir => Path.Combine(dir, assemblyName));

            foreach (var path in p)
                yield return path;

            yield return Path.GetFullPath(library);
        }

        #endregion
    }
}
