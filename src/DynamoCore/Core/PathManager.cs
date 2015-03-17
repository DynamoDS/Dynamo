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
    struct PathManagerParams
    {
        /// <summary>
        /// Major version number to be used to form various data file paths.
        /// If both this and MinorFileVersion are 0, then version information 
        /// is retrieved from DynamoCore.dll.
        /// </summary>
        internal int MajorFileVersion { get; set; }

        /// <summary>
        /// Minor version number to be used to form various data file paths.
        /// If both this and MajorFileVersion are 0, then version information 
        /// is retrieved from DynamoCore.dll.
        /// </summary>
        internal int MinorFileVersion { get;set; }

        /// <summary>
        /// The full path of the directory that contains DynamoCore.dll.
        /// </summary>
        internal string CorePath { get; set; }

        /// <summary>
        /// Reference of an IPathResolver object that supplies 
        /// additional path information. This argument is optional.
        /// </summary>
        internal IPathResolver PathResolver { get; set; }
    }

    class PathManager : IPathManager
    {
        #region Class Private Data Members

        public const string PackagesDirectoryName = "packages";
        public const string LogsDirectoryName = "Logs";
        public const string NodesDirectoryName = "nodes";
        public const string DefinitionsDirectoryName = "definitions";
        public const string PreferenceSettingsFileName = "DynamoSettings.xml";

        private readonly int majorFileVersion;
        private readonly int minorFileVersion;
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

        public string UserDataDirectory
        {
            get { return userDataDir; }
        }

        public string CommonDataDirectory
        {
            get { return commonDataDir; }
        }

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
        /// <param name="pathManagerParams">Parameters to configure the new 
        /// instance of PathManager. See PathManagerParams for details of each 
        /// field.</param>
        /// 
        internal PathManager(PathManagerParams pathManagerParams)
        {
            var corePath = pathManagerParams.CorePath;
            var pathResolver = pathManagerParams.PathResolver;

            if (string.IsNullOrEmpty(corePath) || !Directory.Exists(corePath))
            {
                // If the caller does not provide an alternative core path, 
                // use the default folder in which DynamoCore.dll resides.
                var dynamoCorePath = Assembly.GetExecutingAssembly().Location;
                corePath = Path.GetDirectoryName(dynamoCorePath);
            }

            dynamoCoreDir = corePath;
            var assemblyPath = Path.Combine(dynamoCoreDir, "DynamoCore.dll");
            if (!File.Exists(assemblyPath))
            {
                throw new Exception("Dynamo's core path could not be found. " +
                    "If you are running Dynamo from a test, try specifying the " +
                    "Dynamo core location in the DynamoBasePath variable in " +
                    "TestServices.dll.config.");
            }

            // If both major/minor versions are zero, get from assembly.
            majorFileVersion = pathManagerParams.MajorFileVersion;
            minorFileVersion = pathManagerParams.MinorFileVersion;
            if (majorFileVersion == 0 && (minorFileVersion == 0))
            {
                var v = FileVersionInfo.GetVersionInfo(assemblyPath);
                majorFileVersion = v.FileMajorPart;
                minorFileVersion = v.FileMinorPart;
            }

            // Current user specific directories.
            userDataDir = GetUserDataFolder();

            userDefinitions = Path.Combine(userDataDir, DefinitionsDirectoryName);
            logDirectory = Path.Combine(userDataDir, LogsDirectoryName);
            packagesDirectory = Path.Combine(userDataDir, PackagesDirectoryName);
            preferenceFilePath = Path.Combine(userDataDir, PreferenceSettingsFileName);

            // Common directories.
            commonDataDir = GetCommonDataFolder();

            commonDefinitions = Path.Combine(commonDataDir, DefinitionsDirectoryName);
            samplesDirectory = GetSamplesFolder(commonDataDir);

            nodeDirectories = new HashSet<string>
            {
                Path.Combine(dynamoCoreDir, NodesDirectoryName)
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

        /// <summary>
        /// Call this method to force PathManager to create folders that it 
        /// is referring to. This method call throws exception if any creation 
        /// fails.
        /// </summary>
        internal void EnsureDirectoryExistence()
        {
            // User specific data folders.
            CreateFolderIfNotExist(userDataDir);
            CreateFolderIfNotExist(userDefinitions);
            CreateFolderIfNotExist(logDirectory);
            CreateFolderIfNotExist(packagesDirectory);

            // Common data folders for all users.
            CreateFolderIfNotExist(commonDataDir);
            CreateFolderIfNotExist(commonDefinitions);
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
            return Path.Combine(Environment.GetFolderPath(folder), "Dynamo",
                String.Format("{0}.{1}", majorFileVersion, minorFileVersion));
        }

        private static void CreateFolderIfNotExist(string folderPath)
        {
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
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
