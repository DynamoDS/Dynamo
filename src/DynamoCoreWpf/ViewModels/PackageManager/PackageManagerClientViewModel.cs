using System;
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
using Greg.AuthProviders;

using Dynamo.Wpf.Properties;
using Microsoft.Practices.Prism.Commands;

namespace Dynamo.ViewModels
{
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
            get { return Model.LoginState; }
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

            model.LoginStateChanged += b => RaisePropertyChanged("LoginState");
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
                    ShowNodePublishInfo(new[] { Tuple.Create(currentFunInfo, currentFunDef) });
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
            ShowNodePublishInfo();
        }

        public bool CanPublishNewPackage(object m)
        {
            return HasAuthProvider;
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

            ShowNodePublishInfo(defs);
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
                    var m = MessageBox.Show(String.Format(Resources.MessageSubmitSameNamePackage, pkg.Name),
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
            CachedPackageList =
                    Model.ListAll()
                               .Select((header) => new PackageManagerSearchElement(Model, header))
                               .ToList();

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
                                    MessageBox.Show(String.Format(Resources.MessageFailToUninstallPackage, packageDownloadHandle.Name),
                                        Resources.UninstallFailureMessageBoxTitle, 
                                        MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }

                            if (packageDownloadHandle.Extract(DynamoViewModel.Model, out dynPkg))
                            {
                                var downloadPkg = Package.FromDirectory(dynPkg.RootDirectory, DynamoViewModel.Model.Logger);

                                var loader = DynamoViewModel.Model.Loader;
                                var logger = DynamoViewModel.Model.Logger;
                                var libraryServices = DynamoViewModel.EngineController.LibraryServices;

                                var loadPackageParams = new LoadPackageParams
                                {
                                    Loader = loader,
                                    LibraryServices = libraryServices,
                                    Context = DynamoViewModel.Model.Context,
                                    IsTestMode = DynamoModel.IsTestMode,
                                    CustomNodeManager = DynamoViewModel.Model.CustomNodeManager
                                };

                                downloadPkg.LoadIntoDynamo(loadPackageParams, logger);

                                DynamoViewModel.Model.PackageLoader.LocalPackages.Add(downloadPkg);
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
