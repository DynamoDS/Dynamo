using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Dynamo.Interfaces;
using System.Globalization;

namespace Dynamo.Core
{
    class PathManager : IPathManager
    {
        #region Class Private Data Members

        private readonly string dynamoCoreDir;
        private readonly string userDataDir;
        private readonly string commonDataDir;

        private readonly string userDefinitions;
        private readonly string commonDefinitions;
        private readonly string logDirectory;
        private readonly string packagesDirectory;
        private readonly string samplesDirectory;
        private readonly string preferenceFilePath;

        private readonly HashSet<string> nodeDirectories;
        private readonly HashSet<string> additionalResolutionPaths;
        private readonly HashSet<string> preloadedLibraries;

        #endregion

        #region IPathManager Interface Implementation

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

        public string PackagesDirectory
        {
            get { return packagesDirectory; }
        }

        public string SamplesDirectory
        {
            get { return samplesDirectory; }
        }

        public string PreferenceFilePath
        {
            get { return preferenceFilePath; }
        }

        public IEnumerable<string> NodeDirectories
        {
            get { return nodeDirectories; }
        }

        public IEnumerable<string> PreloadedLibraries
        {
            get { return preloadedLibraries; }
        }

        public void AddResolutionPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");

            if (!additionalResolutionPaths.Contains(path))
            {
                if (!Directory.Exists(path))
                    throw new DirectoryNotFoundException(path);

                additionalResolutionPaths.Add(path);
            }
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
        public bool ResolveLibraryPath(ref string library)
        {
            if (File.Exists(library)) // Absolute path, we're done here.
                return true;

            library = LibrarySearchPaths(library).FirstOrDefault(File.Exists);
            return library != default(string);
        }

        #endregion

        #region Public Class Operational Methods

        /// <summary>
        /// Constructs an instance of PathManager object.
        /// </summary>
        /// <param name="pathResolver">Reference of an IPathResolver object that
        /// supplies additional path information. This argument is optional.</param>
        /// 
        internal PathManager(string corePath, IPathResolver pathResolver)
        {
            if (string.IsNullOrEmpty(corePath) || !Directory.Exists(corePath))
            {
                // If the caller does not provide an alternative core path, 
                // use the default folder in which DynamoCore.dll resides.
                var dynamoCorePath = Assembly.GetExecutingAssembly().Location;
                corePath = Path.GetDirectoryName(dynamoCorePath);
            }

            dynamoCoreDir = corePath;
            if (!File.Exists(Path.Combine(dynamoCoreDir, "DynamoCore.dll")))
            {
                throw new Exception("Dynamo's core path could not be found. " +
                    "If you are running Dynamo from a test, try specifying the " +
                    "Dynamo core location in the DynamoBasePath variable in " +
                    "TestServices.dll.config.");
            }

            // Current user specific directories.
            userDataDir = CreateFolder(GetUserDataFolder());

            userDefinitions = CreateFolder(Path.Combine(userDataDir, "definitions"));
            logDirectory = CreateFolder(Path.Combine(userDataDir, "Logs"));
            packagesDirectory = CreateFolder(Path.Combine(userDataDir, "packages"));
            preferenceFilePath = Path.Combine(userDataDir, "DynamoSettings.xml");

            // Common directories.
            commonDataDir = CreateFolder(GetCommonDataFolder());

            commonDefinitions = CreateFolder(Path.Combine(commonDataDir, "definitions"));
            samplesDirectory = GetSamplesFolder(commonDataDir);

            nodeDirectories = new HashSet<string>
            {
                Path.Combine(dynamoCoreDir, "nodes")
            };

            preloadedLibraries = new HashSet<string>
            {
                "VMDataBridge.dll",
                "ProtoGeometry.dll",
                "DSCoreNodes.dll",
                "DSOffice.dll",
                "DSIronPython.dll",
                "FunctionObject.ds",
                "Optimize.ds",
                "DynamoUnits.dll",
                "Tessellation.dll",
                "Analysis.dll"
            };

            additionalResolutionPaths = new HashSet<string>();
            LoadPathsFromResolver(pathResolver);
        }

        #endregion

        #region Private Class Helper Methods

        private void LoadPathsFromResolver(IPathResolver pathResolver)
        {
            if (pathResolver == null) // No optional path resolver is specified...
                return;

            foreach (var directory in pathResolver.AdditionalNodeDirectories)
            {
                if (!Directory.Exists(directory))
                    throw new DirectoryNotFoundException(directory);

                if (!nodeDirectories.Contains(directory))
                    nodeDirectories.Add(directory);
            }

            foreach (var directory in pathResolver.AdditionalResolutionPaths)
            {
                if (!Directory.Exists(directory))
                    throw new DirectoryNotFoundException(directory);

                if (!additionalResolutionPaths.Contains(directory))
                    additionalResolutionPaths.Add(directory);
            }

            foreach (var path in pathResolver.PreloadedLibraryPaths)
            {
                if (!preloadedLibraries.Contains(path))
                    preloadedLibraries.Add(path);
            }
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

        private static string GetSamplesFolder(string dataRootDirectory)
        {
            var uiCulture = CultureInfo.CurrentUICulture.ToString();
            var sampleDirectory = Path.Combine(dataRootDirectory, "samples", uiCulture);

            // If the localized samples directory does not exist then fall back 
            // to using the en-US samples folder. Do an additional check to see 
            // if the localized folder is available but is empty.
            // 
            var di = new DirectoryInfo(sampleDirectory);
            if (!Directory.Exists(sampleDirectory) ||
                !di.GetDirectories().Any() ||
                !di.GetFiles().Any())
            {
                var neturalCommonSamples = Path.Combine(dataRootDirectory, "samples", "en-US");
                if (Directory.Exists(neturalCommonSamples))
                    sampleDirectory = neturalCommonSamples;
            }

            return sampleDirectory;
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
