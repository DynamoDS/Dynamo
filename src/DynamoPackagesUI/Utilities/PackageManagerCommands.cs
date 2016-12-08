﻿using CefSharp;
using Dynamo.DynamoPackagesUI.ViewModels;
using Dynamo.Models;
using Dynamo.PackageManager;
using Dynamo.UI;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Wpf.Interfaces;
using Dynamo.Wpf.Properties;
using DynamoUtilities;
using Greg.Requests;
using Greg.Responses;
using Greg.Utility;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Dynamo.DynamoPackagesUI.Utilities
{
    /// <summary>
    /// CEF calss to assist exploring packages, authors and logged in user packages.
    /// </summary>
    internal class PackageManagerCommands : CefCommands
    {
        public PackageManagerCommands(PackageLoader loader, DynamoModel model) : 
            base(loader, model)
        {
        }

        public List<string> PackagesToInstall { get; set; }


        private dynamic _downloadRequest;
        public dynamic DownloadRequest
        {
            get { return _downloadRequest; }
            set
            {
                if (value is Newtonsoft.Json.Linq.JObject)
                    _downloadRequest = value;
                else
                    _downloadRequest = JsonConvert.DeserializeObject<dynamic>(value.ToString());
            }
        }

        private dynamic _pkgRequest;
        public dynamic PkgRequest
        {
            get { return _pkgRequest; }
            set
            {
                if (value is Newtonsoft.Json.Linq.JObject)
                    _pkgRequest = value;
                else
                    _pkgRequest = JsonConvert.DeserializeObject<dynamic>(value);
            }
        }

        private string PackageInstallPath { get; set; }

        /// <summary>
        /// Install Dynamo Package
        /// </summary>
        /// <param name="downloadPath"></param>
        private void InstallPackage(string downloadPath)
        {
            var firstOrDefault = Loader.LocalPackages.FirstOrDefault(pkg => pkg.ID == DownloadRequest.asset_id.ToString());
            if (firstOrDefault != null)
            {
                var dynModel = Model;
                try
                {
                    firstOrDefault.UninstallCore(dynModel.CustomNodeManager, Loader, dynModel.PreferenceSettings);
                }
                catch
                {
                    MessageBox.Show(String.Format(Resources.MessageFailToUninstallPackage,
                        ProductName,
                        DownloadRequest.asset_name.ToString()),
                        Resources.UninstallFailureMessageBoxTitle,
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            var settings = Model.PreferenceSettings;
            PackageDownloadHandle packageDownloadHandle = new PackageDownloadHandle();
            packageDownloadHandle.Name = DownloadRequest.asset_name;
            packageDownloadHandle.Done(downloadPath);

            //string installedPkgPath = string.Empty;
            Package dynPkg = null;
            if (packageDownloadHandle.Extract(Model, this.PackageInstallPath, out dynPkg))
            {
                var p = Package.FromDirectory(dynPkg.RootDirectory, Model.Logger);
                p.ID = DownloadRequest.asset_id;
                Application.Current.Dispatcher.Invoke((Action)(() =>
                {
                    Loader.Load(p);
                }));
                packageDownloadHandle.DownloadState = PackageDownloadHandle.State.Installed;
                this.PackageInstallPath = string.Empty;
            }
        }

        /// <summary>
        /// Download and Install Dynamo Package
        /// pkg input parameter contains comma delimitted value as "assetId,fileID"
        /// </summary>
        /// <param name="pkg"></param>
        public void DownloadAndInstall(string package)
        {
            //Get Asset Details
            string[] packageToInstall = package.Split(',');
            PackageManagerRequest req = new PackageManagerRequest(string.Format("assets/{0}", packageToInstall[0]), Method.GET);
            ResponseWithContentBody<dynamic> response = Client.ExecuteAndDeserializeDynamoRequest(req);
            DownloadRequest = response.content;

            //Donwload the file
            PackageManagerRequest fileReq = new PackageManagerRequest(string.Format("files/download?file_ids={0}&asset_id={1}", packageToInstall[1], packageToInstall[0]), Method.GET, true);
            Response res = Client.ExecuteDynamoRequest(fileReq);
            var pathToPackage = Client.GetFileFromResponse(res);
            InstallPackage(pathToPackage);
        }

        /// <summary>
        /// Get the folder path to install the package
        /// </summary>
        /// <returns></returns>
        /// TODO: Change the method name
        public string GetCustomPathForInstall()
        {
            string downloadPath = string.Empty;

            downloadPath = RequestGetDownloadPath();

            if (String.IsNullOrEmpty(downloadPath))
                return string.Empty;

            PackageInstallPath = downloadPath;

            return downloadPath;
        }

        private string RequestGetDownloadPath()
        {
            var args = new PackagePathEventArgs();

            PromptFileSelectionDialog(args);

            if (args.Cancel)
                return string.Empty;

            return args.Path;
        }

        private void PromptFileSelectionDialog(PackagePathEventArgs e)
        {
            string initialPath = Model.PathManager.DefaultPackagesDirectory;

            e.Cancel = true;

            var errorCannotCreateFolder = PathHelper.CreateFolderIfNotExist(initialPath);
            if (errorCannotCreateFolder == null)
            {
                var dialog = new DynamoFolderBrowserDialog
                {
                    // Navigate to initial folder.
                    SelectedPath = initialPath,
                    Owner = this.ParentWindow
                };
                Application.Current.Dispatcher.Invoke((Action)(() =>
                {
                    if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        e.Cancel = false;
                        e.Path = dialog.SelectedPath;
                    }
                }));
            }
            else
            {
                string errorMessage = string.Format(Wpf.Properties.Resources.PackageFolderNotAccessible, initialPath);
                System.Windows.Forms.MessageBox.Show(errorMessage, Wpf.Properties.Resources.UnableToAccessPackageDirectory, System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Install button click event
        /// </summary>
        /// <param name="asset"></param>
        /// <param name="version"></param>
        /// <returns>If "cencel" returned to webclient halt the installation. If not cancel then this method returns the | delimitted list of package and its version to be installed</returns>
        /// TODO: Rename this method to OnInstallPackage
        public string PackageOnExecuted(dynamic asset, dynamic version)
        {
            string downloadPath = this.PackageInstallPath;

            List<Tuple<dynamic, dynamic>> packageVersionData = new List<Tuple<dynamic, dynamic>>();

            string msg = String.IsNullOrEmpty(downloadPath) ?
                String.Format(Resources.MessageConfirmToInstallPackage, asset["asset_name"], version["version"]) :
                String.Format(Resources.MessageConfirmToInstallPackageToFolder, asset["asset_name"], version["version"], downloadPath);

            var result = MessageBox.Show(msg,
                Resources.PackageDownloadConfirmMessageBoxTitle,
                MessageBoxButton.OKCancel, MessageBoxImage.Question);

            if (PackagesToInstall == null)
                PackagesToInstall = new List<string>();
            else
                PackagesToInstall.Clear();

            if (result == MessageBoxResult.OK)
            {
                if (!string.IsNullOrEmpty(version["dependencies"]))
                {
                    GetDependencies(version, out packageVersionData);
                }

                //Add selected package to the list of packges to install
                PackagesToInstall.Add(string.Format("{0},{1},{2}", asset["asset_id"], version["file_id"], asset["asset_name"]));

                // determine if any of the packages contain binaries or python scripts.  
                result = CheckForBinariesPythonScripts(version, packageVersionData);
                if (result == MessageBoxResult.Cancel)
                    return "cancel";

                result = CheckForNewerDynamoVersion(packageVersionData);
                if (result == MessageBoxResult.Cancel)
                    return "cancel";


                var localPkgs = Loader.LocalPackages;

                var uninstallsRequiringRestart = new List<Package>();
                var uninstallRequiringUserModifications = new List<Package>();
                var immediateUninstalls = new List<Package>();

                // if a package is already installed we need to uninstall it, allowing
                // the user to cancel if they do not want to uninstall the package
                foreach (var localPkg in packageVersionData.Select(x => localPkgs.FirstOrDefault(v => v.Name == x.Item1.asset_name.ToString())))
                {
                    if (localPkg == null) continue;

                    if (localPkg.LoadedAssemblies.Any())
                    {
                        uninstallsRequiringRestart.Add(localPkg);
                        continue;
                    }

                    if (localPkg.InUse(Model))
                    {
                        uninstallRequiringUserModifications.Add(localPkg);
                        continue;
                    }

                    immediateUninstalls.Add(localPkg);
                }

                if (uninstallRequiringUserModifications.Any())
                {
                    MessageBox.Show(String.Format(Resources.MessageUninstallToContinue,
                        ProductName,
                        JoinPackageNames(uninstallRequiringUserModifications)),
                        Resources.CannotDownloadPackageMessageBoxTitle,
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return "cancel";
                }

                var settings = Model.PreferenceSettings;

                if (uninstallsRequiringRestart.Any())
                {
                    // mark for uninstallation
                    uninstallsRequiringRestart.ForEach(
                        x => x.MarkForUninstall(settings));

                    MessageBox.Show(String.Format(Resources.MessageUninstallToContinue2,
                        ProductName,
                        JoinPackageNames(uninstallsRequiringRestart)),
                        Resources.CannotDownloadPackageMessageBoxTitle,
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return "cancel";
                }

                if (immediateUninstalls.Any())
                {
                    // if the package is not in use, tell the user we will be uninstall it and give them the opportunity to cancel
                    if (MessageBox.Show(String.Format(Resources.MessageAlreadyInstallDynamo,
                        ProductName,
                        JoinPackageNames(immediateUninstalls)),
                        Resources.DownloadWarningMessageBoxTitle,
                        MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.Cancel)
                        return "cancel";
                }

                // add custom path to custom package folder list
                if (!String.IsNullOrEmpty(downloadPath))
                {
                    if (!settings.CustomPackageFolders.Contains(downloadPath))
                        settings.CustomPackageFolders.Add(downloadPath);
                }
            }
            else
            {
                return "cancel";
            }
            return String.Join("|", PackagesToInstall);
        }
        
        private void GetDependencies(dynamic version, out List<Tuple<dynamic, dynamic>> packageVersionData)
        {
            packageVersionData = new List<Tuple<dynamic, dynamic>>();
            // get all of the headers
            PackageManagerRequest req;
            string[] depends = version["dependencies"].Split(',');
            foreach (string depend in depends)
            {
                string[] temp = depend.Split('|');
                req = new PackageManagerRequest((string.Format("assets/{0}/customdata", temp[0])).Trim(), Method.GET);
                ResponseWithContentBody<dynamic> response = Client.ExecuteAndDeserializeDynamoRequest(req);
                var customData = response.content;

                req = new PackageManagerRequest(("assets/" + temp[0]).Trim(), Method.GET);
                response = Client.ExecuteAndDeserializeDynamoRequest(req);
                var depAsset = response.content;

                if (customData.custom_data.Count > 0)
                {
                    dynamic versionData;
                    string json;
                    for (int i = 0; i < customData.custom_data.Count; i++)
                    {
                        if (customData.custom_data[i].key == string.Format("version:{0}", temp[1]))
                        {
                            json = System.Uri.UnescapeDataString(customData.custom_data[i].data.ToString());
                            versionData = JsonConvert.DeserializeObject<dynamic>(json);

                            packageVersionData.Add(new Tuple<dynamic, dynamic>(depAsset, versionData));
                            PackagesToInstall.Add(string.Format("{0},{1},{2}", temp[0], versionData.file_id.Value, depAsset.asset_name));
                        }
                    }
                }
            }
        } 

        private MessageBoxResult CheckForBinariesPythonScripts(dynamic version, List<Tuple<dynamic,dynamic>> packageVersionData)
        {
            var containsBinaries =
                    packageVersionData.Any(
                        x => x.Item2.contents.ToString().Contains(Dynamo.PackageManager.PackageManagerClient.PackageContainsBinariesConstant) || (bool)x.Item2.contains_binaries);

            containsBinaries = containsBinaries || (version["contents"].ToString().Contains(Dynamo.PackageManager.PackageManagerClient.PackageContainsBinariesConstant) || (bool)version["contains_binaries"]);

            var containsPythonScripts =
                packageVersionData.Any(
                    x => x.Item2.contents.ToString().Contains(Dynamo.PackageManager.PackageManagerClient.PackageContainsPythonScriptsConstant));

            containsPythonScripts = containsPythonScripts || (version["contents"].ToString().Contains(Dynamo.PackageManager.PackageManagerClient.PackageContainsPythonScriptsConstant));

            // if any do, notify user and allow cancellation
            if (containsBinaries || containsPythonScripts)
            {
                var res = MessageBox.Show(Resources.MessagePackageContainPythonScript,
                    Resources.PackageDownloadMessageBoxTitle,
                    MessageBoxButton.OKCancel, MessageBoxImage.Exclamation);

                return res;
            }

            return MessageBoxResult.OK;
        }

        private MessageBoxResult CheckForNewerDynamoVersion(List<Tuple<dynamic, dynamic>> packageVersionData)
        {
            // Determine if there are any dependencies that are made with a newer version
            // of Dynamo (this includes the root package)
            var dynamoVersion = Model.Version;
            var dynamoVersionParsed = VersionUtilities.PartialParse(dynamoVersion, 3);
            var futureDeps = FilterFuturePackages(packageVersionData, dynamoVersionParsed);

            // If any of the required packages use a newer version of Dynamo, show a dialog to the user
            // allowing them to cancel the package download
            if (futureDeps.Any())
            {
                var versionList = FormatPackageVersionList(futureDeps.ToList());

                if (MessageBox.Show(String.Format(Resources.MessagePackageNewerDynamo,
                    ProductName,
                    versionList),

                    string.Format(Resources.PackageUseNewerDynamoMessageBoxTitle,
                    ProductName),
                    MessageBoxButton.OKCancel,
                    MessageBoxImage.Warning) == MessageBoxResult.Cancel)
                {
                    return MessageBoxResult.Cancel;
                }
            }
            return MessageBoxResult.OK;
        }

        private string JoinPackageNames(IEnumerable<Package> pkgs)
        {

            return String.Join(", ", pkgs.Select(x => x.Name));
        }

        private string FormatPackageVersionList(List<Tuple<dynamic, dynamic>> packages)
        {
            return String.Join("\r\n", packages.Select(x => string.Format("{0} {1}", x.Item1.name.ToString(), x.Item2.version.ToString())));
        }

        private IEnumerable<Tuple<dynamic, dynamic>> FilterFuturePackages(List<Tuple<dynamic, dynamic>> pkgVersions, Version currentAppVersion, int numberOfFieldsToCompare = 3)
        {
            foreach (Tuple<dynamic, dynamic> version in pkgVersions)
            {
                var depAppVersion = VersionUtilities.PartialParse(version.Item2.engine_version.ToString(), numberOfFieldsToCompare);

                if (depAppVersion > currentAppVersion)
                {
                    yield return version;
                }
            }
        }

        /// <summary>
        /// Uninstall Dynamo Package
        /// </summary>
        /// <returns></returns>
        public bool Uninstall()
        {
            Package localPkg = Loader.LocalPackages.Where(a => a.Name == this.PkgRequest.asset_name.ToString()).First();

            if (localPkg.LoadedAssemblies.Any())
            {
                var resAssem =
                    MessageBox.Show(string.Format(Resources.MessageNeedToRestart,
                        ProductName),
                        Resources.UninstallingPackageMessageBoxTitle,
                        MessageBoxButton.OKCancel,
                        MessageBoxImage.Exclamation);
                if (resAssem == MessageBoxResult.Cancel) return false;
            }

            var res = MessageBox.Show(String.Format(Resources.MessageConfirmToUninstallPackage, localPkg.Name),
                                      Resources.UninstallingPackageMessageBoxTitle,
                                      MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (res == MessageBoxResult.No)
            {
                return false;
            }

            try
            {
                var dynModel = Model;
                Application.Current.Dispatcher.Invoke((Action)(() =>
                {
                    localPkg.UninstallCore(dynModel.CustomNodeManager, Loader, dynModel.PreferenceSettings);
                }));

                return true;
            }
            catch (Exception)
            {
                MessageBox.Show(string.Format(Resources.MessageFailedToUninstall,
                    ProductName),
                    Resources.UninstallFailureMessageBoxTitle,
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        /// <summary>
        /// Open the root directory for PkgRequest
        /// </summary>
        public void GoToRootDirectory()
        {
            Package localPkg = Loader.LocalPackages.Where(a => a.Name == this.PkgRequest.asset_name.ToString()).First();
            Process.Start(localPkg.RootDirectory);
        }

        /// <summary>
        /// Unmark Uninstall for PkgRequest 
        /// </summary>
        public void UnmarkForUninstallation()
        {
            Package pkg = Loader.LocalPackages.Where(a => a.Name == this.PkgRequest.asset_name.ToString()).First();
            if (pkg != null)
            {
                pkg.UnmarkForUninstall(Model.PreferenceSettings);
            }
        }
    }
}
