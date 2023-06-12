using Dynamo.UI;
using Dynamo.ViewModels;
using Dynamo.Wpf.UI.GuidedTour;
using Dynamo.Wpf.Utilities;
using DynamoUtilities;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.ViewModel;
using System.Collections.ObjectModel;
using System.Windows;

namespace Dynamo.PackageManager
{
    public class PackageManagerViewModel : NotificationObject
    {
        private DynamoViewModel dynamoViewModel;
        private InstalledPackagesViewModel installedPackagesViewModel;

        public PreferencesViewModel PreferencesViewModel { get; set; }
        public PackageManagerSearchViewModel PkgSearchVM { get; set; }
        public PublishPackageViewModel PubPkgVM { get; set; }

        /// <summary>
        /// Returns all installed packages
        /// </summary>
        public ObservableCollection<PackageViewModel> LocalPackages => installedPackagesViewModel.LocalPackages;

        /// <summary>
        /// Returns all available filters
        /// </summary>
        public ObservableCollection<PackageFilter> Filters => installedPackagesViewModel.Filters;

        public PackageManagerViewModel(DynamoViewModel dynamoViewModel, PackageManagerSearchViewModel PkgSearchVM)
        {
            this.dynamoViewModel = dynamoViewModel;            
            this.PkgSearchVM = PkgSearchVM;            

            this.PreferencesViewModel = dynamoViewModel.PreferencesViewModel;
            var pathmanager = PreferencesViewModel.PackagePathsViewModel;

            if (PubPkgVM == null)
            {
                PubPkgVM = new PublishPackageViewModel(dynamoViewModel);
            }

            InitializeInstalledPackages();

            PkgSearchVM.RegisterTransientHandlers();
        }

        private void InitializeInstalledPackages()
        {
            if (this.dynamoViewModel.PackageManagerClientViewModel != null)
            {
                installedPackagesViewModel = new InstalledPackagesViewModel(dynamoViewModel, dynamoViewModel.PackageManagerClientViewModel.PackageManagerExtension.PackageLoader);
                installedPackagesViewModel?.PopulateFilters();
            }
        }

    }
}
