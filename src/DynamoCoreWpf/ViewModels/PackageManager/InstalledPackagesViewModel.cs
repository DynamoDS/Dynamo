using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Dynamo.PackageManager;
using Dynamo.Utilities;
using Dynamo.Wpf.Properties;
using Microsoft.Practices.Prism.ViewModel;

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
        /// <param name="loadState">Load State to filter</param>
        /// <param name="viewModel">Back pointer to parent view model</param>
        public PackageFilter(PackageLoadState loadState, InstalledPackagesViewModel viewModel)
        {
            LoadState = loadState;
            ViewModel = viewModel;
        }

        /// <summary>
        /// Load state used for filtering
        /// </summary>
        public PackageLoadState LoadState { get; private set; }
        
        /// <summary>
        /// Filter name
        /// </summary>
        public string Name
        {
            get
            {
                switch (LoadState.ScheduledState)
                {
                    case PackageLoadState.ScheduledTypes.ScheduledForUnload: return Resources.PackageStateScheduledForUnload;
                    case PackageLoadState.ScheduledTypes.ScheduledForDeletion: return Resources.PackageStateScheduledForDeletion;
                    default:
                        break;
                }

                switch (LoadState.State)
                {
                    case PackageLoadState.StateTypes.Unloaded: return Resources.PackageStateUnloaded;
                    case PackageLoadState.StateTypes.Loaded: return Resources.PackageStateLoaded;
                    case PackageLoadState.StateTypes.Error: return Resources.PackageStateError;
                    default:
                        return Resources.PackageFilter_Name_All;
                }
            }
        }

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
                LocalPackages.AddRange(Model.LocalPackages.Select(NewPackageViewModel).ToObservableCollection());
            }
            else
            {
                var filter = CurrentFilterSelection;
                LocalPackages.AddRange(Model.LocalPackages
                     .Where(pkg =>
                         pkg.LoadState.State == filter.State && pkg.LoadState.ScheduledState == filter.ScheduledState)
                     .Select(NewPackageViewModel).ToObservableCollection());
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
            var loadStates = Model.LocalPackages.Select(pkg => pkg.LoadState)
                .GroupBy(f => new { f.State, f.ScheduledState }).Select(f => f.FirstOrDefault());
            Filters.AddRange(loadStates.Select(f => new PackageFilter(f, this)).ToObservableCollection());

            if (Filters.Any())
            {
                Filters.Insert(0, new PackageFilter(new PackageLoadState(), this));
            }

            ResetCurrentSelection(currentSelection);

            RaisePropertyChanged(nameof(Filters));
        }

        private PackageLoadState CurrentFilterSelection => Filters.FirstOrDefault(f => f.IsChecked)?.LoadState;

        private bool NoActiveFilterSelected
        {
            get
            {
                var currentFilter = CurrentFilterSelection;
                return currentFilter == null || (currentFilter.State == PackageLoadState.StateTypes.None &&
                                                 currentFilter.ScheduledState == PackageLoadState.ScheduledTypes.None);
            }
        }

        private bool IsPackageVisible(Package pkg)
        {
            var currentFilter = CurrentFilterSelection;
            return NoActiveFilterSelected || (pkg.LoadState.State == currentFilter.State &&
                   pkg.LoadState.ScheduledState == currentFilter.ScheduledState);
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
            LocalPackages.AddRange(Model.LocalPackages.Select(pkg => NewPackageViewModel(pkg)).ToObservableCollection());

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

        private void ResetCurrentSelection(PackageLoadState selection)
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

            var selected = Filters.FirstOrDefault(f =>
                f.LoadState.State == selection.State && f.LoadState.ScheduledState == selection.ScheduledState);
            if (selected != null)
            {
                selected.IsChecked = true;
                return;
            }

            Filters[0].IsChecked = true;
        }
    }
}
