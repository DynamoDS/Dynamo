using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using Dynamo.Core;
using Dynamo.DSEngine;
using Dynamo.Interfaces;
using Dynamo.Utilities;
using DynamoUtilities;

namespace Dynamo.PackageManager
{
    public struct LoadPackageParams
    {
        public string Context { get; set; }
        public bool IsTestMode { get; set; }
        public IPreferences Preferences { get; set; }
        public IPathManager PathManager { get; set; }
        public LibraryServices LibraryServices { get; set; }
        public DynamoLoader Loader { get; set; }
        public CustomNodeManager CustomNodeManager { get; set; }
    }

    public class PackageLoader : LogSourceBase
    {
        public string RootPackagesDirectory { get; private set; }

        public PackageLoader(string overridePackageDirectory)
        {
            RootPackagesDirectory = overridePackageDirectory;
            if (!Directory.Exists(RootPackagesDirectory))
                Directory.CreateDirectory(RootPackagesDirectory);
        }

        private readonly ObservableCollection<Package> localPackages = new ObservableCollection<Package>();
        public ObservableCollection<Package> LocalPackages { get { return localPackages; } }

        /// <summary>
        ///     Scan the PackagesDirectory for packages and attempt to load all of them.  Beware! Fails silently for duplicates.
        /// </summary>
        public void LoadPackagesIntoDynamo(LoadPackageParams loadPackageParams)
        {
            ScanAllPackageDirectories(loadPackageParams.Preferences);

            var pathManager = loadPackageParams.PathManager;
            if (pathManager != null)
            {
                foreach (var pkg in LocalPackages)
                    pathManager.AddResolutionPath(pkg.BinaryDirectory);
            }

            foreach (var pkg in LocalPackages)
            {
                pkg.LoadIntoDynamo(loadPackageParams, AsLogger());
            }
        }

        private void ScanAllPackageDirectories(IPreferences preferences)
        { 
            foreach (var dir in 
                Directory.EnumerateDirectories(RootPackagesDirectory, "*", SearchOption.TopDirectoryOnly))
            {
                var pkg = ScanPackageDirectory(dir);
                if (preferences.PackageDirectoriesToUninstall.Contains(dir)) 
                    pkg.MarkForUninstall(preferences);
            }
        }

        public Package ScanPackageDirectory(string directory)
        {
            try
            {
                var headerPath = Path.Combine(directory, "pkg.json");

                Package discoveredPkg;

                // get the package name and the installed version
                if (File.Exists(headerPath))
                {
                    discoveredPkg = Package.FromJson(headerPath, AsLogger());
                    if (discoveredPkg == null)
                        throw new Exception(String.Format(Properties.Resources.MalformedHeaderPackage, headerPath));
                }
                else
                    throw new Exception(String.Format(Properties.Resources.NoHeaderPackage, headerPath));

                // prevent duplicates
                if (LocalPackages.All(pkg => pkg.Name != discoveredPkg.Name))
                {
                    LocalPackages.Add(discoveredPkg);
                    return discoveredPkg; // success
                }
                throw new Exception(String.Format(Properties.Resources.DulicatedPackage, discoveredPkg.Name, discoveredPkg.RootDirectory));
            }
            catch (Exception e)
            {
                Log(String.Format(Properties.Resources.ExceptionEncountered, this.RootPackagesDirectory), WarningLevel.Error);
                Log(e);
            }

            return null;
        }

        /// <summary>
        ///     Attempt to load a managed assembly in to ReflectionOnlyLoadFrom context. 
        /// </summary>
        /// <param name="filename">The filename of a DLL</param>
        /// <param name="assem">out Assembly - the passed value does not matter and will only be set if loading succeeds</param>
        /// <returns>Returns true if success, false if BadImageFormatException (i.e. not a managed assembly)</returns>
        internal static bool TryReflectionOnlyLoadFrom(string filename, out Assembly assem)
        {
            try
            {
                assem = Assembly.ReflectionOnlyLoadFrom(filename);
                return true;
            }
            catch (BadImageFormatException)
            {
                assem = null;
                return false;
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
            return LocalPackages.Any(package => package.LoadedAssemblies.Any(x => x.Assembly == t ));
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
