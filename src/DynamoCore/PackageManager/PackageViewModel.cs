using System;
using System.Windows;

using Dynamo.PackageManager;

using Microsoft.Practices.Prism.Commands;

namespace Dynamo.ViewModels
{
    public class PackageViewModel
    {
        private DynamoViewModel dynamoViewModel;
        public Package Model { get; private set; }

        public DelegateCommand ToggleTypesVisibleInManagerCommand { get; set; }
        public DelegateCommand GetLatestVersionCommand { get; set; }
        public DelegateCommand PublishNewPackageVersionCommand { get; set; }
        public DelegateCommand UninstallCommand { get; set; }
        public DelegateCommand PublishNewPackageCommand { get; set; }
        public DelegateCommand DeprecateCommand { get; set; }
        public DelegateCommand UndeprecateCommand { get; set; }

        public PackageViewModel(DynamoViewModel dynamoViewModel, Package model)
        {
            this.dynamoViewModel = dynamoViewModel;
            this.Model = model;

            ToggleTypesVisibleInManagerCommand = new DelegateCommand(ToggleTypesVisibleInManager, CanToggleTypesVisibleInManager);
            GetLatestVersionCommand = new DelegateCommand(GetLatestVersion, CanGetLatestVersion);
            PublishNewPackageVersionCommand = new DelegateCommand(PublishNewPackageVersion, CanPublishNewPackageVersion);
            PublishNewPackageCommand = new DelegateCommand(PublishNewPackage, CanPublishNewPackage);
            UninstallCommand = new DelegateCommand(Uninstall, CanUninstall);
            DeprecateCommand = new DelegateCommand(this.Deprecate, CanDeprecate);
            UndeprecateCommand = new DelegateCommand(this.Undeprecate, CanUndeprecate);

            this.dynamoViewModel.Model.NodeAdded += (node) => UninstallCommand.RaiseCanExecuteChanged();
            this.dynamoViewModel.Model.NodeDeleted += (node) => UninstallCommand.RaiseCanExecuteChanged();
            this.dynamoViewModel.Model.WorkspaceHidden += (ws) => UninstallCommand.RaiseCanExecuteChanged();
            this.dynamoViewModel.Model.Workspaces.CollectionChanged += (sender, args) => UninstallCommand.RaiseCanExecuteChanged();
        }

        private void Uninstall()
        {
            var res = MessageBox.Show("Are you sure you want to uninstall " + this.Model.Name + "?  This will delete the packages root directory.\n\n You can always redownload the package.", "Uninstalling Package", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res == MessageBoxResult.No) return;

            try
            {
                Model.UninstallCore();
            }
            catch (Exception e)
            {
                MessageBox.Show("Dynamo failed to uninstall the package.  You may need to delete the package's root directory manually.", "Uninstall Failure", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanUninstall()
        {
            return !Model.InUse();
        }

        private void Deprecate()
        {
            var res = MessageBox.Show("Are you sure you want to deprecate " + this.Model.Name + "?  This request will be rejected if you are not a maintainer of the package.  It indicates that you will no longer support the package, although the package will still appear when explicitly searched for.  \n\n You can always undeprecate the package.", "Deprecating Package", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res == MessageBoxResult.No) return;

            dynamoViewModel.Model.PackageManagerClient.Deprecate(this.Model.Name);
        }

        private bool CanDeprecate()
        {
            return true;
        }

        private void Undeprecate()
        {
            var res = MessageBox.Show("Are you sure you want to undeprecate " + this.Model.Name + "?  This request will be rejected if you are not a maintainer of the package.  It indicates that you will continue to support the package and the package will appear when users are browsing packages.  \n\n You can always re-deprecate the package.", "Removing Package Deprecation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res == MessageBoxResult.No) return;

            dynamoViewModel.Model.PackageManagerClient.Undeprecate(this.Model.Name);
        }

        private bool CanUndeprecate()
        {
            return true;
        }

        private void PublishNewPackageVersion()
        {
            this.Model.RefreshCustomNodesFromDirectory();
            var vm = PublishPackageViewModel.FromLocalPackage(dynamoViewModel, this.Model);
            vm.IsNewVersion = true;

            dynamoViewModel.OnRequestPackagePublishDialog(vm);
        }

        private bool CanPublishNewPackageVersion()
        {
            return true;
        }

        private void PublishNewPackage()
        {
            this.Model.RefreshCustomNodesFromDirectory();
            var vm = PublishPackageViewModel.FromLocalPackage(dynamoViewModel, this.Model);
            vm.IsNewVersion = false;

            dynamoViewModel.OnRequestPackagePublishDialog(vm);
        }

        private bool CanPublishNewPackage()
        {
            return true;
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
            this.Model.TypesVisibleInManager = !this.Model.TypesVisibleInManager;
        }

        private bool CanToggleTypesVisibleInManager()
        {
            return true;
        }
    }
}
