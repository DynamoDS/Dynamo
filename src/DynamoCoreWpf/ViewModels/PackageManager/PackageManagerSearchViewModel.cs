using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
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
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
#if NETFRAMEWORK
using Microsoft.Practices.Prism.Commands;
using NotificationObject = Microsoft.Practices.Prism.ViewModel.NotificationObject;
#else
using Prism.Commands;
using NotificationObject = Dynamo.Core.NotificationObject;
#endif

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
            /// <param name="packageManagerSearchViewModel">a reference of the PackageManagerSearchViewModel</param>
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
                // Send filter event with what host filter user using
                Dynamo.Logging.Analytics.TrackEvent(
                    Actions.Filter,
                    Categories.PackageManagerOperations,
                    string.Join(",", pmSearchViewModel.SelectedHosts));
                pmSearchViewModel.SearchAndUpdateResults();
                return;
            }
        }

        #region Properties & Fields
        // Lucene search utility to perform indexing operations.
        internal LuceneSearchUtility LuceneSearchUtility { get; set; }

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

        #endregion Properties & Fields

        internal PackageManagerSearchViewModel()
        {
            SearchResults = new ObservableCollection<PackageManagerSearchElementViewModel>();
            InfectedPackages = new ObservableCollection<PackageManagerSearchElement>();
            MaxNumSearchResults = 35;
            SearchDictionary = new SearchDictionary<PackageManagerSearchElement>();
            ClearCompletedCommand = new DelegateCommand(ClearCompleted, CanClearCompleted);
            SortCommand = new DelegateCommand(Sort, CanSort);
            SearchSortCommand = new DelegateCommand<object>(Sort, CanSort);
            SetSortingKeyCommand = new DelegateCommand<object>(SetSortingKey, CanSetSortingKey);
            SetSortingDirectionCommand = new DelegateCommand<object>(SetSortingDirection, CanSetSortingDirection);
            ViewPackageDetailsCommand = new DelegateCommand<object>(ViewPackageDetails);
            ClearSearchTextBoxCommand = new DelegateCommand<object>(ClearSearchTextBox);
            ClearToastNotificationCommand = new DelegateCommand<object>(ClearToastNotification);
            SearchText = string.Empty;
            SortingKey = PackageSortingKey.LastUpdate;
            SortingDirection = PackageSortingDirection.Descending;
            HostFilter = new List<FilterEntry>();
            SelectedHosts = new List<string>();
        }

        /// <summary>
        /// Add package information to Lucene index
        /// </summary>
        /// <param name="package">package info that will be indexed</param>
        /// <param name="doc">Lucene document in which the package info will be indexed</param>
        private void AddPackageToSearchIndex(PackageManagerSearchElement package, Document doc)
        {
            if (DynamoModel.IsTestMode) return;
            if (LuceneSearchUtility.addedFields == null) return;

            LuceneSearchUtility.SetDocumentFieldValue(doc, nameof(LuceneConfig.NodeFieldsEnum.Name), package.Name);
            LuceneSearchUtility.SetDocumentFieldValue(doc, nameof(LuceneConfig.NodeFieldsEnum.Description), package.Description);

            if (package.Keywords.Count() > 0)
            {
                LuceneSearchUtility.SetDocumentFieldValue(doc, nameof(LuceneConfig.NodeFieldsEnum.SearchKeywords), package.Keywords);
            }

            if (package.Hosts != null && string.IsNullOrEmpty(package.Hosts.ToString()))
            {
                LuceneSearchUtility.SetDocumentFieldValue(doc, nameof(LuceneConfig.NodeFieldsEnum.Hosts), package.Hosts.ToString(), true, true);
            }

            LuceneSearchUtility.writer?.AddDocument(doc);
        }

        /// <summary>
        ///     The class constructor.
        /// </summary>
        public PackageManagerSearchViewModel(PackageManagerClientViewModel client) : this()
        {
            PackageManagerClientViewModel = client;
            HostFilter = InitializeHostFilter();
            InitializeLuceneForPackageManager();
        }

        internal void InitializeLuceneForPackageManager()
        {
            if(LuceneSearchUtility == null)
            {
                LuceneSearchUtility = new LuceneSearchUtility(PackageManagerClientViewModel.DynamoViewModel.Model);
            }          
            LuceneSearchUtility.InitializeLuceneConfig(LuceneConfig.PackagesIndexingDirectory);
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
            // We need the user to be logged in, otherwise there is no point in runnig this routine
            if (PackageManagerClientViewModel.LoginState != Greg.AuthProviders.LoginState.LoggedIn) return;

            List<PackageManagerSearchElement> packageManagerSearchElements;
            List<PackageManagerSearchElementViewModel> myPackages = new List<PackageManagerSearchElementViewModel>();

            // Check if any of the maintainers corresponds to the current logged in username
            var name = PackageManagerClientViewModel.Username;
            var pkgs = PackageManagerClientViewModel.CachedPackageList.Where(x => x.Maintainers != null && x.Maintainers.Contains(name)).ToList();
            foreach(var pkg in pkgs)
            {
                myPackages.Add(new PackageManagerSearchElementViewModel(pkg, false));
            }
    
            SearchMyResults = new ObservableCollection<PackageManagerSearchElementViewModel>(myPackages);
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
            if (searchQuery == null)
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
            foreach (var host in PackageManagerClientViewModel.Model.GetKnownHosts())
            {
                hostFilter.Add(new FilterEntry(host, this));
            }
            
            return hostFilter;
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
            LastSync = pkgs;

            SearchDictionary = new SearchDictionary<PackageManagerSearchElement>();

            foreach (var pkg in pkgs)
            {
                SearchDictionary.Add(pkg, pkg.Name);
                SearchDictionary.Add(pkg, pkg.Description);
                SearchDictionary.Add(pkg, pkg.Maintainers);
                SearchDictionary.Add(pkg, pkg.Keywords);
            }

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
            this.ClearSearchResults();
            this.SearchState = PackageSearchState.Syncing;

            var iDoc = LuceneSearchUtility.InitializeIndexDocumentForPackages();

            Task<IEnumerable<PackageManagerSearchElementViewModel>>.Factory.StartNew(RefreshAndSearch).ContinueWith((t) =>
            {
                lock (SearchResults)
                {
                    ClearSearchResults();
                    foreach (var result in t.Result)
                    {
                        if (result.Model != null)
                        {
                            AddPackageToSearchIndex(result.Model, iDoc);
                        }   
                        this.AddToSearchResults(result);
                    }
                    this.SearchState = HasNoResults ? PackageSearchState.NoResults : PackageSearchState.Results;

                    if (!DynamoModel.IsTestMode)
                    {
                        LuceneSearchUtility.dirReader = LuceneSearchUtility.writer?.GetReader(applyAllDeletes: true);
                        LuceneSearchUtility.Searcher = new IndexSearcher(LuceneSearchUtility.dirReader);

                        LuceneSearchUtility.writer?.Commit();
                        LuceneSearchUtility.writer?.Dispose();
                        LuceneSearchUtility.indexDir?.Dispose();
                        LuceneSearchUtility.writer = null;
                    }
                }
                RefreshInfectedPackages();
            }
            , TaskScheduler.FromCurrentSynchronizationContext()); // run continuation in ui thread
        }

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
            this.SearchResults.Add(element);
        }

        internal void ClearSearchResults()
        {
            foreach (var ele in this.SearchResults)
            {
                ele.RequestDownload -= PackageOnExecuted;
            }

            this.SearchResults.Clear();
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
                    PackageManagerSearchElementViewModel sr = SearchResults.FirstOrDefault(x => x.Model.Name == handle.Name);
                    if (sr == null) return;

                    sr.CanInstall = CanInstallPackage(o as PackageDownloadHandle);
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
        internal IEnumerable<PackageManagerSearchElementViewModel> Filter(IEnumerable<PackageManagerSearchElementViewModel> list)
        {
            // No need to filter by host if nothing selected
            if (SelectedHosts.Count == 0) return list;
            IEnumerable<PackageManagerSearchElementViewModel> filteredList = null;

            filteredList = filteredList ??
                           list.Where(x => x.Model.Hosts != null && SelectedHosts.Intersect(x.Model.Hosts).Count() == SelectedHosts.Count()) ?? Enumerable.Empty<PackageManagerSearchElementViewModel>();

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

            // Don't show deprecated packages
            list = Filter(LastSync.Where(x => !x.IsDeprecated)
                                  .Select(x => new PackageManagerSearchElementViewModel(x,
                                                   PackageManagerClientViewModel.AuthenticationManager.HasAuthProvider,
                                                   CanInstallPackage(x.Name), isEnabledForInstall)))
                                  .ToList();

            Sort(list, this.SortingKey);

            if (SortingDirection == PackageSortingDirection.Descending)
            {
                list.Reverse();
            }

            foreach (var x in list)
                x.RequestShowFileDialog += OnRequestShowFileDialog;

            return list;
        }

        /// <summary>
        ///     Performs a search using the given string as query, but does not update
        ///     the SearchResults object.
        /// </summary>
        /// <returns> Returns a list with a maximum MaxNumSearchResults elements.</returns>
        /// <param name="query"> The search query </param>
        [Obsolete("This method will be removed in future Dynamo versions - please use Search method with Lucene flag.")]
        internal IEnumerable<PackageManagerSearchElementViewModel> Search(string query)
        {
            if (LastSync == null) return new List<PackageManagerSearchElementViewModel>();

            List<PackageManagerSearchElementViewModel> list = null;

            var isEnabledForInstall = !(Preferences as IDisablePackageLoadingPreferences).DisableCustomPackageLocations;
            if (!String.IsNullOrEmpty(query))
            {
                list = Filter(SearchDictionary.Search(query)
                    .Select(x => new PackageManagerSearchElementViewModel(x,
                        PackageManagerClientViewModel.AuthenticationManager.HasAuthProvider,
                        CanInstallPackage(x.Name), isEnabledForInstall))
                    .Take(MaxNumSearchResults))
                    .ToList();
            }
            else
            {
                // with null query, don't show deprecated packages
                list = Filter(LastSync.Where(x => !x.IsDeprecated)
                    .Select(x => new PackageManagerSearchElementViewModel(x,
                        PackageManagerClientViewModel.AuthenticationManager.HasAuthProvider,
                        CanInstallPackage(x.Name), isEnabledForInstall)))
                    .ToList();

                Sort(list, this.SortingKey);

                if (SortingDirection == PackageSortingDirection.Descending)
                {
                    list.Reverse();
                }
            }

            foreach (var x in list)
                x.RequestShowFileDialog += OnRequestShowFileDialog;

            return list;
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
                LuceneSearchUtility.dirReader = LuceneSearchUtility.writer?.GetReader(applyAllDeletes: true);

                if (LuceneSearchUtility.Searcher == null && LuceneSearchUtility.dirReader != null)
                {
                    LuceneSearchUtility.Searcher = new IndexSearcher(LuceneSearchUtility.dirReader);
                }

                var parser = new MultiFieldQueryParser(LuceneConfig.LuceneNetVersion, LuceneConfig.PackageIndexFields, LuceneSearchUtility.Analyzer)
                {
                    AllowLeadingWildcard = true,
                    DefaultOperator = LuceneConfig.DefaultOperator,
                    FuzzyMinSim = LuceneConfig.MinimumSimilarity
                };

                Query query = parser.Parse(LuceneSearchUtility.CreateSearchQuery(LuceneConfig.PackageIndexFields, searchTerm));

                //indicate we want the first 50 results
                TopDocs topDocs = LuceneSearchUtility.Searcher.Search(query, n: LuceneConfig.DefaultResultsCount);
                for (int i = 0; i < topDocs.ScoreDocs.Length; i++)
                {
                    //read back a doc from results
                    Document resultDoc = LuceneSearchUtility.Searcher.Doc(topDocs.ScoreDocs[i].Doc);

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
            var result = PackageManagerClientViewModel.CachedPackageList.Where(e => {
                if (e.Name.Equals(packageName))
                {
                    return true;
                }
                return false;
            });

            if (!result.Any())
            {
                return null;
            }

            return new PackageManagerSearchElementViewModel(result.ElementAt(0), false);
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
                    results.Sort((e1, e2) => e1.Model.Name.ToLower().CompareTo(e2.Model.Name.ToLower()));
                    break;
                case PackageSortingKey.Downloads:
                    results.Sort((e1, e2) => e1.Model.Downloads.CompareTo(e2.Model.Downloads));
                    break;
                case PackageSortingKey.LastUpdate:
                    results.Sort((e1, e2) => e1.Versions.FirstOrDefault().Item1.created.CompareTo(e2.Versions.FirstOrDefault().Item1.created));
                    break;
                case PackageSortingKey.Votes:
                    results.Sort((e1, e2) => e1.Model.Votes.CompareTo(e2.Model.Votes));
                    break;
                case PackageSortingKey.Maintainers:
                    results.Sort((e1, e2) => e1.Model.Maintainers.ToLower().CompareTo(e2.Model.Maintainers.ToLower()));
                    break;
                //This sorting key is applied to search results when user submits a search query on package manager search window,
                //it sorts in the following order: Not Deprecated Packages > search query in Name > Recently Updated
                case PackageSortingKey.Search:
                    results.Sort((e1, e2) => {
                        int ret = e1.Model.IsDeprecated.CompareTo(e2.Model.IsDeprecated);
                        int i1 = e1.Model.Name.ToLower().IndexOf(query.ToLower(), StringComparison.InvariantCultureIgnoreCase);
                        int i2 = e2.Model.Name.ToLower().IndexOf(query.ToLower(), StringComparison.InvariantCultureIgnoreCase);
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

            SearchResults[SelectedIndex].Model.Execute();
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
    }
}
