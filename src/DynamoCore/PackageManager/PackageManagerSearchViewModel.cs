using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

using Dynamo.Search;
using Dynamo.ViewModels;

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

        public enum PackageSortingKey
        {
            NAME,
            DOWNLOADS,
            VOTES,
            MAINTAINERS,
            LAST_UPDATE
        };

        public enum PackageSortingDirection
        {
            ASCENDING,
            DESCENDING
        };

        #region Properties & Fields

        // The results of the last synchronization with the package manager server
        public List<PackageManagerSearchElement> LastSync { get; set; }

        /// <summary>
        ///     SortingKey property
        /// </summary>
        /// <value>
        ///     Set which kind of sorting should be used for displaying search results
        /// </value>
        public PackageSortingKey _sortingKey;
        public PackageSortingKey SortingKey
        {
            get { return _sortingKey; }
            set
            {
                _sortingKey = value;
                RaisePropertyChanged("SortingKey");
            }
        }


        /// <summary>
        ///     SortingDirection property
        /// </summary>
        /// <value>
        ///     Set which kind of sorting should be used for displaying search results
        /// </value>
        public PackageSortingDirection _sortingDirection;
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
            get { return PackageManagerClientViewModel.Downloads.Count > 0; }
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
        public PackageManagerClientViewModel PackageManagerClientViewModel { get; private set; }

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

        /// <summary>
        ///     The class constructor.
        /// </summary>
        public PackageManagerSearchViewModel(PackageManagerClientViewModel client)
        {
            this.PackageManagerClientViewModel = client;

            SearchResults = new ObservableCollection<PackageManagerSearchElement>();
            MaxNumSearchResults = 35;
            SearchDictionary = new SearchDictionary<PackageManagerSearchElement>();
            ClearCompletedCommand = new DelegateCommand(ClearCompleted, CanClearCompleted);
            SortCommand = new DelegateCommand(Sort, CanSort);
            SetSortingKeyCommand = new DelegateCommand<object>(SetSortingKey, CanSetSortingKey);
            SetSortingDirectionCommand = new DelegateCommand<object>(SetSortingDirection, CanSetSortingDirection);
            PackageManagerClientViewModel.Downloads.CollectionChanged += DownloadsOnCollectionChanged;
            SearchResults.CollectionChanged += SearchResultsOnCollectionChanged;
            SearchText = "";
            SortingKey = PackageSortingKey.LAST_UPDATE;
            SortingDirection = PackageSortingDirection.ASCENDING;
        }

        /// <summary>
        /// Sort the search results
        /// </summary>
        public void Sort()
        {
            var list = this.SearchResults.AsEnumerable().ToList();
            Sort(list, this.SortingKey);
            this.SearchResults.Clear();

            if (SortingDirection == PackageSortingDirection.DESCENDING)
            {
                list.Reverse();
            }

            foreach (var ele in list)
            {
                this.SearchResults.Add(ele);
            }
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
                    this.SortingDirection = PackageSortingDirection.ASCENDING;
                }
                else if (key == "DESCENDING")
                {
                    this.SortingDirection = PackageSortingDirection.DESCENDING;
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
                var key = (string) sortingKey;

                if (key == "NAME")
                {
                    this.SortingKey = PackageSortingKey.NAME;
                } 
                else if (key == "DOWNLOADS")
                {
                    this.SortingKey = PackageSortingKey.DOWNLOADS;
                } 
                else if (key == "MAINTAINERS")
                {
                    this.SortingKey = PackageSortingKey.MAINTAINERS;
                }
                else if (key == "LAST_UPDATE")
                {
                    this.SortingKey = PackageSortingKey.LAST_UPDATE;
                } 
                else if (key == "VOTES")
                {
                    this.SortingKey = PackageSortingKey.VOTES;
                }

            } 
            else if (sortingKey is PackageSortingKey)
            {
                this.SortingKey = (PackageSortingKey) sortingKey;
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
        public IEnumerable<PackageManagerSearchElement> RefreshAndSearch()
        {

            Refresh();
            return Search(SearchText);

        }

        public void RefreshAndSearchAsync()
        {
            SearchResults.Clear();
            this.SearchState = PackageSearchState.SYNCING;

            Task<IEnumerable<PackageManagerSearchElement>>.Factory.StartNew(RefreshAndSearch).ContinueWith((t) =>
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
            this.SearchText = query;

            Task<IEnumerable<PackageManagerSearchElement>>.Factory.StartNew(() => Search(query)

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
        internal IEnumerable<PackageManagerSearchElement> Search(string query)
        {
            if (!String.IsNullOrEmpty(query))
            {
                return SearchDictionary.Search( query, MaxNumSearchResults);
            }
            else
            {
                // with null query, don't show deprecated packages
                List<PackageManagerSearchElement> list = LastSync.Where(x => !x.IsDeprecated).ToList();
                Sort(list, this.SortingKey);
                return list;
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

            var results = PackageManagerClientViewModel.Search(search, MaxNumSearchResults);

            if (emptySearch)
            {
                Sort(results, this.SortingKey);
            }

            return results;
        }

        /// <summary>
        /// Sort a list of search results by the given key
        /// </summary>
        /// <param name="results"></param>
        private static void Sort(List<PackageManagerSearchElement> results, PackageSortingKey key)
        {
            switch (key)
            {
                case PackageSortingKey.NAME:
                    results.Sort((e1, e2) => e1.Name.ToLower().CompareTo(e2.Name.ToLower()));
                    break;
                case PackageSortingKey.DOWNLOADS:
                    results.Sort((e1, e2) => e2.Downloads.CompareTo(e1.Downloads));
                    break;
                case PackageSortingKey.LAST_UPDATE:
                    results.Sort((e1, e2) => e2.Versions.Last().Item1.created.CompareTo(e1.Versions.Last().Item1.created));
                    break;
                case PackageSortingKey.VOTES:
                    results.Sort((e1, e2) => e2.Votes.CompareTo(e1.Votes));
                    break;
                case PackageSortingKey.MAINTAINERS:
                    results.Sort((e1, e2) => e1.Maintainers.ToLower().CompareTo(e2.Maintainers.ToLower()));
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

            SearchResults[SelectedIndex].Execute();
        }
        
    }
}
