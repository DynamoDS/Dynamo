using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Dynamo.PackageManager;
using Dynamo.Utilities;
using Dynamo.Wpf.Properties;
using Microsoft.Practices.Prism.ViewModel;

namespace Dynamo.ViewModels
{
    public class PackageFilter : NotificationObject
    {
        private bool isChecked;

        public PackageFilter(PackageLoadState loadState, InstalledPackagesViewModel viewModel)
        {
            LoadState = loadState;
            ViewModel = viewModel;
        }

        public PackageLoadState LoadState { get; private set; }
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

        public InstalledPackagesViewModel ViewModel { get; private set; }

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

    public class InstalledPackagesViewModel : NotificationObject
    {
        public ObservableCollection<PackageViewModel> LocalPackages { get; } = new ObservableCollection<PackageViewModel>();

        public ObservableCollection<PackageFilter> Filters { get; } = new ObservableCollection<PackageFilter>();

        private readonly DynamoViewModel dynamoViewModel;
        public PackageLoader Model { get; private set; }

        public InstalledPackagesViewModel(DynamoViewModel dynamoViewModel, PackageLoader model)
        {
            this.Model = model;
            this.dynamoViewModel = dynamoViewModel;

            InitializeLocalPackages();
            PopulateFilters();
        }

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
