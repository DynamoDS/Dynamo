using System;
using System.Collections.ObjectModel;
using System.Linq;
using Dynamo.PackageManager.ViewModels;
using Dynamo.UI.Commands;
using Dynamo.ViewModels;
using NotificationObject = Dynamo.Core.NotificationObject;

namespace Dynamo.PackageManager
{
    public class PackageManagerViewModel : NotificationObject, IDisposable
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
        ///
        private PublishPackageViewModel publishPackageViewModel;
        public PublishPackageViewModel PublishPackageViewModel
        {
            get { return publishPackageViewModel; }
            set
            {
                if (publishPackageViewModel != value)
                {
                    publishPackageViewModel = value;
                    RaisePropertyChanged(nameof(PublishPackageViewModel));
                }
            }
        }

        /// <summary>
        /// Returns all installed packages
        /// </summary>
        public ObservableCollection<PackageViewModel> LocalPackages => installedPackagesViewModel.LocalPackages;

        /// <summary>
        /// Returns all available filters
        /// </summary>
        public ObservableCollection<PackageFilter> Filters => installedPackagesViewModel.Filters;

        public DelegateCommand PublishNewVersionCommand { get; set; }

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

            PublishNewVersionCommand = new DelegateCommand(PublishNewPackageVersionRelayCommand);
        }


        private void PublishNewPackageVersionRelayCommand(object obj)
        {
            var searchElement = obj as PackageManagerSearchElementViewModel;
            if(searchElement != null)
            {
                var localPackage = LocalPackages.First(x => x.Model.Name.Equals(searchElement.Name));

                if (localPackage == null) { return; }
                localPackage.PublishNewPackageVersionCommand.Execute();
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

        /// <summary>
        /// Dispose method of the PackageManagerViewModel
        /// </summary>
        public void Dispose()
        {
            if (LocalPackages == null) return;

            LocalPackages.CollectionChanged -= LocalPackages_CollectionChanged;
        }
    }
}
