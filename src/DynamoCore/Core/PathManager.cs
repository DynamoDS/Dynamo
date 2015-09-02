using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Dynamo.Interfaces;
using System.Globalization;
using Dynamo.Models;
using Dynamo.Properties;
using Dynamo.UI;
using DynamoUtilities;

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
        public const string ExtensionsDirectoryName = "extensions";
        public const string ViewExtensionsDirectoryName = "viewExtensions";
        public const string DefinitionsDirectoryName = "definitions";
        public const string BackupDirectoryName = "backup";
        public const string PreferenceSettingsFileName = "DynamoSettings.xml";
        public const string GalleryContentsFileName = "GalleryContents.xml";

        private readonly int majorFileVersion;
        private readonly int minorFileVersion;
        private readonly string dynamoCoreDir;
        private readonly string userDataDir;
        private readonly string commonDataDir;

        private readonly string commonDefinitions;
        private readonly string logDirectory;
        private readonly string extensionsDirectory;
        private readonly string viewExtensionsDirectory;
        private readonly string samplesDirectory;
        private readonly string backupDirectory;
        private readonly string preferenceFilePath;
        private readonly string galleryFilePath;

        private readonly List<string> rootDirectories;
        private readonly HashSet<string> nodeDirectories;
        private readonly HashSet<string> additionalResolutionPaths;
        private readonly HashSet<string> preloadedLibraries;

        #endregion

        #region IPathManager Interface Implementation

        public string DynamoCoreDirectory
        {
            get { return dynamoCoreDir; }
        }

        public string UserDataDirectory
        {
            get { return userDataDir; }
        }

        public string CommonDataDirectory
        {
            get { return commonDataDir; }
        }

        public string DefaultUserDefinitions
        {
            get { return TransformPath(rootDirectories[0], DefinitionsDirectoryName); }
        }

        public IEnumerable<string> DefinitionDirectories
        {
            get { return rootDirectories.Select(path => TransformPath(path, DefinitionsDirectoryName)); }
        }

        public string CommonDefinitions
        {
            get { return commonDefinitions; }
        }

        public string LogDirectory
        {
            get { return logDirectory; }
        }

        public string DefaultPackagesDirectory
        {
            get { return TransformPath(rootDirectories[0], PackagesDirectoryName); }
        }

        public IEnumerable<string> PackagesDirectories
        {
            get { return rootDirectories.Select(path => TransformPath(path, PackagesDirectoryName)); }
        }

        public string ExtensionsDirectory
        {
            get { return extensionsDirectory; }
        }

        public string ViewExtensionsDirectory
        {
            get { return viewExtensionsDirectory; }
        }

        public string SamplesDirectory
        {
            get { return samplesDirectory; }
        }

        public string BackupDirectory
        {
            get { return backupDirectory; }
        }

        public string PreferenceFilePath
        {
            get { return preferenceFilePath; }
        }

        public string GalleryFilePath
        {
            get { return galleryFilePath; }
        }

        public IEnumerable<string> NodeDirectories
        {
            get { return nodeDirectories; }
        }

        public IEnumerable<string> PreloadedLibraries
        {
            get { return preloadedLibraries; }
        }

        public int MajorFileVersion
        {
            get { return majorFileVersion; }
        }

        public int MinorFileVersion
        {
            get { return minorFileVersion; }
        }

        public void AddResolutionPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");

            if (!additionalResolutionPaths.Contains(path))
            {
                if (!Directory.Exists(path))
                {
                    throw new Exception(String.Format(Resources.NoBinFolder, path));
                }
                  

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

        public bool ResolveDocumentPath(ref string document)
        {
            if (string.IsNullOrEmpty(document))
            {
                throw new ArgumentNullException("document");
            }

            try
            {
                document = Path.GetFullPath(document);
                if (File.Exists(document)) // "document" is already an absolute path.
                    return true;

                // Restore "document" back to just its file name first...
                document = Path.GetFileName(document);

                // Search alongside the main assembly location...
                var executingAssemblyPathName = Assembly.GetExecutingAssembly().Location;
                var rootModuleDirectory = Path.GetDirectoryName(executingAssemblyPathName);
                document = Path.Combine(rootModuleDirectory, document);

                return File.Exists(document);
            }
            catch(Exception)
            {
                return false;
            }
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

            extensionsDirectory = Path.Combine(dynamoCoreDir, ExtensionsDirectoryName);
            viewExtensionsDirectory = Path.Combine(dynamoCoreDir, ViewExtensionsDirectoryName);

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
            userDataDir = GetUserDataFolder(pathResolver);

            logDirectory = Path.Combine(userDataDir, LogsDirectoryName);
            preferenceFilePath = Path.Combine(userDataDir, PreferenceSettingsFileName);
            backupDirectory = Path.Combine(Directory.GetParent(userDataDir).FullName, BackupDirectoryName);

            // Common directories.
            commonDataDir = GetCommonDataFolder(pathResolver);

            commonDefinitions = Path.Combine(commonDataDir, DefinitionsDirectoryName);
            samplesDirectory = GetSamplesFolder(commonDataDir);
            var galleryDirectory = GetGalleryDirectory(commonDataDir);
            galleryFilePath = Path.Combine(galleryDirectory, GalleryContentsFileName);

            rootDirectories = new List<string> { userDataDir };

            nodeDirectories = new HashSet<string>
            {
                Path.Combine(dynamoCoreDir, NodesDirectoryName)
            };

            preloadedLibraries = new HashSet<string>();
            additionalResolutionPaths = new HashSet<string>();
            LoadPathsFromResolver(pathResolver);
        }

        /// <summary>
        /// Call this method to force PathManager to create folders that it 
        /// is referring to. This method call throws exception if any creation 
        /// fails.
        /// </summary>
        /// <param name="exceptions">The output list of exception, if any of 
        /// the target directories cannot be created during this call.</param>
        internal void EnsureDirectoryExistence(List<Exception> exceptions)
        {
            if (rootDirectories.Count <= 0)
            {
                throw new InvalidOperationException(
                    "At least one custom package directory must be specified");
            }

            if (exceptions == null)
                throw new ArgumentNullException("exceptions");

            exceptions.Clear();

            // User specific data folders.
            exceptions.Add(PathHelper.CreateFolderIfNotExist(userDataDir));
            exceptions.Add(PathHelper.CreateFolderIfNotExist(DefaultUserDefinitions));
            exceptions.Add(PathHelper.CreateFolderIfNotExist(logDirectory));
            exceptions.Add(PathHelper.CreateFolderIfNotExist(DefaultPackagesDirectory));
            exceptions.Add(PathHelper.CreateFolderIfNotExist(backupDirectory));

            // Common data folders for all users.
            exceptions.Add(PathHelper.CreateFolderIfNotExist(commonDataDir));
            exceptions.Add(PathHelper.CreateFolderIfNotExist(commonDefinitions));

            exceptions.RemoveAll(x => x == null); // Remove all null entries.
        }

        /// <summary>
        /// Get the backup file path for a workspace
        /// </summary>
        /// <param name="workspace"></param>
        /// <returns></returns>
        internal string GetBackupFilePath(WorkspaceModel workspace)
        {
            string fileName;
            if (string.IsNullOrEmpty(workspace.FileName))
            {
                if (workspace is HomeWorkspaceModel)
                {
                    fileName = Configurations.BackupFileNamePrefix + ".DYN";
                }
                else
                {
                    fileName = workspace.Name + ".DYF";
                }
            }
            else
            {
                fileName = Path.GetFileName(workspace.FileName);
            }

            return Path.Combine(BackupDirectory, fileName);
        }

        internal void LoadCustomPackageFolders(IEnumerable<string> folders)
        {
            rootDirectories.Clear();
            rootDirectories.AddRange(folders);
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

        internal string GetUserDataFolder(IPathResolver pathResolver = null)
        {
            if (pathResolver != null && !string.IsNullOrEmpty(pathResolver.UserDataRootFolder))
                return GetDynamoDataFolder(pathResolver.UserDataRootFolder);

            var folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return GetDynamoDataFolder(Path.Combine(folder, "Dynamo"));
        }

        private string GetCommonDataFolder(IPathResolver pathResolver)
        {
            if (pathResolver != null && !string.IsNullOrEmpty(pathResolver.CommonDataRootFolder))
                return GetDynamoDataFolder(pathResolver.CommonDataRootFolder);

            var folder = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            return GetDynamoDataFolder(Path.Combine(folder, "Dynamo"));
        }

        private string GetDynamoDataFolder(string folder)
        {
            return Path.Combine(folder,
                String.Format("{0}.{1}", majorFileVersion, minorFileVersion));
        }

        // This method is used to get the locations of packages folder or custom
        // nodes folder given the root path. This is necessary because the packages
        // may be in the root folder or in a packages subfolder of the root folder.
        private string TransformPath(string root, string extension)
        {
            if (root.StartsWith(GetUserDataFolder()))
                return Path.Combine(root, extension);
            try
            {
                var subFolder = Path.Combine(root, extension);
                if (Directory.Exists(subFolder))
                    return subFolder;
            }
            catch (IOException) { }
            catch (ArgumentException) { }
            catch (UnauthorizedAccessException) { }

            return root;
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
                !di.GetFiles("*.dyn", SearchOption.AllDirectories).Any())
            {
                var neturalCommonSamples = Path.Combine(dataRootDirectory, "samples", "en-US");
                if (Directory.Exists(neturalCommonSamples))
                    sampleDirectory = neturalCommonSamples;
            }

            return sampleDirectory;
        }

        private static string GetGalleryDirectory(string commonDataDir)
        {
            var uiCulture = CultureInfo.CurrentUICulture.ToString();
            var galleryDirectory = Path.Combine(commonDataDir, "gallery", uiCulture);

            // If the localized gallery directory does not exist then fall back 
            // to using the en-US gallery folder. Do an additional check to see 
            // if the localized folder is available but is empty.
            // 
            var di = new DirectoryInfo(galleryDirectory);
            if (!Directory.Exists(galleryDirectory) ||
                !di.GetFiles("*.xml",SearchOption.TopDirectoryOnly).Any())
            {
                var neutralCommonGallery = Path.Combine(commonDataDir, "gallery", "en-US");
                if (Directory.Exists(neutralCommonGallery))
                    galleryDirectory = neutralCommonGallery;
            }

            return galleryDirectory;
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
