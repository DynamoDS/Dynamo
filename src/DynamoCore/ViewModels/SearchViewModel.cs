using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using Dynamo.Models;
using Dynamo.Nodes.Search;
using Dynamo.Search;
using Dynamo.Search.SearchElements;
using Dynamo.Selection;
using Microsoft.Practices.Prism.ViewModel;

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

        public SearchModel Model { get; private set; }
        private readonly DynamoViewModel dynamoViewModel;

        #endregion

        #region Initialization

        internal SearchViewModel(DynamoViewModel dynamoViewModel, SearchModel Model)
        {
            this.Model = Model;
            this.dynamoViewModel = dynamoViewModel;

            InitializeCore();
        }

        private void InitializeCore()
        {
            SelectedIndex = 0;
            SearchResults = new ObservableCollection<SearchElementBase>();
            Visible = false;
            searchText = "";

            topResult = this.Model.AddRootCategoryToStart("Top Result");
            
            this.Model.RequestSync += ModelOnRequestSync;
            this.Model.Executed += ExecuteElement;
        }

        #endregion

        #region Destruction

        ~SearchViewModel()
        {
            this.Model.RequestSync -= this.ModelOnRequestSync;
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
        ///     Performs a search and updates the observable SearchResults property.
        /// </summary>
        /// <param name="query"> The search query </param>
        public void SearchAndUpdateResults(string query)
        {
            if (Visible != true)
                return;

            //var sw = new Stopwatch();

            //sw.Start();

            var result = this.Model.Search(query).ToList();

            //sw.Stop();
            
            //this.dynamoViewModel.Model.Logger.Log(String.Format("Search complete in {0}", sw.Elapsed));

            // Remove old execute handler from old top result
            if (topResult.Items.Any() && topResult.Items.First() is NodeSearchElement)
            {
                var oldTopResult = topResult.Items.First() as NodeSearchElement;
                oldTopResult.Executed -= this.ExecuteElement;
            }

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
                foreach (var ele in this.Model.BrowserRootCategories)
                {
                    ele.CollapseToLeaves();
                    ele.SetVisibilityToLeaves(true);
                }

                // hide the top result
                topResult.Visibility = false;
                return;
            }

            // otherwise, first collapse all
            foreach (var root in this.Model.BrowserRootCategories)
            {
                root.CollapseToLeaves();
                root.SetVisibilityToLeaves(false);
            }

            // if there are any results, add the top result 
            if (result.Any() && result.ElementAt(0) is NodeSearchElement)
            {
                topResult.Items.Clear();

                var firstRes = (result.ElementAt(0) as NodeSearchElement);

                var copy = firstRes.Copy();
                copy.Executed += this.ExecuteElement;

                var catName = MakeShortCategoryString(firstRes.FullCategoryName);

                var breadCrumb = new BrowserInternalElement(catName, topResult);
                breadCrumb.AddChild(copy);
                topResult.AddChild(breadCrumb);

                topResult.SetVisibilityToLeaves(true);
                copy.ExpandToRoot();
            }

            // for all of the other results, show them in their category
            foreach (var ele in result)
            {
                ele.Visibility = true;
                ele.ExpandToRoot();
            }

            // create an ordered list of visible search results
            var baseBrowserItem = new BrowserRootElement("root");
            foreach (var root in Model.BrowserRootCategories)
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
            visibleSearchResults.ToList()
                .ForEach(x => SearchResults.Add((NodeSearchElement)x));

        }

        private static string MakeShortCategoryString(string fullCategoryName)
        {
            var catName = fullCategoryName.Replace(".", " > ");

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

            return catName;
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
        public void Execute()
        {
            // none of the elems are selected, return 
            if (SelectedIndex == -1)
                return;

            if (visibleSearchResults.Count <= SelectedIndex)
                return;

            if (!(visibleSearchResults[SelectedIndex] is SearchElementBase)) return;

            ExecuteElement(visibleSearchResults[SelectedIndex] as SearchElementBase);
        }

        private void ExecuteElement(SearchElementBase searchElement)
        {
            dynamic ele = searchElement;
            ExecuteElement(ele);
        }

        private void ExecuteElement(CategorySearchElement searchElement)
        {
            this.SearchText = searchElement.Name + ".";
        }

        private void ExecuteElement(DSFunctionNodeSearchElement element)
        {
            // create node
            var guid = Guid.NewGuid();
            this.dynamoViewModel.ExecuteCommand(
                new DynamoModel.CreateNodeCommand(guid, element.FunctionDescriptor.MangledName, 0, 0, true, true));

            // select node
            var placedNode = dynamoViewModel.Model.Nodes.Find((node) => node.GUID == guid);
            if (placedNode != null)
            {
                DynamoSelection.Instance.ClearSelection();
                DynamoSelection.Instance.Selection.Add(placedNode);
            }
        }

        private void ExecuteElement(CustomNodeSearchElement element)
        {
            string name = element.Guid.ToString();

            // create node
            var guid = Guid.NewGuid();
            dynamoViewModel.ExecuteCommand(
                new DynamoModel.CreateNodeCommand(guid, name, 0, 0, true, true));

            // select node
            var placedNode = dynamoViewModel.Model.Nodes.Find((node) => node.GUID == guid);
            if (placedNode != null)
            {
                DynamoSelection.Instance.ClearSelection();
                DynamoSelection.Instance.Selection.Add(placedNode);
            }
        }

        private void ExecuteElement(NodeSearchElement element)
        {
            // create node
            var guid = Guid.NewGuid();
            dynamoViewModel.ExecuteCommand(
                new DynamoModel.CreateNodeCommand(guid, element.FullName, 0, 0, true, true));

            // select node
            var placedNode = dynamoViewModel.Model.Nodes.Find((node) => node.GUID == guid);
            if (placedNode != null)
            {
                DynamoSelection.Instance.ClearSelection();
                DynamoSelection.Instance.Selection.Add(placedNode);
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
