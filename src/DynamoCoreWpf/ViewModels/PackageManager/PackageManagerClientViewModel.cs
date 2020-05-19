using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Dynamo.Core;
using Dynamo.Graph.Nodes.CustomNodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using Dynamo.PackageManager;
using Dynamo.PackageManager.UI;
using Dynamo.Selection;
using Dynamo.Utilities;
using Dynamo.Wpf.Interfaces;
using Dynamo.Wpf.Properties;
using Greg.AuthProviders;
using Greg.Responses;
using Microsoft.Practices.Prism.Commands;

namespace Dynamo.ViewModels
{
    public class TermsOfUseHelperParams
    {
        internal IBrandingResourceProvider ResourceProvider { get; set; }
        internal PackageManagerClient PackageManagerClient { get; set; }
        internal AuthenticationManager AuthenticationManager { get; set; }
        internal Action AcceptanceCallback { get; set; }
    }

    /// <summary>
    /// A helper class to check asynchronously whether the Terms of Use has 
    /// been accepted, and if so, continue to execute the provided Action.
    /// </summary>
    public class TermsOfUseHelper
    {
        private static int lockCount = 0;
        private readonly IBrandingResourceProvider resourceProvider;
        private readonly Action callbackAction;
        private readonly PackageManagerClient packageManagerClient;
        private readonly AuthenticationManager authenticationManager;

        public TermsOfUseHelper(TermsOfUseHelperParams touParams)
        {
            if (touParams == null)
                throw new ArgumentNullException("touParams");
            if (touParams.PackageManagerClient == null)
                throw new ArgumentNullException("PackageManagerClient");
            if (touParams.AuthenticationManager == null)
                throw new ArgumentNullException("AuthenticationManager");
            if (touParams.AcceptanceCallback == null)
                throw new ArgumentNullException("AcceptanceCallback");
            if (touParams.ResourceProvider == null)
                throw new ArgumentNullException("ResourceProvider");

            resourceProvider = touParams.ResourceProvider;
            packageManagerClient = touParams.PackageManagerClient;
            callbackAction = touParams.AcceptanceCallback;
            authenticationManager = touParams.AuthenticationManager;
        }

        [Obsolete("Please use the other overridden method")]
        public void Execute()
        {
            Execute(true); // Redirected to call the alternative method.
        }

        internal void Execute(bool preventReentrant)
        {
            if (preventReentrant && Interlocked.Increment(ref lockCount) > 1)
            {
                // Determine if there is already a previous request to display
                // terms of use dialog, if so, do nothing for the second time.
                Interlocked.Decrement(ref lockCount);
                return;
            }

            Task<bool>.Factory.StartNew(() => packageManagerClient.GetTermsOfUseAcceptanceStatus()).
                ContinueWith(t =>
                {
                    // The above GetTermsOfUseAcceptanceStatus call will get the
                    // user to sign-in. If the user cancels that sign-in dialog 
                    // without signing in, we won't show the terms of use dialog,
                    // simply return from here.
                    // 
                    if (authenticationManager.LoginState != LoginState.LoggedIn)
                        return;

                    var termsOfUseAccepted = t.Result;
                    if (termsOfUseAccepted)
                    {
                        // Terms of use accepted, proceed to publish.
                        callbackAction.Invoke();
                    }
                    else
                    {
                        // Prompt user to accept the terms of use.
                        ShowTermsOfUseForPublishing();
                    }

                }, TaskScheduler.FromCurrentSynchronizationContext()).
                ContinueWith(t =>
                {
                    // Done with terms of use dialog, decrement counter.
                    Interlocked.Decrement(ref lockCount);

                }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        internal static bool ShowTermsOfUseDialog(bool forPublishing, string additionalTerms)
        {
            var executingAssemblyPathName = Assembly.GetExecutingAssembly().Location;
            var rootModuleDirectory = Path.GetDirectoryName(executingAssemblyPathName);
            var touFilePath = Path.Combine(rootModuleDirectory, "TermsOfUse.rtf");

            var termsOfUseView = new TermsOfUseView(touFilePath);
            termsOfUseView.ShowDialog();
            if (!termsOfUseView.AcceptedTermsOfUse)
                return false; // User rejected the terms, go no further.

            if (string.IsNullOrEmpty(additionalTerms)) // No additional terms.
                return termsOfUseView.AcceptedTermsOfUse;

            // If user has accepted the terms, and if there is an additional 
            // terms specified, then that should be shown, too. Note that if 
            // the file path is provided, it has to represent a valid file path.
            // 
            if (!File.Exists(additionalTerms))
                throw new FileNotFoundException(additionalTerms);

            var additionalTermsView = new TermsOfUseView(additionalTerms);
            additionalTermsView.ShowDialog();
            return additionalTermsView.AcceptedTermsOfUse;
        }

        private void ShowTermsOfUseForPublishing()
        {
            var additionalTerms = string.Empty;
            if (resourceProvider != null)
                additionalTerms = resourceProvider.AdditionalPackagePublisherTermsOfUse;

            if (!ShowTermsOfUseDialog(true, additionalTerms))
                return; // Terms of use not accepted.

            // If user accepts the terms of use, then update the record on 
            // the server, before proceeding to show the publishing dialog. 
            // This method is invoked on the UI thread, so when the server call 
            // returns, invoke the publish dialog on the UI thread's context.
            // 
            Task<bool>.Factory.StartNew(() => packageManagerClient.SetTermsOfUseAcceptanceStatus()).
                ContinueWith(t =>
                {
                    if (t.Result)
                        callbackAction.Invoke();
                },
                TaskScheduler.FromCurrentSynchronizationContext());
        }
    }

    /// <summary>
    ///     A thin wrapper on the Greg rest client for performing IO with
    ///     the Package Manager
    /// </summary>
    public class PackageManagerClientViewModel : NotificationObject, IPackageInstaller
    {

        #region Properties/Fields

        ObservableCollection<PackageUploadHandle> _uploads = new ObservableCollection<PackageUploadHandle>();
        public ObservableCollection<PackageUploadHandle> Uploads
        {
            get { return _uploads; }
            set { _uploads = value; }
        }

        ObservableCollection<PackageDownloadHandle> _downloads = new ObservableCollection<PackageDownloadHandle>();
        public ObservableCollection<PackageDownloadHandle> Downloads
        {
            get { return _downloads; }
            set { _downloads = value; }
        }

        private PackageManagerExtension pmExtension;

        internal PackageManagerExtension PackageManagerExtension
        {
            get { return pmExtension ?? (pmExtension = DynamoViewModel.Model.GetPackageManagerExtension()); }
        }

        public List<PackageManagerSearchElement> CachedPackageList { get; private set; }

        public readonly DynamoViewModel DynamoViewModel;
        public AuthenticationManager AuthenticationManager { get; set; }
        internal PackageManagerClient Model { get; private set; }

        public LoginState LoginState
        {
            get
            {
                return AuthenticationManager.LoginState;
            }
        }

        public string Username
        {
            get
            {
                return AuthenticationManager.Username;
            }
        }

        #endregion

        public ICommand ToggleLoginStateCommand { get; private set; }

        internal PackageManagerClientViewModel(DynamoViewModel dynamoViewModel, PackageManagerClient model )
        {
            this.DynamoViewModel = dynamoViewModel;
            this.AuthenticationManager = dynamoViewModel.Model.AuthenticationManager;
            Model = model;
            CachedPackageList = new List<PackageManagerSearchElement>();

            this.ToggleLoginStateCommand = new DelegateCommand(ToggleLoginState, CanToggleLoginState);

            AuthenticationManager.LoginStateChanged += (loginState) =>
            {
                RaisePropertyChanged("LoginState");
                RaisePropertyChanged("Username");
            };

        }

        private void ToggleLoginState()
        {
            if (AuthenticationManager.LoginState == LoginState.LoggedIn)
            {
                AuthenticationManager.Logout();
            }
            else
            {
                AuthenticationManager.Login();
            }
        }

        private bool CanToggleLoginState()
        {
            return AuthenticationManager.LoginState == LoginState.LoggedOut || AuthenticationManager.LoginState == LoginState.LoggedIn;
        }

        public void PublishCurrentWorkspace(object m)
        {
            var ws = (CustomNodeWorkspaceModel)DynamoViewModel.CurrentSpace;

            CustomNodeDefinition currentFunDef;
            if (DynamoViewModel.Model.CustomNodeManager.TryGetFunctionDefinition(
                ws.CustomNodeId,
                DynamoModel.IsTestMode,
                out currentFunDef))
            {
                CustomNodeInfo currentFunInfo;
                if (DynamoViewModel.Model.CustomNodeManager.TryGetNodeInfo(
                    ws.CustomNodeId,
                    out currentFunInfo))
                {
                    var touParams = new TermsOfUseHelperParams
                    {
                        PackageManagerClient = Model,
                        AuthenticationManager = DynamoViewModel.Model.AuthenticationManager,
                        ResourceProvider = DynamoViewModel.BrandingResourceProvider,
                        AcceptanceCallback = () => ShowNodePublishInfo(new[]
                        {
                            Tuple.Create(currentFunInfo, currentFunDef)
                        })
                    };

                    var termsOfUseCheck = new TermsOfUseHelper(touParams);
                    termsOfUseCheck.Execute();
                    return;
                }
            }
            
            MessageBox.Show(Resources.MessageSelectSymbolNotFound, 
                    Resources.SelectionErrorMessageBoxTitle,
                    MessageBoxButton.OK, MessageBoxImage.Question);
        }

        public bool CanPublishCurrentWorkspace(object m)
        {
            return DynamoViewModel.Model.CurrentWorkspace is CustomNodeWorkspaceModel && AuthenticationManager.HasAuthProvider;
        }

        public void PublishNewPackage(object m)
        {
            var termsOfUseCheck = new TermsOfUseHelper(new TermsOfUseHelperParams
            {
                PackageManagerClient = Model,
                AuthenticationManager = AuthenticationManager,
                ResourceProvider = DynamoViewModel.BrandingResourceProvider,
                AcceptanceCallback = ShowNodePublishInfo
            });

            termsOfUseCheck.Execute();
        }

        public bool CanPublishNewPackage(object m)
        {
            return AuthenticationManager.HasAuthProvider;
        }

        public void PublishCustomNode(Function m)
        {
            CustomNodeInfo currentFunInfo;
            if (DynamoViewModel.Model.CustomNodeManager.TryGetNodeInfo(
                m.Definition.FunctionId,
                out currentFunInfo))
            {
                var termsOfUseCheck = new TermsOfUseHelper(new TermsOfUseHelperParams
                {
                    PackageManagerClient = Model,
                    AuthenticationManager = AuthenticationManager,
                    ResourceProvider = DynamoViewModel.BrandingResourceProvider,
                    AcceptanceCallback = () => ShowNodePublishInfo(new[]
                    {
                        Tuple.Create(currentFunInfo, m.Definition)
                    })
                });

                termsOfUseCheck.Execute();
            }
        }

        public bool CanPublishCustomNode(Function m)
        {
            return AuthenticationManager.HasAuthProvider && m != null;
        }

        public void PublishSelectedNodes(object m)
        {
            var nodeList = DynamoSelection.Instance.Selection
                                .Where(x => x is Function)
                                .Cast<Function>()
                                .Select(x => x.Definition.FunctionId)
                                .ToList();

            if (!nodeList.Any())
            {
                MessageBox.Show(Resources.MessageSelectAtLeastOneNode,
                   Resources.SelectionErrorMessageBoxTitle,
                   MessageBoxButton.OK, MessageBoxImage.Question);
                return;
            }

            var manager = DynamoViewModel.Model.CustomNodeManager;

            var defs = new List<Tuple<CustomNodeInfo, CustomNodeDefinition>>();
            foreach (var node in nodeList)
            {
                CustomNodeInfo info;
                if (manager.TryGetNodeInfo(node, out info))
                {
                    CustomNodeDefinition def;
                    if (manager.TryGetFunctionDefinition(node, DynamoModel.IsTestMode, out def))
                    {
                        defs.Add(Tuple.Create(info, def));
                        continue;
                    }
                }
                MessageBox.Show(Resources.MessageGettingNodeError, 
                    Resources.SelectionErrorMessageBoxTitle, 
                    MessageBoxButton.OK, MessageBoxImage.Question);
            }

            var termsOfUseCheck = new TermsOfUseHelper(new TermsOfUseHelperParams
            {
                PackageManagerClient = Model,
                AuthenticationManager = AuthenticationManager,
                ResourceProvider = DynamoViewModel.BrandingResourceProvider,
                AcceptanceCallback = () => ShowNodePublishInfo(defs)
            });

            termsOfUseCheck.Execute();
        }

        public bool CanPublishSelectedNodes(object m)
        {
            return DynamoSelection.Instance.Selection.Count > 0 &&
                   DynamoSelection.Instance.Selection.All(x => x is Function) && AuthenticationManager.HasAuthProvider; ;
        }

        private void ShowNodePublishInfo()
        {
            var newPkgVm = new PublishPackageViewModel(DynamoViewModel);
            DynamoViewModel.OnRequestPackagePublishDialog(newPkgVm);
        }

        private void ShowNodePublishInfo(ICollection<Tuple<CustomNodeInfo, CustomNodeDefinition>> funcDefs)
        {
            foreach (var f in funcDefs)
            {
                var pkg = PackageManagerExtension.PackageLoader.GetOwnerPackage(f.Item1);

                if (pkg != null)
                {
                    var m = MessageBox.Show(String.Format(Resources.MessageSubmitSameNamePackage, 
                            DynamoViewModel.BrandingResourceProvider.ProductName,pkg.Name),
                            Resources.PackageWarningMessageBoxTitle, 
                            MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (m == MessageBoxResult.Yes)
                    {
                        var pkgVm = new PackageViewModel(DynamoViewModel, pkg);
                        pkgVm.PublishNewPackageVersionCommand.Execute();
                        return;
                    }
                }
            }

            var newPkgVm = new PublishPackageViewModel(DynamoViewModel) { CustomNodeDefinitions = funcDefs.Select(pair => pair.Item2).ToList() };

            DynamoViewModel.OnRequestPackagePublishDialog(newPkgVm);
        }

        public List<PackageManagerSearchElement> ListAll()
        {
            CachedPackageList = new List<PackageManagerSearchElement>();

            foreach (var header in Model.ListAll())
            {
                var ele = new PackageManagerSearchElement(header);

                ele.UpvoteRequested += this.Model.Upvote;
                CachedPackageList.Add( ele );
            }

            return CachedPackageList;
        }

        /// <summary>
        /// Download and install a specific package from the package manager
        /// </summary>
        /// <param name="packageInfo"></param>
        /// <param name="downloadPath"></param>
        public void DownloadAndInstallPackage(IPackageInfo packageInfo, string downloadPath = null)
        {
            // User needs to accept terms of use before any packages can be downloaded from package manager
            var prefSettings = DynamoViewModel.Model.PreferenceSettings;
            var touAccepted = prefSettings.PackageDownloadTouAccepted;
            if (!touAccepted)
            {
                touAccepted = TermsOfUseHelper.ShowTermsOfUseDialog(false, null);
                prefSettings.PackageDownloadTouAccepted = touAccepted;
                if (!touAccepted)
                {
                    return;
                }
            }

            // Try to get the package version for this package
            PackageVersion version;
            try
            {
                version = Model.GetPackageVersionHeader(packageInfo);
            }
            catch
            {
                MessageBox.Show(
                    string.Format(Resources.MessagePackageVersionNotFound, packageInfo.Version.ToString(), packageInfo.Name),
                    Resources.PackageDownloadErrorMessageBoxTitle,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }
            
            ExecutePackageDownload(packageInfo.Name, version, downloadPath);
        }

        private string JoinPackageNames(IEnumerable<Package> pkgs)
        {
            return String.Join(", ", pkgs.Select(x => x.Name + " " + x.VersionName));
        }

        internal void ExecutePackageDownload(string name, PackageVersion version, string downloadPath)
        {
            string msg = String.IsNullOrEmpty(downloadPath) ?
                String.Format(Resources.MessageConfirmToInstallPackage, name, version.version) :
                String.Format(Resources.MessageConfirmToInstallPackageToFolder, name, version.version, downloadPath);

            var result = MessageBox.Show(msg,
                Resources.PackageDownloadConfirmMessageBoxTitle,
                MessageBoxButton.OKCancel, MessageBoxImage.Question);

            var pmExt = DynamoViewModel.Model.GetPackageManagerExtension();
            if (result == MessageBoxResult.OK)
            {
                // get all of the dependency version headers
                var dependencyVersionHeaders = version.full_dependency_ids.Select((dep, i) =>
                {
                    try
                    {
                        var depVersion = version.full_dependency_versions[i];
                        var res = Model.GetPackageVersionHeader(dep._id, depVersion);
                        return res;
                    }
                    catch
                    {
                        MessageBox.Show(
                            String.Format(Resources.MessageFailedToDownloadPackageVersion, dep._id),
                            Resources.PackageDownloadErrorMessageBoxTitle,
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return null;
                    }
                }).ToList();

                // if any header download fails, abort
                if (dependencyVersionHeaders.Any(x => x == null))
                {
                    return;
                }

                // determine if any of the packages contain binaries or python scripts.  
                var containsBinariesOrPythonScripts = dependencyVersionHeaders.Any(x =>
                {
                    // The contents (string) property of the PackageVersion object can be null for an empty package 
                    // like LunchBox.
                    var are_contents_empty = string.IsNullOrEmpty(x.contents);
                    var contains_binaries = x.contains_binaries ||
                                            !are_contents_empty && x.contents.Contains(PackageManagerClient.PackageContainsBinariesConstant);
                    var contains_python =
                        !are_contents_empty && x.contents.Contains(PackageManagerClient.PackageContainsPythonScriptsConstant);
                    return contains_binaries || contains_python;

                });

                // if any do, notify user and allow cancellation
                if (containsBinariesOrPythonScripts)
                {
                    var res = MessageBox.Show(Resources.MessagePackageContainPythonScript,
                        Resources.PackageDownloadMessageBoxTitle,
                        MessageBoxButton.OKCancel, MessageBoxImage.Exclamation);

                    if (res == MessageBoxResult.Cancel) return;
                }

                // Determine if there are any dependencies that are made with a newer version
                // of Dynamo (this includes the root package)
                var dynamoVersion = VersionUtilities.PartialParse(DynamoViewModel.Model.Version);
                var futureDeps = dependencyVersionHeaders.Where(dep => VersionUtilities.PartialParse(dep.engine_version) > dynamoVersion);

                // If any of the required packages use a newer version of Dynamo, show a dialog to the user
                // allowing them to cancel the package download
                if (futureDeps.Any())
                {
                    if (MessageBox.Show(string.Format(Resources.MessagePackageNewerDynamo, DynamoViewModel.BrandingResourceProvider.ProductName),
                        string.Format(Resources.PackageUseNewerDynamoMessageBoxTitle, DynamoViewModel.BrandingResourceProvider.ProductName),
                        MessageBoxButton.OKCancel,
                        MessageBoxImage.Warning) == MessageBoxResult.Cancel)
                    {
                        return;
                    }
                }

                var localPkgs = pmExt.PackageLoader.LocalPackages;

                var uninstallsRequiringRestart = new List<Package>();
                var uninstallRequiringUserModifications = new List<Package>();
                var immediateUninstalls = new List<Package>();

                // if a package is already installed we need to uninstall it, allowing
                // the user to cancel if they do not want to uninstall the package
                foreach (var localPkg in version.full_dependency_ids.Select(dep => localPkgs.FirstOrDefault(v => v.ID == dep._id)))
                {
                    if (localPkg == null) continue;

                    if (localPkg.LoadedAssemblies.Any())
                    {
                        uninstallsRequiringRestart.Add(localPkg);
                        continue;
                    }

                    if (localPkg.InUse(DynamoViewModel.Model))
                    {
                        uninstallRequiringUserModifications.Add(localPkg);
                        continue;
                    }

                    immediateUninstalls.Add(localPkg);
                }

                if (uninstallRequiringUserModifications.Any())
                {
                    MessageBox.Show(String.Format(Resources.MessageUninstallToContinue,
                        DynamoViewModel.BrandingResourceProvider.ProductName,
                        JoinPackageNames(uninstallRequiringUserModifications)),
                        Resources.CannotDownloadPackageMessageBoxTitle,
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var settings = DynamoViewModel.Model.PreferenceSettings;

                if (uninstallsRequiringRestart.Any())
                {

                    var message = string.Format(Resources.MessageUninstallToContinue2,
                        DynamoViewModel.BrandingResourceProvider.ProductName,
                        JoinPackageNames(uninstallsRequiringRestart),
                        name + " " + version.version);
                    // different message for the case that the user is
                    // trying to install the same package/version they already have installed.
                    if (uninstallsRequiringRestart.Count == 1 &&
                        uninstallsRequiringRestart.First().Name == name &&
                        uninstallsRequiringRestart.First().VersionName == version.version)
                    {
                        message = String.Format(Resources.MessageUninstallSamePackage, name + " " + version.version);
                    }
                    var dialogResult = MessageBox.Show(message,
                        Resources.CannotDownloadPackageMessageBoxTitle,
                        MessageBoxButton.YesNo, MessageBoxImage.Error);

                    if (dialogResult == MessageBoxResult.Yes)
                    {
                        // mark for uninstallation
                        uninstallsRequiringRestart.ForEach(x => x.MarkForUninstall(settings));
                    }
                    return;
                }

                if (immediateUninstalls.Any())
                {
                    // if the package is not in use, tell the user we will be uninstall it and give them the opportunity to cancel
                    if (MessageBox.Show(String.Format(Resources.MessageAlreadyInstallDynamo,
                        DynamoViewModel.BrandingResourceProvider.ProductName,
                        JoinPackageNames(immediateUninstalls)),
                        Resources.DownloadWarningMessageBoxTitle,
                        MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.Cancel)
                        return;
                }

                // add custom path to custom package folder list
                if (!String.IsNullOrEmpty(downloadPath))
                {
                    if (!settings.CustomPackageFolders.Contains(downloadPath))
                        settings.CustomPackageFolders.Add(downloadPath);
                }

                // form header version pairs and download and install all packages
                dependencyVersionHeaders
                    .Select((dep, i) => {
                        // Note that Name will be empty when calling from DownloadAndInstallPackage.
                        // This is currently not an issue, as the code path belongs to the Workspace Dependency
                        // extension, which does not display dependencies names.
                        var dependencyPackageHeader = version.full_dependency_ids[i];
                        return new PackageDownloadHandle()
                        {
                            Id = dependencyPackageHeader._id,
                            VersionName = dep.version,
                            Name = dependencyPackageHeader.name
                        };
                    })
                    .ToList()
                    .ForEach(x => DownloadAndInstall(x, downloadPath));
            }
        }

        /// <summary>
        ///     Returns a newline delimited string representing the package name and version of the argument
        /// </summary>
        [Obsolete("No longer used. Remove in 3.0.")]
        public static string FormatPackageVersionList(IEnumerable<Tuple<PackageHeader, PackageVersion>> packages)
        {
            return String.Join("\r\n", packages.Select(x => x.Item1.name + " " + x.Item2.version));
        }

        /// <summary>
        /// This method downloads the package represented by the PackageDownloadHandle,
        /// uninstalls its current installation if necessary, and installs the package.
        /// 
        /// Note that, if the package is already installed, it must be uninstallable
        /// </summary>
        /// <param name="packageDownloadHandle">package download handle</param>
        /// <param name="downloadPath">package download path</param>
        internal void DownloadAndInstall(PackageDownloadHandle packageDownloadHandle, string downloadPath)
        {
            Downloads.Add(packageDownloadHandle);

            packageDownloadHandle.DownloadState = PackageDownloadHandle.State.Downloading;

            Task.Factory.StartNew(() =>
            {
                // Attempt to download package
                string pathDl;
                var res = Model.DownloadPackage(packageDownloadHandle.Id, packageDownloadHandle.VersionName, out pathDl);

                // if you fail, update download handle and return
                if (!res.Success)
                {
                    packageDownloadHandle.Error(res.Error);
                    return;
                }

                // if success, proceed to install the package
                DynamoViewModel.UIDispatcher.BeginInvoke((Action)(() =>
                {
                    try
                    {
                        packageDownloadHandle.Done(pathDl);
                        var pmExtension = DynamoViewModel.Model.GetPackageManagerExtension();
                        var firstOrDefault = pmExtension.PackageLoader.LocalPackages.FirstOrDefault(pkg => pkg.ID == packageDownloadHandle.Id);
                        if (firstOrDefault != null)
                        {
                            var dynModel = DynamoViewModel.Model;
                            try
                            {
                                firstOrDefault.UninstallCore(dynModel.CustomNodeManager, pmExtension.PackageLoader, dynModel.PreferenceSettings);
                            }
                            catch
                            {
                                MessageBox.Show(String.Format(Resources.MessageFailToUninstallPackage, 
                                    DynamoViewModel.BrandingResourceProvider.ProductName,
                                    packageDownloadHandle.Name),
                                    Resources.UninstallFailureMessageBoxTitle, 
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                        SetPackageState(packageDownloadHandle, downloadPath);
                    }
                    catch (Exception e)
                    {
                        packageDownloadHandle.Error(e.Message);
                    }
                }));
            });
        }

        /// <summary>
        /// Check Dynamo package install state
        /// </summary>
        /// <param name="packageDownloadHandle">package download handle</param>
        /// <param name="downloadPath">package download path</param>
        internal void SetPackageState(PackageDownloadHandle packageDownloadHandle, string downloadPath)
        {
            Package dynPkg;
            if (packageDownloadHandle.Extract(DynamoViewModel.Model, downloadPath, out dynPkg))
            {
                pmExtension.PackageLoader.LoadPackages(new List<Package> { dynPkg });
                packageDownloadHandle.DownloadState = PackageDownloadHandle.State.Installed;
            }
            else
            {
                packageDownloadHandle.DownloadState = PackageDownloadHandle.State.Error;
                packageDownloadHandle.Error(Resources.MessageInvalidPackage);
            }
        }

        public void ClearCompletedDownloads()
        {
            Downloads.Where((x) => x.DownloadState == PackageDownloadHandle.State.Installed ||
                x.DownloadState == PackageDownloadHandle.State.Error).ToList().ForEach(x => Downloads.Remove(x));
        }

        public void GoToWebsite()
        {
            if (Uri.IsWellFormedUriString(Model.BaseUrl, UriKind.Absolute))
            {
                var sInfo = new ProcessStartInfo("explorer.exe", new Uri(Model.BaseUrl).AbsoluteUri);
                Process.Start(sInfo);
            }
        }
    }

}
