using CefSharp;
using Dynamo.DynamoPackagesUI.ViewModels;
using Dynamo.Models;
using Dynamo.PackageManager;
using Dynamo.UI;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Wpf.Properties;
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
        public PackageManagerCommands(DynamoViewModel dynamoViewModel, PackageLoader model, PackageManagerViewModel pkgManagerViewModel) : 
            base(dynamoViewModel, model, pkgManagerViewModel)
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
                    _downloadRequest = JsonConvert.DeserializeObject<dynamic>(value);
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
            var firstOrDefault = Model.LocalPackages.FirstOrDefault(pkg => pkg.ID == DownloadRequest.asset_id.ToString());
            if (firstOrDefault != null)
            {
                var dynModel = dynamoViewModel.Model;
                try
                {
                    firstOrDefault.UninstallCore(dynModel.CustomNodeManager, Model, dynModel.PreferenceSettings);
                }
                catch
                {
                    MessageBox.Show(String.Format(Resources.MessageFailToUninstallPackage,
                        dynamoViewModel.BrandingResourceProvider.ProductName,
                        DownloadRequest.asset_name.ToString()),
                        Resources.UninstallFailureMessageBoxTitle,
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            var settings = dynamoViewModel.Model.PreferenceSettings;
            PackageDownloadHandle packageDownloadHandle = new PackageDownloadHandle();
            packageDownloadHandle.Name = DownloadRequest.asset_name;
            packageDownloadHandle.Done(downloadPath);

            //string installedPkgPath = string.Empty;
            Package dynPkg = null;
            if (packageDownloadHandle.Extract(dynamoViewModel.Model, this.PackageInstallPath, out dynPkg))
            {
                var p = Package.FromDirectory(dynPkg.RootDirectory, dynamoViewModel.Model.Logger);
                p.ID = DownloadRequest.asset_id;
                Application.Current.Dispatcher.Invoke((Action)(() =>
                {
                    Model.Load(p);
                }));
                packageDownloadHandle.DownloadState = PackageDownloadHandle.State.Installed;
                this.PackageInstallPath = string.Empty;
            }
        }

        /// <summary>
        /// Download and Install Dynamo Package
        /// </summary>
        /// <param name="pkg"></param>
        public void DownloadAndInstall(string pkg)
        {
            string[] temp = pkg.Split(',');
            PackageManagerRequest req = new PackageManagerRequest(string.Format("assets/{0}", temp[0]), Method.GET);
            ResponseWithContentBody<dynamic> response = Client.ExecuteAndDeserializeDynamoRequest(req);
            DownloadRequest = response.content;

            PackageManagerRequest fileReq = new PackageManagerRequest(string.Format("files/download?file_ids={0}&asset_id={1}", temp[1], temp[0]), Method.GET, true);
            Response res = Client.ExecuteDynamoRequest(fileReq);
            var pathToPackage = Client.GetFileFromResponse(res);
            InstallPackage(pathToPackage);
        }

        public string GetCustomPathForInstall()
        {
            string downloadPath = string.Empty;

            downloadPath = GetDownloadPath();

            if (String.IsNullOrEmpty(downloadPath))
                return string.Empty;

            PackageInstallPath = downloadPath;

            return downloadPath;
        }

        private string GetDownloadPath()
        {
            var args = new PackagePathEventArgs();

            ShowFileDialog(args);

            if (args.Cancel)
                return string.Empty;

            return args.Path;
        }

        private void ShowFileDialog(PackagePathEventArgs e)
        {
            string initialPath = dynamoViewModel.Model.PathManager.DefaultPackagesDirectory;

            e.Cancel = true;

            var dialog = new DynamoFolderBrowserDialog
            {
                // Navigate to initial folder.
                SelectedPath = initialPath,
                Title = "Install Path",
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

        /// <summary>
        /// Install button click event
        /// </summary>
        /// <param name="asset"></param>
        /// <param name="version"></param>
        /// <returns></returns>
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

                PackagesToInstall.Add(string.Format("{0},{1},{2}", asset["asset_id"], version["file_id"], asset["asset_name"]));

                //    // determine if any of the packages contain binaries or python scripts.  
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

                    if (res == MessageBoxResult.Cancel) return "cancel";
                }

                // Determine if there are any dependencies that are made with a newer version
                // of Dynamo (this includes the root package)
                var dynamoVersion = dynamoViewModel.Model.Version;
                var dynamoVersionParsed = VersionUtilities.PartialParse(dynamoVersion, 3);
                var futureDeps = FilterFuturePackages(packageVersionData, dynamoVersionParsed);

                // If any of the required packages use a newer version of Dynamo, show a dialog to the user
                // allowing them to cancel the package download
                if (futureDeps.Any())
                {
                    var versionList = FormatPackageVersionList(futureDeps.ToList());

                    if (MessageBox.Show(String.Format(Resources.MessagePackageNewerDynamo,
                        dynamoViewModel.BrandingResourceProvider.ProductName,
                        versionList),

                        string.Format(Resources.PackageUseNewerDynamoMessageBoxTitle,
                        dynamoViewModel.BrandingResourceProvider.ProductName),
                        MessageBoxButton.OKCancel,
                        MessageBoxImage.Warning) == MessageBoxResult.Cancel)
                    {
                        return "cancel";
                    }
                }

                var localPkgs = Model.LocalPackages;

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

                    if (localPkg.InUse(dynamoViewModel.Model))
                    {
                        uninstallRequiringUserModifications.Add(localPkg);
                        continue;
                    }

                    immediateUninstalls.Add(localPkg);
                }

                if (uninstallRequiringUserModifications.Any())
                {
                    MessageBox.Show(String.Format(Resources.MessageUninstallToContinue,
                        dynamoViewModel.BrandingResourceProvider.ProductName,
                        JoinPackageNames(uninstallRequiringUserModifications)),
                        Resources.CannotDownloadPackageMessageBoxTitle,
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return "cancel";
                }

                var settings = dynamoViewModel.Model.PreferenceSettings;

                if (uninstallsRequiringRestart.Any())
                {
                    // mark for uninstallation
                    uninstallsRequiringRestart.ForEach(
                        x => x.MarkForUninstall(settings));

                    MessageBox.Show(String.Format(Resources.MessageUninstallToContinue2,
                        dynamoViewModel.BrandingResourceProvider.ProductName,
                        JoinPackageNames(uninstallsRequiringRestart)),
                        Resources.CannotDownloadPackageMessageBoxTitle,
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return "cancel";
                }

                if (immediateUninstalls.Any())
                {
                    // if the package is not in use, tell the user we will be uninstall it and give them the opportunity to cancel
                    if (MessageBox.Show(String.Format(Resources.MessageAlreadyInstallDynamo,
                        dynamoViewModel.BrandingResourceProvider.ProductName,
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

        private string JoinPackageNames(IEnumerable<Package> pkgs)
        {

            return String.Join(", ", pkgs.Select(x => x.Name));
        }

        public static string FormatPackageVersionList(List<Tuple<dynamic, dynamic>> packages)
        {
            return String.Join("\r\n", packages.Select(x => string.Format("{0} {1}", x.Item1.name.ToString(), x.Item2.version.ToString())));
        }

        public IEnumerable<Tuple<dynamic, dynamic>> FilterFuturePackages(List<Tuple<dynamic, dynamic>> pkgVersions, Version currentAppVersion, int numberOfFieldsToCompare = 3)
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
            Package localPkg = Model.LocalPackages.Where(a => a.Name == this.PkgRequest.asset_name.ToString()).First();

            if (localPkg.LoadedAssemblies.Any())
            {
                var resAssem =
                    MessageBox.Show(string.Format(Resources.MessageNeedToRestart,
                        dynamoViewModel.BrandingResourceProvider.ProductName),
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
                var dynModel = dynamoViewModel.Model;
                Application.Current.Dispatcher.Invoke((Action)(() =>
                {
                    localPkg.UninstallCore(dynModel.CustomNodeManager, Model, dynModel.PreferenceSettings);
                }));

                return true;
            }
            catch (Exception)
            {
                MessageBox.Show(string.Format(Resources.MessageFailedToUninstall,
                    dynamoViewModel.BrandingResourceProvider.ProductName),
                    Resources.UninstallFailureMessageBoxTitle,
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }


        public void GoToRootDirectory()
        {
            Package localPkg = Model.LocalPackages.Where(a => a.Name == this.PkgRequest.asset_name.ToString()).First();
            Process.Start(localPkg.RootDirectory);
        }

        public void UnmarkForUninstallation()
        {
            Package pkg = Model.LocalPackages.Where(a => a.Name == this.PkgRequest.asset_name.ToString()).First();
            if (pkg != null)
            {
                pkg.UnmarkForUninstall(dynamoViewModel.Model.PreferenceSettings);
            }
        }

        public void PublishNewPackageVersion()
        {
            Package pkg = Model.LocalPackages.Where(a => a.Name == this.PkgRequest.asset_name.ToString()).First();
            pkg.RefreshCustomNodesFromDirectory(dynamoViewModel.Model.CustomNodeManager, DynamoModel.IsTestMode);
            //var vm = PublishCommands.FromLocalPackage(dynamoViewModel, pkg, ViewMdodel);
            //vm.PublishCompCefHelper.IsNewVersion = true;
            //ViewMdodel.PublishCompCefHelper = vm.PublishCompCefHelper;
            //dynamoViewModel.OnRequestPackagePublishDialog(vm);

        }

        public void PublishLocalPackage()
        {
            Package pkg = Model.LocalPackages.Where(a => a.Name == this.PkgRequest.asset_name.ToString()).First();
            pkg.RefreshCustomNodesFromDirectory(dynamoViewModel.Model.CustomNodeManager, DynamoModel.IsTestMode);
            //var vm = PublishCommands.FromLocalPackage(dynamoViewModel, pkg, ViewMdodel);
            //vm.PublishCompCefHelper.IsNewVersion = false;
            //vm.PublishCompCefHelper.PublishLocal = true;
            //ViewMdodel.PublishCompCefHelper = vm.PublishCompCefHelper;
            //dynamoViewModel.OnRequestPackagePublishDialog(vm);

        }

    }
}
