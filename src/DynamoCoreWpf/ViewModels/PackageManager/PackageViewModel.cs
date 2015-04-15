﻿using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using Dynamo.Models;
using Dynamo.PackageManager;

using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.ViewModel;
using Dynamo.Wpf.Properties;

namespace Dynamo.ViewModels
{
    public class PackageViewModel : NotificationObject
    {
        private readonly DynamoViewModel dynamoViewModel;
        public Package Model { get; private set; }

        public bool HasAdditionalFiles
        {
            get { return Model.AdditionalFiles.Any(); }
        }

        public bool HasAdditionalAssemblies
        {
            get { return Model.LoadedAssemblies.Any(x => !x.IsNodeLibrary); }
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

        public DelegateCommand ToggleTypesVisibleInManagerCommand { get; set; }
        public DelegateCommand GetLatestVersionCommand { get; set; }
        public DelegateCommand PublishNewPackageVersionCommand { get; set; }
        public DelegateCommand UninstallCommand { get; set; }
        public DelegateCommand PublishNewPackageCommand { get; set; }
        public DelegateCommand DeprecateCommand { get; set; }
        public DelegateCommand UndeprecateCommand { get; set; }
        public DelegateCommand UnmarkForUninstallationCommand { get; set; }
        public DelegateCommand GoToRootDirectoryCommand { get; set; }

        public PackageViewModel(DynamoViewModel dynamoViewModel, Package model)
        {
            this.dynamoViewModel = dynamoViewModel;
            Model = model;

            ToggleTypesVisibleInManagerCommand = new DelegateCommand(ToggleTypesVisibleInManager, CanToggleTypesVisibleInManager);
            GetLatestVersionCommand = new DelegateCommand(GetLatestVersion, CanGetLatestVersion);
            PublishNewPackageVersionCommand = new DelegateCommand(() => PublishNewPackageVersion(DynamoModel.IsTestMode), CanPublishNewPackageVersion);
            PublishNewPackageCommand = new DelegateCommand(() => PublishNewPackage(DynamoModel.IsTestMode), CanPublishNewPackage);
            UninstallCommand = new DelegateCommand(Uninstall, CanUninstall);
            DeprecateCommand = new DelegateCommand(Deprecate, CanDeprecate);
            UndeprecateCommand = new DelegateCommand(Undeprecate, CanUndeprecate);
            UnmarkForUninstallationCommand = new DelegateCommand(UnmarkForUninstallation, CanUnmarkForUninstallation);
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

        private void ModelOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "MarkedForUninstall")
            {
                UnmarkForUninstallationCommand.RaiseCanExecuteChanged();
                UninstallCommand.RaiseCanExecuteChanged();
            }
        }

        private void UnmarkForUninstallation()
        {
            Model.UnmarkForUninstall( dynamoViewModel.Model.PreferenceSettings );
        }

        private bool CanUnmarkForUninstallation()
        {
            return Model.MarkedForUninstall;
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

            var res = MessageBox.Show(String.Format(Resources.MessageConfirmToUninstallPackage, this.Model.Name),
                                      Resources.UninstallingPackageMessageBoxTitle,
                                      MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (res == MessageBoxResult.No) return;

            try
            {
                var dynModel = dynamoViewModel.Model;
                Model.UninstallCore(dynModel.CustomNodeManager, dynModel.PackageLoader, dynModel.PreferenceSettings);
            }
            catch (Exception)
            {
                MessageBox.Show(string.Format(Resources.MessageFailedToUninstall,
                    dynamoViewModel.BrandingResourceProvider.ProductName),
                    Resources.UninstallFailureMessageBoxTitle,
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanUninstall()
        {
            return (!Model.InUse(dynamoViewModel.Model) || Model.LoadedAssemblies.Any()) 
                && !Model.MarkedForUninstall;
        }

        private void GoToRootDirectory()
        {
            Process.Start(Model.RootDirectory);
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

            dynamoViewModel.Model.PackageManagerClient.Deprecate(Model.Name);
        }

        private bool CanDeprecate()
        {
            return dynamoViewModel.Model.PackageManagerClient.HasAuthProvider;
        }

        private void Undeprecate()
        {
            var res = MessageBox.Show(String.Format(Resources.MessageToUndeprecatePackage, this.Model.Name),
                                      Resources.UndeprecatingPackageMessageBoxTitle, 
                                      MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res == MessageBoxResult.No) return;

            dynamoViewModel.Model.PackageManagerClient.Undeprecate(Model.Name);
        }

        private bool CanUndeprecate()
        {
            return dynamoViewModel.Model.PackageManagerClient.HasAuthProvider;
        }

        private void PublishNewPackageVersion(bool isTestMode)
        {
            Model.RefreshCustomNodesFromDirectory(dynamoViewModel.Model.CustomNodeManager, isTestMode);
            var vm = PublishPackageViewModel.FromLocalPackage(dynamoViewModel, Model);
            vm.IsNewVersion = true;

            dynamoViewModel.OnRequestPackagePublishDialog(vm);
        }

        private bool CanPublishNewPackageVersion()
        {
            return dynamoViewModel.Model.PackageManagerClient.HasAuthProvider;
        }

        private void PublishNewPackage(bool isTestMode)
        {
            Model.RefreshCustomNodesFromDirectory(dynamoViewModel.Model.CustomNodeManager, isTestMode);
            var vm = PublishPackageViewModel.FromLocalPackage(dynamoViewModel, Model);
            vm.IsNewVersion = false;

            dynamoViewModel.OnRequestPackagePublishDialog(vm);
        }

        private bool CanPublishNewPackage()
        {
            return dynamoViewModel.Model.PackageManagerClient.HasAuthProvider;
        }

        private void GetLatestVersion()
        {
            throw new NotImplementedException();
        }

        private bool CanGetLatestVersion()
        {
            return false;
        }

        private void ToggleTypesVisibleInManager()
        {
            Model.TypesVisibleInManager = !Model.TypesVisibleInManager;
        }

        private bool CanToggleTypesVisibleInManager()
        {
            return true;
        }
    }
}
