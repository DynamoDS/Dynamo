using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Dynamo.Utilities;


namespace Dynamo.PackageManager
{
    public class PackageLoader
    {

        public static string DefaultPackagesDirectory = "dynamo_packages";

        public string PackagesDirectory { get; set; }

        public string AbsolutePackagesDirectory { get { return Path.Combine(DynamoLoader.GetDynamoDirectory(), PackagesDirectory); }}

        public PackageLoader()
        {
            this.PackagesDirectory = DefaultPackagesDirectory;
            if (!Directory.Exists(this.PackagesDirectory))
            {
                Directory.CreateDirectory(this.PackagesDirectory);
            }
        }

        public PackageLoader(string packagesDirectory)
        {
            this.PackagesDirectory = packagesDirectory;
            if (!Directory.Exists(this.PackagesDirectory))
            {
                Directory.CreateDirectory(this.PackagesDirectory);
            }
        }

        private ObservableDictionary<string, DynamoInstalledPackage> _installedPackageNames = new ObservableDictionary<string, DynamoInstalledPackage>();
        public ObservableDictionary<string, DynamoInstalledPackage> InstalledPackageNames { get { return _installedPackageNames;  } }

        internal void AppendCustomNodeSearchPaths(CustomNodeLoader customNodeLoader)
        {
            UpdateInstalledPackages();

            foreach (var ele in InstalledPackageNames)
            {
                var dyfPath = Path.Combine(ele.Value.Directory, "dyf");
                if ( Directory.Exists(dyfPath))
                    customNodeLoader.AddDirectoryToSearchPath(dyfPath);
            }
        }

        internal void AppendBinarySearchPath()
        {
            UpdateInstalledPackages();

            foreach (var ele in InstalledPackageNames)
            {
                var binPath = Path.Combine(ele.Value.Directory, "bin");
                if (Directory.Exists(binPath))
                    DynamoLoader.AddBinarySearchPath(binPath);
            }
        }

        private void UpdateInstalledPackages()
        {
            InstalledPackageNames.Clear();

            // enumerate directories in absolute packages directory
            foreach (var dir in Directory.EnumerateDirectories(AbsolutePackagesDirectory))
            {
                var headerPath = Path.Combine(dir, "package_header.json");
                var pkgName = Path.GetFileName(dir);

                DynamoInstalledPackage installedPkg = null;

                // get the package name and the installed version
                if (File.Exists(headerPath))
                {
                    // deserialize pkg header 
                    // contains version, more info
                }
                else // no pkg header, we don't know anything about this package
                {
                    installedPkg = new DynamoInstalledPackage(dir, pkgName, "unknown");
                }

                if (InstalledPackageNames.ContainsKey(pkgName))
                {
                    InstalledPackageNames[pkgName] = installedPkg;
                }
                else
                {
                    InstalledPackageNames.Add(pkgName, installedPkg);
                }
            }
        }

        internal void UninstallPackages()
        {
            
        }

    }
}
