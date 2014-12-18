using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.PackageManager;
using Dynamo.Selection;
using Greg.Requests;

namespace Dynamo.ViewModels
{
    /// <summary>
    ///     A thin wrapper on the Greg rest client for performing IO with
    ///     the Package Manager
    /// </summary>
    public class PackageManagerClientViewModel
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

        #endregion

        public PackageManagerClientViewModel(DynamoViewModel dynamoViewModel, PackageManagerClient model )
        {
            this.DynamoViewModel = dynamoViewModel;
            Model = model;
            CachedPackageList = new List<PackageManagerSearchElement>();
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
            
            MessageBox.Show(
                "The selected symbol was not found in the workspace",
                "Selection Error",
                MessageBoxButton.OK,
                MessageBoxImage.Question);
        }

        public bool CanPublishCurrentWorkspace(object m)
        {
            return DynamoViewModel.Model.CurrentWorkspace is CustomNodeWorkspaceModel
                && Model.HasAuthenticator;
        }

        public void PublishNewPackage(object m)
        {
            ShowNodePublishInfo();
        }

        public bool CanPublishNewPackage(object m)
        {
            return Model.HasAuthenticator;
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
                MessageBox.Show("You must select at least one custom node.", "Selection Error", MessageBoxButton.OK, MessageBoxImage.Question);
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
                MessageBox.Show(
                    "There was a problem getting the node with id \"" + node + "\" from the workspace.",
                    "Selection Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Question);
            }

            ShowNodePublishInfo(defs);
        }

        public bool CanPublishSelectedNodes(object m)
        {
            return DynamoSelection.Instance.Selection.Count > 0 &&
                DynamoSelection.Instance.Selection.All(x => x is Function) &&
                Model.HasAuthenticator;
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
                    var m =
                        MessageBox.Show(
                            "The node is part of the dynamo package called \"" + pkg.Name
                                + "\" - do you want to submit a new version of this package?  \n\nIf not, this node will be moved to the new package you are creating.",
                            "Package Warning",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question);

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

        public List<PackageManagerSearchElement> Search(string search, int maxNumSearchResults)
        {
            return Model.Search(search, maxNumSearchResults)
                               .Select((header) => new PackageManagerSearchElement(Model, header))
                               .ToList();
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
            

            var pkgDownload = new PackageDownload(packageDownloadHandle.Header._id, packageDownloadHandle.VersionName);
            Downloads.Add(packageDownloadHandle);

            packageDownloadHandle.DownloadState = PackageDownloadHandle.State.Downloading;

            Task.Factory.StartNew(() =>
            {
                try
                {
                    var response = Model.Client.Execute(pkgDownload);
                    var pathDl = PackageDownload.GetFileFromResponse(response);

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
                                    MessageBox.Show("Dynamo failed to uninstall the package: " + packageDownloadHandle.Name +
                                        "  The package may need to be reinstalled manually.", "Uninstall Failure", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }

                            if (packageDownloadHandle.Extract(DynamoViewModel.Model, out dynPkg))
                            {
                                var downloadPkg = Package.FromDirectory(dynPkg.RootDirectory, DynamoViewModel.Model.Logger);

                                var loader = DynamoViewModel.Model.Loader;
                                var logger = DynamoViewModel.Model.Logger;
                                var libraryServices = DynamoViewModel.EngineController.LibraryServices;
                                downloadPkg.LoadIntoDynamo(
                                    loader,
                                    logger,
                                    libraryServices,
                                    DynamoViewModel.Model.Context,
                                    DynamoModel.IsTestMode,
                                    DynamoViewModel.Model.CustomNodeManager);

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
            Process.Start(Model.Client.BaseUrl);
        }

    }

}
