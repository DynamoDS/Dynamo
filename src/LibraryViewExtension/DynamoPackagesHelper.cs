using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamo.Models;
using Dynamo.PackageManager;
using Greg.Responses;

namespace Dynamo.LibraryUI
{
    class DynamoPackagesHelper
    {
        private PackageManagerClient dynamopackagesClient;
        private IEventController controller;
        private PackageLoader packageLoader;
        private DynamoModel dynamoModel;

        public DynamoPackagesHelper(IEventController controller, DynamoModel dynamoModel)
        {
            this.controller = controller;
            this.dynamoModel = dynamoModel;
            packageLoader = dynamoModel.GetPackageManagerExtension().PackageLoader;
            dynamopackagesClient = dynamoModel.GetPackageManagerExtension().PackageManagerClient;
        }

        public string GetInstalledPackagesJSON()
        {
            var localPackages = packageLoader.LocalPackages;
            StringBuilder pkgStr = new StringBuilder();

            pkgStr.Append("{ \"success\": \"true\",");
            pkgStr.Append("\"message\": \"Found packages\",");
            pkgStr.Append("\"content\": [");

            foreach (var pkg in localPackages)
            {
                pkgStr.Append("{");
                pkgStr.Append("\"_id\": \"");
                pkgStr.Append(pkg.ID);
                pkgStr.Append("\",");
                pkgStr.Append("\"name\": \"");
                pkgStr.Append(pkg.Name);
                pkgStr.Append("\",");
                pkgStr.Append("\"version\": \"");
                pkgStr.Append(pkg.VersionName);
                pkgStr.Append("\"},");
            }
            if (localPackages.Any())
            {
                pkgStr.Remove(pkgStr.Length - 1, 1); // Remove the last comma
            }
            pkgStr.Append("] }");
            return pkgStr.ToString();
        }

        public string GetInstalledPackageVersion(string packageName)
        {
            var pkg = packageLoader.LocalPackages.FirstOrDefault(p => string.Compare(p.Name, packageName, true) == 0);
            if (pkg == null) return string.Empty;

            return pkg.VersionName;
        }

        public void DownlodAndInstall(string packageId, string packageName, string version, string installPath)
        {
            if (string.IsNullOrEmpty(packageId) || string.IsNullOrEmpty(packageName))
            {
                controller.RaiseEvent("error",
                    string.Format("Invalid package info 'Package Id: {0} Name: {1} Version: {2}'", packageId, packageName, version));
                return;
            }
            //Notify install progress
            controller.RaiseEvent("installPercentComplete", packageId, 0.001);

            var installedVersion = GetInstalledPackageVersion(packageName);
            if(!string.IsNullOrEmpty(installedVersion))
            {
                controller.RaiseEvent("error", 
                    string.Format("The package '{0} ({1})' is already installed, please uninstall it before installing version: {2}", 
                    packageName, installedVersion, version));
                return;
            }

            string path;
            var result = dynamopackagesClient.DownloadPackage(packageId, version, out path);
            if (result.Success)
            {
                controller.RaiseEvent("installPercentComplete", packageId, 0.5);
            }
            else
            {
                controller.RaiseEvent("error",
                    string.Format("Failed to download package: {0} ({1})", packageName, version));
                return;
            }

            PackageDownloadHandle packageDownloadHandle = new PackageDownloadHandle();

            packageDownloadHandle.Name = packageName;
            if (string.IsNullOrEmpty(installPath))
            {
                installPath = dynamoModel.PathManager.DefaultPackagesDirectory;
            }

            packageDownloadHandle.Done(path);

            //string installedPkgPath = string.Empty;
            Package dynPkg = null;
            if (packageDownloadHandle.Extract(dynamoModel, installPath, out dynPkg))
            {
                packageLoader.Load(dynPkg);

                packageDownloadHandle.DownloadState = PackageDownloadHandle.State.Installed;
                controller.RaiseEvent("installPercentComplete", packageId, 1.0);
            }
        }

        public void UninstallPackage(string packageName)
        {
            var pkg = packageLoader.LocalPackages.FirstOrDefault(p => string.Compare(p.Name, packageName, false) == 0);
            if (pkg == null)
            {
                controller.RaiseEvent("error",
                    string.Format("The package '{0}' is not installed yet, hence cannot be uninstalled!", packageName));
            }

            pkg.UninstallCore(dynamoModel.CustomNodeManager, packageLoader, dynamoModel.PreferenceSettings);
        }
    }
}
