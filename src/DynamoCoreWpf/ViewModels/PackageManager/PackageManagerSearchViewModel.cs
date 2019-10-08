using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Dynamo.PackageManager.ViewModels;
using Dynamo.Search;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Wpf.Properties;
using Greg.Responses;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.ViewModel;

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
            Results
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
            LastUpdate
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
        public class FilterEntry
        {
            /// <summary>
            /// Name of the host
            /// </summary>
            public string FilterName { get; set; }

            /// <summary>
            /// Filter entry click command, notice this is a dynamic command
            /// with command param set to FilterName so that the code is robust
            /// in a way UI could handle as many hosts as possible
            /// </summary>
            public DelegateCommand<object> FilterCommand { get; set; }

            /// <summary>
            /// Boolean indicates if the Filter entry is checked, data binded to
            /// is checked property of each filter
            /// </summary>
            public bool OnChecked { get; set; }

            /// <summary>
            /// Private reference of PackageManagerSearchViewModel,
            /// used in the FilterCommand to filter search results
            /// </summary>
            private PackageManagerSearchViewModel pmSearchViewModel;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="filterName">Filter name, same as host name</param>
            /// <param name="pmSearchViewModel">a reference of the PackageManagerSearchViewModel</param>
            public FilterEntry(string filterName, PackageManagerSearchViewModel packageManagerSearchViewModel)
            {
                FilterName = filterName;
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
                if (OnChecked)
                {
                    pmSearchViewModel.SelectedHosts.Add(obj as string);
                }
                else
                {
                    pmSearchViewModel.SelectedHosts.Remove(obj as string);
                }
                pmSearchViewModel.SearchAndUpdateResults();
                return;
            }
        }

        #region Properties & Fields

        // The results of the last synchronization with the package manager server
        public List<PackageManagerSearchElement> LastSync { get; set; }

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
                RaisePropertyChanged("SortingKey");
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
                RaisePropertyChanged("HostFilter");
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
                RaisePropertyChanged("SortingDirection");
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
        /// Determines whether the the search text box should be displayed.
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
                _SearchText = value;
                RaisePropertyChanged("SearchText");
            }
        }

        /// <summary>
        ///     SelectedIndex property
        /// </summary>
        /// <value>
        ///     This is the currently selected element in the UI.
        /// </value>
        private int _selectedIndex;
        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                if (_selectedIndex != value)
                {
                    _selectedIndex = value;
                    RaisePropertyChanged("SelectedIndex");
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
        ///     MaxNumSearchResults property
        /// </summary>
        /// <value>
        ///     Internal limit on the number of search results returned by SearchDictionary
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
        /// Current selected filter hosts
        /// </summary>
        public List<string> SelectedHosts { get; set; }

        private SearchDictionary<PackageManagerSearchElement> SearchDictionary;

        /// <summary>
        ///     Command to clear the completed package downloads
        /// </summary>
        public DelegateCommand ClearCompletedCommand { get; set; }

        /// <summary>
        ///     Sort the search results
        /// </summary>
        public DelegateCommand SortCommand { get; set; }

        /// <summary>
        ///     Set the sorting key for search results and resort
        /// </summary>
        public DelegateCommand<object> SetSortingKeyCommand { get; set; }

        /// <summary>
        ///     Command to set the sorting direction and resort the search results
        /// </summary>
        public DelegateCommand<object> SetSortingDirectionCommand { get; set; }

        /// <summary>
        ///     Current downloads
        /// </summary>
        public ObservableCollection<PackageDownloadHandle> Downloads
        {
            get { return PackageManagerClientViewModel.Downloads; }
        }

        #endregion Properties & Fields

        internal PackageManagerSearchViewModel()
        {
            SearchResults = new ObservableCollection<PackageManagerSearchElementViewModel>();
            MaxNumSearchResults = 35;
            SearchDictionary = new SearchDictionary<PackageManagerSearchElement>();
            ClearCompletedCommand = new DelegateCommand(ClearCompleted, CanClearCompleted);
            SortCommand = new DelegateCommand(Sort, CanSort);
            SetSortingKeyCommand = new DelegateCommand<object>(SetSortingKey, CanSetSortingKey);
            SetSortingDirectionCommand = new DelegateCommand<object>(SetSortingDirection, CanSetSortingDirection);
            SearchResults.CollectionChanged += SearchResultsOnCollectionChanged;
            SearchText = string.Empty;
            SortingKey = PackageSortingKey.LastUpdate;
            SortingDirection = PackageSortingDirection.Ascending;
            HostFilter = new List<FilterEntry>();
            SelectedHosts = new List<string>();
        }

        /// <summary>
        ///     The class constructor.
        /// </summary>
        public PackageManagerSearchViewModel(PackageManagerClientViewModel client) : this()
        {
            this.PackageManagerClientViewModel = client;
            HostFilter = InitializeHostFilter();
            PackageManagerClientViewModel.Downloads.CollectionChanged += DownloadsOnCollectionChanged;
            PackageManagerClientViewModel.PackageManagerExtension.PackageLoader.ConflictingCustomNodePackageLoaded += 
                ConflictingCustomNodePackageLoaded;
        }
        
        /// <summary>
        /// Sort the search results
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
        /// Based on the known hosts received from Package Manager server,
        /// initialize the host filter in Dynamo
        /// </summary>
        public List<FilterEntry> InitializeHostFilter()
        {
            var hostFilter = new List<FilterEntry>();
            foreach (var host in PackageManagerClientViewModel.Model.GetKnownHosts())
            {
                hostFilter.Add(new FilterEntry(host, this));
            }

            return hostFilter;
        }

        /// <summary>
        /// Can search be performed.  Used by the associated command
        /// </summary>
        /// <returns></returns>
        public bool CanSort()
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
        /// Set the associated key
        /// </summary>
        /// <returns></returns>
        public bool CanSetSortingDirection(object par)
        {
            return true;
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
            LastSync = pkgs;

            SearchDictionary = new SearchDictionary<PackageManagerSearchElement>();

            foreach (var pkg in pkgs)
            {
                SearchDictionary.Add(pkg, pkg.Name);
                SearchDictionary.Add(pkg, pkg.Description);
                SearchDictionary.Add(pkg, pkg.Maintainers);
                SearchDictionary.Add(pkg, pkg.Keywords);
            }
        }

        /// <summary>
        /// Synchronously perform a refresh and then search
        /// </summary>
        /// <returns></returns>
        public IEnumerable<PackageManagerSearchElementViewModel> RefreshAndSearch()
        {
            Refresh();
            return Search(SearchText);
        }

        public void RefreshAndSearchAsync()
        {
            this.ClearSearchResults();
            this.SearchState = PackageSearchState.Syncing;

            Task<IEnumerable<PackageManagerSearchElementViewModel>>.Factory.StartNew(RefreshAndSearch).ContinueWith((t) =>
            {
                lock (SearchResults)
                {
                    ClearSearchResults();
                    foreach (var result in t.Result)
                    {
                        this.AddToSearchResults(result);
                    }
                    this.SearchState = HasNoResults ? PackageSearchState.NoResults : PackageSearchState.Results;
                }
            }
            , TaskScheduler.FromCurrentSynchronizationContext()); // run continuation in ui thread

        }

        private void AddToSearchResults(PackageManagerSearchElementViewModel element)
        {
            element.RequestDownload += this.PackageOnExecuted;
            this.SearchResults.Add(element);
        }

        private void ClearSearchResults()
        {
            foreach (var ele in this.SearchResults)
            {
                ele.RequestDownload -= PackageOnExecuted;
            }

            this.SearchResults.Clear();
        }

        private void PackageOnExecuted(PackageManagerSearchElement element, PackageVersion version, string downloadPath)
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

            var dialogResult = MessageBox.Show(message,
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
            if (args.NewItems != null)
            {
                foreach (var item in args.NewItems)
                {
                    var handle = (PackageDownloadHandle)item;
                    handle.PropertyChanged += (o, eventArgs) => this.ClearCompletedCommand.RaiseCanExecuteChanged();
                }
            }

            if (PackageManagerClientViewModel.Downloads.Count == 0)
            {
                this.ClearCompletedCommand.RaiseCanExecuteChanged();
            }

            this.RaisePropertyChanged("HasDownloads");

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

            this.SearchText = query;

            var results = Search(query);

            this.ClearSearchResults();

            foreach (var result in results)
            {
                this.AddToSearchResults(result);
            }

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

        internal void UnregisterHandlers()
        {
            RequestShowFileDialog -= OnRequestShowFileDialog;
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
        internal IEnumerable<PackageManagerSearchElementViewModel> Filter(IEnumerable<PackageManagerSearchElementViewModel> list)
        {
            // No need to filter by host if nothing selected
            if (SelectedHosts.Count == 0) return list;
            IEnumerable<PackageManagerSearchElementViewModel> filteredList = null;
            foreach (var host in SelectedHosts)
            {
                filteredList = (filteredList ?? Enumerable.Empty<PackageManagerSearchElementViewModel>()).Union(
                    list.Where(x => x.Model.Hosts != null && x.Model.Hosts.Contains(host)) ?? Enumerable.Empty<PackageManagerSearchElementViewModel>());
            }
            return filteredList;
        }

        /// <summary>
        ///     Performs a search using the given string as query, but does not update
        ///     the SearchResults object.
        /// </summary>
        /// <returns> Returns a list with a maximum MaxNumSearchResults elements.</returns>
        /// <param name="query"> The search query </param>
        internal IEnumerable<PackageManagerSearchElementViewModel> Search(string query)
        {
            if (LastSync == null) return new List<PackageManagerSearchElementViewModel>();

            var canLogin = PackageManagerClientViewModel.AuthenticationManager.HasAuthProvider;
            List<PackageManagerSearchElementViewModel> list = null;

            if (!String.IsNullOrEmpty(query))
            {
                list = Filter(SearchDictionary.Search(query)
                    .Select(x => new PackageManagerSearchElementViewModel(x, canLogin))
                    .Take(MaxNumSearchResults))
                    .ToList();
            }
            else
            {
                // with null query, don't show deprecated packages
                list = Filter(LastSync.Where(x => !x.IsDeprecated)
                    .Select(x => new PackageManagerSearchElementViewModel(x, canLogin)))
                    .ToList();
                Sort(list, this.SortingKey);
            }

            foreach (var x in list)
                x.RequestShowFileDialog += OnRequestShowFileDialog;

            return list;
        }

        /// <summary>
        /// Sort a list of search results by the given key
        /// </summary>
        /// <param name="results"></param>
        private static void Sort(List<PackageManagerSearchElementViewModel> results, PackageSortingKey key)
        {
            switch (key)
            {
                case PackageSortingKey.Name:
                    results.Sort((e1, e2) => e1.Model.Name.ToLower().CompareTo(e2.Model.Name.ToLower()));
                    break;
                case PackageSortingKey.Downloads:
                    results.Sort((e1, e2) => e2.Model.Downloads.CompareTo(e1.Model.Downloads));
                    break;
                case PackageSortingKey.LastUpdate:
                    results.Sort((e1, e2) => e2.Versions.Last().Item1.created.CompareTo(e1.Versions.Last().Item1.created));
                    break;
                case PackageSortingKey.Votes:
                    results.Sort((e1, e2) => e2.Model.Votes.CompareTo(e1.Model.Votes));
                    break;
                case PackageSortingKey.Maintainers:
                    results.Sort((e1, e2) => e1.Model.Maintainers.ToLower().CompareTo(e2.Model.Maintainers.ToLower()));
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

            SearchResults[SelectedIndex].Model.Execute();
        }

    }
}
