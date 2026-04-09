using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using Dynamo.Configuration;
using Dynamo.Interfaces;
using Dynamo.Logging;
using Dynamo.Models;
using Dynamo.PackageManager.ViewModels;
using Dynamo.Search;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Wpf.Properties;
using Dynamo.Wpf.Utilities;
using Greg.Responses;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Prism.Commands;
using NotificationObject = Dynamo.Core.NotificationObject;

namespace Dynamo.PackageManager
{
    public class PackageManagerSearchViewModel : NotificationObject
    {
        #region enums
        /// <summary>
        /// Enum for the Package search state
        /// </summary>
        public enum PackageSearchState
        {
            Syncing,
            Searching,
            NoResults,
            Results,
            Retry
        };

        /// <summary>
        /// Enum for the Package sort key, utilized by sorting context menu
        /// </summary>
        public enum PackageSortingKey
        {
            Name,
            Downloads,
            Votes,
            Maintainers,    
            LastUpdate,
            Search
        };

        /// <summary>
        /// Enum for the Package sort direction, utilized by sorting context menu
        /// </summary>
        public enum PackageSortingDirection
        {
            Ascending,
            Descending
        };
        #endregion enums

        /// <summary>
        /// Package Manager filter entry, binded to the host filter context menu
        /// </summary>
        public class FilterEntry : NotificationObject
        {
            /// <summary>
            /// Name of the host
            /// </summary>
            public string FilterName { get; set; }

            /// <summary>
            /// The context sub-menu name the filter belongs to
            /// </summary>
            public string GroupName { get; set; }

            /// <summary>
            /// The tooltip associated with the filter entry
            /// </summary>
            public string Tooltip { get; set; }

            /// <summary>
            /// Controls the IsEnabled status of the filter
            /// </summary>
            private bool isEnabled = true;

            public bool IsEnabled
            {
                get { return isEnabled; }
                set
                {
                    isEnabled = value;
                    RaisePropertyChanged(nameof(IsEnabled));
                }
            }


            /// <summary>
            /// Filter entry click command, notice this is a dynamic command
            /// with command param set to FilterName so that the code is robust
            /// in a way UI could handle as many hosts as possible
            /// </summary>
            public DelegateCommand<object> FilterCommand { get; set; }

            private bool _onChecked;
            /// <summary>
            /// Boolean indicates if the Filter entry is checked, data binded to
            /// is checked property of each filter
            /// </summary>
            public bool OnChecked
            {
                get { return _onChecked; }
                set
                {
                    _onChecked = value;

                    RaisePropertyChanged(nameof(OnChecked));
                    pmSearchViewModel.SetFilterChange();
                }
            }

            /// <summary>
            /// Private reference of PackageManagerSearchViewModel,
            /// used in the FilterCommand to filter search results
            /// </summary>
            private PackageManagerSearchViewModel pmSearchViewModel;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="filterName">Filter name, same as host name</param>
            /// <param name="packageManagerSearchViewModel">a reference of the PackageManagerSearchViewModel</param>
            public FilterEntry(string filterName, string groupName, string tooltip, PackageManagerSearchViewModel packageManagerSearchViewModel)
            {
                FilterName = filterName;
                GroupName = groupName;
                Tooltip = tooltip;
                FilterCommand = new DelegateCommand<object>(SetFilterHosts, CanSetFilterHosts);
                pmSearchViewModel = packageManagerSearchViewModel;
                OnChecked = false;
            }

            /// <summary>
            /// Each filter is enabled for now, we may enable more smartly in the future
            /// </summary>
            /// <param name="arg"></param>
            /// <returns></returns>
            private bool CanSetFilterHosts(object arg)
            {
                return true;
            }

            /// <summary>
            /// This function will adjust the SelectedHosts in the SearchViewModel
            /// Affecting search results globally
            /// </summary>
            /// <param name="obj"></param>
            private void SetFilterHosts(object obj)
            {
                if(GroupName.Equals(Resources.PackageFilterByHost))
                {
                    if (OnChecked)
                    {
                        pmSearchViewModel.SelectedHosts.Add(obj as string);
                    }
                    else
                    {
                        pmSearchViewModel.SelectedHosts.Remove(obj as string);
                    }
                    // Send filter event with what host filter user using
                    Dynamo.Logging.Analytics.TrackEvent(
                        Actions.Filter,
                        Categories.PackageManagerOperations,
                        string.Join(",", pmSearchViewModel.SelectedHosts));
                }
                if(GroupName.Equals(Resources.PackageFilterByCompatibility))
                {
                    // Send filter event with what compatibility state filter user using
                    Dynamo.Logging.Analytics.TrackEvent(
                        Actions.Filter,
                        Categories.PackageManagerOperations,
                        pmSearchViewModel.CompatibilityFilter.FirstOrDefault(x=>x.OnChecked)?.FilterName);
                }
                pmSearchViewModel.SearchAndUpdateResults();
                return;
            }
        }

        #region Properties & Fields
        // Lucene search utility to perform indexing operations.
        internal LuceneSearchUtility LuceneUtility
        {
            get
            {
                return LuceneSearch.LuceneUtilityPackageManager;
            }
        }

        private ObservableCollection<PackageManagerSearchElementViewModel> searchMyResults;

        // The results of the last synchronization with the package manager server
        public List<PackageManagerSearchElement> LastSync { get; set; }

        /// <summary>
        /// Stores a list of latest infected package versions published by the current user, if any.
        /// </summary>
        public ObservableCollection<PackageManagerSearchElement> InfectedPackages { get; set; }

        /// <summary>
        ///     SortingKey property
        /// </summary>
        /// <value>
        ///     Set which kind of sorting should be used for displaying search results
        /// </value>
        public PackageSortingKey _sortingKey; // TODO: Set private for 3.0.
        public PackageSortingKey SortingKey
        {
            get { return _sortingKey; }
            set
            {
                _sortingKey = value;
                RaisePropertyChanged(nameof(SortingKey));
            }
        }

        private List<FilterEntry> hostFilter;

        /// <summary>
        /// Dynamic Filter for package hosts, should include all Dynamo known hosts from PM backend
        ///  e.g. "Advance Steel", "Alias", "Civil 3D", "FormIt", "Revit"
        /// </summary>
        public List<FilterEntry> HostFilter
        {
            get { return hostFilter; }
            set
            {
                hostFilter = value;
                RaisePropertyChanged(nameof(HostFilter));
            }
        }

        private List<FilterEntry> nonHostFilter;

        /// <summary>
        /// A collection of dynamic non-hosted filters
        /// such as New, Updated, Deprecated, Has/HasNoDependencies
        /// </summary>
        public List<FilterEntry> NonHostFilter
        {
            get { return nonHostFilter; }
            set
            {
                nonHostFilter = value;
                RaisePropertyChanged(nameof(NonHostFilter));
            }
        }

        private List<FilterEntry> compatibilityFilter;

        /// <summary>
        /// Compatibility filters (Compatible, Non-compatible, Unknown)
        /// </summary>
        public List<FilterEntry> CompatibilityFilter
        {
            get { return compatibilityFilter; }
            set
            {
                compatibilityFilter = value;
                RaisePropertyChanged(nameof(CompatibilityFilter));
            }
        }

        /// <summary>
        ///     SortingDirection property
        /// </summary>
        /// <value>
        ///     Set which kind of sorting should be used for displaying search results
        /// </value>
        public PackageSortingDirection _sortingDirection; // TODO: Set private for 3.0.
        public PackageSortingDirection SortingDirection
        {
            get { return _sortingDirection; }
            set
            {
                _sortingDirection = value;
                RaisePropertyChanged(nameof(SortingDirection));
            }
        }

        /// <summary>
        /// The string that is displayed in the search box prompt depending on the search state.
        /// </summary>
        public string SearchBoxPrompt
        {
            get
            {
                if (SearchState == PackageSearchState.Syncing)
                {
                    return Resources.PackageSearchViewSearchTextBoxSyncing;
                }
                return Resources.PackageSearchViewSearchTextBox;
            }
        }

        /// <summary>
        /// Determines whether the search text box should be displayed.
        /// <para>
        /// Returns false if the search state is syncing, 
        /// </para>
        /// </summary>
        public bool ShowSearchText
        {
            get
            {
                if (SearchState == PackageSearchState.Syncing)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        /// <summary>
        ///     SearchText property
        /// </summary>
        /// <value>
        ///     This is the core UI for Dynamo, primarily used for logging.
        /// </value>
        public string _SearchText; // TODO: Set private for 3.0.
        public string SearchText
        {
            get { return _SearchText; }
            set
            {
                if(_SearchText != value)
                {
                    _SearchText = value;
                    SearchAndUpdateResults();
                    RaisePropertyChanged(nameof(SearchText));
                }
            }
        }

        /// <summary>
        ///     SelectedIndex property
        /// </summary>
        /// <value>
        ///     This is the currently selected element in the UI.
        ///     No initially selected item by setting to -1
        /// </value>
        private int _selectedIndex = -1;
        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                if (_selectedIndex != value)
                {
                    _selectedIndex = value;
                    RaisePropertyChanged(nameof(SelectedIndex));
                }
            }
        }

        /// <summary>
        ///     SearchResults property
        /// </summary>
        /// <value>
        ///     This property is observed by SearchView to see the search results
        /// </value>
        public ObservableCollection<PackageManagerSearchElementViewModel> SearchResults { get; internal set; }

        /// <summary>
        /// Returns a new filtered collection of packages based on the current user
        /// </summary>
        public ObservableCollection<PackageManagerSearchElementViewModel> SearchMyResults
        {
            set
            {                
                searchMyResults = value;
                RaisePropertyChanged(nameof(SearchMyResults));                
            }
            get
            {
                return searchMyResults;
            }
        }

        /// <summary>
        ///     MaxNumSearchResults property
        /// </summary>
        /// <value>
        ///     Internal limit on the number of search results returned by EntryDictionary
        /// </value>
        public int MaxNumSearchResults { get; set; }

        public bool HasDownloads
        {
            get { return PackageManagerClientViewModel.Downloads.Count > 0; }
        }

        public bool HasNoResults
        {
            get { return this.SearchResults.Count == 0; }
        }

        private bool isAnyFilterOn;
        /// <summary>
        /// Returns true if any filter is currently active (set to `true`)
        /// </summary>  
        public bool IsAnyFilterOn
        {
            get
            {
                return isAnyFilterOn;
            }
            set
            {
                if(isAnyFilterOn != value) isAnyFilterOn = value;
                RaisePropertyChanged(nameof(IsAnyFilterOn));
            }
        }

        private void SetFilterChange()
        {
            IsAnyFilterOn = HostFilter.Any(f => f.OnChecked) || NonHostFilter.Any(f => f.OnChecked) || CompatibilityFilter.Any(f => f.OnChecked);
            ApplyFilterRules();
        }

        /// <summary>
        /// Executes any additional logic on the filters
        /// </summary>
        internal void ApplyFilterRules()
        {
            // Enable/disable Compatibility filters if any HostFilter is on
            if (CompatibilityFilter.Any(f => f.OnChecked))
            {
                HostFilter.ForEach(x => x.IsEnabled = false);   
            }
            else
            {
                HostFilter.ForEach(x => x.IsEnabled = true);
            }

            // Enable/disable Host filters if any CompatibilityFilter is on
            if (HostFilter.Any(f => f.OnChecked))
            {
                CompatibilityFilter.ForEach(x => x.IsEnabled = false);
            }
            else
            {
                CompatibilityFilter.ForEach(x => x.IsEnabled = true);
            }
        }

        /// <summary>
        /// Checks if the package can be installed.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        internal bool CanInstallPackage(string name)
        {
            // Return true if there are no matching non built-in packages
            return !PackageManagerClientViewModel.PackageManagerExtension.PackageLoader.LocalPackages
                .Any(x => (x.Name == name) && !x.BuiltInPackage);
        }

        /// <summary>
        /// Checks if the package corresponding to the PackageDownloadHandle can be installed.
        /// </summary>
        /// <param name="dh"></param>
        /// <returns></returns>
        internal bool CanInstallPackage(PackageDownloadHandle dh)
        {
            switch (dh.DownloadState)
            {
                case PackageDownloadHandle.State.Uninitialized:
                case PackageDownloadHandle.State.Error:
                    return true;// Allowed if Download/Install not yet begun or if in Error state.
                case PackageDownloadHandle.State.Downloaded:
                case PackageDownloadHandle.State.Downloading:
                case PackageDownloadHandle.State.Installing:
                    return false;
                default:
                    return CanInstallPackage(dh.Name);// All other states need to check with PackageLoader's LocalPackages
            }
        }

        private void UpdateInstallState(PackageManagerSearchElementViewModel element, bool setDefaultSelection = false)
        {
            if (element?.SearchElementModel == null)
            {
                return;
            }

            var installedPackages = PackageManagerClientViewModel.PackageManagerExtension.PackageLoader.LocalPackages
                .Where(x => (x.Name == element.SearchElementModel.Name) && !x.BuiltInPackage)
                .ToList();

            var installedVersion = installedPackages.Any() ? GetNewestInstalledVersion(installedPackages) : null;
            element.UpdateInstalledVersion(installedVersion, setDefaultSelection);
            element.HasUpdateAvailable = IsUpdateAvailable(installedVersion, element.VersionInformationList);

            var hasBlockingDownload = PackageManagerClientViewModel.Downloads.Any(handle =>
                handle.Name == element.SearchElementModel.Name &&
                (handle.DownloadState == PackageDownloadHandle.State.Downloaded ||
                 handle.DownloadState == PackageDownloadHandle.State.Downloading ||
                 handle.DownloadState == PackageDownloadHandle.State.Installing));
            if (hasBlockingDownload)
            {
                element.CanInstall = false;
                return;
            }
            if (!installedPackages.Any())
            {
                element.CanInstall = true;
                return;
            }
            element.CanInstall = false;
        }

        private void UninstallPackage(PackageManagerSearchElementViewModel element)
        {
            var installedPackage = GetInstalledPackageForSelectedVersion(element);
            if (installedPackage == null)
            {
                return;
            }

            var dynamoViewModel = PackageManagerClientViewModel?.DynamoViewModel;
            if (dynamoViewModel == null)
            {
                return;
            }

            if (installedPackage.LoadedAssemblies.Any())
            {
                var message = string.Format(Resources.MessageNeedToRestartAfterDelete,
                    dynamoViewModel.BrandingResourceProvider.ProductName);
                var title = Resources.MessageNeedToRestartAfterDeleteTitle;
                var result = MessageBoxService.Show(dynamoViewModel.Owner, message, title,
                    MessageBoxButton.OKCancel, MessageBoxImage.Exclamation);
                if (result == MessageBoxResult.Cancel || result == MessageBoxResult.None)
                {
                    return;
                }
            }

            var confirmResult = MessageBoxService.Show(dynamoViewModel.Owner,
                string.Format(Resources.MessageConfirmToDeletePackage, installedPackage.Name),
                Resources.MessageNeedToRestartAfterDeleteTitle,
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirmResult == MessageBoxResult.No || confirmResult == MessageBoxResult.None)
            {
                return;
            }

            try
            {
                installedPackage.UninstallCore(dynamoViewModel.Model.CustomNodeManager,
                    PackageManagerClientViewModel.PackageManagerExtension.PackageLoader,
                    dynamoViewModel.Model.PreferenceSettings);
            }
            catch (Exception ex)
            {
                var baseMessage = string.Format(Resources.MessageFailedToDelete,
                    dynamoViewModel.BrandingResourceProvider.ProductName);
                var detailedMessage = baseMessage + Environment.NewLine + ex.Message;

                MessageBoxService.Show(dynamoViewModel.Owner,
                    detailedMessage,
                    Resources.DeleteFailureMessageBoxTitle,
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                UpdateInstallState(element);
            }
        }

        private bool CanUninstallPackage(PackageManagerSearchElementViewModel element)
        {
            var installedPackage = GetInstalledPackageForSelectedVersion(element);
            if (installedPackage == null)
            {
                return false;
            }

            var dynamoModel = PackageManagerClientViewModel?.DynamoViewModel?.Model;
            if (dynamoModel == null)
            {
                return false;
            }

            if (!installedPackage.InUse(dynamoModel) || installedPackage.LoadedAssemblies.Any())
            {
                return IsLoadedWithNoScheduledOperation(installedPackage);
            }

            return false;
        }

        private static bool IsLoadedWithNoScheduledOperation(Package package)
        {
            return package.BuiltInPackage
                ? package.LoadState.State != PackageLoadState.StateTypes.Unloaded &&
                  package.LoadState.ScheduledState != PackageLoadState.ScheduledTypes.ScheduledForUnload
                : package.LoadState.ScheduledState != PackageLoadState.ScheduledTypes.ScheduledForDeletion;
        }

        private Package GetInstalledPackageForSelectedVersion(PackageManagerSearchElementViewModel element)
        {
            if (element?.SelectedVersion?.IsInstalled != true || element.SearchElementModel == null)
            {
                return null;
            }

            var installedPackages = PackageManagerClientViewModel.PackageManagerExtension.PackageLoader.LocalPackages
                .Where(x => x.Name == element.SearchElementModel.Name && !x.BuiltInPackage)
                .ToList();

            if (!installedPackages.Any())
            {
                return null;
            }

            return installedPackages.FirstOrDefault(pkg =>
                string.Equals(pkg.VersionName, element.SelectedVersion.Version, StringComparison.OrdinalIgnoreCase));
        }

        private static string GetNewestInstalledVersion(IEnumerable<Package> installedPackages)
        {
            if (installedPackages == null)
            {
                return null;
            }

            var parsedVersions = installedPackages
                .Select(pkg => new { Version = pkg.VersionName, Parsed = VersionUtilities.Parse(pkg.VersionName) })
                .Where(x => x.Parsed != null)
                .OrderBy(x => x.Parsed)
                .LastOrDefault();

            if (parsedVersions != null)
            {
                return parsedVersions.Version;
            }

            return installedPackages.Select(pkg => pkg.VersionName).FirstOrDefault();
        }

        private static bool IsUpdateAvailable(string installedVersion, IEnumerable<VersionInformation> availableVersions)
        {
            if (string.IsNullOrEmpty(installedVersion) || availableVersions == null)
            {
                return false;
            }

            var parsedInstalledVersion = VersionUtilities.Parse(installedVersion);
            if (parsedInstalledVersion == null)
            {
                return false;
            }

            var newestAvailableVersion = availableVersions
                .Select(version => VersionUtilities.Parse(version.Version))
                .Where(parsedVersion => parsedVersion != null)
                .OrderBy(parsedVersion => parsedVersion)
                .LastOrDefault();

            if (newestAvailableVersion == null)
            {
                return false;
            }

            return newestAvailableVersion > parsedInstalledVersion;
        }

        private PackageManagerSearchElementViewModel GetSearchElementViewModelByName(string name)
        {
            return SearchResults?.FirstOrDefault(x => x.SearchElementModel.Name == name)
                ?? SearchMyResults?.FirstOrDefault(x => x.SearchElementModel.Name == name);
        }

        public PackageSearchState _searchState; // TODO: Set private for 3.0.

        /// <summary>
        /// Gives the current state of search.
        /// </summary>
        public PackageSearchState SearchState
        {
            get { return _searchState; }
            set
            {
                _searchState = value;
                RaisePropertyChanged(nameof(this.SearchState));
                RaisePropertyChanged(nameof(this.SearchBoxPrompt));
                RaisePropertyChanged(nameof(this.ShowSearchText));

                if (value == PackageSearchState.Results && !this.InitialResultsLoaded)
                {
                    this.InitialResultsLoaded = true;
                }
            }
        }

        private bool _initialResultsLoaded = false;
        /// <summary>
        /// Will only be set to true once after the initial search has been finished.
        /// </summary>
        public bool InitialResultsLoaded
        {
            get { return _initialResultsLoaded; }
            set
            {
                _initialResultsLoaded = value;
                RaisePropertyChanged(nameof(InitialResultsLoaded));
            }
        }

        /// <summary>
        ///     PackageManagerClient property
        /// </summary>
        /// <value>
        ///     A handle on the package manager client object 
        /// </value>
        public PackageManagerClientViewModel PackageManagerClientViewModel { get; private set; }

        /// <summary>
        /// A getter boolean identifying if the user is currently logged in
        /// </summary>
        public bool IsLoggedIn { get { return PackageManagerClientViewModel.AuthenticationManager.IsLoggedIn(); } }

        /// <summary>
        /// Current selected filter hosts
        /// </summary>
        public List<string> SelectedHosts { get; set; }

        /// <summary>
        ///     Command to clear the completed package downloads
        /// </summary>
        public DelegateCommand ClearCompletedCommand { get; set; }

        /// <summary>
        ///     Sort the default results
        /// </summary>
        public DelegateCommand SortCommand { get; set; }

        /// <summary>
        ///     Sort the search results
        /// </summary>
        public DelegateCommand<object> SearchSortCommand { get; set; }

        /// <summary>
        ///     Set the sorting key for search results and resort
        /// </summary>
        public DelegateCommand<object> SetSortingKeyCommand { get; set; }

        /// <summary>
        ///     Command to set the sorting direction and resort the search results
        /// </summary>
        public DelegateCommand<object> SetSortingDirectionCommand { get; set; }

        /// <summary>
        /// Opens the Package Details ViewExtension
        /// </summary>
        public DelegateCommand<object> ViewPackageDetailsCommand { get; set; }

        /// <summary>
        /// Clears the search text box and resets the search
        /// </summary>
        public DelegateCommand<object> ClearSearchTextBoxCommand { get; set; }

        /// <summary>
        /// When the user downloads new package via the package search manager, a toast notification
        /// appears at the base of the window. This command fires when the user clicks to dismiss
        /// one of these toast notifications.
        /// </summary>
        public DelegateCommand<object> ClearToastNotificationCommand { get; set; }

        /// <summary>
        /// Clears all filters, setting them to `false`
        /// </summary>
        public DelegateCommand<object> ClearFiltersCommand { get; set; }

        /// <summary>
        ///     Current downloads
        /// </summary>
        public ObservableCollection<PackageDownloadHandle> Downloads
        {
            get { return PackageManagerClientViewModel.Downloads; }
        }

        public IPreferences Preferences
        {
            get { return PackageManagerClientViewModel.DynamoViewModel.PreferenceSettings; }
        }

        private bool isDetailPackagesExtensionOpened;
        public bool IsDetailPackagesExtensionOpened
        {
            get { return isDetailPackagesExtensionOpened; }
            set
            {
                if (isDetailPackagesExtensionOpened != value)
                {
                    isDetailPackagesExtensionOpened = value;
                    RaisePropertyChanged(nameof(IsDetailPackagesExtensionOpened));
                }
            }
        }
        #endregion Properties & Fields

        internal PackageManagerSearchViewModel()
        {
            SearchResults = new ObservableCollection<PackageManagerSearchElementViewModel>();
            InfectedPackages = new ObservableCollection<PackageManagerSearchElement>();
            MaxNumSearchResults = 35;
            ClearCompletedCommand = new DelegateCommand(ClearCompleted, CanClearCompleted);
            SortCommand = new DelegateCommand(Sort, CanSort);
            SearchSortCommand = new DelegateCommand<object>(Sort, CanSort);
            SetSortingKeyCommand = new DelegateCommand<object>(SetSortingKey, CanSetSortingKey);
            SetSortingDirectionCommand = new DelegateCommand<object>(SetSortingDirection, CanSetSortingDirection);
            ViewPackageDetailsCommand = new DelegateCommand<object>(ViewPackageDetails);
            ClearSearchTextBoxCommand = new DelegateCommand<object>(ClearSearchTextBox);
            ClearToastNotificationCommand = new DelegateCommand<object>(ClearToastNotification);
            ClearFiltersCommand = new DelegateCommand<object>(ClearAllFilters);
            SearchText = string.Empty;
            SortingKey = PackageSortingKey.LastUpdate;
            SortingDirection = PackageSortingDirection.Descending;
            HostFilter = new List<FilterEntry>();
            NonHostFilter = new List<FilterEntry>();
            CompatibilityFilter = new List<FilterEntry>();
            SelectedHosts = new List<string>();
        }



        /// <summary>
        /// Add package information to Lucene index
        /// </summary>
        /// <param name="package">package info that will be indexed</param>
        /// <param name="doc">Lucene document in which the package info will be indexed</param>
        internal void AddPackageToSearchIndex(PackageManagerSearchElement package, Document doc)
        {
            if (LuceneUtility.addedFields == null) return;

            LuceneUtility.SetDocumentFieldValue(doc, nameof(LuceneConfig.NodeFieldsEnum.Name), package.Name.Trim().Replace(" ", string.Empty));
            LuceneUtility.SetDocumentFieldValue(doc, nameof(LuceneConfig.NodeFieldsEnum.Description), package.Description);
            LuceneUtility.SetDocumentFieldValue(doc, nameof(LuceneConfig.NodeFieldsEnum.Author), package.Maintainers);

            if (package.Keywords.Length > 0)
            {
                LuceneUtility.SetDocumentFieldValue(doc, nameof(LuceneConfig.NodeFieldsEnum.SearchKeywords), package.Keywords);
            }

            if (package.Hosts != null && string.IsNullOrEmpty(package.Hosts.ToString()))
            {
                LuceneUtility.SetDocumentFieldValue(doc, nameof(LuceneConfig.NodeFieldsEnum.Hosts), package.Hosts.ToString(), true, true);
            }

            LuceneUtility.writer?.AddDocument(doc);
        }

        /// <summary>
        ///     The class constructor.
        /// </summary>
        public PackageManagerSearchViewModel(PackageManagerClientViewModel client) : this()
        {
            PackageManagerClientViewModel = client;
            HostFilter = InitializeHostFilter();
            NonHostFilter = InitializeNonHostFilter();
            CompatibilityFilter = InitializeCompatibilityFilter();
            InitializeLuceneForPackageManager();
        }

        internal void InitializeLuceneForPackageManager()
        {
            if(LuceneUtility == null)
            {
                LuceneSearch.LuceneUtilityPackageManager = new LuceneSearchUtility(PackageManagerClientViewModel.DynamoViewModel.Model, LuceneSearchUtility.DefaultPkgIndexStartConfig);
            }
        }
        
        /// <summary>
        /// Populates SearchMyResults collection containing all packages by current user
        /// </summary>
        private void PopulateMyPackages()
        {
            // First, clear already existing results to prevent stacking 
            if (SearchMyResults != null) return;
            // We should have already populated the CachedPackageList by this step
            if (PackageManagerClientViewModel.CachedPackageList == null ||
                !PackageManagerClientViewModel.CachedPackageList.Any()) return;

            List<PackageManagerSearchElementViewModel> myPackages = new List<PackageManagerSearchElementViewModel>();

            // We need the user to be logged in, otherwise there is no point in running this routine
            if (PackageManagerClientViewModel.LoginState != Greg.AuthProviders.LoginState.LoggedIn)
            {
                SearchMyResults = new ObservableCollection<PackageManagerSearchElementViewModel>(myPackages);
                return;
            }

            // Check if any of the maintainers corresponds to the current logged in username
            var name = PackageManagerClientViewModel.Username;
            var pkgs = PackageManagerClientViewModel.CachedPackageList.Where(x => x.Maintainers != null && x.Maintainers.Contains(name)).ToList();
            foreach(var pkg in pkgs)
            {
                var p = GetSearchElementViewModel(pkg, true);

                p.RequestDownload += this.PackageOnExecuted;
                p.RequestUninstall += SearchElementViewModelOnRequestUninstall;
                p.IsOnwer = true;

                myPackages.Add(p);
            }
    
            SearchMyResults = new ObservableCollection<PackageManagerSearchElementViewModel>(myPackages);
        }

        private void ClearMySearchResults()
        {
            if (this.SearchMyResults == null) return;
            foreach (var ele in this.SearchMyResults)
            {
                ele.RequestDownload -= PackageOnExecuted;
                ele.RequestShowFileDialog -= OnRequestShowFileDialog;
                ele.PropertyChanged -= SearchElementViewModelOnPropertyChanged;
                ele.RequestUninstall -= SearchElementViewModelOnRequestUninstall;
            }

            this.SearchMyResults = null;
        }

        /// <summary>
        /// Sort the default package results in the view based on the sorting key and sorting direction.
        /// </summary>
        public void Sort()
        {
            var list = this.SearchResults.AsEnumerable().ToList();
            Sort(list, this.SortingKey);

            if (SortingDirection == PackageSortingDirection.Descending)
            {
                list.Reverse();
            }

            // temporarily hide binding
            var temp = this.SearchResults;
            this.SearchResults = null;

            temp.Clear();

            foreach (var ele in list)
            {
                temp.Add(ele);
            }

            this.SearchResults = temp;
        }

        /// <summary>
        /// Sort the package search results in the view based on the closest hit to the search key.
        /// </summary>
        private void Sort(object searchQuery = null)
        {
            if (searchQuery == null || searchQuery as string == "")
            {
                this.Sort();
            }
            else
            {
                var list = this.SearchResults.AsEnumerable().ToList();
                Sort(list, PackageSortingKey.Search, searchQuery.ToString());

                // temporarily hide binding
                var temp = this.SearchResults;
                this.SearchResults = null;

                temp.Clear();

                foreach (var ele in list)
                {
                    temp.Add(ele);
                }

                this.SearchResults = temp;
            }
        }

        /// <summary>
        /// Based on the known hosts received from Package Manager server,
        /// initialize the host filter in Dynamo
        /// </summary>
        public List<FilterEntry> InitializeHostFilter()
        {
            var hostFilter = new List<FilterEntry>();
            try
            {
                foreach (var host in PackageManagerClientViewModel.Model?.GetKnownHosts())
                {
                    hostFilter.Add(new FilterEntry(host, Resources.PackageFilterByHost, Resources.PackageHostDependencyFilterContextItem, this));
                }
            }
            catch (Exception ex)
            {
                PackageManagerClientViewModel.DynamoViewModel.Model.Logger.Log("Could not fetch hosts: " + ex.Message);
            }
            return hostFilter;
        }

        private List<FilterEntry> InitializeNonHostFilter()
        {
            var nonHostFilter = new List<FilterEntry>() { new FilterEntry(Resources.PackageManagerPackageNew, Resources.PackageFilterByStatus, Resources.PackageFilterNewTooltip, this),
                                                          new FilterEntry(Resources.PackageManagerPackageUpdated, Resources.PackageFilterByStatus, Resources.PackageFilterUpdatedTooltip, this),
                                                          new FilterEntry(Resources.PackageSearchViewContextMenuFilterDeprecated, Resources.PackageFilterByStatus, Resources.PackageFilterDeprecatedTooltip, this),
                                                          new FilterEntry(Resources.PackageSearchViewContextMenuFilterDependencies, Resources.PackageFilterByDependency, Resources.PackageFilterHasDependenciesTooltip, this),
                                                          new FilterEntry(Resources.PackageSearchViewContextMenuFilterNoDependencies, Resources.PackageFilterByDependency, Resources.PackageFilterHasNoDependenciesTooltip, this)
            } ;

            nonHostFilter.ForEach(f => f.PropertyChanged += filter_PropertyChanged);

            return nonHostFilter;
        }

        private List<FilterEntry> InitializeCompatibilityFilter()
        {
            var compatibilityFilter = new List<FilterEntry>() { new FilterEntry(Resources.PackageCompatible, Resources.PackageFilterByCompatibility, Resources.PackageCompatibleFilterTooltip, this),
                                                          new FilterEntry(Resources.PackageUnknownCompatibility, Resources.PackageFilterByCompatibility, Resources.PackageUnknownCompatibilityFilterTooltip, this),
                                                          new FilterEntry(Resources.PackageIncompatible, Resources.PackageFilterByCompatibility, Resources.PackageIncompatibleFilterTooltip, this)
            };

            compatibilityFilter.ForEach(f => f.PropertyChanged += filter_PropertyChanged);

            return compatibilityFilter;
        }

        /// <summary>
        /// Toggles `dependency` and `no dependency` filters so that both cannot be 'ON' at the same time.
        /// We need both filters to function individually in their 'OFF' states (it's not a simple toggle state switch)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void filter_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var filter = sender as FilterEntry;
            switch (filter.FilterName)
            {
                case var name when name.Equals( Resources.PackageSearchViewContextMenuFilterDependencies ):
                    if(!filter.OnChecked)
                        break;
                    var negateFilter = NonHostFilter.First(x => x.FilterName.Equals(Resources.PackageSearchViewContextMenuFilterNoDependencies));
                    if (!negateFilter.OnChecked)
                        break;
                    negateFilter.OnChecked = false;
                    break;
                case var name when name.Equals(Resources.PackageSearchViewContextMenuFilterNoDependencies):
                    if (!filter.OnChecked)
                        break;
                    negateFilter = NonHostFilter.First(x => x.FilterName.Equals(Resources.PackageSearchViewContextMenuFilterDependencies));
                    if (!negateFilter.OnChecked)
                        break;
                    negateFilter.OnChecked = false;
                    break;

                // Mutually exclusive case for Compatible and Incompatible
                case var name when name.Equals(Resources.PackageCompatible):
                    if (!filter.OnChecked)
                        break;
                    var incompatibleFilter = CompatibilityFilter.First(x => x.FilterName.Equals(Resources.PackageIncompatible));
                    if (!incompatibleFilter.OnChecked)
                        break;
                    incompatibleFilter.OnChecked = false;
                    break;

                case var name when name.Equals(Resources.PackageIncompatible):
                    if (!filter.OnChecked)
                        break;
                    var compatibleFilter = CompatibilityFilter.First(x => x.FilterName.Equals(Resources.PackageCompatible));
                    if (!compatibleFilter.OnChecked)
                        break;
                    compatibleFilter.OnChecked = false;
                    break;
            }
        }


        /// <summary>
        /// Can search be performed.  Used by the associated command : SortCommand
        /// </summary>
        /// <returns></returns>
        public bool CanSort()
        {
            return true;
        }

        /// <summary>
        /// Can search be performed.  Used by the associated command : SearchSortCommand
        /// </summary>
        /// <returns></returns>
        private bool CanSort(object s)
        {
            return true;
        }

        /// <summary>
        /// Set the sorting direction.  Used by the associated command.
        /// </summary>
        /// <param name="sortingDir"></param>
        public void SetSortingDirection(object sortingDir)
        {
            if (sortingDir is string)
            {
                var key = (string)sortingDir;

                if (key == "ASCENDING")
                {
                    this.SortingDirection = PackageSortingDirection.Ascending;
                }
                else if (key == "DESCENDING")
                {
                    this.SortingDirection = PackageSortingDirection.Descending;
                }

            }
            else if (sortingDir is PackageSortingDirection)
            {
                this.SortingDirection = (PackageSortingDirection)sortingDir;
            }

            this.Sort();
        }

        /// <summary>
        /// Set the sorting direction.  Used by the associated command.
        /// </summary>
        /// <param name="obj"></param>
        public void ViewPackageDetails(object obj)
        {
            if (!(obj is PackageManagerSearchElement packageManagerSearchElement)) return;

            PackageManagerClientViewModel
                .DynamoViewModel
                .OnViewExtensionOpenWithParameterRequest("C71CA1B9-BF9F-425A-A12C-53DF56770406", packageManagerSearchElement);

            Analytics.TrackEvent(Actions.View, Categories.PackageManagerOperations, $"{packageManagerSearchElement?.Name}");
        }

        /// <summary>
        /// Clears the search text box and resets the search.
        /// </summary>
        public void ClearSearchTextBox(object obj)
        {
            SearchText = string.Empty;
        }

        /// <summary>
        /// When the user downloads new package via the package search manager, a toast notification
        /// appears at the base of the window. This command fires when the user clicks to dismiss
        /// one of these toast notifications.
        /// </summary>
        /// <param name="obj"></param>
        public void ClearToastNotification(object obj)
        {
            if (obj is PackageDownloadHandle packageDownloadHandle)
            {

                PackageDownloadHandle packageDownloadHandleToRemove = PackageManagerClientViewModel.Downloads
                    .FirstOrDefault(x => x.Id == packageDownloadHandle.Id);

                if (packageDownloadHandleToRemove == null) return;

                PackageManagerClientViewModel.Downloads.Remove(packageDownloadHandleToRemove);
                RaisePropertyChanged(nameof(Downloads));
            }
            else if (obj is PackageManagerSearchElement packageSearchElement)
            {
                PackageManagerSearchElement packageInfectedToRemove = this.InfectedPackages
                    .FirstOrDefault(x => x.InfectedPackageName == packageSearchElement.InfectedPackageName);

                this.InfectedPackages.Remove(packageInfectedToRemove);
                RaisePropertyChanged(nameof(InfectedPackages));
            }
            return;
        }
        
        /// <summary>
        /// Set the associated key
        /// </summary>
        /// <returns></returns>
        public bool CanSetSortingDirection(object par)
        {
            return true;
        }

        /// <summary>
        /// Sets all current filters to false
        /// </summary>
        /// <param name="obj"></param>
        private void ClearAllFilters(object obj)
        {
            foreach (var filter in HostFilter)
            {
                if (filter.OnChecked)
                {
                    filter.OnChecked = false;
                    filter.FilterCommand.Execute(filter.FilterName);
                }
            }

            foreach (var filter in NonHostFilter)
            {
                if (filter.OnChecked)
                {
                    filter.OnChecked = false;
                    filter.FilterCommand.Execute(filter.FilterName);
                }
            }

            foreach (var filter in CompatibilityFilter)
            {
                if (filter.OnChecked)
                {
                    filter.OnChecked = false;
                    filter.FilterCommand.Execute(filter.FilterName);
                }
            }

            RaisePropertyChanged(nameof(IsAnyFilterOn));
        }

        /// <summary>
        /// Set the key for search.  Used by the associated command.
        /// </summary>
        /// <param name="sortingKey"></param>
        public void SetSortingKey(object sortingKey)
        {
            if (sortingKey is string)
            {
                var key = (string)sortingKey;

                if (key == "NAME")
                {
                    this.SortingKey = PackageSortingKey.Name;
                }
                else if (key == "DOWNLOADS")
                {
                    this.SortingKey = PackageSortingKey.Downloads;
                }
                else if (key == "MAINTAINERS")
                {
                    this.SortingKey = PackageSortingKey.Maintainers;
                }
                else if (key == "LAST_UPDATE")
                {
                    this.SortingKey = PackageSortingKey.LastUpdate;
                }
                else if (key == "VOTES")
                {
                    this.SortingKey = PackageSortingKey.Votes;
                }

            }
            else if (sortingKey is PackageSortingKey)
            {
                this.SortingKey = (PackageSortingKey)sortingKey;
            }

            Analytics.TrackEvent(Actions.Sort, Categories.PackageManagerOperations, $"{sortingKey}");

            this.Sort();
        }

        /// <summary>
        /// Set the associated key
        /// </summary>
        /// <returns></returns>
        public bool CanSetSortingKey(object par)
        {
            return true;
        }

        public event EventHandler<PackagePathEventArgs> RequestShowFileDialog;
        public event EventHandler<PackagePathEventArgs> RequestDisableTextSearch;
        public virtual void OnRequestShowFileDialog(object sender, PackagePathEventArgs e)
        {
            if (RequestShowFileDialog != null)
            {
                RequestShowFileDialog(sender, e);
            }
        }

        /// <summary>
        /// Attempts to obtain the list of search results.  If it fails, it does nothing
        /// </summary>
        public void Refresh()
        {
            var pkgs = PackageManagerClientViewModel.ListAll();

            pkgs.Sort((e1, e2) => e1.Name.ToLower().CompareTo(e2.Name.ToLower()));
            pkgs = pkgs.Where(x => x.Header.versions != null && x.Header.versions.Count > 0).ToList(); // We expect compliant data structure
            LastSync = pkgs;

            PopulateMyPackages();   // adding 
        }

        /// <summary>
        /// Synchronously perform a refresh and then search
        /// </summary>
        /// <returns></returns>
        public IEnumerable<PackageManagerSearchElementViewModel> RefreshAndSearch()
        {
            Refresh();
            PopulateMyPackages();  
            return GetAllPackages();
        }

        public void RefreshAndSearchAsync()
        {
            StartTimer();   // Times out according to MAX_LOAD_TIME

            this.ClearSearchResults();
            this.SearchState = PackageSearchState.Syncing;

            var iDoc = LuceneUtility.InitializeIndexDocumentForPackages();

            Task<IEnumerable<PackageManagerSearchElementViewModel>>.Factory.StartNew(RefreshAndSearch).ContinueWith((t) =>
            {
                lock (SearchResults)
                {
                    ClearSearchResults();
                    foreach (var result in t.Result)
                    {
                        if (result.SearchElementModel != null)
                        {
                            AddPackageToSearchIndex(result.SearchElementModel, iDoc);
                        }   
                        this.AddToSearchResults(result);
                    }
                    this.SearchState = HasNoResults ? PackageSearchState.NoResults : PackageSearchState.Results;
                    TimedOut = false;

                    if (!DynamoModel.IsTestMode)
                    {
                        LuceneUtility.dirReader = LuceneUtility.writer != null ? LuceneUtility.writer.GetReader(applyAllDeletes: true) : DirectoryReader.Open(LuceneUtility.indexDir);
                        LuceneUtility.Searcher = new IndexSearcher(LuceneUtility.dirReader);

                        LuceneUtility.CommitWriterChanges();
                        LuceneUtility.DisposeWriter();
                    }
                }
                RefreshInfectedPackages();
            }
            , TaskScheduler.FromCurrentSynchronizationContext()); // run continuation in ui thread
        }

        #region Time Out

        // maximum loading time for packages - 30 seconds
        // if exceeded will trigger `timed out` event and failure screen
        internal int MAX_LOAD_TIME = 30 * 1000;
        private bool _timedOut;
        /// <summary>
        /// Will trigger timed out event
        /// </summary>
        public bool TimedOut
        {
            get { return _timedOut; }
            set
            {
                _timedOut = value;
                RaisePropertyChanged(nameof(TimedOut));
            }
        }

        private System.Timers.Timer aTimer;

        private void StartTimer()
        {
            if(aTimer == null) 
                aTimer = new System.Timers.Timer();

            aTimer.Elapsed += OnTimedEvent;
            aTimer.Interval = MAX_LOAD_TIME;
            aTimer.AutoReset = false;
            aTimer.Enabled = true;
            aTimer.Start();
        }

        private void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            var aTimer = (System.Timers.Timer)sender;
            aTimer.Stop();

            // If we have managed to get all the results
            // Simply dispose of the timer
            // Otherwise act 
            if (this.SearchState != PackageSearchState.Results)
            {
                TimedOut = true;
            }
        }

        #endregion

        internal void RefreshInfectedPackages()
        {
            //do not call a protected route, if user is not logged in already.
            if (PackageManagerClientViewModel.LoginState !=  Greg.AuthProviders.LoginState.LoggedIn) return;

            var infectedPkgs = PackageManagerClientViewModel.GetInfectedPackages();
            infectedPkgs.Sort((e1, e2) => e1.InfectedPackageCreationDate.CompareTo(e2.InfectedPackageCreationDate));

            this.InfectedPackages.Clear();
            foreach (var pkg in infectedPkgs) {
                this.InfectedPackages.Add(pkg);

                // Limiting the infected packages list to 3. As the packages are resolved other unresolved packages will be displayed.
                if (this.InfectedPackages.Count() == 3) 
                { 
                    break; 
                }
            }
        }

        internal void AddToSearchResults(PackageManagerSearchElementViewModel element)
        {
            element.RequestDownload += this.PackageOnExecuted;
            element.RequestShowFileDialog += this.OnRequestShowFileDialog;
            element.PropertyChanged += SearchElementViewModelOnPropertyChanged;
            element.RequestUninstall += SearchElementViewModelOnRequestUninstall;

            this.SearchResults.Add(element);
        }

        internal void ClearSearchResults()
        {
            if (this.SearchResults == null) return;
            foreach (var ele in this.SearchResults)
            {
                ele.RequestDownload -= PackageOnExecuted;
                ele.RequestShowFileDialog -= OnRequestShowFileDialog;
                ele.PropertyChanged -= SearchElementViewModelOnPropertyChanged;
                ele.RequestUninstall -= SearchElementViewModelOnRequestUninstall;

                ele?.Dispose();
            }
            this.SearchResults.Clear();
        }
        private void SearchElementViewModelOnRequestUninstall(PackageManagerSearchElementViewModel element)
        {
            UninstallPackage(element);
        }

        private void SearchElementViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is PackageManagerSearchElementViewModel element &&
                e.PropertyName == nameof(PackageManagerSearchElementViewModel.SelectedVersion))
            {
                UpdateInstallState(element);
            }
        }

        internal void PackageOnExecuted(PackageManagerSearchElement element, PackageVersion version, string downloadPath)
        {
            this.PackageManagerClientViewModel.ExecutePackageDownload(element.Name, version, downloadPath);
        }

        /// <summary>
        ///     Returns a newline delimited string representing the package name and version of the argument
        /// </summary>
        public static string FormatPackageVersionList(IEnumerable<Tuple<PackageHeader, PackageVersion>> packages)
        {
            return String.Join("\r\n", packages.Select(x => x.Item1.name + " " + x.Item2.version));
        }

        private void ConflictingCustomNodePackageLoaded(Package installed, Package conflicting)
        {
            var message = string.Format(Resources.MessageUninstallCustomNodeToContinue,
                installed.Name + " " + installed.VersionName, conflicting.Name + " " + conflicting.VersionName);

            var dialogResult = MessageBoxService.Show(message,
                Resources.CannotDownloadPackageMessageBoxTitle,
                MessageBoxButton.YesNo, MessageBoxImage.Error);

            if (dialogResult == MessageBoxResult.Yes)
            {
                // mark for uninstallation
                var settings = PackageManagerClientViewModel.DynamoViewModel.Model.PreferenceSettings;
                installed.MarkForUninstall(settings);
            }
        }

        private void DownloadsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            void canExecuteHandler(object o, PropertyChangedEventArgs eArgs)
                => ClearCompletedCommand.RaiseCanExecuteChanged();

            void canInstallHandler(object o, PropertyChangedEventArgs eArgs)
            {
                var handle = o as PackageDownloadHandle;
                // Hooking into propertyChanged works only if each download handle is added to
                // the Downloads collection before Download/Install begins.
                if (eArgs.PropertyName == nameof(PackageDownloadHandle.DownloadState))
                {
                    var searchElement = GetSearchElementViewModelByName(handle.Name);
                    if (searchElement == null) return;
                    UpdateInstallState(searchElement);
                }
            }

            if (args.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var item in args.NewItems)
                {
                    var handle = (PackageDownloadHandle)item;
                    // Update CanInstall property every time the download handle's state changes
                    handle.PropertyChanged += canInstallHandler;
                    handle.PropertyChanged += canExecuteHandler;
                }
            } 
            else if (args.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var item in args.OldItems)
                {
                    var handle = (PackageDownloadHandle)item;
                    handle.PropertyChanged -= canInstallHandler;
                    handle.PropertyChanged -= canExecuteHandler;
                }
            }

            if (PackageManagerClientViewModel.Downloads.Count == 0)
            {
                ClearCompletedCommand.RaiseCanExecuteChanged();
            }

            RaisePropertyChanged(nameof(HasDownloads));
        }

        private void SearchResultsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            this.RaisePropertyChanged("HasNoResults");
        }

        public void ClearCompleted()
        {
            PackageManagerClientViewModel.ClearCompletedDownloads();
        }

        public bool CanClearCompleted()
        {
            if (PackageManagerClientViewModel == null) return false;

            return PackageManagerClientViewModel.Downloads
                                       .Any(x => x.DownloadState == PackageDownloadHandle.State.Installed
                                           || x.DownloadState == PackageDownloadHandle.State.Error);
        }

        /// <summary>
        ///     Asynchronously performs a search and updates the observable SearchResults property.
        /// </summary>
        /// <param name="query"> The search query </param>
        internal void SearchAndUpdateResults(string query)
        {
            // if last sync isn't populated, we can't search
            if (LastSync == null) return;

            IEnumerable<PackageManagerSearchElementViewModel> results;
            this.SearchText = query;

            // If the search query is empty, just call regular search API
            // If the search query is not empty, then call Lucene search API
            if (string.IsNullOrEmpty(query))
            {
                results = GetAllPackages();
            }
            else
            {
                results = Search(query, true);
                results = ApplyNonHostFilters(results);
                results = ApplyHostFilters(results);
                results = ApplyCompatibilityFilters(results);
            }

            this.ClearSearchResults();

            foreach (var result in results)
            {
                this.AddToSearchResults(result);
            }

            this.Sort(query);

            SearchState = HasNoResults ? PackageSearchState.NoResults : PackageSearchState.Results;
        }

        /// <summary>
        ///     Increments the selected element by 1, unless it is the last element already
        /// </summary>
        public void SelectNext()
        {
            if (SelectedIndex == SearchResults.Count - 1
                || SelectedIndex == -1)
                return;

            SelectedIndex = SelectedIndex + 1;
        }

        /// <summary>
        ///     Decrements the selected element by 1, unless it is the first element already
        /// </summary>
        public void SelectPrevious()
        {
            if (SelectedIndex <= 0)
                return;

            SelectedIndex = SelectedIndex - 1;
        }

        internal void RegisterTransientHandlers()
        {
            SearchResults.CollectionChanged += SearchResultsOnCollectionChanged;
            PackageManagerClientViewModel.Downloads.CollectionChanged += DownloadsOnCollectionChanged;
            PackageManagerClientViewModel.PackageManagerExtension.PackageLoader.ConflictingCustomNodePackageLoaded +=
                ConflictingCustomNodePackageLoaded;
        }

        internal void UnregisterTransientHandlers()
        {
            SearchResults.CollectionChanged -= SearchResultsOnCollectionChanged;
            PackageManagerClientViewModel.Downloads.CollectionChanged -= DownloadsOnCollectionChanged;
            PackageManagerClientViewModel.PackageManagerExtension.PackageLoader.ConflictingCustomNodePackageLoaded -=
                ConflictingCustomNodePackageLoaded;
        }

        /// <summary>
        ///     Performs a search using the internal SearchText as the query and
        ///     updates the observable SearchResults property.
        /// </summary>
        internal void SearchAndUpdateResults()
        {
            SearchAndUpdateResults(SearchText);
        }

        /// <summary>
        /// Performs a filter to the assuming pre-searched results
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        internal IEnumerable<PackageManagerSearchElementViewModel> ApplyHostFilters(IEnumerable<PackageManagerSearchElementViewModel> list)
        {
            // No need to filter by host if nothing selected
            if (SelectedHosts.Count == 0) return list;
            IEnumerable<PackageManagerSearchElementViewModel> filteredList = null;

            filteredList = filteredList ??
                           list.Where(x => x.SearchElementModel.Hosts != null && SelectedHosts.Intersect(x.SearchElementModel.Hosts).Count() == SelectedHosts.Count()) ?? Enumerable.Empty<PackageManagerSearchElementViewModel>();

            return filteredList;
        }

        /// <summary>   
        ///     Get all the package results in the package manager.
        /// </summary>
        /// <returns> Returns a list with a maximum MaxNumSearchResults elements.</returns>
        internal IEnumerable<PackageManagerSearchElementViewModel> GetAllPackages()
        {
            if (LastSync == null) return new List<PackageManagerSearchElementViewModel>();

            List<PackageManagerSearchElementViewModel> list = null;

            var isEnabledForInstall = !(Preferences as IDisablePackageLoadingPreferences).DisableCustomPackageLocations;

            // Filter based on user preference
            // A package has dependencies if the number of direct_dependency_ids is more than 1
            var initialResults = LastSync?.Select(x => GetSearchElementViewModel(x));
            list = ApplyNonHostFilters(initialResults);
            list = ApplyHostFilters(list).ToList();
            list = ApplyCompatibilityFilters(list).ToList();

            Sort(list, this.SortingKey);

            if (SortingDirection == PackageSortingDirection.Descending)
            {
                list.Reverse();
            }

            return list;
        }

        /// <summary>
        /// Applies non-host filters to a list of PackageManagerSearchElementViewModel
        /// </summary>
        /// <param name="list">The list to filter</param>
        /// <returns></returns>
        private List<PackageManagerSearchElementViewModel> ApplyNonHostFilters(IEnumerable<PackageManagerSearchElementViewModel> list)
        {
            return list.Where(x => NonHostFilter.First(f => f.FilterName.Equals(Resources.PackageSearchViewContextMenuFilterDeprecated)).OnChecked ? x.SearchElementModel.IsDeprecated : !x.SearchElementModel.IsDeprecated)
                                  .Where(x => NonHostFilter.First(f => f.FilterName.Equals(Resources.PackageManagerPackageNew)).OnChecked ? IsNewPackage(x.SearchElementModel) : true)
                                  .Where(x => NonHostFilter.First(f => f.FilterName.Equals(Resources.PackageManagerPackageUpdated)).OnChecked ? IsUpdatedPackage(x.SearchElementModel) : true)
                                  .Where(x => !NonHostFilter.First(f => f.FilterName.Equals(Resources.PackageSearchViewContextMenuFilterDependencies)).OnChecked ? true : PackageHasDependencies(x.SearchElementModel))
                                  .Where(x => !NonHostFilter.First(f => f.FilterName.Equals(Resources.PackageSearchViewContextMenuFilterNoDependencies)).OnChecked ? true : !PackageHasDependencies(x.SearchElementModel))
                                  .ToList();
        }

        /// <summary>
        /// Applies the compatibility filters - works on the package versions rather than the packages themselves
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        internal IEnumerable<PackageManagerSearchElementViewModel> ApplyCompatibilityFilters(IEnumerable<PackageManagerSearchElementViewModel> list)
        {
            // No need to filter by host if nothing selected
            if (!CompatibilityFilter.Any(f => f.OnChecked))
            {
                return list;
            }

            // Filter the list by checking if the selected version's compatibility matches the filters
            IEnumerable<PackageManagerSearchElementViewModel> filteredList = list.Where(x =>
                // Apply filter for Compatible (IsSelectedVersionCompatible == true)
                CompatibilityFilter.FirstOrDefault(f => f.FilterName.Equals(Wpf.Properties.Resources.PackageCompatible))?.OnChecked == true && x.IsSelectedVersionCompatible == true ||

                // Apply filter for Incompatible (IsSelectedVersionCompatible == false)
                CompatibilityFilter.FirstOrDefault(f => f.FilterName.Equals(Wpf.Properties.Resources.PackageIncompatible))?.OnChecked == true && x.IsSelectedVersionCompatible == false ||

                // Apply filter for Unknown compatibility (IsSelectedVersionCompatible == null)
                CompatibilityFilter.FirstOrDefault(f => f.FilterName.Equals(Wpf.Properties.Resources.PackageUnknownCompatibility))?.OnChecked == true && x.IsSelectedVersionCompatible == null
            ).ToList();

            return filteredList;
        }

        /// <summary>
        /// Checks if a package has any dependencies (will always have at least itself as 1 dependency) 
        /// </summary>
        /// <param name="package">The package to be filtered</param>
        /// <returns></returns>
        private bool PackageHasDependencies(PackageManagerSearchElement package)
        {
            return package.Header.versions.First(v => v.version.Equals(package.LatestVersion)).direct_dependency_ids.Count() > 1;
        }

        /// <summary>
        /// Follows DateToPackageLabelConverter logic:
        /// If the package is brand new (only has 1 version) and is less than 30 days it says 'New'.
        /// </summary>
        /// <param name="package">The package to be filtered</param>
        /// <returns></returns>
        private bool IsNewPackage(PackageManagerSearchElement package)
        {
            DateTime.TryParse(package.LatestVersionCreated, out DateTime dateLastUpdated);
            TimeSpan difference = DateTime.Now - dateLastUpdated;
            int numberVersions = package.Header.num_versions;

            if(difference.TotalDays >= 30 || numberVersions > 1) return false;
            return true;
        }

        /// <summary>
        /// Follows DateToPackageLabelConverter logic:
        /// If the package was updated in the last 30 days and has more than 1 version it is 'Updated'.
        /// </summary>
        /// <param name="package">The package to be filtered</param>
        /// <returns></returns>
        private bool IsUpdatedPackage(PackageManagerSearchElement package)
        {
            DateTime.TryParse(package.LatestVersionCreated, out DateTime dateLastUpdated);
            TimeSpan difference = DateTime.Now - dateLastUpdated;
            int numberVersions = package.Header.num_versions;

            if (difference.TotalDays >= 30 || numberVersions == 1) return false;
            return true;
        }

        /// <summary>
        /// Performs a search using the given string as query, but does not update
        /// the SearchResults object.
        /// </summary>
        /// <returns> Returns a list with a maximum MaxNumSearchResults elements.</returns>
        /// <param name="searchText"> The search query </param>
        ///  /// <param name="useLucene"> Temporary flag that will be used for searching using Lucene.NET </param>
        internal IEnumerable<PackageManagerSearchElementViewModel> Search(string searchText, bool useLucene)
        {
            if (useLucene)
            {
                string searchTerm = searchText.Trim();
                var packages = new List<PackageManagerSearchElementViewModel>();

                //The DirectoryReader and IndexSearcher have to be assigned after commiting indexing changes and before executing the Searcher.Search() method,otherwise new indexed info won't be reflected
                LuceneUtility.dirReader = LuceneUtility.writer != null ? LuceneUtility.writer.GetReader(applyAllDeletes: true): DirectoryReader.Open(LuceneUtility.indexDir);

                if (LuceneUtility.Searcher == null && LuceneUtility.dirReader != null)
                {
                    LuceneUtility.Searcher = new IndexSearcher(LuceneUtility.dirReader);
                }

                var parser = new MultiFieldQueryParser(LuceneConfig.LuceneNetVersion, LuceneConfig.PackageIndexFields, LuceneUtility.Analyzer)
                {
                    AllowLeadingWildcard = true,
                    DefaultOperator = LuceneConfig.DefaultOperator,
                    FuzzyMinSim = LuceneConfig.MinimumSimilarity
                };

                Query query = parser.Parse(LuceneUtility.CreateSearchQuery(LuceneConfig.PackageIndexFields, searchTerm, true));

                //indicate we want the first 50 results
                TopDocs topDocs = LuceneUtility.Searcher.Search(query, n: LuceneConfig.DefaultResultsCount);
                for (int i = 0; i < topDocs.ScoreDocs.Length; i++)
                {
                    //read back a doc from results
                    Document resultDoc = LuceneUtility.Searcher.Doc(topDocs.ScoreDocs[i].Doc);

                    // Get the view model of the package element and add it to the results.
                    string name = resultDoc.Get(nameof(LuceneConfig.NodeFieldsEnum.Name));

                    var foundPackage = GetViewModelForPackageSearchElement(name);
                    if (foundPackage != null)
                    {
                        packages.Add(foundPackage);
                    }
                }
                return packages;
            }
            else
            {
                return GetAllPackages();
            }
        }

        /// <summary>
        /// To get view model for a package based on its name.
        /// </summary>s
        /// <param name="packageName">Name of the package</param>
        /// <returns></returns>
        private PackageManagerSearchElementViewModel GetViewModelForPackageSearchElement(string packageName)
        {
            var result = PackageManagerClientViewModel.CachedPackageList.Where(e => e.Name.Replace(" ", string.Empty).Equals(packageName));

            if (!result.Any())
            {
                return null;
            }

            return GetSearchElementViewModel(result.FirstOrDefault());
        }

        /// <summary>
        ///    Returns a new PackageManagerSearchElementViewModel for the given package, with updated properties.
        /// </summary>
        /// <param name="package">Package to cast</param>
        /// <param name="bypassCustomPackageLocations">When true, will bypass the check for loading package from custom locations.</param>
        /// <returns></returns>
        private PackageManagerSearchElementViewModel GetSearchElementViewModel(PackageManagerSearchElement package, bool bypassCustomPackageLocations = false)
        {
            var isEnabledForInstall = bypassCustomPackageLocations || !(Preferences as IDisablePackageLoadingPreferences).DisableCustomPackageLocations;
            var viewModel = new PackageManagerSearchElementViewModel(package,
                PackageManagerClientViewModel.AuthenticationManager.HasAuthProvider,
                CanInstallPackage(package.Name),
                isEnabledForInstall);

            viewModel.CanUninstall = () => CanUninstallPackage(viewModel);
            UpdateInstallState(viewModel, true);

            return viewModel;
        }

        /// <summary>
        /// Sort a list of search results by the given key
        /// </summary>
        /// <param name="results"></param>
        /// <param name="key"></param>
        /// <param name="query"></param>
        private static void Sort(List<PackageManagerSearchElementViewModel> results, PackageSortingKey key, string query = null)
        {
            switch (key)
            {
                case PackageSortingKey.Name:
                    results.Sort((e1, e2) => e1.SearchElementModel.Name.ToLower().CompareTo(e2.SearchElementModel.Name.ToLower()));
                    break;
                case PackageSortingKey.Downloads:
                    results.Sort((e1, e2) => e1.SearchElementModel.Downloads.CompareTo(e2.SearchElementModel.Downloads));
                    break;
                case PackageSortingKey.LastUpdate:
                    results.Sort((e1, e2) => e1.Versions.FirstOrDefault().Item1.created.CompareTo(e2.Versions.FirstOrDefault().Item1.created));
                    break;
                case PackageSortingKey.Votes:
                    results.Sort((e1, e2) => e1.SearchElementModel.Votes.CompareTo(e2.SearchElementModel.Votes));
                    break;
                case PackageSortingKey.Maintainers:
                    results.Sort((e1, e2) => e1.SearchElementModel.Maintainers.ToLower().CompareTo(e2.SearchElementModel.Maintainers.ToLower()));
                    break;
                //This sorting key is applied to search results when user submits a search query on package manager search window,
                //it sorts in the following order: Not Deprecated Packages > search query in Name > Recently Updated
                case PackageSortingKey.Search:
                    results.Sort((e1, e2) => {
                        int ret = e1.SearchElementModel.IsDeprecated.CompareTo(e2.SearchElementModel.IsDeprecated);
                        int i1 = e1.SearchElementModel.Name.ToLower().IndexOf(query.ToLower(), StringComparison.InvariantCultureIgnoreCase);
                        int i2 = e2.SearchElementModel.Name.ToLower().IndexOf(query.ToLower(), StringComparison.InvariantCultureIgnoreCase);
                        ret = ret != 0 ? ret : ((i1 == -1) ? int.MaxValue : i1).CompareTo((i2 == -1) ? int.MaxValue : i2);
                        ret = ret != 0 ? ret : -e1.Versions.FirstOrDefault().Item1.created.CompareTo(e2.Versions.FirstOrDefault().Item1.created);
                        return ret;
                    });
                    break;
            }
        }

        /// <summary>
        ///     A KeyHandler method used by SearchView, increments decrements and executes based on input.
        /// </summary>
        /// <param name="sender">Originating object for the KeyHandler </param>
        /// <param name="e">Parameters describing the key push</param>
        public void KeyHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ExecuteSelected();
            }
            else if (e.Key == Key.Down)
            {
                SelectNext();
            }
            else if (e.Key == Key.Up)
            {
                SelectPrevious();
            }
        }

        /// <summary>
        ///     Runs the Execute() method of the current selected SearchElementBase object
        ///     amongst the SearchResults.
        /// </summary>
        public void ExecuteSelected()
        {
            // none of the elems are selected, return 
            if (SelectedIndex == -1)
                return;

            if (SearchResults.Count <= SelectedIndex)
                return;

            SearchResults[SelectedIndex].SearchElementModel.Execute();
        }

        /// <summary>
        /// Once the sample package is filled in the textbox search we rise the event to the view to disable the actions to change it , filter and sort it.
        /// </summary>
        public void DisableSearchTextBox()
        {
            if (RequestDisableTextSearch != null)
            {
                RequestDisableTextSearch(null, null);
            }
        }

        /// <summary>
        /// Clear after closing down
        /// </summary>
        internal void PackageManagerViewClose()
        {
            SearchAndUpdateResults(String.Empty); // reset the search text property
            InitialResultsLoaded = false;
            TimedOut = false;

            RequestShowFileDialog -= OnRequestShowFileDialog; // adding this back in

            ClearSearchResults();   // also clear all SearchResults and unsubscribe 
            ClearMySearchResults();
        }

        /// <summary>
        /// Remove PackageManagerSearchViewModel resources
        /// </summary>
        internal void Dispose()
        {
            if(LastSync != null)
            {
                foreach(var package in LastSync)
                {
                    package.UpvoteRequested -= PackageManagerClientViewModel.Model.Upvote;
                }
                LastSync.Clear();
            }

            nonHostFilter?.ForEach(f => f.PropertyChanged -= filter_PropertyChanged);
            nonHostFilter.Clear();

            compatibilityFilter?.ForEach(f => f.PropertyChanged -= filter_PropertyChanged);
            compatibilityFilter.Clear();

            if (aTimer != null)
            {
                aTimer.Stop();
                aTimer.Elapsed -= OnTimedEvent;
                aTimer = null;
            }

            TimedOut = false;   // reset the timedout screen 
            InitialResultsLoaded = false;   // reset the loading screen settings

            RequestShowFileDialog -= OnRequestShowFileDialog;   // adding this back in

            ClearSearchResults();   // also clear all SearchResults and unsubscribe 
            ClearMySearchResults();

        }
    }
}
