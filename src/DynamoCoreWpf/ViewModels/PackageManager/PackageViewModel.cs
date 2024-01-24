using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using Dynamo.Graph.Workspaces;
using Dynamo.Logging;
using Dynamo.Models;
using Dynamo.PackageManager;
using Dynamo.Wpf.Properties;
using Dynamo.Wpf.Utilities;
using Prism.Commands;
using NotificationObject = Dynamo.Core.NotificationObject;

namespace Dynamo.ViewModels
{
    public class PackageViewModel : NotificationObject
    {
        private readonly DynamoViewModel dynamoViewModel;
        private readonly PackageManagerClient packageManagerClient;
        private readonly DynamoModel dynamoModel;

        public Package Model { get; private set; }

        public bool HasAdditionalFiles
        {
            get { return Model.AdditionalFiles.Any(); }
        }

        public bool HasAdditionalAssemblies
        {
            get { return Model.LoadedAssemblies.Any(x => !x.IsNodeLibrary); }
        }

        /// <summary>
        /// Specifies whether or not this Package's LoadState is Loaded with no scheduled operation.
        /// </summary>
        public bool LoadedWithNoScheduledOperation
        {
            get
            {
                return Model.BuiltInPackage ?
                    Model.LoadState.State != PackageLoadState.StateTypes.Unloaded &&
                    Model.LoadState.ScheduledState != PackageLoadState.ScheduledTypes.ScheduledForUnload :
                    Model.LoadState.ScheduledState != PackageLoadState.ScheduledTypes.ScheduledForDeletion;
            }
        }

        /// <summary>
        /// Specifies whether or not this Package's LoadState is set to Unloaded
        /// </summary>
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
            get
            {
                // Built in package
                if (Model.BuiltInPackage)
                {
                    return Resources.PackageContextMenuUnloadPackageTooltip;
                }

                // Package with custom nodes that are in use
                if (!CanUninstall())
                {
                    return Resources.PackageContextMenuDeletePackageCustomNodesInUseTooltip;
                }

                // Package that can be uninstalled
                return Resources.PackageContextMenuDeletePackageTooltip;
            }
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
            get { return Model.LoadedCustomNodes.Any(); }
        }

        public bool HasAssemblies
        {
            get { return Model.LoadedAssemblies.Any(); }
        }

        public bool CanPublish => dynamoModel.AuthenticationManager.HasAuthProvider;

        [Obsolete("Do not use. This command will be removed. It does nothing.")]
        public DelegateCommand ToggleTypesVisibleInManagerCommand { get; set; }
        [Obsolete("Do not use. This command will be removed. It does nothing.")]
        public DelegateCommand GetLatestVersionCommand { get; set; }
        public DelegateCommand PublishNewPackageVersionCommand { get; set; }
        public DelegateCommand UninstallCommand { get; set; }
        public DelegateCommand UnmarkForUninstallationCommand { get; set; }
        public DelegateCommand LoadCommand { get; set; }
        public DelegateCommand PublishNewPackageCommand { get; set; }
        public DelegateCommand DeprecateCommand { get; set; }
        public DelegateCommand UndeprecateCommand { get; set; }
        public DelegateCommand GoToRootDirectoryCommand { get; set; }

        public PackageViewModel(DynamoViewModel dynamoViewModel, Package model)
        {
            this.dynamoViewModel = dynamoViewModel;
            this.dynamoModel = dynamoViewModel.Model;

            var pmExtension = dynamoModel.GetPackageManagerExtension();
            this.packageManagerClient = pmExtension.PackageManagerClient;
            Model = model;

            ToggleTypesVisibleInManagerCommand = new DelegateCommand(() => { }, () => true);
            GetLatestVersionCommand = new DelegateCommand(() => { }, () => false);
            PublishNewPackageVersionCommand = new DelegateCommand(() => ExecuteWithTou(PublishNewPackageVersion), IsOwner);
            PublishNewPackageCommand = new DelegateCommand(() => ExecuteWithTou(PublishNewPackage), () => CanPublish);
            UninstallCommand = new DelegateCommand(Uninstall, CanUninstall);
            UnmarkForUninstallationCommand = new DelegateCommand(UnmarkForUninstallation, CanUnmarkForUninstallation);
            LoadCommand = new DelegateCommand(Load, CanLoad);
            DeprecateCommand = new DelegateCommand(Deprecate, IsOwner);
            UndeprecateCommand = new DelegateCommand(Undeprecate, CanUndeprecate);
            GoToRootDirectoryCommand = new DelegateCommand(GoToRootDirectory, () => true);

            Model.LoadedAssemblies.CollectionChanged += LoadedAssembliesOnCollectionChanged;
            Model.PropertyChanged += ModelOnPropertyChanged;

            this.dynamoModel.WorkspaceAdded += WorkspaceAdded;
            this.dynamoModel.WorkspaceRemoved += WorkspaceRemoved;
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
            Model.UnmarkForUninstall( dynamoModel.PreferenceSettings );
            NotifyLoadStatePropertyChanged();
        }

        private bool CanUnmarkForUninstallation()
        {
            return Model.BuiltInPackage ?
                Model.LoadState.ScheduledState == PackageLoadState.ScheduledTypes.ScheduledForUnload :
                Model.LoadState.ScheduledState == PackageLoadState.ScheduledTypes.ScheduledForDeletion;
        }

        private string MessageNeedToRestart => Model.BuiltInPackage ? Resources.MessageNeedToRestartAfterUnload : Resources.MessageNeedToRestartAfterDelete;

        private string MessageNeedToRestartTitle => Model.BuiltInPackage ? Resources.MessageNeedToRestartAfterUnloadTitle : Resources.MessageNeedToRestartAfterDeleteTitle;

        private string MessageFailedToDeleteOrUnload => Model.BuiltInPackage ? Resources.MessageFailedToUnload : Resources.MessageFailedToDelete;

        private string MessageFailedToDeleteOrUnloadTitle => Model.BuiltInPackage ? Resources.UnloadFailureMessageBoxTitle : Resources.DeleteFailureMessageBoxTitle;

        private void Uninstall()
        {
            if (Model.LoadedAssemblies.Any())
            {
                var resAssem =
                    MessageBoxService.Show(dynamoViewModel.Owner,string.Format(MessageNeedToRestart,
                        dynamoViewModel.BrandingResourceProvider.ProductName),
                        MessageNeedToRestartTitle,
                        MessageBoxButton.OKCancel,
                        MessageBoxImage.Exclamation);
                if (resAssem == MessageBoxResult.Cancel || resAssem == MessageBoxResult.None) return;
            }

            if (!Model.BuiltInPackage)
            {
                var res = MessageBoxService.Show(dynamoViewModel.Owner,String.Format(Resources.MessageConfirmToDeletePackage, this.Model.Name),
                    Resources.MessageNeedToRestartAfterDeleteTitle,
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (res == MessageBoxResult.No || res == MessageBoxResult.None) return;
            }


            try
            {
                Model.UninstallCore(dynamoModel.CustomNodeManager, 
                    dynamoModel.GetPackageManagerExtension().PackageLoader, 
                    dynamoModel.PreferenceSettings);
            }
            catch (Exception)
            {
                MessageBoxService.Show(dynamoViewModel.Owner,string.Format(MessageFailedToDeleteOrUnload,
                    dynamoViewModel.BrandingResourceProvider.ProductName),
                    MessageFailedToDeleteOrUnloadTitle,
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                NotifyLoadStatePropertyChanged();
            }
        }

        private bool CanUninstall()
        {
            if (!Model.InUse(dynamoModel) || Model.LoadedAssemblies.Any())
            {
                return LoadedWithNoScheduledOperation;
            }
            return false;
        }

        // Loads a built-in package that was previously set as Unloaded
        private void Load()
        {
            var packageLoader = dynamoModel.GetPackageManagerExtension().PackageLoader;
            var conflicts = packageLoader.LocalPackages.ToList().Where(x => x.Name == Model.Name && x != Model);
            bool hasConflictsWithLoadedAssemblies = conflicts.Any(x => x.InUse(dynamoModel));

            if (conflicts.Any())
            {
                string conflictsMsg = string.Join(",", conflicts.Select(x => x.Name + " " + x.VersionName));

                // Found conflicting packages
                // 1. If the conflicting packages have assemblies loaded, then we can only mark them as "Scheduled to be deleted" and
                // have the user re-load the built-in package upon restart.
                // 2. If the the conflicting packages do not have any assemblies loaded, then we can immediately
                // delete the conflicting packages and also load the built in one. 
                string msgText = hasConflictsWithLoadedAssemblies ? 
                    string.Format(Resources.MessageLoadBuiltInWithRestartPackage, dynamoViewModel.BrandingResourceProvider.ProductName, 
                        Model.Name + " " + Model.VersionName, conflictsMsg) : 
                    string.Format(Resources.MessageLoadBuiltInPackage, dynamoViewModel.BrandingResourceProvider.ProductName, 
                        Model.Name + " " + Model.VersionName, conflictsMsg);

                var dialogResult = MessageBoxService.Show(msgText,
                        Resources.CannotLoadPackageMessageBoxTitle,
                        MessageBoxButton.OKCancel,
                        MessageBoxImage.Exclamation);

                // Proceed only if we have the user's consent.
                if (dialogResult == MessageBoxResult.Cancel || dialogResult == MessageBoxResult.None ) return;
            }

            try
            {
                foreach (var package in conflicts)
                {
                    // Only markForUninstall is done if any loaded assemblies are found
                    package.UninstallCore(dynamoModel.CustomNodeManager, packageLoader, dynamoModel.PreferenceSettings);
                }

                if (!hasConflictsWithLoadedAssemblies)
                {
                    // Set the package load state as None so that we can try to load it again
                    Model.LoadState.ResetState();

                    packageLoader.LoadPackages(new List<Package>() { Model });
                    dynamoModel.PreferenceSettings.PackageDirectoriesToUninstall.Remove(Model.RootDirectory);
                }
            }
            catch (Exception e) 
            {
                dynamoViewModel.Model.Logger.Log(e);
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

        private bool CanLoad()
        {
            return Model.BuiltInPackage && 
                (Model.LoadState.State == PackageLoadState.StateTypes.Unloaded);
        }

        private void GoToRootDirectory()
        {
            // Check for the existance of RootDirectory
            if (Directory.Exists(Model.RootDirectory))
            {
                Process.Start(new ProcessStartInfo(Model.RootDirectory) { UseShellExecute = true });
                Analytics.TrackEvent(Actions.Open, Categories.PackageManagerOperations, $"{Model?.Name}");
            }
            else
            {
                MessageBoxService.Show(Wpf.Properties.Resources.PackageNotExisted, Wpf.Properties.Resources.DirectoryNotFound, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Deprecate()
        {
            var res = MessageBoxService.Show(String.Format(Resources.MessageToDeprecatePackage, this.Model.Name),
                                      Resources.DeprecatingPackageMessageBoxTitle, 
                                      MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res == MessageBoxResult.No) return;

            packageManagerClient.Deprecate(Model.Name);
        }

        private bool IsOwner()
        {
            if (!CanPublish) return false;
            return packageManagerClient.DoesCurrentUserOwnPackage(Model, dynamoModel.AuthenticationManager.Username);
        }

        private void Undeprecate()
        {
            var res = MessageBoxService.Show(String.Format(Resources.MessageToUndeprecatePackage, this.Model.Name),
                                      Resources.UndeprecatingPackageMessageBoxTitle, 
                                      MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res == MessageBoxResult.No) return;
            packageManagerClient.Undeprecate(Model.Name);
        }

        private bool CanUndeprecate()
        {
            if (!CanPublish) return false;
            return packageManagerClient.DoesCurrentUserOwnPackage(Model, dynamoModel.AuthenticationManager.Username);
        }

        private void PublishNewPackageVersion()
        {
            Model.RefreshCustomNodesFromDirectory(dynamoModel.CustomNodeManager, DynamoModel.IsTestMode);
            var vm = PublishPackageViewModel.FromLocalPackage(dynamoViewModel, Model, true);
            vm.IsNewVersion = true;

            dynamoViewModel.OnRequestPackagePublishDialog(vm);
        }

        private void PublishNewPackage()
        {
            Model.RefreshCustomNodesFromDirectory(dynamoModel.CustomNodeManager, DynamoModel.IsTestMode);
            var vm = PublishPackageViewModel.FromLocalPackage(dynamoViewModel, Model, false);
            vm.IsNewVersion = false;

            dynamoViewModel.OnRequestPackagePublishDialog(vm);
        }

        private void ExecuteWithTou(Action acceptanceCallback)
        {
            // create TermsOfUseHelper object to check asynchronously whether the Terms of Use has 
            // been accepted, and if so, continue to execute the provided Action.
            var termsOfUseCheck = new TermsOfUseHelper(new TermsOfUseHelperParams
            {
                PackageManagerClient = dynamoModel.GetPackageManagerExtension().PackageManagerClient,
                AuthenticationManager = dynamoModel.AuthenticationManager,
                ResourceProvider = dynamoViewModel.BrandingResourceProvider,
                AcceptanceCallback = acceptanceCallback
            });

            termsOfUseCheck.Execute(false);
        }
    }
}
