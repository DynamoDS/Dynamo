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
        public enum PackageSearchState
        {
            Syncing,
            Searching,
            NoResults,
            Results
        };

        public enum PackageSortingKey
        {
            Name,
            Downloads,
            Votes,
            Maintainers,
            LastUpdate
        };

        public enum PackageSortingDirection
        {
            Ascending,
            Descending
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
            SortingKey = PackageSortingKey.LastUpdate;
            SortingDirection = PackageSortingDirection.Ascending;
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
                var key = (string) sortingKey;

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
        
        private string JoinPackageNames(IEnumerable<Package> pkgs)
        {
            return String.Join(", ", pkgs.Select(x => x.Name));
        } 

        private void PackageOnExecuted(PackageManagerSearchElement element, PackageVersion version, string downloadPath)
        {
            string msg = String.IsNullOrEmpty(downloadPath) ?
                String.Format(Resources.MessageConfirmToInstallPackage, element.Name, version.version) :
                String.Format(Resources.MessageConfirmToInstallPackageToFolder, element.Name, version.version, downloadPath);

            var result = MessageBox.Show(msg, 
                Resources.PackageDownloadConfirmMessageBoxTitle,
                MessageBoxButton.OKCancel, MessageBoxImage.Question);

            var pmExt = PackageManagerClientViewModel.DynamoViewModel.Model.GetPackageManagerExtension();
            if (result == MessageBoxResult.OK)
            {
                // get all of the headers
                var headers = version.full_dependency_ids.Select(dep => dep._id).Select((id) =>
                {
                    PackageHeader pkgHeader;
                    var res = pmExt.PackageManagerClient.DownloadPackageHeader(id, out pkgHeader);

                    if (!res.Success)
                        MessageBox.Show(String.Format(Resources.MessageFailedToDownloadPackage, id),
                            Resources.PackageDownloadErrorMessageBoxTitle,
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
                    var res = MessageBox.Show(Resources.MessagePackageContainPythonScript,
                        Resources.PackageDownloadMessageBoxTitle,
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
                    var versionList = FormatPackageVersionList(futureDeps);

                    if (MessageBox.Show(String.Format(Resources.MessagePackageNewerDynamo,
                        PackageManagerClientViewModel.DynamoViewModel.BrandingResourceProvider.ProductName,
                        versionList),
                        string.Format(Resources.PackageUseNewerDynamoMessageBoxTitle,
                        PackageManagerClientViewModel.DynamoViewModel.BrandingResourceProvider.ProductName),
                        MessageBoxButton.OKCancel,
                        MessageBoxImage.Warning) == MessageBoxResult.Cancel)
                    {
                        return;
                    }
                }

                var localPkgs = pmExt.PackageLoader.LocalPackages;

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

                if (uninstallRequiringUserModifications.Any())
                {
                    MessageBox.Show(String.Format(Resources.MessageUninstallToContinue,
                        PackageManagerClientViewModel.DynamoViewModel.BrandingResourceProvider.ProductName,
                        JoinPackageNames(uninstallRequiringUserModifications)),
                        Resources.CannotDownloadPackageMessageBoxTitle, 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var settings = PackageManagerClientViewModel.DynamoViewModel.Model.PreferenceSettings;

                if (uninstallsRequiringRestart.Any())
                {
                    var message = string.Format(Resources.MessageUninstallToContinue2,
                        PackageManagerClientViewModel.DynamoViewModel.BrandingResourceProvider.ProductName,
                        JoinPackageNames(uninstallsRequiringRestart),
                        element.Name + " " + version.version);
                    var dialogResult = MessageBox.Show(message, 
                        Resources.CannotDownloadPackageMessageBoxTitle,
                        MessageBoxButton.YesNo, MessageBoxImage.Error);

                    if (dialogResult == MessageBoxResult.Yes)
                    {
                        // mark for uninstallation
                        uninstallsRequiringRestart.ForEach(x => x.MarkForUninstall(settings));
                    }
                    return;
                }

                if (immediateUninstalls.Any())
                {
                    // if the package is not in use, tell the user we will be uninstall it and give them the opportunity to cancel
                    if (MessageBox.Show(String.Format(Resources.MessageAlreadyInstallDynamo, 
                        PackageManagerClientViewModel.DynamoViewModel.BrandingResourceProvider.ProductName,
                        JoinPackageNames(immediateUninstalls)),
                        Resources.DownloadWarningMessageBoxTitle, 
                        MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.Cancel)
                        return;
                }

                // add custom path to custom package folder list
                if (!String.IsNullOrEmpty(downloadPath))
                {
                    if (!settings.CustomPackageFolders.Contains(downloadPath))
                        settings.CustomPackageFolders.Add(downloadPath);
                }

                // form header version pairs and download and install all packages
                allPackageVersions
                        .Select(x => new PackageDownloadHandle(x.Item1, x.Item2))
                        .ToList()
                        .ForEach(x => this.PackageManagerClientViewModel.DownloadAndInstall(x, downloadPath));

            }
        }

        /// <summary>
        ///     Returns a newline delimited string representing the package name and version of the argument
        /// </summary>
        public static string FormatPackageVersionList(IEnumerable<Tuple<PackageHeader, PackageVersion>> packages)
        {
            return String.Join("\r\n", packages.Select(x => x.Item1.name + " " + x.Item2.version));
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

            var canLogin = PackageManagerClientViewModel.AuthenticationManager.HasAuthProvider;
            List<PackageManagerSearchElementViewModel> list = null;

            if (!String.IsNullOrEmpty(query))
            {
                list = SearchDictionary.Search(query)
                    .Select(x => new PackageManagerSearchElementViewModel(x, canLogin))
                    .Take(MaxNumSearchResults).ToList();
            }
            else
            {
                // with null query, don't show deprecated packages
                list = LastSync.Where(x => !x.IsDeprecated)
                    .Select(x => new PackageManagerSearchElementViewModel(x, canLogin)).ToList();
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
