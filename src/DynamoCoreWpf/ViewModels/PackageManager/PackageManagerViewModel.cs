using System;
using System.Collections.ObjectModel;
using Dynamo.ViewModels;
using NotificationObject = Dynamo.Core.NotificationObject;

namespace Dynamo.PackageManager
{
    public class PackageManagerViewModel : NotificationObject
    {
        private DynamoViewModel dynamoViewModel;
        private InstalledPackagesViewModel installedPackagesViewModel;
        private double width = 1076;
        private double height = 718;

        /// <summary>
        /// PreferenceViewModel containing the PackageManager paths and installed packages
        /// </summary>
        public PreferencesViewModel PreferencesViewModel { get; set; }

        /// <summary>
        /// PackageManagerSearchViewModel functionality to load Dynamo packages
        /// </summary>
        public PackageManagerSearchViewModel PackageSearchViewModel { get; set; }

        /// <summary>
        /// PublishPackageViewModel containing information about all the published packages
        /// </summary>
        public PublishPackageViewModel PublishPackageViewModel { get; set; }

        /// <summary>
        /// Returns all installed packages
        /// </summary>
        public ObservableCollection<PackageViewModel> LocalPackages => installedPackagesViewModel.LocalPackages;

        /// <summary>
        /// Returns all available filters
        /// </summary>
        public ObservableCollection<PackageFilter> Filters => installedPackagesViewModel.Filters;


        //Width of the PackageManagerView the default value is 1076
        public double Width
        {
            get
            {
                return width;
            }
            set
            {
                width = value;
                RaisePropertyChanged(nameof(Width));
            }
        }

        //Height of the PackageManagerView the default value is 718
        public double Height
        {
            get
            {
                return height;
            }
            set
            {
                height = value;
                RaisePropertyChanged(nameof(Height));
            }
        }

        public PackageManagerViewModel(DynamoViewModel dynamoViewModel, PackageManagerSearchViewModel PkgSearchVM)
        {
            this.dynamoViewModel = dynamoViewModel;
            this.PackageSearchViewModel = PkgSearchVM;            

            this.PreferencesViewModel = dynamoViewModel.PreferencesViewModel;
            var pathmanager = PreferencesViewModel.PackagePathsViewModel;

            if (PublishPackageViewModel == null)
            {
                PublishPackageViewModel = new PublishPackageViewModel(dynamoViewModel);
            }

            InitializeInstalledPackages();

            PkgSearchVM.RegisterTransientHandlers();

            LocalPackages.CollectionChanged += LocalPackages_CollectionChanged;

            // We are forced to make the update ourselves if the Preferences ViewModel has not been initialized yet
            if (String.IsNullOrEmpty(PreferencesViewModel?.SelectedPackagePathForInstall))
            {
                PreferencesViewModel.SelectedPackagePathForInstall = dynamoViewModel.PreferenceSettings.SelectedPackagePathForInstall;
            }
        }

        private void LocalPackages_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(LocalPackages));
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
