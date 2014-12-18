using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using Dynamo.DSEngine;
using Dynamo.Interfaces;
using Dynamo.Utilities;
using DynamoUtilities;

namespace Dynamo.PackageManager
{
    public class PackageLoader
    {
        public string RootPackagesDirectory { get; private set; }

        private readonly ILogger logger;
        private readonly DynamoLoader loader;

        public PackageLoader(DynamoLoader dynamoLoader, ILogger logger)
            : this(dynamoLoader, logger, Path.Combine(DynamoPathManager.Instance.MainExecPath, DynamoPathManager.Instance.Packages))
        {
        }

        public PackageLoader(DynamoLoader dynamoLoader, ILogger logger, string overridePackageDirectory)
        {
            this.loader = dynamoLoader;
            this.logger = logger;

            this.RootPackagesDirectory = overridePackageDirectory;
            if (!Directory.Exists(this.RootPackagesDirectory))
            {
                Directory.CreateDirectory(this.RootPackagesDirectory);
            }
        }

        private ObservableCollection<Package> _localPackages = new ObservableCollection<Package>();
        public ObservableCollection<Package> LocalPackages { get { return _localPackages; } }

        /// <summary>
        ///     Scan the PackagesDirectory for packages and attempt to load all of them.  Beware! Fails silently for duplicates.
        /// </summary>
        public void LoadPackagesIntoDynamo( IPreferences preferences, LibraryServices libraryServices )
        {
            this.ScanAllPackageDirectories( preferences );

            foreach (var pkg in LocalPackages)
            {
                DynamoPathManager.Instance.AddResolutionPath(pkg.BinaryDirectory);
            }

            foreach (var pkg in LocalPackages)
            {
                pkg.LoadIntoDynamo(loader, logger, libraryServices);
            }
        }

        private void ScanAllPackageDirectories(IPreferences preferences)
        { 
            foreach (var dir in 
                Directory.EnumerateDirectories(RootPackagesDirectory, "*", SearchOption.TopDirectoryOnly))
            {
                var pkg = ScanPackageDirectory(dir);
                if (preferences.PackageDirectoriesToUninstall.Contains(dir)) pkg.MarkForUninstall(preferences);
            }
        }

        public Package ScanPackageDirectory(string directory)
        {
            try
            {
                var headerPath = Path.Combine(directory, /*NXLT*/"pkg.json");

                Package discoveredPkg = null;

                // get the package name and the installed version
                if (File.Exists(headerPath))
                {
                    discoveredPkg = Package.FromJson(headerPath, this.logger);
                    if (discoveredPkg == null)
                        throw new Exception(headerPath + /*NXLT*/" contains a package with a malformed header.  Ignoring it.");
                }
                else
                {
                    throw new Exception(headerPath + /*NXLT*/" contains a package without a header.  Ignoring it.");
                }

                // prevent duplicates
                if (LocalPackages.All(pkg => pkg.Name != discoveredPkg.Name))
                {
                    LocalPackages.Add(discoveredPkg);
                    return discoveredPkg; // success
                }
                else
                {
                    throw new Exception(/*NXLT*/"A duplicate of the package called " + discoveredPkg.Name +
                        /*NXLT*/" was found at " + discoveredPkg.RootDirectory + /*NXLT*/".  Ignoring it.");
                }
            }
            catch (Exception e)
            {
                this.logger.Log(/*NXLT*/"Exception encountered scanning the package directory at " + this.RootPackagesDirectory);
                this.logger.Log(e.GetType() + ": " + e.Message);
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

        public bool IsUnderPackageControl(CustomNodeDefinition def)
        {
            return IsUnderPackageControl(def.WorkspaceModel.FileName);
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

        public Package GetOwnerPackage(CustomNodeDefinition def)
        {
            return GetOwnerPackage(def.WorkspaceModel.FileName);
        }

        public Package GetOwnerPackage(string path)
        {
            return LocalPackages.FirstOrDefault(ele => ele.ContainsFile(path));
        }

        private static bool hasAttemptedUninstall = false;

        internal void DoCachedPackageUninstalls( IPreferences preferences )
        {
            // this can only be run once per app run
            if (hasAttemptedUninstall) return;
            hasAttemptedUninstall = true;

            var pkgDirsRemoved = new List<string>();
            foreach (var pkgNameDirTup in preferences.PackageDirectoriesToUninstall)
            {
                try
                {
                    Directory.Delete(pkgNameDirTup, true);
                    pkgDirsRemoved.Add(pkgNameDirTup);
                    this.logger.Log(String.Format(/*NXLT*/"Successfully uninstalled package from \"{0}\"", pkgNameDirTup));
                }
                catch
                {
                    this.logger.LogWarning(
                        String.Format(/*NXLT*/"Failed to delete package directory at \"{0}\", you may need to delete the directory manually.", 
                        pkgNameDirTup), WarningLevel.Moderate);
                }
            }
            
            preferences.PackageDirectoriesToUninstall.RemoveAll(pkgDirsRemoved.Contains);
        }
    }
}
