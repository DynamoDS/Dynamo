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
using Dynamo.Logging;
using Dynamo.Models;
using Dynamo.PackageManager;
using Dynamo.PackageManager.UI;
using Dynamo.Selection;
using Dynamo.Utilities;
using Dynamo.Wpf.Interfaces;
using Dynamo.Wpf.Properties;
using Dynamo.Wpf.UI.GuidedTour;
using Dynamo.Wpf.Utilities;
using DynamoServices;
using Greg.AuthProviders;
using Greg.Responses;
using Prism.Commands;

namespace Dynamo.ViewModels
{
    public class TermsOfUseHelperParams
    {
        internal IBrandingResourceProvider ResourceProvider { get; set; }
        internal PackageManagerClient PackageManagerClient { get; set; }
        internal AuthenticationManager AuthenticationManager { get; set; }
        internal Action AcceptanceCallback { get; set; }
        internal Window Parent { get; set; }
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
        private readonly Window parent;
        private static bool isTermsOfUseCreated;

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
            parent = touParams.Parent;
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

        internal static bool ShowTermsOfUseDialog(bool forPublishing, string additionalTerms, Window parent = null)
        {
            if (isTermsOfUseCreated) return false;

            var executingAssemblyPathName = Assembly.GetExecutingAssembly().Location;
            var rootModuleDirectory = Path.GetDirectoryName(executingAssemblyPathName);
            var touFilePath = Path.Combine(rootModuleDirectory, "TermsOfUse.rtf");

            var termsOfUseView = new TermsOfUseView(touFilePath);
            termsOfUseView.Closed += TermsOfUseView_Closed;
            isTermsOfUseCreated = true;
            termsOfUseView.Owner = parent;

            //If any Guide is being executed then the ShowTermsOfUse Window WON'T be modal otherwise will be modal (as in the normal behavior)
            if (!GuideFlowEvents.IsAnyGuideActive)
            {
                termsOfUseView.ShowDialog();
            }
            else
            {
                //When a Guide is being executed then the TermsOfUseView cannot be modal and has the DynamoView as owner
                termsOfUseView.Show();
            }

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
            if (parent == null)
                additionalTermsView.ShowDialog();
            else
            {
                //Means that a Guide is being executed then the TermsOfUseView cannot be modal and has the DynamoView as owner
                additionalTermsView.Owner = parent;
                additionalTermsView.Show();
            }
            return additionalTermsView.AcceptedTermsOfUse;
        }

        private static void TermsOfUseView_Closed(object sender, EventArgs e)
        {
            isTermsOfUseCreated = false;
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

        private readonly string QUARANTINED = "quarantined";

        /// <summary>
        /// The System.Windows.Window owner of the view model.
        /// Used to align messagebox dialogs created by this model
        /// </summary>
        public Window ViewModelOwner { get; set; }

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

        internal virtual PackageManagerExtension PackageManagerExtension
        {
            get { return pmExtension ?? (pmExtension = DynamoViewModel.Model.GetPackageManagerExtension()); }
        }

        public virtual List<PackageManagerSearchElement> CachedPackageList { get; private set; }
        public List<PackageManagerSearchElement> InfectedPackageList { get; private set; }

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

        /// <summary>
        /// Contains all votes the user has submitted.
        /// Will allow the user to vote for a package they have not upvoted before
        /// </summary>
        private List<string> Uservotes { get; set; }

        internal PackageManagerClientViewModel(DynamoViewModel dynamoViewModel, PackageManagerClient model)
        {
            this.DynamoViewModel = dynamoViewModel;
            this.AuthenticationManager = dynamoViewModel.Model.AuthenticationManager;
            Model = model;
            CachedPackageList = new List<PackageManagerSearchElement>();

            this.ToggleLoginStateCommand = new DelegateCommand(ToggleLoginState, CanToggleLoginState);

            if (AuthenticationManager != null)
            {
                AuthenticationManager.LoginStateChanged += (loginState) =>
                {
                    RaisePropertyChanged("LoginState");
                    RaisePropertyChanged("Username");
                };
            }
        }

        private void ToggleLoginState()
        {
            if(!this.DynamoViewModel.IsIDSDKInitialized()) return;
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
            if (!this.DynamoViewModel.IsIDSDKInitialized(true, ViewModelOwner)) return;
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
                        }),
                        Parent = ViewModelOwner ?? DynamoViewModel.Owner,
                    };

                    var termsOfUseCheck = new TermsOfUseHelper(touParams);
                    termsOfUseCheck.Execute(true);
                    return;
                }
            }

            MessageBoxService.Show(
                ViewModelOwner,
                Resources.MessageSelectSymbolNotFound,
                Resources.SelectionErrorMessageBoxTitle,
                MessageBoxButton.OK, MessageBoxImage.Question);
        }

        public bool CanPublishCurrentWorkspace(object m)
        {
            return DynamoViewModel.Model.CurrentWorkspace is CustomNodeWorkspaceModel && AuthenticationManager.HasAuthProvider && !Model.NoNetworkMode;
        }

        public void PublishNewPackage(object m)
        {
            if (!this.DynamoViewModel.IsIDSDKInitialized(true, ViewModelOwner)) return;
            var termsOfUseCheck = new TermsOfUseHelper(new TermsOfUseHelperParams
            {
                PackageManagerClient = Model,
                AuthenticationManager = AuthenticationManager,
                ResourceProvider = DynamoViewModel.BrandingResourceProvider,
                AcceptanceCallback = ShowNodePublishInfo,
                Parent = ViewModelOwner ?? DynamoViewModel.Owner,
            });

            termsOfUseCheck.Execute(true);
        }

        public bool CanPublishNewPackage(object m)
        {
            return AuthenticationManager.HasAuthProvider && !Model.NoNetworkMode;
        }

        public void PublishCustomNode(Function m)
        {
            if (!this.DynamoViewModel.IsIDSDKInitialized(true, ViewModelOwner)) return;
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
                    }),
                    Parent = ViewModelOwner ?? DynamoViewModel.Owner,

                });

                termsOfUseCheck.Execute(true);
            }
        }

        public bool CanPublishCustomNode(Function m)
        {
            return AuthenticationManager.HasAuthProvider && m != null && !Model.NoNetworkMode;
        }

        public void PublishSelectedNodes(object m)
        {
            if (!this.DynamoViewModel.IsIDSDKInitialized(true, ViewModelOwner)) return;
            var nodeList = DynamoSelection.Instance.Selection
                                .Where(x => x is Function)
                                .Cast<Function>()
                                .Select(x => x.Definition.FunctionId)
                                .ToList();

            if (!nodeList.Any())
            {
                MessageBoxService.Show(
                    ViewModelOwner,
                    Resources.MessageSelectAtLeastOneNode,
                    Resources.SelectionErrorMessageBoxTitle,
                    MessageBoxButton.OK,
                    MessageBoxImage.Question);
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
                MessageBoxService.Show(
                    ViewModelOwner,
                    Resources.MessageGettingNodeError,
                    Resources.SelectionErrorMessageBoxTitle,
                    MessageBoxButton.OK, MessageBoxImage.Question);
            }

            var termsOfUseCheck = new TermsOfUseHelper(new TermsOfUseHelperParams
            {
                PackageManagerClient = Model,
                AuthenticationManager = AuthenticationManager,
                ResourceProvider = DynamoViewModel.BrandingResourceProvider,
                AcceptanceCallback = () => ShowNodePublishInfo(defs),
                Parent = ViewModelOwner ?? DynamoViewModel.Owner,
            });

            termsOfUseCheck.Execute(true);
        }

        public bool CanPublishSelectedNodes(object m)
        {
            return DynamoSelection.Instance.Selection.Count > 0 &&
                   DynamoSelection.Instance.Selection.All(x => x is Function) && AuthenticationManager.HasAuthProvider && !Model.NoNetworkMode;
        }

        private void ShowNodePublishInfo()
        {
            if (!this.DynamoViewModel.IsIDSDKInitialized(true, ViewModelOwner)) return;
            var newPkgVm = new PublishPackageViewModel(DynamoViewModel);
            DynamoViewModel.OnRequestPackagePublishDialog(newPkgVm);
        }

        private void ShowNodePublishInfo(ICollection<Tuple<CustomNodeInfo, CustomNodeDefinition>> funcDefs)
        {
            if (!this.DynamoViewModel.IsIDSDKInitialized(true, ViewModelOwner)) return;
            foreach (var f in funcDefs)
            {
                var pkg = PackageManagerExtension.PackageLoader.GetOwnerPackage(f.Item1);

                if (pkg != null)
                {
                    var m = Dynamo.Wpf.Utilities.MessageBoxService.Show(ViewModelOwner,
                        String.Format(Resources.MessageSubmitSameNamePackage,
                        DynamoViewModel.BrandingResourceProvider.ProductName, pkg.Name),
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

            // Calls to Model.UserVotes and Model.ListAll might take a long time to run (so do not use them syncronously in the UI thread)
            Uservotes = this.Model.UserVotes();
            foreach (var header in Model.ListAll())
            {
                var ele = new PackageManagerSearchElement(header);

                ele.UpvoteRequested += this.Model.Upvote;
                if (Uservotes != null)
                {
                    ele.HasUpvote = Uservotes.Contains(header._id);
                }

                CachedPackageList.Add(ele);
            }

            return CachedPackageList;
        }

        /// <summary>
        /// Returns a dictionary of infected package(s) with name and version, if the last published version of package uploaded by the current user was flagged as infected.
        /// </summary>
        /// <returns></returns>
        public List<PackageManagerSearchElement> GetInfectedPackages()
        {
            if (!this.DynamoViewModel.IsIDSDKInitialized(true, ViewModelOwner)) return null;
            InfectedPackageList = new List<PackageManagerSearchElement>();
            var latestPkgs = Model.GetUsersLatestPackages();
            if (latestPkgs != null && latestPkgs.maintains?.Count > 0)
            {
                foreach (var infectedVer in latestPkgs.maintains)
                {
                    if (infectedVer.scan_status == QUARANTINED)
                    {
                        var ele = new PackageManagerSearchElement(infectedVer);
                        InfectedPackageList.Add(ele);
                    }
                }
            }
            return InfectedPackageList;
        }

        /// <summary>
        /// Download and install a specific package from the package manager
        /// </summary>
        /// <param name="packageInfo"></param>
        /// <param name="downloadPath"></param>
        public void DownloadAndInstallPackage(IPackageInfo packageInfo, string downloadPath = null)
        {
            // User needs to accept terms of use before any packages can be downloaded from package manager
            if (!this.DynamoViewModel.IsIDSDKInitialized(true, ViewModelOwner)) return;
            var prefSettings = DynamoViewModel.Model.PreferenceSettings;
            var touAccepted = prefSettings.PackageDownloadTouAccepted;
            if (!touAccepted)
            {
                touAccepted = TermsOfUseHelper.ShowTermsOfUseDialog(false, null, ViewModelOwner);
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
                MessageBoxService.Show(
                    ViewModelOwner,
                    string.Format(Resources.MessagePackageVersionNotFound, packageInfo.Version.ToString(), packageInfo.Name),
                    Resources.PackageDownloadErrorMessageBoxTitle,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            ExecutePackageDownload(packageInfo.Name, version, downloadPath);
        }

        private string JoinPackageNames(IEnumerable<Package> pkgs, string seperator = ", ")
        {
            return String.Join(seperator, pkgs.Select(x => x.Name + " " + x.VersionName));
        }

        /// <summary>
        /// Warns about conflicts with both the main package and its dependencies.  
        /// </summary>
        /// <param name="package">package version being downloaded</param>
        /// <param name="duplicatePackage">local package found to be duplicate of one being downloaded</param>
        /// <param name="dependencyConflicts">List of packages that are in conflict with the dependencies of the package version to be downloaded (does not include the main package)</param>
        /// <returns>True if the User opted to continue with the download operation. False otherwise</returns>
        private bool WarnAboutDuplicatePackageConflicts(PackageVersion package,
                                                        Package duplicatePackage,
                                                        List<Package> dependencyConflicts)
        {
            var packageToDownload = $"{package.name} {package.version}";
            if (duplicatePackage != null)
            {
                var dupPkg = JoinPackageNames(new[] { duplicatePackage });
                if (duplicatePackage.BuiltInPackage)
                {
                    if (package.version == duplicatePackage.VersionName)
                    {
                        var message = string.Format(Resources.MessageSamePackageSameVersInBuiltinPackages, packageToDownload);

                        MessageBoxService.Show(ViewModelOwner, message, Resources.CannotDownloadPackageMessageBoxTitle,
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        var message = string.Format(Resources.MessageSamePackageDiffVersInBuiltinPackages, packageToDownload, dupPkg,
                            DynamoViewModel.BrandingResourceProvider.ProductName);

                        MessageBoxService.Show(ViewModelOwner, message, Resources.CannotDownloadPackageMessageBoxTitle,
                            MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                    Analytics.TrackEvent(Actions.BuiltInPackageConflict, Categories.PackageManagerOperations, packageToDownload);
                    return false;// All conflicts with built-in packages must be first resolved manually before continuing to download.
                }

                if (package.version == duplicatePackage.VersionName)
                {
                    var message = string.Format(Resources.MessageSamePackageSameVersInLocalPackages, packageToDownload);
                    MessageBoxService.Show(ViewModelOwner, message, Resources.CannotDownloadPackageMessageBoxTitle,
                    MessageBoxButton.OK, MessageBoxImage.Information);

                    return false;
                }
                else
                {
                    MessageBoxResult dialogResult;

                    if (duplicatePackage.InUse(DynamoViewModel.Model))
                    {// Loaded assemblies or package in use in workspace
                        dialogResult = MessageBoxService.Show(
                            string.Format(Resources.MessageSamePackageDiffVersInLocalPackages, packageToDownload, dupPkg, DynamoViewModel.BrandingResourceProvider.ProductName),
                            Resources.LoadedPackagesConflictMessageBoxTitle,
                            MessageBoxButton.OKCancel, new string[] { Resources.UninstallLoadedPackage, Resources.GenericTaskDialogOptionCancel }, MessageBoxImage.Exclamation);

                        if (dialogResult == MessageBoxResult.OK)
                        {
                            // mark for uninstallation
                            duplicatePackage.MarkForUninstall(DynamoViewModel.Model.PreferenceSettings);
                        }
                        return false;// Any package that is in use must first be uninstalled before continuing to download.
                    }
                    else
                    {
                        dialogResult = MessageBoxService.Show(
                            String.Format(Resources.MessageAlreadyInstallDynamo,
                            DynamoViewModel.BrandingResourceProvider.ProductName, dupPkg, packageToDownload),
                            Resources.PackagesInUseConflictMessageBoxTitle,
                            MessageBoxButton.OKCancel, new string[] { Resources.UninstallLoadedPackage, Resources.GenericTaskDialogOptionCancel },
                            MessageBoxImage.Exclamation);

                        if (dialogResult != MessageBoxResult.OK)
                        {
                            return false;
                        }
                        // The conflicting package will be uninstalled just before the new package is installed
                    }
                }
            }

            var uninstallsRequiringRestart = new List<Package>();
            var uninstallRequiringUserModifications = new List<Package>();
            var immediateUninstalls = new List<Package>();
            var builtinPackages = new List<Package>();
            foreach (var pkg in dependencyConflicts)
            {
                if (pkg == null) continue;

                if (pkg.BuiltInPackage)
                {
                    builtinPackages.Add(pkg);
                    continue;
                }

                if (pkg.LoadedAssemblies.Any())
                {
                    uninstallsRequiringRestart.Add(pkg);
                    continue;
                }

                if (pkg.InUse(DynamoViewModel.Model))
                {
                    uninstallRequiringUserModifications.Add(pkg);
                    continue;
                }

                immediateUninstalls.Add(pkg);
            }

            if (builtinPackages.Any())
            {
                // Conflicts with builtin packages
                var message = string.Format(Resources.MessagePackageDepsInBuiltinPackages, packageToDownload,
                        JoinPackageNames(builtinPackages));
                Analytics.TrackEvent(Actions.BuiltInPackageConflict, Categories.PackageManagerOperations, packageToDownload);
                var dialogResult = MessageBoxService.Show(ViewModelOwner, message,
                    Resources.BuiltInPackageConflictMessageBoxTitle,
                    MessageBoxButton.OKCancel, MessageBoxImage.Exclamation);

                if (dialogResult == MessageBoxResult.Cancel || dialogResult == MessageBoxResult.None) return false;
            }

            if (uninstallRequiringUserModifications.Any())
            {
                var conflictingPkgs = JoinPackageNames(uninstallRequiringUserModifications);
                var message = string.Format(Resources.MessageForceInstallOrUninstallToContinue, packageToDownload, conflictingPkgs,
                    DynamoViewModel.BrandingResourceProvider.ProductName);

                var dialogResult = MessageBoxService.Show(ViewModelOwner, message,
                    Resources.PackagesInUseConflictMessageBoxTitle,
                    MessageBoxButton.YesNo, MessageBoxImage.Exclamation);

                if (dialogResult == MessageBoxResult.No || dialogResult == MessageBoxResult.None) return false;
            }

            var settings = DynamoViewModel.Model.PreferenceSettings;
            if (uninstallsRequiringRestart.Any())
            {
                var conflictingPkgs = JoinPackageNames(uninstallsRequiringRestart, Environment.NewLine);
                var message = string.Format(Resources.MessageForceInstallOrUninstallUponRestart, packageToDownload,
                    conflictingPkgs);

                var dialogResult = MessageBoxService.Show(message,
                    Resources.LoadedPackagesConflictMessageBoxTitle,
                    MessageBoxButton.YesNoCancel, new string[] { Resources.ContinueInstall, Resources.UninstallLoadedPackages, Resources.GenericTaskDialogOptionCancel }, MessageBoxImage.Exclamation);

                if (dialogResult == MessageBoxResult.No)
                {
                    // mark for uninstallation
                    uninstallsRequiringRestart.ForEach(x => x.MarkForUninstall(settings));
                    return false;
                }
                else if (dialogResult == MessageBoxResult.Cancel || dialogResult == MessageBoxResult.None) return false;
            }

            if (immediateUninstalls.Any())
            {
                // if the package is not in use, tell the user we will uninstall it and give them the opportunity to cancel
                var message = String.Format(Resources.MessageAlreadyInstallDynamo,
                    DynamoViewModel.BrandingResourceProvider.ProductName,
                    JoinPackageNames(immediateUninstalls), packageToDownload);

                var dialogResult = MessageBoxService.Show(ViewModelOwner, message,
                    Resources.DownloadWarningMessageBoxTitle, MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                if (dialogResult == MessageBoxResult.Cancel || dialogResult == MessageBoxResult.None)
                {
                    return false;
                }
            }
            return true;
        }

        internal async void ExecutePackageDownload(string name, PackageVersion package, string installPath)
        {
            string msg;
            MessageBoxResult result;

            // initialize default download consent message
            msg = String.IsNullOrEmpty(installPath) ?
                    String.Format(Resources.MessageConfirmToInstallPackage, name, package.version) :
                    String.Format(Resources.MessageConfirmToInstallPackageToFolder, name, package.version, installPath);

            // Calculate compatibility and display a single download consent across cases
            var compatible = PackageManagerSearchElement.CalculateCompatibility(package.compatibility_matrix);

            // Unknown package compatibility with current Dynamo env, this is expected to be the most popular case for now
            if (compatible == null && !DynamoModel.IsTestMode)
            {
                msg = msg + "\n\n" + Resources.PackageUnknownCompatibilityVersionDownloadMsg;
                result = MessageBoxService.Show(ViewModelOwner, msg,
                    Resources.PackageDownloadConfirmMessageBoxTitle,
                    MessageBoxButton.OKCancel, MessageBoxImage.Question);

                if (result != MessageBoxResult.OK)
                {
                    return;
                }
            }
            // Package incompatible with current Dynamo env
            else if (compatible == false && !DynamoModel.IsTestMode)
            {
                msg = msg + "\n\n" + Resources.PackageManagerIncompatibleVersionDownloadMsg;
                result = MessageBoxService.Show(ViewModelOwner, msg,
                    Resources.PackageManagerIncompatibleVersionDownloadTitle,
                    MessageBoxButton.OKCancel, MessageBoxImage.Warning);

                if (result != MessageBoxResult.OK)
                {
                    return;
                }
            }
            // Package compatible with current Dynamo env
            else
            {
                result = MessageBoxService.Show(ViewModelOwner, msg,
                    Resources.PackageDownloadConfirmMessageBoxTitle,
                    MessageBoxButton.OKCancel, MessageBoxImage.Question);
            }

            var pmExt = DynamoViewModel.Model.GetPackageManagerExtension();
            if (result == MessageBoxResult.OK)
            {
                if (string.IsNullOrEmpty(package.name))
                {// package.name is not set sometimes
                    package.name = name;
                }

                Debug.Assert(package.full_dependency_ids.Count == package.full_dependency_versions.Count);
                // get all of the dependency version headers
                // we reverse these arrays because the package manager returns dependencies in topological order starting at 
                // the current package - and we want to install the dependencies first!.
                var reversedVersions = package.full_dependency_versions.Select(x => x).Reverse().ToList();
                var dependencyVersionHeaders = package.full_dependency_ids.Select(x => x).Reverse().Select((dep, i) =>
                {
                    var depVersion = reversedVersions[i];
                    try
                    {
                        return Model.GetPackageVersionHeader(dep._id, depVersion);
                    }
                    catch
                    {
                        MessageBoxService.Show(
                            ViewModelOwner,
                            String.Format(Resources.MessageFailedToDownloadPackageVersion, depVersion, dep._id),
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

                if (!dependencyVersionHeaders.Any(x => x.name == name))
                {// Add the main package if it does not exist
                    dependencyVersionHeaders.Add(package);
                }

                var localPkgs = pmExt.PackageLoader.LocalPackages;
                // if the new package has one or more dependencies that are already installed
                // we need to either first uninstall them, or allow the user to forcibly install the package,
                // or cancel the installation if they do not want either of the first two options.

                // local package that conflicts with package itself.
                Package duplicatePackage = null;

                // list of local packages that conflict (have different versions) with package dependencies.
                // Does not contain the main package since it is handled separately by duplicatePackage
                var localPkgsConflictingWithPkgDeps = new List<Package>();
                var newPackageHeaders = new List<PackageVersion>();

                foreach (var dependencyHeader in dependencyVersionHeaders)
                {
                    var localPkgWithSameName = localPkgs.FirstOrDefault(x =>
                        (x.LoadState.State == PackageLoadState.StateTypes.Loaded ||
                        x.LoadState.State == PackageLoadState.StateTypes.Error) &&
                        x.Name.Equals(dependencyHeader.name));

                    bool exactMatch = false;
                    if (localPkgWithSameName != null)
                    {
                        // Packages with same name and same version
                        exactMatch = localPkgWithSameName.VersionName.Equals(dependencyHeader.version);

                        if (name.Equals(localPkgWithSameName.Name))
                        {// Handle the main package duplicate
                            // Main package has a duplicate in local packages
                            duplicatePackage = localPkgWithSameName;
                        }
                        else
                        {// Handle the dependency duplicates here
                            // exclude dependencies that exactly match existing local packages
                            if (!exactMatch)
                            {
                                // Local packages that have the same name but different versions
                                localPkgsConflictingWithPkgDeps.Add(localPkgWithSameName);
                            }
                        }
                    }

                    if (!exactMatch)
                    {
                        // Package headers that do not match by name or version with existing local packages
                        newPackageHeaders.Add(dependencyHeader);
                    }
                }

                if (!WarnAboutDuplicatePackageConflicts(package, duplicatePackage, localPkgsConflictingWithPkgDeps))
                {
                    // User chose to cancel because of conflicts.
                    return;
                }

                // determine if any of the packages contain binaries or python scripts.  
                var containsBinariesOrPythonScripts = newPackageHeaders.Any(x =>
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
                    var res = MessageBoxService.Show(ViewModelOwner,
                        Resources.MessagePackageContainPythonScript,
                        Resources.PackageDownloadMessageBoxTitle,
                        MessageBoxButton.OKCancel, MessageBoxImage.Exclamation);

                    if (res == MessageBoxResult.Cancel || res == MessageBoxResult.None) return;
                }

                var containsPackagesThatTargetOtherHosts = PackageManagerExtension.CheckIfPackagesTargetOtherHosts(newPackageHeaders);

                // if unknown compatibility, and package target other hosts, notify user and allow cancellation
                if (compatible == null && containsPackagesThatTargetOtherHosts)
                {
                    var res = MessageBoxService.Show(ViewModelOwner,
                        Resources.MessagePackageTargetOtherHosts,
                        Resources.PackageDownloadMessageBoxTitle,
                        MessageBoxButton.OKCancel, MessageBoxImage.Exclamation);

                    if (res == MessageBoxResult.Cancel || res == MessageBoxResult.None) return;
                }

                // Determine if there are any dependencies that have a newer dynamo version, (this includes the root package).
                // We assume this means this package is compatibile with that dynamo version, but we should warn the user it
                // may not work with the current version of Dynamo.

                //wrap in try catch as it's possible version info could be missing. If so, we install the package and log an error.
                try
                {
                    var dynamoVersion = Version.Parse(DynamoModel.Version);
                    var futureDeps = newPackageHeaders.Where(dep => Version.Parse(dep.engine_version) > dynamoVersion);
                    // also identify packages that have a dynamo engine version less than 3.x as a special case,
                    // as Dynamo 3.x uses .net8 and older versions used .net framework - these packages may not be compatible.
                    // This check will return empty if the current major version is not 3.
                    var preDYN3Deps = newPackageHeaders.Where(dep => dynamoVersion.Major == 3 && Version.Parse(dep.engine_version).Major < dynamoVersion.Major);

                    // If any of the required packages use a newer version of Dynamo, show a dialog to the user
                    // allowing them to cancel the package download
                    if (futureDeps.Any())
                    {
                        var res = MessageBoxService.Show(ViewModelOwner,
                            $"{string.Format(Resources.MessagePackageNewerDynamo, DynamoViewModel.BrandingResourceProvider.ProductName)} {Resources.MessagePackOlderDynamoLink}",
                            string.Format(Resources.PackageUseNewerDynamoMessageBoxTitle, DynamoViewModel.BrandingResourceProvider.ProductName),
                            //this message has a url link so we use the rich text box version of the message box.
                            showRichTextBox: true,
                            MessageBoxButton.OKCancel,
                            MessageBoxImage.Warning);
                        if (res == MessageBoxResult.Cancel || res == MessageBoxResult.None)
                        {
                            return;
                        }
                    }

                    //if any of the required packages use a pre 3.x version of Dynamo, show a dialog to the user
                    //allowing them to cancel the package download
                    if (preDYN3Deps.Any())
                    {
                        var res = MessageBoxService.Show(ViewModelOwner,
                        $"{string.Format(Resources.MessagePackageOlderDynamo, DynamoViewModel.BrandingResourceProvider.ProductName)} {Resources.MessagePackOlderDynamoLink}",
                        string.Format(Resources.PackageUseOlderDynamoMessageBoxTitle, DynamoViewModel.BrandingResourceProvider.ProductName),
                        //this message has a url link so we use the rich text box version of the message box.
                        showRichTextBox: true,
                        MessageBoxButton.OKCancel,
                        MessageBoxImage.Warning);
                        if (res == MessageBoxResult.Cancel || res == MessageBoxResult.None)
                        {
                            return;
                        }
                    }
                }
                catch (ArgumentException ex)
                {
                    DynamoConsoleLogger.OnLogMessageToDynamoConsole($"exception while trying to compare version info between package and dynamo {ex}");
                }
                catch (FormatException ex)
                {
                    DynamoConsoleLogger.OnLogMessageToDynamoConsole($"exception while trying to compare version info between package and dynamo {ex}");
                }

                // add custom path to custom package folder list
                if (!String.IsNullOrEmpty(installPath))
                {
                    var settings = DynamoViewModel.Model.PreferenceSettings;
                    if (!settings.CustomPackageFolders.Contains(installPath))
                    {
                        settings.CustomPackageFolders.Add(installPath);
                    }
                }

                // form header version pairs and download and install all packages
                var downloadTasks = newPackageHeaders
                    .Select((dep) =>
                    {
                        return new PackageDownloadHandle()
                        {
                            Id = dep.id,
                            VersionName = dep.version,
                            Name = dep.name
                        };
                    })
                    .Select(x => Download(x)).ToArray();
                //wait for all downloads.
                await Task.WhenAll(downloadTasks);

                // When above downloads complete, start installing packages in dependency order.
                // The downloads have completed in a random order, but the dependencyVersionHeaders list is in correct topological
                // install order.
                foreach (var dep in newPackageHeaders)
                {
                    var matchingDownload = downloadTasks.Where(x => x.Result.handle.Id == dep.id).FirstOrDefault();
                    if (matchingDownload != null)
                    {
                        InstallPackage(matchingDownload.Result.handle, matchingDownload.Result.downloadPath, installPath);
                    }
                }
            }
        }

        /// <summary>
        /// This method downloads the package represented by the PackageDownloadHandle,
        /// 
        /// </summary>
        /// <param name="packageDownloadHandle">package download handle</param>
        internal virtual Task<(PackageDownloadHandle handle, string downloadPath)> Download(PackageDownloadHandle packageDownloadHandle)
        {
            // We only want to display the last 3 downloaded packages to the user
            // in the form of toast notifications.

            // We remove all but the last 2 packages and add the most recently-downloaded package
            if (Downloads.Count > 2) Downloads.RemoveRange(index: 0, count: Downloads.Count - 2);
            Downloads.Add(packageDownloadHandle);

            packageDownloadHandle.DownloadState = PackageDownloadHandle.State.Downloading;

            return Task.Factory.StartNew(() =>
            {
                // Attempt to download package
                string pathDl;
                var res = Model.DownloadPackage(packageDownloadHandle.Id, packageDownloadHandle.VersionName, out pathDl);

                // if you fail, update download handle and return
                if (!res.Success)
                {
                    packageDownloadHandle.Error(res.Error);
                    pathDl = string.Empty;
                }

                return (handle: packageDownloadHandle, downloadPath: pathDl);
            });
        }

        internal virtual void InstallPackage(PackageDownloadHandle packageDownloadHandle, string downloadPath, string installPath)
        {
            // if success, proceed to install the package
            if (string.IsNullOrEmpty(downloadPath) || packageDownloadHandle.DownloadState == PackageDownloadHandle.State.Error)
            {
                return;
            }
            DynamoViewModel.UIDispatcher.BeginInvoke((Action)(() =>
            {
                try
                {
                    packageDownloadHandle.Done(downloadPath);
                    var firstOrDefault = PackageManagerExtension.PackageLoader.LocalPackages.FirstOrDefault(pkg => pkg.Name == packageDownloadHandle.Name);
                    if (firstOrDefault != null)
                    {
                        var dynModel = DynamoViewModel.Model;
                        try
                        {
                            firstOrDefault.UninstallCore(dynModel.CustomNodeManager, PackageManagerExtension.PackageLoader, dynModel.PreferenceSettings);
                        }
                        catch
                        {
                            MessageBoxService.Show(
                                ViewModelOwner,
                                String.Format(Resources.MessageFailToUninstallPackage,
                                DynamoViewModel.BrandingResourceProvider.ProductName,
                                packageDownloadHandle.Name),
                                Resources.DeleteFailureMessageBoxTitle,
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    SetPackageState(packageDownloadHandle, installPath);
                    // Dispose Index writer to avoid file lock after new package is installed
                    Search.LuceneSearch.LuceneUtilityNodeSearch.DisposeWriter();
                    Analytics.TrackEvent(Actions.Installed, Categories.PackageManagerOperations, $"{packageDownloadHandle?.Name}");
                }
                catch (Exception e)
                {
                    packageDownloadHandle.Error(e.Message);
                }
            }));
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
                PackageManagerExtension.PackageLoader.LoadPackages(new List<Package> { dynPkg });
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
                var sInfo = new ProcessStartInfo("explorer.exe", new Uri(Model.BaseUrl).AbsoluteUri) { UseShellExecute = true };
                Process.Start(sInfo);
            }
        }

    }

}
