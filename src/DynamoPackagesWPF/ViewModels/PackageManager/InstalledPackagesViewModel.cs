using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Dynamo.PackageManager;
using Dynamo.Utilities;
using Dynamo.Wpf.Properties;
using NotificationObject = Dynamo.Core.NotificationObject;

namespace Dynamo.ViewModels
{
    /// <summary>
    /// Package Filter model
    /// </summary>
    public class PackageFilter : NotificationObject
    {
        private bool isChecked;

        /// <summary>
        /// Create a package filter
        /// </summary>
        /// <param name="name">Name of filter</param>
        /// <param name="viewModel">Back pointer to parent view model</param>
        public PackageFilter(string name, InstalledPackagesViewModel viewModel)
        {
            Name = name;
            ViewModel = viewModel;
        }

        /// <summary>
        /// Create a package filter for All
        /// </summary>
        /// <param name="viewModel">Back pointer to parent view model</param>
        public PackageFilter(InstalledPackagesViewModel viewModel)
        {
            Name = Resources.PackageFilter_Name_All;
            ViewModel = viewModel;
        }


        /// <summary>
        /// Filter name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Back pointer to owner view model
        /// </summary>
        public InstalledPackagesViewModel ViewModel { get; private set; }

        /// <summary>
        /// True if this filter is active
        /// </summary>
        public bool IsChecked
        {
            get => isChecked;
            set
            {
                if (isChecked != value)
                {
                    isChecked = value;
                    RaisePropertyChanged(nameof(IsChecked));
                }
            }
        }
    }

    /// <summary>
    /// View model for installed package control
    /// </summary>
    public class InstalledPackagesViewModel : NotificationObject
    {
        /// <summary>
        /// The current filtered list of local packages
        /// </summary>
        public ObservableCollection<PackageViewModel> LocalPackages { get; } = new ObservableCollection<PackageViewModel>();

        /// <summary>
        /// Possible filters to use for the filtered list of packages
        /// </summary>
        public ObservableCollection<PackageFilter> Filters { get; } = new ObservableCollection<PackageFilter>();

        private readonly DynamoViewModel dynamoViewModel;

        /// <summary>
        /// Back pointer to the Package Loader
        /// </summary>
        public PackageLoader Model { get; private set; }

        /// <summary>
        /// Create a new view model for installed packages
        /// </summary>
        /// <param name="dynamoViewModel">Back pointer to the dynamo view model</param>
        /// <param name="model">Back pointer to the package loader</param>
        public InstalledPackagesViewModel(DynamoViewModel dynamoViewModel, PackageLoader model)
        {
            this.Model = model;
            this.dynamoViewModel = dynamoViewModel;

            InitializeLocalPackages();
            PopulateFilters();
        }

        /// <summary>
        /// Filter the list of local packages based on the current filter selection
        /// </summary>
        internal void ApplyPackageFilter()
        {
            ClearPackagesViewModel();


            if (NoActiveFilterSelected)
            {
                LocalPackages.AddRange(Model.LocalPackages.Select(NewPackageViewModel));
            }
            else
            {
                var currentFilter = CurrentFilterSelection;
                foreach (var pkg in Model.LocalPackages)
                {
                    var viewModel = NewPackageViewModel(pkg);
                    if (viewModel.PackageLoadStateText == currentFilter)
                    {
                        LocalPackages.Add(viewModel);
                    }
                    else
                    {
                        ClearPackageViewModel(viewModel);
                    }
                }
            }
            RaisePropertyChanged(nameof(LocalPackages));
        }

        /// <summary>
        /// Populate the list of available filters based on the packages currently known by the dynamo view model
        /// </summary>
        internal void PopulateFilters()
        {
            var currentSelection = CurrentFilterSelection;
            Filters.Clear();
            var filterNames = Model.LocalPackages
                .Where(pkg =>
                    !(pkg.LoadState.ScheduledState == PackageLoadState.ScheduledTypes.None &&
                      pkg.LoadState.State == PackageLoadState.StateTypes.None))
                .Select(pkg => new PackageViewModel(dynamoViewModel, pkg)).Select(vm => vm.PackageLoadStateText)
                .Distinct();
            Filters.AddRange(filterNames.Select(f => new PackageFilter(f, this)));

            if (Filters.Any())
            {
                Filters.Insert(0, new PackageFilter(this));
            }

            ResetCurrentSelection(currentSelection);

            RaisePropertyChanged(nameof(Filters));
        }

        private string CurrentFilterSelection => Filters.FirstOrDefault(f => f.IsChecked)?.Name;

        private bool NoActiveFilterSelected
        {
            get
            {
                var currentFilter = CurrentFilterSelection;
                return currentFilter == null || currentFilter == Resources.PackageFilter_Name_All;
            }
        }

        private bool IsPackageVisible(Package pkg)
        {
            var currentFilter = CurrentFilterSelection;
            var pkgViewModel = new PackageViewModel(dynamoViewModel, pkg);
            return NoActiveFilterSelected || pkgViewModel.PackageLoadStateText == currentFilter;
        }

        private PackageViewModel NewPackageViewModel(Package pkg)
        {
            var viewModel = new PackageViewModel(dynamoViewModel, pkg);
            viewModel.Model.PropertyChanged += PackageModelOnPropertyChanged;
            return viewModel;
        }

        private void ClearPackageViewModel(PackageViewModel viewModel)
        {
            viewModel.Model.PropertyChanged -= PackageModelOnPropertyChanged;
        }

        private void ClearPackagesViewModel()
        {
            foreach (var viewModel in LocalPackages)
            {
                ClearPackageViewModel(viewModel);
            }

            LocalPackages.Clear();
        }

        private void InitializeLocalPackages()
        {
            ClearPackagesViewModel();
            LocalPackages.AddRange(Model.LocalPackages.Select(NewPackageViewModel));

            Model.PackageAdded += (pkg) =>
            {
                if (IsPackageVisible(pkg))
                {
                    var viewModel = NewPackageViewModel(pkg);
                    LocalPackages.Add(viewModel);
                    RaisePropertyChanged(nameof(LocalPackages));
                    PopulateFilters();
                }
            };

            Model.PackageRemoved += (pkg) =>
            {
                if (IsPackageVisible(pkg))
                {
                    var viewModel = LocalPackages.First(x => x.Model == pkg);
                    ClearPackageViewModel(viewModel);
                    LocalPackages.Remove(viewModel);
                    RaisePropertyChanged(nameof(LocalPackages));
                    PopulateFilters();
                }
            };
        }

        private void PackageModelOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "LoadState")
            {
                PopulateFilters();
                ApplyPackageFilter();
            }
        }

        private void ResetCurrentSelection(string selection)
        {
            if (!Filters.Any())
            {
                return;
            }

            if (selection == null)
            {
                Filters[0].IsChecked = true;
                return;
            }

            var selected = Filters.FirstOrDefault(f => f.Name == selection);
            if (selected != null)
            {
                selected.IsChecked = true;
                return;
            }

            Filters[0].IsChecked = true;
        }
    }
}
