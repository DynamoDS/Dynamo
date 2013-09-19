using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Dynamo.Commands;
using Dynamo.Nodes.Search;
using Dynamo.Search;
using Dynamo.Search.SearchElements;
using Dynamo.Utilities;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.ViewModel;

namespace Dynamo.PackageManager
{
    public class PackageManagerSearchViewModel : NotificationObject
    {
        public enum PackageSearchState
        {
            SYNCING,
            SEARCHING,
            NORESULTS,
            RESULTS
        };

        #region Properties & Fields

        // The results of the last synchronization with the package manager server
        public List<PackageManagerSearchElement> LastSync { get; set; }

        /// <summary>
        ///     SearchText property
        /// </summary>
        /// <value>
        ///     This is the core UI for Dynamo, primarily used for logging.
        /// </value>
        public string _SearchText;
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
        public ObservableCollection<PackageManagerSearchElement> SearchResults { get; private set; }

        /// <summary>
        ///     MaxNumSearchResults property
        /// </summary>
        /// <value>
        ///     Internal limit on the number of search results returned by SearchDictionary
        /// </value>
        public int MaxNumSearchResults { get; set; }

        public bool HasDownloads
        {
            get { return dynSettings.PackageManagerClient.Downloads.Count > 0; }
        }

        public bool HasNoResults
        {
            get { return this.SearchResults.Count == 0; }
        }

        /// <summary>
        /// Gives the current state of search.
        /// </summary>
        public PackageSearchState _searchState;
        public PackageSearchState SearchState
        {
            get { return _searchState; }
            set
            {
                _searchState = value;
                RaisePropertyChanged("SearchState");
            }
        }

        /// <summary>
        ///     PackageManagerClient property
        /// </summary>
        /// <value>
        ///     A handle on the package manager client object 
        /// </value>
        public PackageManagerClient PackageManagerClient { get; private set; }

        /// <summary>
        ///     An ordered list representing all of the visible items in the browser.
        ///     This is used to manage up-down navigation through the menu.
        /// </summary>
        private List<BrowserItem> _visibleSearchResults = new List<BrowserItem>();

        private SearchDictionary<PackageManagerSearchElement> SearchDictionary;

        /// <summary>
        ///     Command to clear the completed package downloads
        /// </summary>
        public DelegateCommand ClearCompletedCommand { get; set; }

        /// <summary>
        ///     Current downloads
        /// </summary>
        public ObservableCollection<PackageDownloadHandle> Downloads
        {
            get { return PackageManagerClient.Downloads; }
        }

#endregion Properties & Fields

        /// <summary>
        ///     The class constructor.
        /// </summary>
        /// <param name="bench"> Reference to dynBench object for logging </param>
        public PackageManagerSearchViewModel(PackageManagerClient client)
        {
            PackageManagerClient = client;
            SearchResults = new ObservableCollection<PackageManagerSearchElement>();
            MaxNumSearchResults = 1000;
            SearchDictionary = new SearchDictionary<PackageManagerSearchElement>();
            ClearCompletedCommand = new DelegateCommand(ClearCompleted, CanClearCompleted);
            PackageManagerClient.Downloads.CollectionChanged += DownloadsOnCollectionChanged;
            this.SearchResults.CollectionChanged += SearchResultsOnCollectionChanged;
            SearchText = "";
        }

        public void Refresh()
        {

            var pkgs = PackageManagerClient.ListAll();

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

        public List<PackageManagerSearchElement> RefreshAndSearch()
        {

            Refresh();
            return Search(SearchText);

        }

        public void RefreshAndSearchAsync()
        {
            SearchResults.Clear();
            this.SearchState = PackageSearchState.SYNCING;

            Task<List<PackageManagerSearchElement>>.Factory.StartNew(RefreshAndSearch).ContinueWith((t) =>
            {
                lock (SearchResults)
                {
                    SearchResults.Clear();
                    foreach (var result in t.Result)
                    {
                        SearchResults.Add(result);
                    }
                    this.SearchState = HasNoResults ? PackageSearchState.NORESULTS : PackageSearchState.RESULTS;
                }
            }
            , TaskScheduler.FromCurrentSynchronizationContext()); // run continuation in ui thread

        }

        private void DownloadsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (args.NewItems != null)
            {
                foreach (var item in args.NewItems)
                {
                    var handle = (PackageDownloadHandle) item;
                    handle.PropertyChanged += (o, eventArgs) => this.ClearCompletedCommand.RaiseCanExecuteChanged();
                }
            }

            if (PackageManagerClient.Downloads.Count == 0)
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
            PackageManagerClient.Downloads
                .Where(x => x.DownloadState == PackageDownloadHandle.State.Installed)
                .ToList()
                .ForEach(x=>PackageManagerClient.Downloads.Remove(x));
        }

        public bool CanClearCompleted()
        {
            return PackageManagerClient.Downloads
                                       .Any(x => x.DownloadState == PackageDownloadHandle.State.Installed);
        }

        /// <summary>
        ///     Asynchronously performs a search and updates the observable SearchResults property.
        /// </summary>
        /// <param name="query"> The search query </param>
        internal void SearchAndUpdateResults(string query)
        {
            this.SearchText = query;

            Task<List<PackageManagerSearchElement>>.Factory.StartNew(() => Search(query)

            ).ContinueWith((t) =>
                {

                lock (SearchResults)
                {
                    SearchResults.Clear();
                    foreach (var result in t.Result)
                    {
                        SearchResults.Add(result);
                    }
                    this.SearchState = HasNoResults ? PackageSearchState.NORESULTS : PackageSearchState.RESULTS;
                }
            }
            , TaskScheduler.FromCurrentSynchronizationContext()); // run continuation in ui thread
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

        /// <summary>
        ///     Performs a search using the internal SearcText as the query and
        ///     updates the observable SearchResults property.
        /// </summary>
        internal void SearchAndUpdateResults()
        {
            SearchAndUpdateResults(SearchText);
        }

        /// <summary>
        ///     Performs a search using the given string as query, but does not update
        ///     the SearchResults object.
        /// </summary>
        /// <returns> Returns a list with a maximum MaxNumSearchResults elements.</returns>
        /// <param name="search"> The search query </param>
        internal List<PackageManagerSearchElement> Search(string query)
        {

            if (!String.IsNullOrEmpty(query))
            {
                return SearchDictionary.Search( query, MaxNumSearchResults);
            }
            else
            {
                // with null query, don't show deprecated packages
                return LastSync.Where(x => !x.IsDeprecated).ToList();
            }
        }

        /// <summary>
        ///     Performs a search using the given string as query, but does not update
        ///     the SearchResults object.
        /// </summary>
        /// <returns> Returns a list with a maximum MaxNumSearchResults elements.</returns>
        /// <param name="search"> The search query </param>
        internal List<PackageManagerSearchElement> SearchOnline(string search)
        {
            bool emptySearch = false;
            if (search == "")
            {
                search = "dyn*";
                emptySearch = true;
            }
            else
            {
                search = String.Join("* ", search.Split(' ')) + "*"; // append wild card to each search
            }

            var results = PackageManagerClient.Search(search, MaxNumSearchResults);

            if (emptySearch)
            {
                results.Sort((e1, e2) => e1.Name.ToLower().CompareTo(e2.Name.ToLower()));
            }

            return results;
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

            SearchResults[SelectedIndex].Execute();

        }

        
    }
}
