﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Dynamo.Core;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.PackageManager;
using Dynamo.Selection;
using Dynamo.Wpf.Interfaces;
using Greg.AuthProviders;

using Dynamo.Wpf.Properties;
using Microsoft.Practices.Prism.Commands;
using Dynamo.PackageManager.UI;
using System.Reflection;
using System.IO;

namespace Dynamo.ViewModels
{
    public class TermsOfUseHelperParams
    {
        internal IBrandingResourceProvider ResourceProvider { get; set; }
        internal PackageManagerClient PackageManagerClient { get; set; }
        internal Action AcceptanceCallback { get; set; }
    }

    /// <summary>
    /// A helper class to check asynchronously whether the Terms of Use has 
    /// been accepted, and if so, continue to execute the provided Action.
    /// </summary>
    public class TermsOfUseHelper
    {
        private readonly IBrandingResourceProvider resourceProvider;
        private readonly Action callbackAction;
        private readonly PackageManagerClient packageManagerClient;

        public TermsOfUseHelper(TermsOfUseHelperParams touParams)
        {
            if (touParams == null)
                throw new ArgumentNullException("touParams");
            if (touParams.PackageManagerClient == null)
                throw new ArgumentNullException("PackageManagerClient");
            if (touParams.AcceptanceCallback == null)
                throw new ArgumentNullException("AcceptanceCallback");
            if (touParams.ResourceProvider == null)
                throw new ArgumentNullException("ResourceProvider");

            resourceProvider = touParams.ResourceProvider;
            packageManagerClient = touParams.PackageManagerClient;
            callbackAction = touParams.AcceptanceCallback;
        }

        public void Execute()
        {
            Task<bool>.Factory.StartNew(() => packageManagerClient.GetTermsOfUseAcceptanceStatus()).
                ContinueWith(t =>
                {
                    // The above GetTermsOfUseAcceptanceStatus call will get the
                    // user to sign-in. If the user cancels that sign-in dialog 
                    // without signing in, we won't show the terms of use dialog,
                    // simply return from here.
                    // 
                    if (packageManagerClient.LoginState != LoginState.LoggedIn)
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
    public class PackageManagerClientViewModel : NotificationObject
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

        public List<PackageManagerSearchElement> CachedPackageList { get; private set; }

        public readonly DynamoViewModel DynamoViewModel;
        public PackageManagerClient Model { get; private set; }

        public LoginState LoginState
        {
            get
            {
                return Model.LoginState;
            }
        }

        public string Username
        {
            get
            {
                return Model.Username;
            }
        }

        public bool HasAuthProvider
        {
            get { return Model.HasAuthProvider; }
        }

        #endregion

        public ICommand ToggleLoginStateCommand { get; private set; }

        public PackageManagerClientViewModel(DynamoViewModel dynamoViewModel, PackageManagerClient model )
        {
            this.DynamoViewModel = dynamoViewModel;
            Model = model;
            CachedPackageList = new List<PackageManagerSearchElement>();

            this.ToggleLoginStateCommand = new DelegateCommand(ToggleLoginState, CanToggleLoginState);

            model.LoginStateChanged += (loginState) =>
            {
                RaisePropertyChanged("LoginState");
                RaisePropertyChanged("Username");
            };

        }

        private void ToggleLoginState()
        {
            if (this.LoginState == LoginState.LoggedIn)
            {
                this.Model.Logout();
            }
            else
            {
                this.Model.Login();
            }
        }

        private bool CanToggleLoginState()
        {
            return this.LoginState == LoginState.LoggedOut || this.LoginState == LoginState.LoggedIn;
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
            return DynamoViewModel.Model.CurrentWorkspace is CustomNodeWorkspaceModel && HasAuthProvider;
        }

        public void PublishNewPackage(object m)
        {
            var termsOfUseCheck = new TermsOfUseHelper(new TermsOfUseHelperParams
            {
                PackageManagerClient = Model,
                ResourceProvider = DynamoViewModel.BrandingResourceProvider,
                AcceptanceCallback = ShowNodePublishInfo
            });

            termsOfUseCheck.Execute();
        }

        public bool CanPublishNewPackage(object m)
        {
            return HasAuthProvider;
        }

        public void PublishCustomNode(Dynamo.Nodes.Function m)
        {
            CustomNodeInfo currentFunInfo;
            if (DynamoViewModel.Model.CustomNodeManager.TryGetNodeInfo(
                m.Definition.FunctionId,
                out currentFunInfo))
            {
                var termsOfUseCheck = new TermsOfUseHelper(new TermsOfUseHelperParams
                {
                    PackageManagerClient = Model,
                    ResourceProvider = DynamoViewModel.BrandingResourceProvider,
                    AcceptanceCallback = () => ShowNodePublishInfo(new[]
                    {
                        Tuple.Create(currentFunInfo, m.Definition)
                    })
                });

                termsOfUseCheck.Execute();
            }
        }

        public bool CanPublishCustomNode(Dynamo.Nodes.Function m)
        {
            return HasAuthProvider && m != null;
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
                ResourceProvider = DynamoViewModel.BrandingResourceProvider,
                AcceptanceCallback = () => ShowNodePublishInfo(defs)
            });

            termsOfUseCheck.Execute();
        }

        public bool CanPublishSelectedNodes(object m)
        {
            return DynamoSelection.Instance.Selection.Count > 0 &&
                   DynamoSelection.Instance.Selection.All(x => x is Function) && HasAuthProvider;;
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
                var pkg = DynamoViewModel.Model.PackageLoader.GetOwnerPackage(f.Item1);

                if (DynamoViewModel.Model.PackageLoader.GetOwnerPackage(f.Item1) != null)
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
                ele.DownvoteRequested += this.Model.Downvote;

                CachedPackageList.Add( ele );
            }

            return CachedPackageList;
        }

        /// <summary>
        /// This method downloads the package represented by the PackageDownloadHandle,
        /// uninstalls its current installation if necessary, and installs the package.
        /// 
        /// Note that, if the package is already installed, it must be uninstallable
        /// </summary>
        /// <param name="packageDownloadHandle"></param>
        internal void DownloadAndInstall(PackageDownloadHandle packageDownloadHandle)
        {
            Downloads.Add(packageDownloadHandle);

            packageDownloadHandle.DownloadState = PackageDownloadHandle.State.Downloading;

            Task.Factory.StartNew(() =>
            {
                try
                {
                    var pathDl = Model.DownloadPackage(packageDownloadHandle.Header._id,
                        packageDownloadHandle.VersionName);

                    DynamoViewModel.UIDispatcher.BeginInvoke((Action)(() =>
                    {
                        try
                        {
                            packageDownloadHandle.Done(pathDl);

                            Package dynPkg;

                            var firstOrDefault = DynamoViewModel.Model.PackageLoader.LocalPackages.FirstOrDefault(pkg => pkg.Name == packageDownloadHandle.Name);
                            if (firstOrDefault != null)
                            {
                                var dynModel = DynamoViewModel.Model;
                                try
                                {
                                    firstOrDefault.UninstallCore(dynModel.CustomNodeManager, dynModel.PackageLoader, dynModel.PreferenceSettings);
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

                            if (packageDownloadHandle.Extract(DynamoViewModel.Model, out dynPkg))
                            {
                                var p = Package.FromDirectory(dynPkg.RootDirectory, DynamoViewModel.Model.Logger);
                                DynamoViewModel.Model.PackageLoader.Load(p);

                                packageDownloadHandle.DownloadState = PackageDownloadHandle.State.Installed;
                            }
                        }
                        catch (Exception e)
                        {
                            packageDownloadHandle.Error(e.Message);
                        }
                    }));

                }
                catch (Exception e)
                {
                    packageDownloadHandle.Error(e.Message);
                }
            });

        }

        public void ClearCompletedDownloads()
        {
            Downloads.Where((x) => x.DownloadState == PackageDownloadHandle.State.Installed ||
                x.DownloadState == PackageDownloadHandle.State.Error).ToList().ForEach(x => Downloads.Remove(x));
        }

        internal void GoToWebsite()
        {
            if (Uri.IsWellFormedUriString(Model.BaseUrl, UriKind.Absolute))
            {
                var sInfo = new ProcessStartInfo("explorer.exe", new Uri(Model.BaseUrl).AbsoluteUri);
                Process.Start(sInfo);
            }
        }

    }

}
