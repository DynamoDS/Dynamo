using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using Dynamo.Configuration;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using Dynamo.PackageManager;
using Dynamo.Wpf.Properties;
using Dynamo.Wpf.Utilities;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.ViewModel;

namespace Dynamo.ViewModels
{
    public class PackageViewModel : NotificationObject
    {
        private readonly DynamoViewModel dynamoViewModel;
        private readonly PackageManagerClient packageManagerClient;
        public Package Model { get; private set; }

        public bool HasAdditionalFiles
        {
            get { return Model.AdditionalFiles.Any(); }
        }

        public bool HasAdditionalAssemblies
        {
            get { return Model.LoadedAssemblies.Any(x => !x.IsNodeLibrary); }
        }

        public bool Unloaded
        {
            get
            {
                return Model.LoadState.State == PackageLoadState.StateTypes.Unloaded;
            }
        }

        public string PackageLoadStateText
        {
            get
            {
                switch (Model.LoadState.ScheduledState)
                {
                    case PackageLoadState.ScheduledTypes.ScheduledForLoad: return Resources.PackageStateScheduledForLoad;
                    case PackageLoadState.ScheduledTypes.ScheduledForUnload: return Resources.PackageStateScheduledForUnload;
                    case PackageLoadState.ScheduledTypes.ScheduledForDeletion: return Resources.PackageStateScheduledForDeletion;
                    default:
                        break;
                }

                switch (Model.LoadState.State)
                {
                    case PackageLoadState.StateTypes.Unloaded: return Resources.PackageStateUnloaded;
                    case PackageLoadState.StateTypes.Loaded: return Resources.PackageStateLoaded;
                    case PackageLoadState.StateTypes.Error: return Resources.PackageStateError;
                    default:
                        return "Unknown package state";
                }
            }
        }

        public string PackageLoadStateTooltip
        {
            get
            {
                switch (Model.LoadState.ScheduledState)
                {
                    case PackageLoadState.ScheduledTypes.ScheduledForLoad: return Resources.PackageStateScheduledForLoadTooltip;
                    case PackageLoadState.ScheduledTypes.ScheduledForUnload: return Resources.PackageStateScheduledForUnloadTooltip;
                    case PackageLoadState.ScheduledTypes.ScheduledForDeletion: return Resources.PackageStateScheduledForDeletionTooltip;
                    default:
                        break;
                }

                switch (Model.LoadState.State)
                {
                    case PackageLoadState.StateTypes.Unloaded: return Resources.PackageStateUnloadedTooltip;
                    case PackageLoadState.StateTypes.Loaded: return Resources.PackageStateLoadedTooltip;
                    case PackageLoadState.StateTypes.Error: return string.Format(Resources.PackageStateErrorTooltip, Model.LoadState.ErrorMessage);
                    default:
                        return "Unknown package state";
                }
            }
        }

        public string PackageViewContextMenuUninstallText
        {
            get { return Model.BuiltInPackage ? Resources.PackageContextMenuUnloadPackageText : Resources.PackageContextMenuDeletePackageText; }
        }

        public string PackageViewContextMenuUninstallTooltip
        {
            get { return Model.BuiltInPackage ? Resources.PackageContextMenuUnloadPackageTooltip : Resources.PackageContextMenuDeletePackageTooltip; }
        }

        public string PackageViewContextMenuUnmarkUninstallText
        {
            get { return Model.BuiltInPackage ? Resources.PackageContextMenuUnmarkUnloadPackageText : Resources.PackageContextMenuUnmarkDeletePackageText; }
        }

        public string PackageViewContextMenuUnmarkUninstallTooltip
        {
            get { return Model.BuiltInPackage ? Resources.PackageContextMenuUnmarkUnloadPackageTooltip : Resources.PackageContextMenuUnmarkDeletePackageTooltip; }
        }

        public bool HasNodeLibraries
        {
            get { return Model.LoadedAssemblies.Any(x => x.IsNodeLibrary); }
        }

        public bool HasCustomNodes
        {
            get { return Model.LoadedCustomNodes.Any();  }
        }

        public bool HasAssemblies
        {
            get { return Model.LoadedAssemblies.Any(); }
        }

        [Obsolete("Do not use. This command will be removed. It does nothing.")]
        public DelegateCommand ToggleTypesVisibleInManagerCommand { get; set; }
        public DelegateCommand GetLatestVersionCommand { get; set; }
        public DelegateCommand PublishNewPackageVersionCommand { get; set; }
        public DelegateCommand UninstallCommand { get; set; }
        public DelegateCommand UnmarkForUninstallationCommand { get; set; }
        public DelegateCommand LoadCommand { get; set; }
        public DelegateCommand UnmarkForLoadCommand { get; set; }
        public DelegateCommand PublishNewPackageCommand { get; set; }
        public DelegateCommand DeprecateCommand { get; set; }
        public DelegateCommand UndeprecateCommand { get; set; }
        public DelegateCommand GoToRootDirectoryCommand { get; set; }

        public PackageViewModel(DynamoViewModel dynamoViewModel, Package model)
        {
            this.dynamoViewModel = dynamoViewModel;
            var pmExtension = dynamoViewModel.Model.GetPackageManagerExtension();
            this.packageManagerClient = pmExtension.PackageManagerClient;
            Model = model;

            ToggleTypesVisibleInManagerCommand = new DelegateCommand(() => { }, () => true);
            GetLatestVersionCommand = new DelegateCommand(GetLatestVersion, CanGetLatestVersion);
            PublishNewPackageVersionCommand = new DelegateCommand(() => ExecuteWithTou(PublishNewPackageVersion), CanPublishNewPackageVersion);
            PublishNewPackageCommand = new DelegateCommand(() => ExecuteWithTou(PublishNewPackage), CanPublishNewPackage);
            UninstallCommand = new DelegateCommand(Uninstall, CanUninstall);
            UnmarkForUninstallationCommand = new DelegateCommand(UnmarkForUninstallation, CanUnmarkForUninstallation);
            LoadCommand = new DelegateCommand(Load, CanLoad);
            UnmarkForLoadCommand = new DelegateCommand(UnmarkForLoad, CanUnmarkForLoad);
            DeprecateCommand = new DelegateCommand(Deprecate, CanDeprecate);
            UndeprecateCommand = new DelegateCommand(Undeprecate, CanUndeprecate);
            GoToRootDirectoryCommand = new DelegateCommand(GoToRootDirectory, CanGoToRootDirectory);

            Model.LoadedAssemblies.CollectionChanged += LoadedAssembliesOnCollectionChanged;
            Model.PropertyChanged += ModelOnPropertyChanged;

            this.dynamoViewModel.Model.WorkspaceAdded += WorkspaceAdded;
            this.dynamoViewModel.Model.WorkspaceRemoved += WorkspaceRemoved;
        }

        private void NodeAddedOrRemovedHandler(object _)
        {
            UninstallCommand.RaiseCanExecuteChanged();
        }

        private void WorkspaceAdded(WorkspaceModel ws)
        {
            UninstallCommand.RaiseCanExecuteChanged();
            ws.NodeAdded += NodeAddedOrRemovedHandler;
            ws.NodeRemoved += NodeAddedOrRemovedHandler;
        }

        private void WorkspaceRemoved(WorkspaceModel ws)
        {
            UninstallCommand.RaiseCanExecuteChanged();
            ws.NodeAdded -= NodeAddedOrRemovedHandler;
            ws.NodeRemoved -= NodeAddedOrRemovedHandler;
        }

        private void LoadedAssembliesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            RaisePropertyChanged("HasAdditionalAssemblies");
            RaisePropertyChanged("HasAssemblies");
            RaisePropertyChanged("HasNodeLibraries");
        }

        // Calls RaisePropertyChanged for all PackageLoadState related properties
        internal void NotifyLoadStatePropertyChanged()
        {
            UninstallCommand.RaiseCanExecuteChanged();
            UnmarkForUninstallationCommand.RaiseCanExecuteChanged();
            LoadCommand.RaiseCanExecuteChanged();
            UnmarkForLoadCommand.RaiseCanExecuteChanged();
            RaisePropertyChanged(nameof(PackageLoadStateTooltip));
            RaisePropertyChanged(nameof(PackageLoadStateText));
            RaisePropertyChanged(nameof(Unloaded));
        }

        private void ModelOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == nameof(Model.LoadState))
            {
                NotifyLoadStatePropertyChanged();
            }
        }

        private void UnmarkForUninstallation()
        {
            Model.UnmarkForUninstall( dynamoViewModel.Model.PreferenceSettings );
            NotifyLoadStatePropertyChanged();
        }

        private bool CanUnmarkForUninstallation()
        {
            return Model.BuiltInPackage ?
                Model.LoadState.ScheduledState == PackageLoadState.ScheduledTypes.ScheduledForUnload :
                Model.LoadState.ScheduledState == PackageLoadState.ScheduledTypes.ScheduledForDeletion;
        }

        private void Uninstall()
        {
            if (Model.LoadedAssemblies.Any())
            {
                var resAssem =
                    MessageBox.Show(string.Format(Resources.MessageNeedToRestart,
                        dynamoViewModel.BrandingResourceProvider.ProductName),
                        Resources.UninstallingPackageMessageBoxTitle,
                        MessageBoxButton.OKCancel,
                        MessageBoxImage.Exclamation);
                if (resAssem == MessageBoxResult.Cancel) return;
            }

            if (!Model.BuiltInPackage) {
                var res = MessageBox.Show(String.Format(Resources.MessageConfirmToUninstallPackage, this.Model.Name),
                          Resources.UninstallingPackageMessageBoxTitle,
                          MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (res == MessageBoxResult.No) return;
            }

            try
            {
                var dynModel = dynamoViewModel.Model;
                Model.UninstallCore(dynModel.CustomNodeManager, dynModel.GetPackageManagerExtension().PackageLoader, dynModel.PreferenceSettings);
            }
            catch (Exception)
            {
                MessageBox.Show(string.Format(Resources.MessageFailedToUninstall,
                    dynamoViewModel.BrandingResourceProvider.ProductName),
                    Resources.UninstallFailureMessageBoxTitle,
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                NotifyLoadStatePropertyChanged();
            }
        }

        private bool CanUninstall()
        {
            if (!Model.InUse(dynamoViewModel.Model) || Model.LoadedAssemblies.Any())
            {
                return Model.BuiltInPackage ?
                    Model.LoadState.State != PackageLoadState.StateTypes.Unloaded &&
                    Model.LoadState.ScheduledState != PackageLoadState.ScheduledTypes.ScheduledForUnload :
                    Model.LoadState.ScheduledState != PackageLoadState.ScheduledTypes.ScheduledForDeletion;
            }
            return false;
        }

        // Loads a built-in package that was previously set as Unloaded
        private void Load()
        {
            var dynModel = dynamoViewModel.Model;
            var packageLoader = dynModel.GetPackageManagerExtension().PackageLoader;
            var conflicts = packageLoader.LocalPackages.ToList().Where(x => x.Name == Model.Name && x != Model);
            bool hasConflictsWithLoadedAssemblies = conflicts.Any(x => x.InUse(dynamoViewModel.Model));

            if (conflicts.Any())
            {
                string conflictsMsg = string.Join(",", conflicts.Select(x => x.Name + " " + x.VersionName));

                string message = hasConflictsWithLoadedAssemblies ? 
                        string.Format(Resources.MessageLoadBuiltInPackageWithRestart,
                        conflictsMsg, Model.Name + " " + Model.VersionName) 
                        :
                        string.Format(Resources.MessageLoadBuiltInPackage,
                        conflictsMsg, Model.Name + " " + Model.VersionName);

                var dialogResult = MessageBoxService.Show(message,
                    Resources.CannotDownloadPackageMessageBoxTitle,
                        MessageBoxButton.OKCancel,
                        MessageBoxImage.Exclamation);

                if (dialogResult == MessageBoxResult.Cancel) return;
            }

            try
            {
                foreach (var package in conflicts)
                {
                    package.UninstallCore(dynModel.CustomNodeManager, packageLoader, dynModel.PreferenceSettings);
                }

                if (!hasConflictsWithLoadedAssemblies)
                {
                    try
                    {
                        packageLoader.TryLoadPackageIntoLibrary(Model);
                        Model.LoadState.SetAsLoaded();
                    } 
                    catch 
                    {
                        Model.MarkForLoad(dynModel.PreferenceSettings);
                    }
                }
                else
                {
                    Model.MarkForLoad(dynModel.PreferenceSettings);
                }
            }
            catch (Exception e) 
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                NotifyLoadStatePropertyChanged();
                foreach (var package in dynamoViewModel.PreferencesViewModel.LocalPackages.Where(x => conflicts.Contains(x.Model)))
                {
                    package.NotifyLoadStatePropertyChanged();
                }
            }
        }

        // Cancels the ScheduledForLoad status on a built-in package
        private void UnmarkForLoad()
        {
            var dynModel = dynamoViewModel.Model;
            var packageLoader = dynModel.GetPackageManagerExtension().PackageLoader;
            var conflicts = packageLoader.LocalPackages.ToList().Where(x => x.Name == Model.Name && x != Model);

            foreach (var package in conflicts)
            {
                if (package.LoadState.ScheduledState == PackageLoadState.ScheduledTypes.ScheduledForDeletion)
                {
                    package.UnmarkForUninstall(dynModel.PreferenceSettings);
                }
            }

            Model.UnmarkForLoad(dynModel.PreferenceSettings);

            NotifyLoadStatePropertyChanged();
            foreach (var package in dynamoViewModel.PreferencesViewModel.LocalPackages.Where(x => conflicts.Contains(x.Model)))
            {
                package.NotifyLoadStatePropertyChanged();
            }
        }

        private bool CanUnmarkForLoad()
        {
            return Model.BuiltInPackage &&
                (Model.LoadState.ScheduledState == PackageLoadState.ScheduledTypes.ScheduledForLoad);
        }

        private bool CanLoad()
        {
            return Model.BuiltInPackage && 
                (Model.LoadState.State == PackageLoadState.StateTypes.Unloaded) && 
                (Model.LoadState.ScheduledState != PackageLoadState.ScheduledTypes.ScheduledForLoad);
        }

    private void GoToRootDirectory()
        {
            // Check for the existance of RootDirectory
            if (Directory.Exists(Model.RootDirectory))
            {
                Process.Start(Model.RootDirectory);
            }
            else
            {
                System.Windows.Forms.MessageBox.Show(Wpf.Properties.Resources.PackageNotExisted, Wpf.Properties.Resources.DirectoryNotFound, System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
        }

        private bool CanGoToRootDirectory()
        {
            return true;
        }

        private void Deprecate()
        {
            var res = MessageBox.Show(String.Format(Resources.MessageToDeprecatePackage, this.Model.Name),
                                      Resources.DeprecatingPackageMessageBoxTitle, 
                                      MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res == MessageBoxResult.No) return;

            packageManagerClient.Deprecate(Model.Name);
        }

        private bool CanDeprecate()
        {
            if (!dynamoViewModel.Model.AuthenticationManager.HasAuthProvider) return false;
            return packageManagerClient.DoesCurrentUserOwnPackage(Model, dynamoViewModel.Model.AuthenticationManager.Username);
        }

        private void Undeprecate()
        {
            var res = MessageBox.Show(String.Format(Resources.MessageToUndeprecatePackage, this.Model.Name),
                                      Resources.UndeprecatingPackageMessageBoxTitle, 
                                      MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res == MessageBoxResult.No) return;
            packageManagerClient.Undeprecate(Model.Name);
        }

        private bool CanUndeprecate()
        {
            if (!dynamoViewModel.Model.AuthenticationManager.HasAuthProvider) return false;
            return packageManagerClient.DoesCurrentUserOwnPackage(Model, dynamoViewModel.Model.AuthenticationManager.Username);
        }

        private void PublishNewPackageVersion()
        {
            Model.RefreshCustomNodesFromDirectory(dynamoViewModel.Model.CustomNodeManager, DynamoModel.IsTestMode);
            var vm = PublishPackageViewModel.FromLocalPackage(dynamoViewModel, Model);
            vm.IsNewVersion = true;

            dynamoViewModel.OnRequestPackagePublishDialog(vm);
        }

        private bool CanPublishNewPackageVersion()
        {
            return dynamoViewModel.Model.AuthenticationManager.HasAuthProvider;
        }

        private void PublishNewPackage()
        {
            Model.RefreshCustomNodesFromDirectory(dynamoViewModel.Model.CustomNodeManager, DynamoModel.IsTestMode);
            var vm = PublishPackageViewModel.FromLocalPackage(dynamoViewModel, Model);
            vm.IsNewVersion = false;

            dynamoViewModel.OnRequestPackagePublishDialog(vm);
        }

        private void ExecuteWithTou(Action acceptanceCallback)
        {
            var dModel = dynamoViewModel.Model;
            // create TermsOfUseHelper object to check asynchronously whether the Terms of Use has 
            // been accepted, and if so, continue to execute the provided Action.
            var termsOfUseCheck = new TermsOfUseHelper(new TermsOfUseHelperParams
            {
                PackageManagerClient = dModel.GetPackageManagerExtension().PackageManagerClient,
                AuthenticationManager = dModel.AuthenticationManager,
                ResourceProvider = dynamoViewModel.BrandingResourceProvider,
                AcceptanceCallback = acceptanceCallback
            });

            termsOfUseCheck.Execute(false);
        }

        private bool CanPublishNewPackage()
        {
            return dynamoViewModel.Model.AuthenticationManager.HasAuthProvider;
        }

        private void GetLatestVersion()
        {
            throw new NotImplementedException();
        }

        private bool CanGetLatestVersion()
        {
            return false;
        }
    }
}
