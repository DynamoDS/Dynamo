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

        private const string stdLibName = @"Standard Library";
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

        private readonly List<string> packagesDirectories = new List<string>();


        /// <summary>
        /// Returns the default package directory where new packages will be installed
        /// This is the first non standard library directory
        /// The first entry is the standard library.
        /// </summary>
        /// <returns>Returns the path to the DefaultPackagesDirectory if found - or null if something has gone wrong.</returns>
        public string DefaultPackagesDirectory
        {
            get { return defaultPackagesDirectoryIndex != -1 ? packagesDirectories[defaultPackagesDirectoryIndex] : null; }
        }

        private int defaultPackagesDirectoryIndex = -1;

        private readonly List<string> packagesDirectoriesToVerifyCertificates = new List<string>();

        private string stdLibDirectory = null;

        /// <summary>
        /// The standard library directory is located in the same directory as the DynamoPackages.dll
        /// Property should only be set during testing.
        /// </summary>
        internal string StandardLibraryDirectory
        {
            get
            {
                return stdLibDirectory == null ?
                    Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(GetType()).Location),stdLibName, @"Packages")
                    : stdLibDirectory;
            }
            set
            {
                if (stdLibDirectory != value)
                {
                    stdLibDirectory = value;
                }
            }
        }

        public PackageLoader(string overridePackageDirectory)
            : this(new[] { overridePackageDirectory })
        {
        }

        /// <summary>
        /// This constructor is only intended for testing of stdLib using a non standard directory.
        /// </summary>
        /// <param name="packagesDirectories"></param>
        /// <param name="stdLibDirectory"></param>
        internal PackageLoader(IEnumerable<string> packagesDirectories, string stdLibDirectory)
        {
            InitPackageLoader(packagesDirectories, stdLibDirectory);

            //override the property.
            this.StandardLibraryDirectory = stdLibDirectory;
        }

        public PackageLoader(IEnumerable<string> packagesDirectories)
        {
            InitPackageLoader(packagesDirectories, StandardLibraryDirectory);
        }

        /// <summary>
        /// Initialize a new instance of PackageLoader class
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

        private void InitPackageLoader(IEnumerable<string> packagesDirectories, string stdLibDirectory)
        {
            if (packagesDirectories == null)
                throw new ArgumentNullException("packagesDirectories");

            this.packagesDirectories.AddRange(packagesDirectories);

            // Setup standard library
            var standardLibraryIndex = this.packagesDirectories.IndexOf(DynamoModel.StandardLibraryToken);
            if (standardLibraryIndex == -1)
            {
                // No standard library in list
                // Add standard library first
                this.packagesDirectories.Insert(0, stdLibDirectory);
            }
            else
            {
                // Replace token with runtime library location
                this.packagesDirectories[standardLibraryIndex] = stdLibDirectory;
            }

            // Setup Default Package Directory
            if (standardLibraryIndex == -1)
            {
                defaultPackagesDirectoryIndex = this.packagesDirectories.Count > 1 ? 1 : -1;
            }
            else
            {
                var safeIndex = this.packagesDirectories.Count > 1 ? 1 : -1;
                defaultPackagesDirectoryIndex = standardLibraryIndex == 1 ? 0 : safeIndex;
            }

            var error = PathHelper.CreateFolderIfNotExist(DefaultPackagesDirectory);

            if (error != null)
                Log(error);

            packagesDirectoriesToVerifyCertificates.Add(stdLibDirectory);
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

        private void OnPackagesLoaded(IEnumerable<Assembly> assemblies)
        {
            PackagesLoaded?.Invoke(assemblies);
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
        internal void TryLoadPackageIntoLibrary(Package package)
        {
            Add(package);

            // Prevent duplicate loads
            if (package.Loaded) return;

            try
            {
                // load node libraries
                foreach (var assem in package.EnumerateAssembliesInBinDirectory())
                {
                    if (assem.IsNodeLibrary)
                    {
                        try
                        {
                            OnRequestLoadNodeLibrary(assem.Assembly);
                        }
                        catch (LibraryLoadFailedException ex)
                        {
                            Log(ex.GetType() + ": " + ex.Message);
                        }
                    }
                }
                // load custom nodes
                var packageInfo = new Graph.Workspaces.PackageInfo(package.Name, new Version(package.VersionName));
                var customNodes = OnRequestLoadCustomNodeDirectory(package.CustomNodeDirectory, packageInfo);
                package.LoadedCustomNodes.AddRange(customNodes);

                package.EnumerateAdditionalFiles();
                // If the additional files contained an extension manifest, then request it be loaded.
                var extensionManifests = package.AdditionalFiles.Where(
                    file => file.Model.Name.Contains("ExtensionDefinition.xml") && !(file.Model.Name.Contains("ViewExtensionDefinition.xml"))).ToList();
                foreach (var extPath in extensionManifests)
                {
                    var extension = RequestLoadExtension?.Invoke(extPath.Model.FullName);
                    if (extension != null)
                    {
                        RequestAddExtension?.Invoke(extension);
                    }
                    requestedExtensions.Add(extension);
                }

                package.Loaded = true;
                PackgeLoaded?.Invoke(package);
            }
            catch (CustomNodePackageLoadException e)
            {
                Package originalPackage =
                    localPackages.FirstOrDefault(x => x.CustomNodeDirectory == e.InstalledPath);
                OnConflictingPackageLoaded(originalPackage, package);
            }
            catch (Exception e)
            {
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
                LocalPackages.SelectMany(x => x.EnumerateAssembliesInBinDirectory().Where(y => y.IsNodeLibrary));
            OnPackagesLoaded(assemblies.Select(x => x.Assembly));
        }

        /// <summary>
        ///     Scan the PackagesDirectory for packages and attempt to load all of them.  Beware! Fails silently for duplicates.
        /// </summary>
        public void LoadAll(LoadPackageParams loadPackageParams)
        {
            ScanAllPackageDirectories(loadPackageParams.Preferences);

            var pathManager = loadPackageParams.PathManager;
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
            var packageList = packages.ToList();

            foreach (var pkg in packageList)
            {
                // If the pkg is null, then don't load that package into the Library.
                if (pkg != null)
                {
                    TryLoadPackageIntoLibrary(pkg);
                }
            }

            var assemblies = packageList
                .SelectMany(p => p.LoadedAssemblies.Where(y => y.IsNodeLibrary))
                .Select(a => a.Assembly)
                .ToList();
            OnPackagesLoaded(assemblies);
        }

        public void LoadCustomNodesAndPackages(LoadPackageParams loadPackageParams, CustomNodeManager customNodeManager)
        {
            foreach (var path in loadPackageParams.Preferences.CustomPackageFolders)
            {
                customNodeManager.AddUninitializedCustomNodesInPath(path, false, false);
                if (!packagesDirectories.Contains(path))
                {
                    packagesDirectories.Add(path);
                }
            }
            LoadAll(loadPackageParams);
        }

        private void ScanAllPackageDirectories(IPreferences preferences)
        {
            foreach (var packagesDirectory in packagesDirectories)
            {

                if (preferences is IDisablePackageLoadingPreferences disablePrefs
                    &&
                    //if this directory is the standard library location
                    //and loading from there is disabled, don't scan the directory.
                    ((disablePrefs.DisableStandardLibrary && packagesDirectory == StandardLibraryDirectory)
                    //or if custom package directories are disabled, and this is a custom package directory, don't scan.
                    || (disablePrefs.DisableCustomPackageLocations && preferences.CustomPackageFolders.Contains(packagesDirectory))))
                {
                    Log(string.Format(Resources.PackagesDirectorySkipped,packagesDirectory));
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
                        pkg.MarkForUninstall(preferences);
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

                var existingPackage = LocalPackages.FirstOrDefault(package => package.Name == discoveredPackage.Name);

                // Is this a new package?
                if (existingPackage == null)
                {
                    // Yes
                    Add(discoveredPackage);
                    return discoveredPackage; // success
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
        private static void CheckPackageNodeLibraryCertificates(string packageDirectoryPath, Package discoveredPkg)
        {
            var dllfiles = new System.IO.DirectoryInfo(discoveredPkg.BinaryDirectory).EnumerateFiles("*.dll");
            if (discoveredPkg.Header.node_libraries.Count() == 0 && dllfiles.Count() != 0)
            {
                throw new LibraryLoadFailedException(packageDirectoryPath,
                    String.Format(
                        Resources.InvalidPackageNoNodeLibrariesDefinedInPackageJson,
                        discoveredPkg.Name, discoveredPkg.RootDirectory));
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
                    CertificateVerification.CheckAssemblyForValidCertificate(filepath);
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
