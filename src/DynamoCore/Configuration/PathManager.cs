using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using Dynamo.Configuration;
using Dynamo.Graph.Workspaces;
using Dynamo.Interfaces;
using Dynamo.Properties;
using DynamoUtilities;
using Dynamo.Models;

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
        /// The full path of the host application such as DynamoRevit or DynamoStudio
        /// </summary>
        internal string HostPath { get; set; }

        /// <summary>
        /// Reference of an IPathResolver object that supplies 
        /// additional path information. This argument is optional.
        /// </summary>
        internal IPathResolver PathResolver { get; set; }
    }

    internal class PathManager : IPathManager
    {
        private static Lazy<PathManager> lazy;
        private static readonly object lockObject = new object();

        /// <summary>
        /// Initialize the PathManager singleton passing as a parameter a PathManagerParams object (which contains the Major and Minor version values).
        /// </summary>
        /// <param name="parameters"></param>
        public static void Initialize(PathManagerParams parameters)
        {
            lock (lockObject)
            {
                //If is already initialized then do nothing
                if (lazy == null)
                {
                    lazy = new Lazy<PathManager>(() => new PathManager(parameters));
                }                 
            }
        }

        /// <summary>
        /// Instance is the property used as an access point to the PathManager singleton (if is not created will be created with default parameters).
        /// </summary>
        public static PathManager Instance
        {
            get
            {
                if (lazy == null)
                {
                    lock (lockObject)
                    {
                        if (lazy == null)
                        {
                            // Fallback to default if not initialized
                            lazy = new Lazy<PathManager>(() => new PathManager(new PathManagerParams()));
                        }
                    }
                }

                return lazy.Value;
            }
        }

        #region Class Private Data Members

        public const string PackagesDirectoryName = "packages";
        public const string LogsDirectoryName = "Logs";
        public const string NodesDirectoryName = "nodes";
        public const string ExtensionsDirectoryName = "extensions";
        public const string ViewExtensionsDirectoryName = "viewExtensions";
        public const string DefinitionsDirectoryName = "definitions";
        public const string BackupDirectoryName = "backup";
        public const string PreferenceSettingsFileName = "DynamoSettings.xml";
        public const string PythonTemplateFileName = "PythonTemplate.py";

        private readonly int majorFileVersion;
        private readonly int minorFileVersion;
        private Updates.BinaryVersion productVersion;
        private readonly string dynamoCoreDir;
        private string hostApplicationDirectory;
        private string userDataDir;
        private string commonDataDir;

        private string logDirectory;
        private string samplesDirectory;
        private string templatesDirectory;
        private string defaultTemplatesDirectory;
        private string backupDirectory;
        private string defaultBackupDirectory;
        private string preferenceFilePath;
        private string pythonTemplateFilePath;

        private List<string> rootDirectories;
        private HashSet<string> nodeDirectories;
        private HashSet<string> additionalResolutionPaths;
        private HashSet<string> preloadedLibraries;
        private readonly HashSet<string> extensionsDirectories;
        private readonly HashSet<string> viewExtensionsDirectories;
        private IPathResolver pathResolver;

        #endregion

        internal IPreferences Preferences { get; set; }

        /// <summary>
        /// PathResolver is used to resolve paths for custom nodes, packages, and preloaded libraries.
        /// </summary>
        public IPathResolver PathResolver
        {
            get { return pathResolver; }
        }

        private IEnumerable<string> RootDirectories
        {
            get 
            { 
                return Preferences != null ? 
                    Preferences.CustomPackageFolders.Select(path => path == DynamoModel.BuiltInPackagesToken ? BuiltinPackagesDirectory : path) 
                    : rootDirectories;
            }
        }

        private const string builtinPackagesDirName = @"Built-In Packages";
        private static string builtinPackagesDirectory = null;

        //Todo in Dynamo 3.0, Add this to the IPathManager interface
        /// <summary>
        /// The Built-In Packages directory is located in the same directory as the DynamoCore.dll
        /// Property should only be set during testing. During testing, keep in mind that previous tests
        /// may have altered this static property, and it may need to be restored.
        /// </summary>
        internal static string BuiltinPackagesDirectory
        {
            get
            {
                if (builtinPackagesDirectory == null)
                {
                    builtinPackagesDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(PathManager)).Location), builtinPackagesDirName, @"Packages");
                }
                return builtinPackagesDirectory;
            }
            set
            {
                if (builtinPackagesDirectory != value)
                {
                    builtinPackagesDirectory = value;
                }
            }
        }

        #region IPathManager Interface Implementation

        public string DynamoCoreDirectory
        {
            get { return dynamoCoreDir; }
        }

        public string HostApplicationDirectory
        {
            get { return hostApplicationDirectory; }
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
            get 
            {
                if (Preferences is PreferenceSettings preferences)
                {
                    return TransformPath(preferences.SelectedPackagePathForInstall, DefinitionsDirectoryName);
                }
                return TransformPath(RootDirectories.First(), DefinitionsDirectoryName); 
            }
        }

        public IEnumerable<string> DefinitionDirectories
        {
            get { return RootDirectories.Select(path => TransformPath(path, DefinitionsDirectoryName)); }
        }

        [Obsolete("This property will be removed in a future version of Dynamo.", false)]
        public string CommonDefinitions
        {
            get { return string.Empty; }
        }


        public string LogDirectory
        {
            get { return logDirectory; }
        }

        /// <summary>
        /// The enum will contain the possible values for Preference Item 
        /// </summary>
        public enum PreferenceItem
        {
            Backup,
            Templates,
            Samples
        }

        /// <summary>
        /// Default directory where new packages are downloaded to.
        /// This directory path is user configurable and if set to something other than the default,
        /// the currently selected path can be obtained from preference settings.
        /// </summary>
        public string DefaultPackagesDirectory
        {
            get
            {
                if (Preferences is PreferenceSettings preferences)
                {
                    return TransformPath(preferences.SelectedPackagePathForInstall, PackagesDirectoryName);
                }
                return TransformPath(RootDirectories.First(), PackagesDirectoryName);
            }
        }

        public IEnumerable<string> PackagesDirectories
        {
            get { return RootDirectories.Select(path => TransformPath(path, PackagesDirectoryName)); }
        }

        public IEnumerable<string> ExtensionsDirectories
        {
            get { return extensionsDirectories; }
        }

        public IEnumerable<string> ViewExtensionsDirectories
        {
            get { return viewExtensionsDirectories; }
        }

        public string SamplesDirectory
        {
            get
            {
                if (samplesDirectory == null)
                {
                    var preferences = Preferences as PreferenceSettings;
                    var locale = preferences?.Locale ?? CultureInfo.CurrentUICulture.Name;

                    if (string.Equals(locale, "Default", StringComparison.OrdinalIgnoreCase))
                    {
                        // When locale is "Default", resolve from process cultures in priority order:
                        // 1. DefaultThreadCurrentCulture (explicitly set by host/application)
                        // 2. CurrentUICulture (current thread's UI culture)
                        // 3. FallbackUiCulture (Dynamo's default: "en-US")
                        var effectiveCulture = CultureInfo.DefaultThreadCurrentCulture
                                            ?? CultureInfo.CurrentUICulture
                                            ?? new CultureInfo(Configurations.FallbackUiCulture);

                        locale = effectiveCulture.Name;
                    }
                    samplesDirectory = GetSamplesFolder(commonDataDir, locale);
                }
                return samplesDirectory;
            }
        }

        /// <summary>
        /// Dynamo Templates folder
        /// </summary>
        public string TemplatesDirectory
        {
            get { return templatesDirectory; }
        }
        /// <summary>
        /// Default templates directory, it is used when the user resets the custom template path
        /// </summary>
        public string DefaultTemplatesDirectory
        {
            get { return defaultTemplatesDirectory; }
        }

        public string BackupDirectory
        {
            get { return backupDirectory; }
        }

        public string DefaultBackupDirectory
        {
            get { return defaultBackupDirectory; }
        }

        public string PreferenceFilePath
        {
            get { return preferenceFilePath; }
        }

        public string PythonTemplateFilePath
        {
            get { return pythonTemplateFilePath; }
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

        /// <summary>
        /// This function indicates if there is an already assigned Path Resolver , otherwise it will take from the ctor config
        /// </summary>
        public bool HasPathResolver
        {
            get { return pathResolver != null; }
        }

        public void AddResolutionPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");

            if (!additionalResolutionPaths.Contains(path))
            {
                if (!Directory.Exists(path))
                {
                    throw new Exception(String.Format(Resources.DirectoryNotFound, path));
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
            if (PathHelper.IsValidPath(library)) // Absolute path, we're done here.
                return true;

            library = LibrarySearchPaths(library).FirstOrDefault(PathHelper.IsValidPath);
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
                if (PathHelper.IsValidPath(document)) // "document" is already an absolute path.
                    return true;

                // Restore "document" back to just its file name first...
                document = Path.GetFileName(document);

                // Search alongside the main assembly location...
                var executingAssemblyPathName = Assembly.GetExecutingAssembly().Location;
                var rootModuleDirectory = Path.GetDirectoryName(executingAssemblyPathName);
                document = Path.Combine(rootModuleDirectory, document);

                return PathHelper.IsValidPath(document);
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Public Class Operational Methods

        /// <summary>
        /// Assigns a hostPath and IPathResolver on demand with the same behavior as the Ctor.
        /// </summary>
        /// <param name="hostPath"></param>
        /// /// <param name="resolver"></param>
        internal void AssignHostPathAndIPathResolver(string hostPath, IPathResolver resolver)
        {
            pathResolver = resolver;
            BuildHostDirectories(hostPath);
            BuildUserSpecificDirectories();
            BuildCommonDirectories();
            LoadPathsFromResolver();
        }

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
            pathResolver = pathManagerParams.PathResolver;

            if (string.IsNullOrEmpty(corePath) || !Directory.Exists(corePath))
            {
                // If the caller does not provide an alternative core path, 
                // use the default folder in which DynamoCore.dll resides.
                var dynamoCorePath = Assembly.GetExecutingAssembly().Location;
                corePath = Path.GetDirectoryName(dynamoCorePath);
            }

            dynamoCoreDir = corePath;

            var assemblyPath = Path.Combine(dynamoCoreDir, "DynamoCore.dll");
            if (!PathHelper.IsValidPath(assemblyPath))
            {
                throw new Exception("Dynamo's core path could not be found. " +
                    "If you are running Dynamo from a test, try specifying the " +
                    "Dynamo core location in the DynamoBasePath variable in " +
                    "TestServices.dll.config.");
            }

            extensionsDirectories = new HashSet<string>();
            viewExtensionsDirectories = new HashSet<string>();

            extensionsDirectories.Add(Path.Combine(dynamoCoreDir, ExtensionsDirectoryName));
            viewExtensionsDirectories.Add(Path.Combine(dynamoCoreDir, ViewExtensionsDirectoryName));

            BuildHostDirectories(pathManagerParams.HostPath);

            // If both major/minor versions are zero, get from assembly.
            majorFileVersion = pathManagerParams.MajorFileVersion;
            minorFileVersion = pathManagerParams.MinorFileVersion;
            if (majorFileVersion == 0 && (minorFileVersion == 0))
            {
                var v = FileVersionInfo.GetVersionInfo(assemblyPath);
                majorFileVersion = v.FileMajorPart;
                minorFileVersion = v.FileMinorPart;
            }

            BuildUserSpecificDirectories();
            BuildCommonDirectories();
            LoadPathsFromResolver();
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
            if (!RootDirectories.Any())
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
            exceptions.Add(PathHelper.CreateFolderIfNotExist(DefaultTemplatesDirectory));

            // Common data folders for all users.
            exceptions.Add(PathHelper.CreateFolderIfNotExist(commonDataDir));

            exceptions.RemoveAll(x => x == null); // Remove all null entries.
        }

        internal bool UpdatePreferenceItemPath(PreferenceItem item, string newLocation)
        {
            bool isValidFolder = PathHelper.CreateFolderIfNotExist(newLocation) == null;
            if (!isValidFolder)
                return false;

            switch (item)
            {
                case PreferenceItem.Backup:
                    backupDirectory = newLocation;
                    break;
                case PreferenceItem.Templates:
                    templatesDirectory = newLocation;
                    break;
            }
            return true;
        }

        /// <summary>
        /// Returns the backup file path for a workspace
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

        /// <summary>
        /// Backup the XML file.
        /// </summary>
        /// <param name="xmlDoc">The XML document.</param>
        /// <param name="filePath">The file path.</param>
        /// <returns></returns>
        internal bool BackupXMLFile(XmlDocument xmlDoc, string filePath)
        {
            try
            {
                var fileName = Path.GetFileNameWithoutExtension(filePath) + "_xml";
                var extension = Path.GetExtension(filePath);
                var savePath = Path.Combine(this.BackupDirectory, fileName + extension);
                xmlDoc.Save(savePath);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion

        #region Private Class Helper Methods

        /// <summary>
        /// Build the Extensions and ViewExtensions directories based on the Host.
        /// </summary>
        /// <param name="hostPath"></param>
        private void BuildHostDirectories(string hostPath)
        {
            hostApplicationDirectory = hostPath;

            if (!string.IsNullOrEmpty(hostApplicationDirectory))
            {
                extensionsDirectories.Add(Path.Combine(hostApplicationDirectory, ExtensionsDirectoryName));
                viewExtensionsDirectories.Add(Path.Combine(hostApplicationDirectory, ViewExtensionsDirectoryName));
            }
        }

        /// <summary>
        /// Build directories based on the User.
        /// </summary>
        private void BuildUserSpecificDirectories()
        {
            // Current user specific directories.
            userDataDir = GetUserDataFolder();

            // When running as a headless process, put the logs directory in a consistent
            // location that doesn't change every time the version number changes.
            var userDataDirNoVersion = Directory.GetParent(userDataDir).FullName;
            logDirectory = Path.Combine(Dynamo.Models.DynamoModel.IsHeadless ? userDataDirNoVersion : userDataDir,
                                        LogsDirectoryName);

            preferenceFilePath = Path.Combine(userDataDir, PreferenceSettingsFileName);
            pythonTemplateFilePath = Path.Combine(userDataDir, PythonTemplateFileName);
            backupDirectory = Path.Combine(userDataDirNoVersion, BackupDirectoryName);
            defaultBackupDirectory = backupDirectory;
        }

        /// <summary>
        /// Build common Directories.
        /// </summary>
        private void BuildCommonDirectories()
        {
            // Common directories.
            commonDataDir = GetCommonDataFolder();

            defaultTemplatesDirectory = GetTemplateFolder(commonDataDir);
            rootDirectories = new List<string> { userDataDir };

            nodeDirectories = new HashSet<string>
            {
                Path.Combine(dynamoCoreDir, NodesDirectoryName)
            };

            preloadedLibraries = new HashSet<string>();
            additionalResolutionPaths = new HashSet<string>();
        }

        /// <summary>
        /// Load the Paths based on the Resolver
        /// </summary>
        /// <exception cref="DirectoryNotFoundException"></exception>
        private void LoadPathsFromResolver()
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

        internal string GetUserDataFolder()
        {
            if (pathResolver != null && !string.IsNullOrEmpty(pathResolver.UserDataRootFolder))
                return GetDynamoDataFolder(pathResolver.UserDataRootFolder);

            if (!string.IsNullOrEmpty(userDataDir))
                return userDataDir; //Return the cached userDataDir if we have one.

            var folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return GetDynamoDataFolder(Path.Combine(folder, Configurations.DynamoAsString, "Dynamo Core"));
        }

        /// <summary>
        /// Returns the current Dynamo product version.
        /// </summary>
        /// <returns></returns>
        public Updates.BinaryVersion GetProductVersion()
        {
            if (null != productVersion) return productVersion;
            var executingAssemblyName = Assembly.GetExecutingAssembly().GetName();
            productVersion = Updates.BinaryVersion.FromString(executingAssemblyName.Version.ToString());
            return productVersion;
        }

        private string GetCommonDataFolder()
        {
            //This piece of code is only executed if we are running a host like Revit or Civil3D due that pathResolver is not null
            if (pathResolver != null && !string.IsNullOrEmpty(pathResolver.CommonDataRootFolder))
                return GetDynamoDataFolder(pathResolver.CommonDataRootFolder);

            //This piece of code is only executed if we are running DynamoSandbox
            return DynamoCoreDirectory;
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

        private static string GetSamplesFolder(string dataRootDirectory, string locale)
        {
            var versionedDirectory = dataRootDirectory;
            if (!Directory.Exists(versionedDirectory))
            {
                // Try to see if folder "%ProgramData%\{...}\{major}.{minor}" exists, if it
                // does not, then root directory would be "%ProgramData%\{...}".
                //
                dataRootDirectory = Directory.GetParent(versionedDirectory).FullName;
            }
            else if (!Directory.Exists(Path.Combine(versionedDirectory, Configurations.SamplesAsString)))
            {
                // If the folder "%ProgramData%\{...}\{major}.{minor}" exists, then try to see
                // if the folder "%ProgramData%\{...}\{major}.{minor}\samples" exists. If it
                // doesn't exist, then root directory would be "%ProgramData%\{...}".
                //
                dataRootDirectory = Directory.GetParent(versionedDirectory).FullName;
            }

            var sampleDirectory = Path.Combine(dataRootDirectory, Configurations.SamplesAsString, locale);

            // If the localized samples directory does not exist then fall back 
            // to using the en-US samples folder. Do an additional check to see 
            // if the localized folder is available but is empty.
            // 
            var di = new DirectoryInfo(sampleDirectory);
            if (!Directory.Exists(sampleDirectory) ||
                !di.GetDirectories().Any() ||
                !di.GetFiles("*.dyn", SearchOption.AllDirectories).Any())
            {
                var neutralCommonSamples = Path.Combine(dataRootDirectory, Configurations.SamplesAsString, "en-US");
                if (Directory.Exists(neutralCommonSamples))
                    sampleDirectory = neutralCommonSamples;
            }

            return sampleDirectory;
        }

        /// <summary>
        /// Get template folder path from common data directory
        /// </summary>
        /// <param name="dataRootDirectory"></param>
        /// <returns></returns>
        private string GetTemplateFolder(string dataRootDirectory)
        {
            // Means that we are running on a host like Revit or Civil3D.
            if (!string.IsNullOrEmpty(hostApplicationDirectory) && pathResolver != null)
            {
                var versionedDirectory = dataRootDirectory;
                if (!Directory.Exists(versionedDirectory))
                {
                    // Try to see if folder "%ProgramData%\{...}\{major}.{minor}" exists, if it
                    // does not, then root directory would be "%ProgramData%\{...}".
                    //
                    dataRootDirectory = Directory.GetParent(versionedDirectory).FullName;
                }
                else if (!Directory.Exists(Path.Combine(versionedDirectory, Configurations.TemplatesAsString)))
                {
                    // If the folder "%ProgramData%\{...}\{major}.{minor}" exists, then try to see
                    // if the folder "%ProgramData%\{...}\{major}.{minor}\templates" exists. If it
                    // doesn't exist, then root directory would be "%ProgramData%\{...}".
                    //
                    dataRootDirectory = Directory.GetParent(versionedDirectory).FullName;
                }
            }

            var uiCulture = CultureInfo.CurrentUICulture.Name;
            var templateDirectory = Path.Combine(dataRootDirectory, Configurations.TemplatesAsString, uiCulture);

            // If the localized template directory does not exist then fall back 
            // to using the en-US template folder. Do an additional check to see 
            // if the localized folder is available but is empty.
            // 
            var di = new DirectoryInfo(templateDirectory);
            if (!Directory.Exists(templateDirectory) ||
                !di.GetDirectories().Any() ||
                !di.GetFiles("*.dyn", SearchOption.AllDirectories).Any())
            {
                var neturalCommonTemplates = Path.Combine(dataRootDirectory, Configurations.TemplatesAsString, "en-US");
                if (Directory.Exists(neturalCommonTemplates))
                    templateDirectory = neturalCommonTemplates;
            }

            return templateDirectory;
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
