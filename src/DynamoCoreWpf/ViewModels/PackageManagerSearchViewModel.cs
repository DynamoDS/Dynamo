using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Dynamo.PackageManager.ViewModels;
using Dynamo.Search;
using Dynamo.Search.SearchElements;
using Dynamo.Utilities;

using Dynamo.ViewModels;

using Greg.Responses;

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
            SearchText = "";
            SortingKey = PackageSortingKey.LAST_UPDATE;
            SortingDirection = PackageSortingDirection.ASCENDING;
        }

        /// <summary>
        ///     The class constructor.
        /// </summary>
        public PackageManagerSearchViewModel(PackageManagerClientViewModel client) : this()
        {
            this.PackageManagerClientViewModel = client;
            PackageManagerClientViewModel.Downloads.CollectionChanged += DownloadsOnCollectionChanged;
        }

        /// <summary>
        /// Sort the search results
        /// </summary>
        public void Sort()
        {
            var list = this.SearchResults.AsEnumerable().ToList();
            Sort(list, this.SortingKey);

            if (SortingDirection == PackageSortingDirection.DESCENDING)
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
        public IEnumerable<PackageManagerSearchElementViewModel> RefreshAndSearch()
        {
            Refresh();
            return Search(SearchText);
        }

        public void RefreshAndSearchAsync()
        {
            this.ClearSearchResults();
            this.SearchState = PackageSearchState.SYNCING;

            Task<IEnumerable<PackageManagerSearchElementViewModel>>.Factory.StartNew(RefreshAndSearch).ContinueWith((t) =>
            {
                lock (SearchResults)
                {
                    ClearSearchResults();
                    foreach (var result in t.Result)
                    {
                        this.AddToSearchResults(result);
                    }
                    this.SearchState = HasNoResults ? PackageSearchState.NORESULTS : PackageSearchState.RESULTS;
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
        
        private string JoinPackageNames(IEnumerable<Package> pkgs)
        {
            return String.Join(", ", pkgs.Select(x => x.Name));
        } 

        private void PackageOnExecuted(PackageManagerSearchElement element, PackageVersion version)
        {
            string message = "Are you sure you want to install " + element.Name + " " + version.version + "?";

            var result = MessageBox.Show(message, "Package Download Confirmation",
                            MessageBoxButton.OKCancel, MessageBoxImage.Question);

            if (result == MessageBoxResult.OK)
            {
                // get all of the headers
                var headers = version.full_dependency_ids.Select(dep => dep._id).Select((id) =>
                {
                    PackageHeader pkgHeader;
                    var res = this.PackageManagerClientViewModel.DynamoViewModel.Model.PackageManagerClient.DownloadPackageHeader(id, out pkgHeader);

                    if (!res.Success)
                        MessageBox.Show("Failed to download package with id: " + id + ".  Please try again and report the package if you continue to have problems.", "Package Download Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);

                    return pkgHeader;
                }).ToList();

                // if any header download fails, abort
                if (headers.Any(x => x == null))
                {
                    return;
                }

                var allPackageVersions = PackageManagerSearchElement.ListRequiredPackageVersions(headers, version);

                // determine if any of the packages contain binaries or python scripts.  
                var containsBinaries =
                    allPackageVersions.Any(
                        x => x.Item2.contents.Contains(PackageManagerClient.PackageContainsBinariesConstant) || x.Item2.contains_binaries);

                var containsPythonScripts =
                    allPackageVersions.Any(
                        x => x.Item2.contents.Contains(PackageManagerClient.PackageContainsPythonScriptsConstant));

                // if any do, notify user and allow cancellation
                if (containsBinaries || containsPythonScripts)
                {
                    var res = MessageBox.Show("The package or one of its dependencies contains Python scripts or binaries. " +
                        "Do you want to continue?", "Package Download",
                        MessageBoxButton.OKCancel, MessageBoxImage.Exclamation);

                    if (res == MessageBoxResult.Cancel) return;
                }

                // Determine if there are any dependencies that are made with a newer version
                // of Dynamo (this includes the root package)
                var dynamoVersion = this.PackageManagerClientViewModel.DynamoViewModel.Model.Version;
                var dynamoVersionParsed = VersionUtilities.PartialParse(dynamoVersion, 3);
                var futureDeps = allPackageVersions.FilterFuturePackages(dynamoVersionParsed);

                // If any of the required packages use a newer version of Dynamo, show a dialog to the user
                // allowing them to cancel the package download
                if (futureDeps.Any())
                {
                    var sb = new StringBuilder();

                    sb.AppendLine(
                        "The following packages use a newer version of Dynamo than you are currently using: ");
                    sb.AppendLine();

                    foreach (var elem in futureDeps)
                    {
                        sb.AppendLine(elem.Item1.name + " " + elem.Item2);
                    }

                    sb.AppendLine();
                    sb.AppendLine("Do you want to continue?");

                    // If the user
                    if (MessageBox.Show(
                        sb.ToString(),
                        "Package Uses Newer Version of Dynamo!",
                        MessageBoxButton.OKCancel,
                        MessageBoxImage.Warning) == MessageBoxResult.Cancel)
                    {
                        return;
                    }
                }

                var localPkgs = this.PackageManagerClientViewModel.DynamoViewModel.Model.Loader.PackageLoader.LocalPackages;

                var uninstallsRequiringRestart = new List<Package>();
                var uninstallRequiringUserModifications = new List<Package>();
                var immediateUninstalls = new List<Package>();

                // if a package is already installed we need to uninstall it, allowing
                // the user to cancel if they do not want to uninstall the package
                foreach (var localPkg in headers.Select(x => localPkgs.FirstOrDefault(v => v.Name == x.name)))
                {
                    if (localPkg == null) continue;

                    if (localPkg.LoadedAssemblies.Any())
                    {
                        uninstallsRequiringRestart.Add(localPkg);
                        continue;
                    }

                    if (localPkg.InUse(this.PackageManagerClientViewModel.DynamoViewModel.Model))
                    {
                        uninstallRequiringUserModifications.Add(localPkg);
                        continue;
                    }

                    immediateUninstalls.Add(localPkg);
                }

                string msg;

                if (uninstallRequiringUserModifications.Any())
                {
                    msg = "Dynamo needs to uninstall " + JoinPackageNames(uninstallRequiringUserModifications) +
                        " to continue, but cannot as one of its types appears to be in use.  Try restarting Dynamo.";
                    MessageBox.Show(msg, "Cannot Download Package", MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                    return;
                }

                if (uninstallsRequiringRestart.Any())
                {
                    // mark for uninstallation
                    uninstallsRequiringRestart.ForEach(
                        x =>
                            x.MarkForUninstall(
                                this.PackageManagerClientViewModel.DynamoViewModel.Model.PreferenceSettings));

                    msg = "Dynamo needs to uninstall " + JoinPackageNames(uninstallsRequiringRestart) +
                        " to continue but it contains binaries already loaded into Dynamo.  It's now marked " +
                        "for removal, but you'll need to first restart Dynamo.";
                    MessageBox.Show(msg, "Cannot Download Package", MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                    return;
                }

                if (immediateUninstalls.Any())
                {
                    // if the package is not in use, tell the user we will be uninstall it and give them the opportunity to cancel
                    msg = "Dynamo has already installed " + JoinPackageNames(immediateUninstalls) +
                        ".  \n\nDynamo will attempt to uninstall this package before installing.  ";
                    if (MessageBox.Show(msg, "Download Warning", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.Cancel)
                        return;
                }

                // form header version pairs and download and install all packages
                allPackageVersions
                        .Select(x => new PackageDownloadHandle(x.Item1, x.Item2))
                        .ToList()
                        .ForEach(x => this.PackageManagerClientViewModel.DownloadAndInstall(x));

            }

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

            var t = Search(query);

            var currentResults = this.SearchResults;

            // stop WPF from listening to the changes that we're about
            // to perform
            this.SearchResults = null;

            currentResults.Clear();
            foreach (var result in t)
            {
                currentResults.Add(result);
            }

            // cause WPF to rebind--but only once instead of once for
            // each ele
            SearchResults = currentResults;

            SearchState = HasNoResults ? PackageSearchState.NORESULTS : PackageSearchState.RESULTS;
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
        /// <param name="query"> The search query </param>
        internal IEnumerable<PackageManagerSearchElementViewModel> Search(string query)
        {
            if (LastSync == null) return new List<PackageManagerSearchElementViewModel>();

            if (!String.IsNullOrEmpty(query))
            {
                return
                    SearchDictionary.Search(query, MaxNumSearchResults)
                        .Select(x => new PackageManagerSearchElementViewModel(x));
            }

            // with null query, don't show deprecated packages
            var list =
                LastSync.Where(x => !x.IsDeprecated)
                    .Select(x => new PackageManagerSearchElementViewModel(x)).ToList();
            Sort(list, this.SortingKey);
            return list;

        }

        /// <summary>
        ///     Performs a search using the given string as query, but does not update
        ///     the SearchResults object.
        /// </summary>
        /// <returns> Returns a list with a maximum MaxNumSearchResults elements.</returns>
        /// <param name="search"> The search query </param>
        internal List<PackageManagerSearchElementViewModel> SearchOnline(string search)
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

            var results =
                PackageManagerClientViewModel.Search(search, MaxNumSearchResults)
                    .Select(x => new PackageManagerSearchElementViewModel(x)).ToList();

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
        private static void Sort(List<PackageManagerSearchElementViewModel> results, PackageSortingKey key)
        {
            switch (key)
            {
                case PackageSortingKey.NAME:
                    results.Sort((e1, e2) => e1.Model.Name.ToLower().CompareTo(e2.Model.Name.ToLower()));
                    break;
                case PackageSortingKey.DOWNLOADS:
                    results.Sort((e1, e2) => e2.Model.Downloads.CompareTo(e1.Model.Downloads));
                    break;
                case PackageSortingKey.LAST_UPDATE:
                    results.Sort((e1, e2) => e2.Versions.Last().Item1.created.CompareTo(e1.Versions.Last().Item1.created));
                    break;
                case PackageSortingKey.VOTES:
                    results.Sort((e1, e2) => e2.Model.Votes.CompareTo(e1.Model.Votes));
                    break;
                case PackageSortingKey.MAINTAINERS:
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
