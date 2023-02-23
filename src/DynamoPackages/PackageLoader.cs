using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Dynamo.Configuration;
using Dynamo.Core;
using Dynamo.Exceptions;
using Dynamo.Extensions;
using Dynamo.Interfaces;
using Dynamo.Logging;
using Dynamo.Utilities;
using DynamoPackages.Properties;
using DynamoUtilities;
using Dynamo.Models;

namespace Dynamo.PackageManager
{
    public struct LoadPackageParams
    {
        public IPreferences Preferences { get; set; }

        [Obsolete("Do not use. This will be removed in Dynamo 3.0")]
        public IPathManager PathManager { get; set; }
    }

    public enum AssemblyLoadingState
    {
        Success,
        NotManagedAssembly,
        AlreadyLoaded
    }

    public class PackageLoader : LogSourceBase
    {
        internal event Action<Assembly> RequestLoadNodeLibrary;
        internal event Action<IEnumerable<Assembly>> PackagesLoaded;
        internal event Func<string, Graph.Workspaces.PackageInfo, IEnumerable<CustomNodeInfo>> RequestLoadCustomNodeDirectory;
        internal event Func<string, IExtension> RequestLoadExtension;
        internal event Action<IExtension> RequestAddExtension;

        /// <summary>
        /// This event is raised when a package is first added to the list of packages this package loader is loading.
        /// This event occurs before the package is fully loaded. 
        /// </summary>
        public event Action<Package> PackageAdded;

        /// <summary>
        /// This event is raised when a package is fully loaded. It will be true that when this event is raised
        /// Packge.Loaded will be true for the package.
        /// </summary>
        // TODO 3.0 Fix spelling
        public event Action<Package> PackgeLoaded;

        /// <summary>
        /// This event is raised when the package is removed from the list of packages loaded by this packageLoader.
        /// </summary>
        public event Action<Package> PackageRemoved;

        private readonly List<IExtension> requestedExtensions = new List<IExtension>();
        /// <summary>
        /// Collection of ViewExtensions the ViewExtensionSource requested be loaded.
        /// </summary>
        public IEnumerable<IExtension> RequestedExtensions
        {
            get
            {
                return requestedExtensions;
            }
        }

        private readonly List<Package> localPackages = new List<Package>();
        public IEnumerable<Package> LocalPackages { get { return localPackages; } }

        /// <summary>
        /// Returns the default package directory where new packages will be installed
        /// This is the first non builtin packages directory
        /// The first entry is the builtin packages.
        /// </summary>
        /// <returns>Returns the path to the DefaultPackagesDirectory if found - or null if something has gone wrong.</returns>
        [Obsolete("This property is redundant, please use the PathManager.DefaultPackagesDirectory property instead.")]
        public string DefaultPackagesDirectory
        {
            get { return pathManager.DefaultPackagesDirectory; }
        }

        /// <summary>
        /// Combines the extension with the root path and returns it if the path exists. 
        /// If not, the root path is returned unchanged.
        /// </summary>
        /// <param name="root">root path to transform</param>
        /// <param name="userDataFolder"></param>
        /// <param name="extension">subdirectory or subpath</param>
        /// <returns>combined root and extension path</returns>
        private static string TransformPath(string root, string extension)
        {
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

        private readonly List<string> packagesDirectoriesToVerifyCertificates = new List<string>();

        private readonly IPathManager pathManager;

        [Obsolete("This constructor will be removed in Dynamo 3.0 and should not be used any longer. If used, it should be passed parameters from PathManager properties.")]
        /// <summary>
        /// This constructor is currently being used for testing and these tests should be updated to use 
        /// another constructor when this is obsoleted.
        /// </summary>
        public PackageLoader(string overridePackageDirectory)
            : this(new[] { overridePackageDirectory })
        {
        }

        [Obsolete("This constructor will be removed in Dynamo 3.0 and should not be used any longer. If used, it should be passed parameters from PathManager properties.")]
        /// <summary>
        /// This constructor is currently being used by other constructors that have also been deprecated and by tests,
        /// which should be updated to use another constructor when this is obsoleted.
        /// </summary>
        public PackageLoader(IEnumerable<string> packagesDirectories)
        {
            InitPackageLoader(packagesDirectories, null);
        }

        internal PackageLoader(IPathManager pathManager)
        {
            this.pathManager = pathManager;
            InitPackageLoader(pathManager.PackagesDirectories, PathManager.BuiltinPackagesDirectory);

            if (!string.IsNullOrEmpty(pathManager.CommonDataDirectory))
            {
                packagesDirectoriesToVerifyCertificates.Add(pathManager.CommonDataDirectory);
            }
        }

        [Obsolete("This constructor will be removed in Dynamo 3.0 and should not be used any longer. If used, it should be passed parameters from PathManager properties.")]
        /// <summary>
        /// Initialize a new instance of PackageLoader class.
        /// This constructor is currently being used for testing and these tests should be updated to use 
        /// another constructor when this is obsoleted.
        /// </summary>
        /// <param name="packagesDirectories">Default package directories</param>
        /// <param name="packageDirectoriesToVerify">Default package directories where node library files require certificate verification before loading</param>
        public PackageLoader(IEnumerable<string> packagesDirectories, IEnumerable<string> packageDirectoriesToVerify)
            : this(packagesDirectories)
        {
            if (packageDirectoriesToVerify == null)
                throw new ArgumentNullException("packageDirectoriesToVerify");

            packagesDirectoriesToVerifyCertificates.AddRange(packageDirectoriesToVerify);
        }

        private void InitPackageLoader(IEnumerable<string> packagesDirectories, string builtinPackagesDir)
        {
            if (packagesDirectories == null)
            {
                throw new ArgumentNullException("packagesDirectories");
            }

            if (builtinPackagesDir == null) return;

            packagesDirectoriesToVerifyCertificates.Add(builtinPackagesDir);
        }

        private void OnPackageAdded(Package pkg)
        {
            if (PackageAdded != null)
            {
                PackageAdded(pkg);
            }
        }

        private void OnPackageRemoved(Package pkg)
        {
            if (PackageRemoved != null)
            {
                PackageRemoved(pkg);
            }
        }

        internal void Add(Package pkg)
        {
            if (!localPackages.Contains(pkg))
            {
                localPackages.Add(pkg);
                pkg.MessageLogged += OnPackageMessageLogged;
                OnPackageAdded(pkg);
            }
        }

        internal void Remove(Package pkg)
        {
            if (localPackages.Contains(pkg))
            {
                localPackages.Remove(pkg);
                pkg.MessageLogged -= OnPackageMessageLogged;
                OnPackageRemoved(pkg);
            }
        }

        private void OnPackageMessageLogged(ILogMessage obj)
        {
            Log(obj);
        }

        private void OnRequestLoadNodeLibrary(Assembly assem)
        {
            if (RequestLoadNodeLibrary != null)
            {
                RequestLoadNodeLibrary(assem);
            }
        }

        private IEnumerable<CustomNodeInfo> OnRequestLoadCustomNodeDirectory(string directory, Graph.Workspaces.PackageInfo packageInfo)
        {
            if (RequestLoadCustomNodeDirectory != null)
            {
                return RequestLoadCustomNodeDirectory(directory, packageInfo);
            }

            return new List<CustomNodeInfo>();
        }

        /// <summary>
        /// Try loading package into Library (including all node libraries and custom nodes)
        /// and add to LocalPackages.
        /// </summary>
        /// <param name="package"></param>
        private void TryLoadPackageIntoLibrary(Package package)
        {
            Add(package);

            // Prevent duplicate loads
            if (package.LoadState.State == PackageLoadState.StateTypes.Loaded) return;

            // Prevent loading packages that have been specifically marked as unloaded
            if (package.LoadState.State == PackageLoadState.StateTypes.Unloaded) return;

            List<Assembly> loadedNodeLibs = new List<Assembly>();
            List<Assembly> failedNodeLibs = new List<Assembly>();
            try
            {
                List<Assembly> blockedAssemblies = new List<Assembly>();
                // Try to load node libraries from all assemblies
                foreach (var assem in package.EnumerateAndLoadAssembliesInBinDirectory())
                {
                    if (assem.IsNodeLibrary)
                    {
                        try
                        {
                            OnRequestLoadNodeLibrary(assem.Assembly);
                            loadedNodeLibs.Add(assem.Assembly);
                        }
                        catch (LibraryLoadFailedException ex)
                        {
                            // Managed exception
                            // We can still try to load other parts of the package
                            Log(ex.GetType() + ": " + ex.Message);
                        }
                        catch (DynamoServices.AssemblyBlockedException)
                        {
                            blockedAssemblies.Add(assem.Assembly);
                        }
                        catch (Exception)
                        {
                            failedNodeLibs.Add(assem.Assembly);
                        }
                    }
                }

                if (loadedNodeLibs.Count > 0)
                {
                    // Try to load any valid node libraries regardless package state
                    PackagesLoaded?.Invoke(loadedNodeLibs);
                }

                if (blockedAssemblies.Count > 0)
                {
                    throw new DynamoServices.AssemblyBlockedException("The following assemblies are blocked : " + string.Join(", ", blockedAssemblies.Select(x => Path.GetFileName(x.Location))));
                }

                // Generic fatal error
                if (failedNodeLibs.Count > 0)
                {
                    throw new Exception("Failed to load the following assemblies : " + string.Join(", ", failedNodeLibs.Select(x => Path.GetFileName(x.Location))));
                }

                // load custom nodes
                var packageInfo = new Graph.Workspaces.PackageInfo(package.Name, new Version(package.VersionName));
                var customNodes = OnRequestLoadCustomNodeDirectory(package.CustomNodeDirectory, packageInfo);
                package.LoadedCustomNodes.AddRange(customNodes);

                package.EnumerateAdditionalFiles();

                // If the additional files contained an extension manifest, then request it be loaded.
                var extensionManifests = package.AdditionalFiles.Where(
                    file => file.Model.Name.Contains("ExtensionDefinition.xml") && !file.Model.Name.Contains("ViewExtensionDefinition.xml")).ToList();
                foreach (var extPath in extensionManifests)
                {
                    var extension = RequestLoadExtension?.Invoke(extPath.Model.FullName);
                    if (extension != null)
                    {
                        RequestAddExtension?.Invoke(extension);
                    }
                    requestedExtensions.Add(extension);
                }

                package.SetAsLoaded();


                PythonServices.PythonEngineManager.Instance.
                    LoadPythonEngine(package.LoadedAssemblies.Select(x => x.Assembly));

                PackgeLoaded?.Invoke(package);
            }
            catch (CustomNodePackageLoadException e)
            {
                Package originalPackage =
                    localPackages.FirstOrDefault(x => x.CustomNodeDirectory == e.InstalledPath);
                OnConflictingPackageLoaded(originalPackage, package);

                package.LoadState.SetAsError(e.Message);
            }
            catch (Exception e)
            {
                if (e is DynamoServices.AssemblyBlockedException)
                {
                    var failureMessage = string.Format(Properties.Resources.PackageLoadFailureForBlockedAssembly, e.Message);
                    DynamoServices.LoadLibraryEvents.OnLoadLibraryFailure(failureMessage, Properties.Resources.LibraryLoadFailureMessageBoxTitle);
                }
                package.LoadState.SetAsError(e.Message);
                Log("Exception when attempting to load package " + package.Name + " from " + package.RootDirectory);
                Log(e.GetType() + ": " + e.Message);
            }
        }

        /// <summary>
        /// Event raised when a custom node package containing conflicting node definition
        /// with an existing package is tried to load.
        /// </summary>
        public event Action<Package, Package> ConflictingCustomNodePackageLoaded;
        private void OnConflictingPackageLoaded(Package installed, Package conflicting)
        {
            var handler = ConflictingCustomNodePackageLoaded;
            handler?.Invoke(installed, conflicting);
        }

        /// <summary>
        ///     Load the package into Dynamo (including all node libraries and custom nodes)
        ///     and add to LocalPackages.
        /// </summary>
        // TODO: Remove in 3.0 (Refer to PR #9736).
        [Obsolete("This API will be deprecated in 3.0. Use LoadPackages(IEnumerable<Package> packages) instead.")]
        public void Load(Package package)
        {
            TryLoadPackageIntoLibrary(package);

            var assemblies =
                LocalPackages.SelectMany(x => x.EnumerateAndLoadAssembliesInBinDirectory().Where(y => y.IsNodeLibrary));
            PackagesLoaded?.Invoke(assemblies.Select(x => x.Assembly));
        }

        /// <summary>
        ///     Scan the PackagesDirectory for packages and attempt to load all of them.  Beware! Fails silently for duplicates.
        /// </summary>
        public void LoadAll(LoadPackageParams loadPackageParams)
        {
            ScanAllPackageDirectories(loadPackageParams.Preferences);

            if (pathManager != null)
            {
                foreach (var pkg in LocalPackages)
                {
                    if (Directory.Exists(pkg.BinaryDirectory))
                    {
                        pathManager.AddResolutionPath(pkg.BinaryDirectory);
                    }
                }
            }

            if (LocalPackages.Any())
            {
                LoadPackages(LocalPackages);
            }
        }

        /// <summary>
        /// Loads and imports all packages. 
        /// </summary>
        /// <param name="packages"></param>
        public void LoadPackages(IEnumerable<Package> packages)
        {
            foreach (var pkg in packages)
            {
                // If the pkg is null, then don't load that package into the Library.
                if (pkg != null)
                {
                    TryLoadPackageIntoLibrary(pkg);
                }
            }
        }

        /// <summary>
        /// Helper function to load new custom nodes and packages.
        /// </summary>
        /// <param name="newPaths">New package paths to load custom nodes and packages from.</param>
        /// <param name="preferences">Can be a temporary local preferences object.</param>
        /// <param name="customNodeManager"></param>
        private void LoadCustomNodesAndPackagesHelper(IEnumerable<string> newPaths, IPreferences preferences, 
            CustomNodeManager customNodeManager)
        {
            foreach (var path in preferences.CustomPackageFolders)
            {
                // Append the definitions subdirectory for custom nodes.
                var dir = path == DynamoModel.BuiltInPackagesToken ? PathManager.BuiltinPackagesDirectory : path;
                dir = TransformPath(dir, PathManager.DefinitionsDirectoryName);

                customNodeManager.AddUninitializedCustomNodesInPath(dir, false, false);
            }
            foreach (var path in newPaths)
            {
                if (DynamoModel.IsDisabledPath(path, preferences))
                {
                    Log(string.Format(Resources.PackagesDirectorySkipped, path));
                    continue;
                }
                else
                {
                    ScanPackageDirectories(path, preferences);
                }
            }

            if (pathManager != null)
            {
                foreach (var pkg in LocalPackages)
                {
                    if (Directory.Exists(pkg.BinaryDirectory))
                    {
                        pathManager.AddResolutionPath(pkg.BinaryDirectory);
                    }

                }
            }

            if (LocalPackages.Any())
            {
                // Load only those recently addeed local packages (that are located in any of the new paths)
                var newPackages = LocalPackages.Where(x => newPaths.Any(y => x.RootDirectory.Contains(y)));
                LoadPackages(newPackages);
            }
        }

        /// <summary>
        /// This method is used when custom nodes and packages need to be loaded from new package paths 
        /// that have been added to preference settings.
        /// </summary>
        /// <param name="newPaths">New package paths to load custom nodes and packages from.</param>
        /// <param name="customNodeManager"></param>
        internal void LoadNewCustomNodesAndPackages(IEnumerable<string> newPaths, CustomNodeManager customNodeManager)
        {
            if(newPaths == null || !newPaths.Any()) return;

            var preferences = (pathManager as PathManager).Preferences;
            var packageDirsToScan = new List<string>();

            foreach (var path in newPaths)
            {
                var packageDirectory = pathManager.PackagesDirectories.FirstOrDefault(x => x.StartsWith(path));
                if (packageDirectory != null)
                {
                    packageDirsToScan.Add(packageDirectory);
                }
            }
            LoadCustomNodesAndPackagesHelper(packageDirsToScan, preferences, customNodeManager);

        }

        /// <summary>
        /// LoadCustomNodesAndPackages can be used to load custom nodes and packages
        /// from temporary paths that do not need to be added to preference settings. 
        /// To load from temporary custom paths, initialize a local PreferenceSettings object 
        /// and add the paths to its CustomPackageFolders property, then initialize a new 
        /// LoadPackageParams with this preferences object and use as input to this method.
        /// To load from custom paths that need to be persisted to the preferences, 
        /// initialize a LoadPackageParams from an existing preferences object.
        /// </summary>
        /// <param name="loadPackageParams">LoadPackageParams initialized with local PreferenceSettings object containing custom package path.</param>
        /// <param name="customNodeManager"></param>
        public void LoadCustomNodesAndPackages(LoadPackageParams loadPackageParams, CustomNodeManager customNodeManager)
        {
            var preferences = loadPackageParams.Preferences;
            LoadCustomNodesAndPackagesHelper(preferences.CustomPackageFolders, preferences, customNodeManager);
        }

        private void ScanAllPackageDirectories(IPreferences preferences)
        {
            foreach (var packagesDirectory in pathManager.PackagesDirectories)
            {

                if (DynamoModel.IsDisabledPath(packagesDirectory, preferences))
                {
                    Log(string.Format(Resources.PackagesDirectorySkipped, packagesDirectory));
                    continue;
                }
                else
                {
                    ScanPackageDirectories(packagesDirectory, preferences);
                }
            }
        }

        private void ScanPackageDirectories(string root, IPreferences preferences)
        {
            try
            {
                if (!Directory.Exists(root))
                {
                    string extension = null;
                    if (root != null)
                    {
                        extension = Path.GetExtension(root);
                    }

                    // If the path has a .dll or .ds extension it is a locally imported library
                    // so do not output an error about the directory
                    if (extension == ".dll" || extension == ".ds")
                    {
                        return;
                    }

                    Log(string.Format(Resources.InvalidPackageFolderWarning, root));
                    return;
                }

                foreach (var dir in
                    Directory.EnumerateDirectories(root, "*", SearchOption.TopDirectoryOnly))
                {

                    // verify if the package directory requires certificate verifications
                    // This is done by default for the package directory defined in PathManager common directory location.
                    var checkCertificates = false;
                    foreach (var pathToVerifyCert in packagesDirectoriesToVerifyCertificates)
                    {
                        if (root.Contains(pathToVerifyCert))
                        {
                            checkCertificates = true;
                            break;
                        }
                    }

                    var pkg = ScanPackageDirectory(dir, checkCertificates);
                    if (pkg != null && preferences.PackageDirectoriesToUninstall.Contains(dir))
                    {
                        if (pkg.BuiltInPackage)
                        {
                            // If the built-in package's package root dir was contained in the uninstall list in preferences,
                            // then we set it directly to Unloaded state.
                            pkg.LoadState.SetAsUnloaded();
                        } 
                        else
                        {
                            pkg.LoadState.SetScheduledForDeletion();
                        }
                    }
                    
                }
            }
            catch (UnauthorizedAccessException ex) { }
            catch (IOException ex) { }
            catch (ArgumentException ex) { }
        }

        public Package ScanPackageDirectory(string directory)
        {
            return ScanPackageDirectory(directory, false);
        }

        public Package ScanPackageDirectory(string directory, bool checkCertificates)
        {
            try
            {
                var headerPath = Path.Combine(directory, "pkg.json");

                Package discoveredPackage;

                // get the package name and the installed version
                if (PathHelper.IsValidPath(headerPath))
                {
                    discoveredPackage = Package.FromJson(headerPath, AsLogger());
                    if (discoveredPackage == null)
                        throw new LibraryLoadFailedException(directory, String.Format(Properties.Resources.MalformedHeaderPackage, headerPath));
                }
                else
                {
                    throw new LibraryLoadFailedException(directory, String.Format(Properties.Resources.NoHeaderPackage, headerPath));
                }

                // prevent loading unsigned packages if the certificates are required on package dlls
                if (checkCertificates)
                {
                    CheckPackageNodeLibraryCertificates(directory, discoveredPackage);
                }

                var discoveredVersion = CheckAndGetPackageVersion(discoveredPackage.VersionName, discoveredPackage.Name, discoveredPackage.RootDirectory);

                var existingPackage = LocalPackages.FirstOrDefault(package => 
                                        (package.Name == discoveredPackage.Name) && 
                                        (package.LoadState.State != PackageLoadState.StateTypes.Unloaded));

                // Is this a new package?
                if (existingPackage == null)
                {
                    Add(discoveredPackage);
                    return discoveredPackage; // success
                }

                // Conflict invloving a built-in package
                if (discoveredPackage.BuiltInPackage || existingPackage.BuiltInPackage)
                {
                    // We show both packages but we mark the new one as unloaded.
                    discoveredPackage.LoadState.SetAsUnloaded();
                    Add(discoveredPackage);
                }

                var existingVersion = CheckAndGetPackageVersion(existingPackage.VersionName, existingPackage.Name, existingPackage.RootDirectory);

                // Is this a duplicated package?
                if (existingVersion == discoveredVersion)
                {
                    // Duplicated with the same version
                    throw new LibraryLoadFailedException(directory,
                        String.Format(Properties.Resources.DulicatedPackage,
                            discoveredPackage.Name,
                            discoveredPackage.RootDirectory));
                } 
                
                // Is the existing version newer?
                if (existingVersion > discoveredVersion)
                {
                    // Older version found, show notification
                    throw new LibraryLoadFailedException(directory, String.Format(Properties.Resources.DuplicatedOlderPackage,
                            existingPackage.Name,
                            discoveredPackage.RootDirectory,
                            existingVersion.ToString(),
                            discoveredVersion.ToString()));
                }

                // Newer version found, show notification.
                throw new LibraryLoadFailedException(directory, String.Format(Properties.Resources.DuplicatedNewerPackage,
                        existingPackage.Name,
                        discoveredPackage.RootDirectory,
                        existingVersion.ToString(),
                        discoveredVersion.ToString()));
            }
            catch (Exception e)
            {
                Log(String.Format(Properties.Resources.ExceptionEncountered, directory), WarningLevel.Error);
                Log(e);
            }

            return null;
        }

        /// <summary>
        /// Check and get the version from the version string. Throw a library load exception if anything is wrong with the version
        /// </summary>
        /// <param name="version">the version string</param>
        /// <param name="name">name of the package</param>
        /// <param name="directory">package directory</param>
        /// <returns>Returns a valid Version</returns>
        private Version CheckAndGetPackageVersion(string version, string name, string directory)
        {
            try
            {
                return VersionUtilities.PartialParse(version);
            }
            catch (Exception e) when (e is ArgumentException || e is FormatException || e is OverflowException)
            {
                throw new LibraryLoadFailedException(directory, String.Format(Properties.Resources.InvalidPackageVersion, name, directory, version));
            }
        }

        /// <summary>
        /// Check the node libraries defined in the package json file are valid and have a valid certificate
        /// </summary>
        /// <param name="packageDirectoryPath">path to package location</param>
        /// <param name="discoveredPkg">package object to check</param>
        private void CheckPackageNodeLibraryCertificates(string packageDirectoryPath, Package discoveredPkg)
        {
            var dllfiles = new System.IO.DirectoryInfo(discoveredPkg.BinaryDirectory).EnumerateFiles("*.dll");
            if (discoveredPkg.Header.node_libraries.Count() == 0 && dllfiles.Count() != 0)
            {
                Log(String.Format(
                    String.Format(Resources.InvalidPackageNoNodeLibrariesDefinedInPackageJson,
                    discoveredPkg.Name, discoveredPkg.RootDirectory)));
            }

            foreach (var nodeLibraryAssembly in discoveredPkg.Header.node_libraries)
            {

                //Try to get the assembly name from the manifest file
                string filename;
                try
                {
                    filename = new AssemblyName(nodeLibraryAssembly).Name + ".dll";
                }
                catch
                {
                    throw new LibraryLoadFailedException(packageDirectoryPath,
                        String.Format(
                            Resources.InvalidPackageMalformedNodeLibraryDefinition,
                            discoveredPkg.Name, discoveredPkg.RootDirectory));
                }

                //Verify the node library exists in the package bin directory and has a valid certificate
                var filepath = Path.Combine(discoveredPkg.BinaryDirectory, filename);
                try
                {
                    if (OSHelper.IsWindows())
                    {
                        CertificateVerification.CheckAssemblyForValidCertificate(filepath);
                    }
                }
                catch (Exception e)
                {
                    throw new LibraryLoadFailedException(packageDirectoryPath,
                        String.Format(
                            Resources.InvalidPackageNodeLibraryIsNotSigned,
                            discoveredPkg.Name, discoveredPkg.RootDirectory, e.Message));
                }

            }

            discoveredPkg.RequiresSignedEntryPoints = true;
        }

        /// <summary>
        ///     Attempt to load a managed assembly in to ReflectionOnlyLoadFrom context. 
        /// </summary>
        /// <param name="filename">The filename of a DLL</param>
        /// <param name="assem">out Assembly - the passed value does not matter and will only be set if loading succeeds</param>
        /// <returns>Returns Success if success, NotManagedAssembly if BadImageFormatException, AlreadyLoaded if FileLoadException</returns>
        internal static AssemblyLoadingState TryReflectionOnlyLoadFrom(string filename, out Assembly assem)
        {
            try
            {
                assem = Assembly.ReflectionOnlyLoadFrom(filename);
                return AssemblyLoadingState.Success;
            }
            catch (BadImageFormatException)
            {
                assem = null;
                return AssemblyLoadingState.NotManagedAssembly;
            }
            catch (FileLoadException)
            {
                assem = null;
                return AssemblyLoadingState.AlreadyLoaded;
            }
        }

        /// <summary>
        ///     Attempt to load a managed assembly in to LoadFrom context. 
        /// </summary>
        /// <param name="filename">The filename of a DLL</param>
        /// <param name="assem">out Assembly - the passed value does not matter and will only be set if loading succeeds</param>
        /// <returns>Returns true if success, false if BadImageFormatException (i.e. not a managed assembly)</returns>
        internal static bool TryLoadFrom(string filename, out Assembly assem)
        {
            try
            {
                assem = Assembly.LoadFrom(filename);
                return true;
            }
            catch (FileLoadException e)
            {
                // If the exception is having HRESULT of 0x80131515, then we need to instruct the user to "unblock" the downloaded DLL.
                if (e.HResult == unchecked((int)0x80131515))
                {
                    throw new DynamoServices.AssemblyBlockedException(e.Message);
                }
                assem = null;
                return false;
            }
            catch (BadImageFormatException)
            {
                assem = null;
                return false;
            }
        }

        public bool IsUnderPackageControl(string path)
        {
            return LocalPackages.Any(ele => ele.ContainsFile(path));
        }

        public bool IsUnderPackageControl(CustomNodeInfo def)
        {
            return IsUnderPackageControl(def.Path);
        }

        public bool IsUnderPackageControl(Type t)
        {
            return LocalPackages.Any(package => package.LoadedTypes.Contains(t));
        }

        public bool IsUnderPackageControl(Assembly t)
        {
            return LocalPackages.Any(package => package.LoadedAssemblies.Any(x => x.Assembly == t));
        }

        public Package GetPackageFromRoot(string path)
        {
            return LocalPackages.FirstOrDefault(pkg => pkg.RootDirectory == path);
        }

        public Package GetOwnerPackage(Type t)
        {
            return LocalPackages.FirstOrDefault(package => package.LoadedTypes.Contains(t));
        }

        public Package GetOwnerPackage(CustomNodeInfo def)
        {
            return GetOwnerPackage(def.Path);
        }

        public Package GetOwnerPackage(string path)
        {
            return LocalPackages.FirstOrDefault(ele => ele.ContainsFile(path));
        }

        private static bool hasAttemptedUninstall;

        internal void DoCachedPackageUninstalls(IPreferences preferences)
        {
            // this can only be run once per app run
            if (hasAttemptedUninstall) return;
            hasAttemptedUninstall = true;

            var pkgDirsRemoved = new HashSet<string>();
            foreach (var pkgNameDirTup in preferences.PackageDirectoriesToUninstall)
            {
                if (pkgNameDirTup.StartsWith(PathManager.BuiltinPackagesDirectory))
                {
                    // do not delete packages from the built in dir
                    continue;
                }
                
                try
                {
                    Directory.Delete(pkgNameDirTup, true);
                    pkgDirsRemoved.Add(pkgNameDirTup);
                    Log(String.Format("Successfully uninstalled package from \"{0}\"", pkgNameDirTup));
                }
                catch
                {
                    Log(
                        String.Format(
                            "Failed to delete package directory at \"{0}\", you may need to delete the directory manually.",
                            pkgNameDirTup),
                        WarningLevel.Moderate);
                }
            }

            preferences.PackageDirectoriesToUninstall.RemoveAll(pkgDirsRemoved.Contains);
        }
    }
}
