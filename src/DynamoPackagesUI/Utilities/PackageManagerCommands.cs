using ACGClientForCEF;
using ACGClientForCEF.Requests;
using ACGClientForCEF.Utility;
using CefSharp;
using Dynamo.DynamoPackagesUI.ViewModels;
using Dynamo.Models;
using Dynamo.PackageManager;
using Dynamo.UI;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Wpf.Properties;
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

        internal void GoToWebsite()
        {
            dynamoViewModel.PackageManagerClientViewModel.GoToWebsite();
        }

        private void InstallPackage(string downloadPath)
        {
            var firstOrDefault = Model.LocalPackages.FirstOrDefault(pkg => pkg.ID == DownloadRequest.asset_id.ToString());
            if (firstOrDefault != null)
            {
                var dynModel = dynamoViewModel.Model;
                try
                {
                    firstOrDefault.UninstallCore(dynModel.CustomNodeManager, dynamoViewModel.Model.GetPackageManagerExtension().PackageLoader, dynModel.PreferenceSettings);
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

            string installedPkgPath = string.Empty;
            if (packageDownloadHandle.Extract(dynamoViewModel.Model, this.PackageInstallPath, out installedPkgPath))
            {
                var p = Package.FromDirectory(installedPkgPath, dynamoViewModel.Model.Logger);
                p.ID = DownloadRequest.asset_id;
                Application.Current.Dispatcher.Invoke((Action)(() =>
                {
                    dynamoViewModel.Model.GetPackageManagerExtension().PackageLoader.Load(p);
                }));
                packageDownloadHandle.DownloadState = PackageDownloadHandle.State.Installed;
                this.PackageInstallPath = string.Empty;
            }
        }

        public void DownloadAndInstall(string pkg)
        {
            string[] temp = pkg.Split(',');
            DynamoRequest req = new DynamoRequest("assets/" + temp[0], Method.GET);
            CefResponseWithContentBody response = DPClient.ExecuteAndDeserializeDynamoCefRequest(req);
            DownloadRequest = response.content;

            DynamoRequest fileReq = new DynamoRequest(@"files/download?file_ids=" + temp[1] + "&asset_id=" + temp[0], Method.GET, true);
            CefResponse res = DPClient.ExecuteDynamoCefRequest(fileReq);
            var pathToPackage = GetFileFromResponse(res.InternalRestReponse);
            InstallPackage(pathToPackage);
        }

        public string GetFileFromResponse(IRestResponse gregResponse)
        {
            var response = gregResponse;

            if (!(response.ResponseUri != null && response.ResponseUri.AbsolutePath != null)) return "";

            var tempOutput = System.IO.Path.Combine(FileUtilities.GetTempFolder(), System.IO.Path.GetFileName(response.ResponseUri.AbsolutePath));
            using (var f = new FileStream(tempOutput, FileMode.Create))
            {
                f.Write(response.RawBytes, 0, (int)response.ContentLength);
            }
            //TODO: Jitendra verify if this needed
            //var md5HeaderResp = response.Headers.FirstOrDefault(x => x.Name == "ETag");
            //if (md5HeaderResp == null) throw new Exception("Could not check integrity of package download!");

            //var md5HeaderComputed =
            //    String.Join("", FileUtilities.GetMD5Checksum(tempOutput).Select(x => x.ToString("X"))).ToLower();

            //if (md5HeaderResp.Value.ToString() == md5HeaderComputed )
            //    throw new Exception("Could not validate package integrity!  Please try again!");

            return tempOutput;

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

            var pmExt = dynamoViewModel.Model.GetPackageManagerExtension();

            if (PackagesToInstall == null)
                PackagesToInstall = new List<string>();
            else
                PackagesToInstall.Clear();

            if (result == MessageBoxResult.OK)
            {
                if (!string.IsNullOrEmpty(version["dependencies"]))
                {
                    // get all of the headers
                    DynamoRequest req;
                    string[] depends = version["dependencies"].Split(',');
                    foreach (string depend in depends)
                    {
                        string[] temp = depend.Split('|');
                        req = new DynamoRequest(("assets/" + temp[0] + "/customdata").Trim(), Method.GET);
                        CefResponseWithContentBody response = DPClient.ExecuteAndDeserializeDynamoCefRequest(req);
                        var customData = response.content;

                        req = new DynamoRequest(("assets/" + temp[0]).Trim(), Method.GET);
                        response = DPClient.ExecuteAndDeserializeDynamoCefRequest(req);
                        var depAsset = response.content;

                        if (customData.custom_data.Count > 0)
                        {
                            dynamic versionData;
                            string json;
                            for (int i = 0; i < customData.custom_data.Count; i++)
                            {
                                if (customData.custom_data[i].key == "version:" + temp[1])
                                {
                                    json = System.Uri.UnescapeDataString(customData.custom_data[i].data.ToString());
                                    versionData = JsonConvert.DeserializeObject<dynamic>(json);

                                    packageVersionData.Add(new Tuple<dynamic, dynamic>(depAsset, versionData));
                                    PackagesToInstall.Add(temp[0] + "," + versionData.file_id.Value + "," + depAsset.asset_name);
                                }
                            }
                        }
                    }
                }

                PackagesToInstall.Add(asset["asset_id"] + "," + version["file_id"] + "," + asset["asset_name"]);

                //    // determine if any of the packages contain binaries or python scripts.  
                var containsBinaries =
                    packageVersionData.Any(
                        x => x.Item2.contents.ToString().Contains(PackageManagerClient.PackageContainsBinariesConstant) || (bool)x.Item2.contains_binaries);

                containsBinaries = containsBinaries || (version["contents"].ToString().Contains(PackageManagerClient.PackageContainsBinariesConstant) || (bool)version["contains_binaries"]);

                var containsPythonScripts =
                    packageVersionData.Any(
                        x => x.Item2.contents.ToString().Contains(PackageManagerClient.PackageContainsPythonScriptsConstant));

                containsPythonScripts = containsPythonScripts || (version["contents"].ToString().Contains(PackageManagerClient.PackageContainsPythonScriptsConstant));

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

                var localPkgs = pmExt.PackageLoader.LocalPackages;

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
            return String.Join("\r\n", packages.Select(x => x.Item1.name.ToString() + " " + x.Item2.version.ToString()));
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
                var pmExtension = dynModel.GetPackageManagerExtension();
                Application.Current.Dispatcher.Invoke((Action)(() =>
                {
                    localPkg.UninstallCore(dynModel.CustomNodeManager, pmExtension.PackageLoader, dynModel.PreferenceSettings);
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
            //var vm = PublishCommands.FromLocalPackage(dynamoViewModel, pkg, PackageMgrViewMdodel);
            //vm.PublishCompCefHelper.IsNewVersion = true;
            //PackageMgrViewMdodel.PublishCompCefHelper = vm.PublishCompCefHelper;
            //dynamoViewModel.OnRequestPackagePublishDialog(vm);

        }

        public void PublishLocalPackage()
        {
            Package pkg = Model.LocalPackages.Where(a => a.Name == this.PkgRequest.asset_name.ToString()).First();
            pkg.RefreshCustomNodesFromDirectory(dynamoViewModel.Model.CustomNodeManager, DynamoModel.IsTestMode);
            //var vm = PublishCommands.FromLocalPackage(dynamoViewModel, pkg, PackageMgrViewMdodel);
            //vm.PublishCompCefHelper.IsNewVersion = false;
            //vm.PublishCompCefHelper.PublishLocal = true;
            //PackageMgrViewMdodel.PublishCompCefHelper = vm.PublishCompCefHelper;
            //dynamoViewModel.OnRequestPackagePublishDialog(vm);

        }

    }
}
