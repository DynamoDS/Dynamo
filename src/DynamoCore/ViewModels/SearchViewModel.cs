using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Nodes.Search;
using Dynamo.Search;
using Dynamo.Search.SearchElements;
using Dynamo.Utilities;
using Microsoft.Practices.Prism.ViewModel;
using Dynamo.DSEngine;

namespace Dynamo.ViewModels
{
    public partial class SearchViewModel : NotificationObject
    {
        #region events

        public event EventHandler RequestFocusSearch;
        public virtual void OnRequestFocusSearch(object sender, EventArgs e)
        {
            if (RequestFocusSearch != null)
                RequestFocusSearch(this, e);
        }

        public event EventHandler RequestReturnFocusToSearch;
        public virtual void OnRequestReturnFocusToSearch(object sender, EventArgs e)
        {
            if (RequestReturnFocusToSearch != null)
                RequestReturnFocusToSearch(this, e);
        }

        #endregion

        #region Properties/Fields

        /// <summary>
        ///     Indicates whether the node browser is visible or not
        /// </summary>
        private bool browserVisibility = true;
        public bool BrowserVisibility
        {
            get { return browserVisibility; }
            set { browserVisibility = value; RaisePropertyChanged("BrowserVisibility"); }
        }

        /// <summary>
        ///     SearchText property
        /// </summary>
        /// <value>
        ///     This is the core UI for Dynamo, primarily used for logging.
        /// </value>
        private string searchText;
        public string SearchText
        {
            get { return searchText; }
            set
            {
                searchText = value;
                RaisePropertyChanged("SearchText");
            }
        }

        /// <summary>
        ///     SelectedIndex property
        /// </summary>
        /// <value>
        ///     This is the currently selected element in the UI.
        /// </value>
        private int selectedIndex;
        public int SelectedIndex
        {
            get { return selectedIndex; }
            set
            {
                if (selectedIndex != value)
                {
                    if (visibleSearchResults.Count > selectedIndex)
                        visibleSearchResults[selectedIndex].IsSelected = false;
                    selectedIndex = value;
                    if (visibleSearchResults.Count > selectedIndex)
                        visibleSearchResults[selectedIndex].IsSelected = true;
                    RaisePropertyChanged("SelectedIndex");
                }
            }
        }

        /// <summary>
        ///     Visible property
        /// </summary>
        /// <value>
        ///     Tells whether the View is visible or not
        /// </value>
        private bool visible;
        public bool Visible
        {
            get { return visible; }
            set
            {
                visible = value;
                RaisePropertyChanged("Visible");
            }
        }

        /// <summary>
        ///     SearchResults property
        /// </summary>
        /// <value>
        ///     This property is observed by SearchView to see the search results
        /// </value>
        public ObservableCollection<SearchElementBase> SearchResults { get; private set; }

        /// <summary>
        /// A category representing the "Top Result"
        /// </summary>
        private BrowserRootElement topResult;

        /// <summary>
        ///     An ordered list representing all of the visible items in the browser.
        ///     This is used to manage up-down navigation through the menu.
        /// </summary>
        private List<BrowserItem> visibleSearchResults = new List<BrowserItem>();

        private bool searchScrollBarVisibility = true;
        public bool SearchScrollBarVisibility
        {
            get { return searchScrollBarVisibility; }
            set { searchScrollBarVisibility = value; RaisePropertyChanged("SearchScrollBarVisibility"); }
        }

        private readonly SearchModel model;
        private readonly DynamoViewModel dynamoViewModel;

        #endregion

        #region Initialization

        internal SearchViewModel(DynamoViewModel dynamoViewModel, SearchModel model)
        {
            this.model = model;
            this.dynamoViewModel = dynamoViewModel;

            this.model.RequestSync += ModelOnRequestSync;

            InitializeCore();
        }

        private void InitializeCore()
        {
            SelectedIndex = 0;
            SearchResults = new ObservableCollection<SearchElementBase>();
            Visible = false;
            searchText = "";

            topResult = this.model.AddRootCategory("Top Result");
            this.model.AddRootCategory(BuiltinNodeCategories.CORE);
            this.model.AddRootCategory(BuiltinNodeCategories.LOGIC);
            this.model.AddRootCategory(BuiltinNodeCategories.GEOMETRY);
            this.model.AddRootCategory(BuiltinNodeCategories.REVIT);
            this.model.AddRootCategory(BuiltinNodeCategories.ANALYZE);
            this.model.AddRootCategory(BuiltinNodeCategories.IO);
        }

        #endregion

        #region Destruction

        ~SearchViewModel()
        {
            this.model.RequestSync -= this.ModelOnRequestSync;
        }

        #endregion

        #region Search

        private void ModelOnRequestSync(object sender, EventArgs eventArgs)
        {
            this.SearchAndUpdateResults();
        }

        /// <summary>
        ///     Performs a search using the internal SearcText as the query and
        ///     updates the observable SearchResults property.
        /// </summary>
        internal void SearchAndUpdateResults()
        {
            this.SearchAndUpdateResults(SearchText);
        }

        /// <summary>
        ///     Asynchronously performs a search and updates the observable SearchResults property.
        /// </summary>
        /// <param name="query"> The search query </param>
        public void SearchAndUpdateResults(string query)
        {
            if (Visible != true)
                return;

            Task<IEnumerable<SearchElementBase>>.Factory.StartNew(() =>
            {
                lock (model.SearchDictionary)
                {
                    return model.Search(query);
                }
            }).ContinueWith((t) =>
            {
                lock (visibleSearchResults)
                {

                    // deselect the last selected item
                    if (visibleSearchResults.Count > SelectedIndex)
                    {
                        visibleSearchResults[SelectedIndex].IsSelected = false;
                    }

                    // clear visible results list
                    visibleSearchResults.Clear();

                    // if the search query is empty, go back to the default treeview
                    if (string.IsNullOrEmpty(query))
                    {

                        foreach (var ele in this.model.BrowserRootCategories)
                        {
                            ele.CollapseToLeaves();
                            ele.SetVisibilityToLeaves(true);
                        }

                        // hide the top result
                        topResult.Visibility = false;

                        return;
                    }

                    // otherwise, first collapse all
                    foreach (var root in this.model.BrowserRootCategories)
                    {
                        root.CollapseToLeaves();
                        root.SetVisibilityToLeaves(false);
                    }

                    // if there are any results, add the top result 
                    if (t.Result.Any() && t.Result.ElementAt(0) is NodeSearchElement)
                    {
                        topResult.Items.Clear();

                        var firstRes = (t.Result.ElementAt(0) as NodeSearchElement);

                        var copy = firstRes.Copy();

                        var catName = firstRes.FullCategoryName.Replace(".", " > ");

                        // if the category name is too long, we strip off the interior categories
                        if (catName.Length > 50)
                        {
                            var s = catName.Split('>').Select(x => x.Trim()).ToList();
                            if (s.Count() > 4)
                            {
                                s = new List<string>()
                                        {
                                            s[0],
                                            "...",
                                            s[s.Count - 3],
                                            s[s.Count - 2],
                                            s[s.Count - 1]
                                        };
                                catName = String.Join(" > ", s);
                            }
                        }

                        var breadCrumb = new BrowserInternalElement(catName, topResult);
                        breadCrumb.AddChild(copy);
                        topResult.AddChild(breadCrumb);

                        topResult.SetVisibilityToLeaves(true);
                        copy.ExpandToRoot();

                    }

                    // for all of the other results, show them in their category
                    foreach (var ele in this.model.SearchElements)
                    {
                        if (t.Result.Contains(ele))
                        {
                            ele.Visibility = true;
                            ele.ExpandToRoot();
                        }
                    }

                    // create an ordered list of visible search results
                    var baseBrowserItem = new BrowserRootElement("root");
                    foreach (var root in model.BrowserRootCategories)
                    {
                        baseBrowserItem.Items.Add(root);
                    }

                    baseBrowserItem.GetVisibleLeaves(ref visibleSearchResults);

                    if (visibleSearchResults.Any())
                    {
                        this.SelectedIndex = 0;
                        visibleSearchResults[0].IsSelected = true;
                    }

                    SearchResults.Clear();
                    visibleSearchResults.ToList().ForEach(x => SearchResults.Add((NodeSearchElement)x));
                }

            }
            , TaskScheduler.FromCurrentSynchronizationContext()); // run continuation in ui thread
        }

        /// <summary>
        ///     Synchronously performs a search using the current SearchText
        /// </summary>
        /// <param name="query"> The search query </param>
        public void SearchAndUpdateResultsSync()
        {
            SearchAndUpdateResultsSync(SearchText);
        }

        /// <summary>
        ///     Synchronously Performs a search and updates the observable SearchResults property
        ///     on the current thread.
        /// </summary>
        /// <param name="query"> The search query </param>
        public void SearchAndUpdateResultsSync(string query)
        {
            var result = model.Search(query);

            SearchResults.Clear();
            foreach (var node in result)
            {
                SearchResults.Add(node);
            }
            SelectedIndex = 0;
        }

        #endregion

        #region Selection

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

        #endregion

        #region Search field manipulation

        /// <summary>
        ///     If there's a period in the SearchText property, remove text
        ///     to the end until you hit a period.  Otherwise, remove the
        ///     last character.  If the SearchText property is empty or null
        ///     return doing nothing.
        /// </summary>
        public void RemoveLastPartOfSearchText()
        {
            SearchText = RemoveLastPartOfText(SearchText);
        }

        /// <summary>
        ///     If there's a period in the argument, remove text
        ///     to the end until you hit a period.  Otherwise, remove the
        ///     last character.  If the argument is empty or null
        ///     return the empty string.
        /// </summary>
        /// <returns>The string cleaved of everything </returns>
        public static string RemoveLastPartOfText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var matches = Regex.Matches(text, Regex.Escape("."));

            // no period
            if (matches.Count == 0)
            {
                return "";
            }

            // if period is in last position, remove that period and recurse
            if (matches[matches.Count - 1].Index + 1 == text.Length)
            {
                return RemoveLastPartOfText(text.Substring(0, text.Length - 1));
            }

            // remove to the last period
            return text.Substring(0, matches[matches.Count - 1].Index + 2);
        }

        /// <summary>
        ///     If there are results, fill the search field with the text from the
        ///     name property of the first search result.
        /// </summary>
        public void PopulateSearchTextWithSelectedResult()
        {
            if (SearchResults.Count == 0) return;

            // none of the elems are selected, return 
            if (SelectedIndex == -1)
                return;

            SearchText = SearchResults[SelectedIndex].Name;
        }

        #endregion

        #region Execution

        /// <summary>
        ///     Runs the Execute() method of the current selected SearchElementBase object
        ///     amongst the SearchResults.
        /// </summary>
        public void ExecuteSelected()
        {

            // none of the elems are selected, return 
            if (SelectedIndex == -1)
                return;

            if (visibleSearchResults.Count <= SelectedIndex)
                return;

            if (visibleSearchResults[SelectedIndex] is SearchElementBase)
            {
                ( (SearchElementBase) visibleSearchResults[SelectedIndex]).Execute();
            }

        }

        #endregion

        #region Commands

        public void Search(object parameter)
        {
            this.SearchAndUpdateResults();
        }

        internal bool CanSearch(object parameter)
        {
            return true;
        }

        internal void HideSearch(object parameter)
        {
            this.Visible = false;
        }

        internal bool CanHideSearch(object parameter)
        {
            if (this.Visible == true)
                return true;
            return false;
        }

        public void ShowSearch(object parameter)
        {
            this.Visible = true;
        }

        internal bool CanShowSearch(object parameter)
        {
            if (this.Visible == false)
                return true;
            return false;
        }

        public void FocusSearch(object parameter)
        {
            this.OnRequestFocusSearch(this.dynamoViewModel, EventArgs.Empty);
        }

        internal bool CanFocusSearch(object parameter)
        {
            return true;
        }

        #endregion

    }
}
