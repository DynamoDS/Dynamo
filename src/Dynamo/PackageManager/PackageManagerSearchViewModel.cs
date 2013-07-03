using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Microsoft.Practices.Prism.ViewModel;

namespace Dynamo.PackageManager
{
    public class PackageManagerSearchViewModel : NotificationObject
    {
        #region Properties & Fields

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
                DynamoCommands.Search.Execute(null);
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
                    //if (_visibleSearchResults.Count > _selectedIndex)
                    //    _visibleSearchResults[_selectedIndex].IsSelected = false;
                    _selectedIndex = value;
                    //if (_visibleSearchResults.Count > _selectedIndex)
                    //    _visibleSearchResults[_selectedIndex].IsSelected = true;
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

#endregion Properties & Fields

        /// <summary>
        ///     The class constructor.
        /// </summary>
        /// <param name="bench"> Reference to dynBench object for logging </param>
        public PackageManagerSearchViewModel(PackageManagerClient client)
        {
            PackageManagerClient = client;
            SearchResults = new ObservableCollection<PackageManagerSearchElement>();
            MaxNumSearchResults = 12;
        }
        /// <summary>
        ///     Asynchronously performs a search and updates the observable SearchResults property.
        /// </summary>
        /// <param name="query"> The search query </param>
        internal void SearchAndUpdateResults(string query)
        {
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
                }
            }
            , TaskScheduler.FromCurrentSynchronizationContext()); // run continuation in ui thread
        }

        ///// <summary>
        /////     Synchronously performs a search using the current SearchText
        ///// </summary>
        ///// <param name="query"> The search query </param>
        //internal void SearchAndUpdateResultsSync()
        //{
        //    SearchAndUpdateResultsSync(SearchText);
        //}

        ///// <summary>
        /////     Synchronously Performs a search and updates the observable SearchResults property
        /////     on the current thread.
        ///// </summary>
        ///// <param name="query"> The search query </param>
        //internal void SearchAndUpdateResultsSync(string query)
        //{
        //    var result = Search(query);

        //    SearchResults.Clear();
        //    foreach (var node in result)
        //    {
        //        SearchResults.Add(node);
        //    }
        //    SelectedIndex = 0;
        //}

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
        internal List<PackageManagerSearchElement> Search(string search)
        {
            if (string.IsNullOrEmpty(search) || search == "Search...")
            {
                return new List<PackageManagerSearchElement>();
            }

            return PackageManagerClient.Search(search, MaxNumSearchResults);

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
