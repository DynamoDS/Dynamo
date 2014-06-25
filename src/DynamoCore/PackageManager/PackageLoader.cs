using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

using Dynamo.Utilities;
using DynamoUtilities;


namespace Dynamo.PackageManager
{
    public class PackageLoader
    {
        public string RootPackagesDirectory { get; private set; }

        public PackageLoader() : this( Path.Combine (DynamoLoader.GetDynamoDirectory(), DynamoPathManager.Instance.Packages) )
        {
        }

        public PackageLoader(string overridePackageDirectory)
        {
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
        public void LoadPackages()
        {
            this.ScanAllPackageDirectories();
            LocalPackages.ToList().ForEach( (pkg) => pkg.Load() );
        }

        private List<Package> ScanAllPackageDirectories()
        {
            return
                Directory.EnumerateDirectories(RootPackagesDirectory, "*", SearchOption.TopDirectoryOnly)
                         .Select(ScanPackageDirectory)
                         .ToList();
        }

        public Package ScanPackageDirectory(string directory)
        {
            try
            {
                var headerPath = Path.Combine(directory, "pkg.json");

                Package discoveredPkg = null;

                // get the package name and the installed version
                if (File.Exists(headerPath))
                {
                    discoveredPkg = Package.FromJson(headerPath);
                    if (discoveredPkg == null)
                        throw new Exception(headerPath + " contains a package with a malformed header.  Ignoring it.");
                }
                else
                {
                    throw new Exception(headerPath + " contains a package without a header.  Ignoring it.");
                }

                // prevent duplicates
                if (LocalPackages.All(pkg => pkg.Name != discoveredPkg.Name))
                {
                    LocalPackages.Add(discoveredPkg);
                    return discoveredPkg; // success
                }
                else
                {
                    throw new Exception("A duplicate of the package called " + discoveredPkg.Name +
                                              " was found at " + discoveredPkg.RootDirectory + ".  Ignoring it.");
                }
            }
            catch (Exception e)
            {
                dynSettings.DynamoLogger.Log("Exception encountered scanning the package directory at " + this.RootPackagesDirectory );
                dynSettings.DynamoLogger.Log(e.GetType() + ": " + e.Message);
            }

            return null;

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

        internal void DoCachedPackageUninstalls()
        {
            // scan dynSettings for cached packages to unload
            // unload them
            // throw new NotImplementedException();
        }
    }
}
