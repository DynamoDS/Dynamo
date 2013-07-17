using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using Dynamo.Utilities;
using Greg.Requests;
using RestSharp;


namespace Dynamo.PackageManager
{
    public class PackageLoader
    {

        public static string DefaultRelativePackagesDirectory = "dynamo_packages";
        public string RootPackagesDirectory { get; private set; }

        public PackageLoader() : this( Path.Combine (DynamoLoader.GetDynamoDirectory(), DefaultRelativePackagesDirectory) )
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

        private ObservableCollection<LocalPackage> _localPackages = new ObservableCollection<LocalPackage>();
        public ObservableCollection<LocalPackage> LocalPackages { get { return _localPackages; } }

        /// <summary>
        ///     Scan the PackagesDirectory for packages and attempt to load all of them.  Beware! Fails silently for duplicates.
        /// </summary>
        internal void LoadPackages()
        {
            this.ScanAllPackageDirectories();
            LocalPackages.ToList().ForEach( (pkg) => pkg.Load() );
        }

        private void ScanAllPackageDirectories()
        {
            LocalPackages.Clear();
            Directory.EnumerateDirectories(RootPackagesDirectory).ToList().ForEach(ScanPackageDirectory);
        }

        public void ScanPackageDirectory(string directory)
        {
            try
            {
                var headerPath = Path.Combine(directory, "pkg.json");
                var pkgName = Path.GetFileName(directory);

                LocalPackage discoveredPkg = null;

                // get the package name and the installed version
                if (File.Exists(headerPath))
                {
                    discoveredPkg = LocalPackage.FromJson(headerPath);
                }
                else
                {
                    throw new Exception(headerPath + " contains a package without a header.  Ignoring it.");
                }

                if (LocalPackages.All(pkg => pkg.Name != discoveredPkg.Name))
                {
                    LocalPackages.Add(discoveredPkg);
                }
                else
                {
                    // todo: tell user that dup pkg was found
                    throw new Exception("A duplicate of the package called " + discoveredPkg.Name +
                                              " was found at " + discoveredPkg.RootDirectory + ".  Ignoring it.");
                }
            }
            catch (Exception e)
            {
                DynamoLogger.Instance.Log("Exception encountered scanning the package directory at " + this.RootPackagesDirectory );
                DynamoLogger.Instance.Log(e.GetType() + ": " + e.Message);
            }
            
        }

        public bool IsUnderPackageControl(string path)
        {
            return LocalPackages.Any(ele => ele.ContainsFile(path));
        }

        public bool IsUnderPackageControl(FunctionDefinition def)
        {
            return IsUnderPackageControl(def.Workspace.FilePath);
        }

        public bool IsUnderPackageControl(Type t)
        {
            return LocalPackages.Any(package => package.LoadedTypes.Contains(t));
        }

        public LocalPackage GetLocalPackage(string path)
        {
            return LocalPackages.FirstOrDefault(pkg => pkg.RootDirectory == path);
        }

        internal void DoCachedPackageUninstalls()
        {
            // scan dynSettings for cached packages to unload
            // unload them
            // throw new NotImplementedException();
        }
    }
}
