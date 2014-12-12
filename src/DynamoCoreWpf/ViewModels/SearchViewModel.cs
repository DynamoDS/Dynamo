using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text.RegularExpressions;

using Dynamo.Models;
using Dynamo.Search;
using Dynamo.Search.SearchElements;
using Dynamo.Selection;
using Dynamo.Wpf.ViewModels;

using Microsoft.Practices.Prism.ViewModel;

using CustomNodeSearchElement = Dynamo.Search.SearchElements.CustomNodeSearchElement;
using NodeModelSearchElement = Dynamo.Search.SearchElements.NodeModelSearchElement;

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
                        visibleSearchResults[selectedIndex].Model.IsSelected = false;
                    selectedIndex = value;
                    if (visibleSearchResults.Count > selectedIndex)
                        visibleSearchResults[selectedIndex].Model.IsSelected = true;
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
        public ObservableCollection<SearchElementBaseViewModel> SearchResults { get; private set; }

        /// <summary>
        /// A category representing the "Top Result"
        /// </summary>
        private BrowserRootElementViewModel topResult;

        /// <summary>
        ///     An ordered list representing all of the visible items in the browser.
        ///     This is used to manage up-down navigation through the menu.
        /// </summary>
        private List<BrowserItemViewModel> visibleSearchResults = new List<BrowserItemViewModel>();

        private bool searchScrollBarVisibility = true;
        public bool SearchScrollBarVisibility
        {
            get { return searchScrollBarVisibility; }
            set { searchScrollBarVisibility = value; RaisePropertyChanged("SearchScrollBarVisibility"); }
        }

        public ObservableCollection<BrowserRootElementViewModel> BrowserRootCategories { get; private set; }
        public NodeSearchModel Model { get; private set; }
        private readonly DynamoViewModel dynamoViewModel;

        #endregion

        #region Initialization

        internal SearchViewModel(DynamoViewModel dynamoViewModel, NodeSearchModel model)
        {
            Model = model;
            this.dynamoViewModel = dynamoViewModel;

            InitializeCore();
        }

        private void InitializeCore()
        {
            SelectedIndex = 0;
            SearchResults = new ObservableCollection<SearchElementBaseViewModel>();
            BrowserRootCategories = new ObservableCollection<BrowserRootElementViewModel>();
            Visible = false;
            searchText = "";

            // Here, we add a root category: "Top Result"
            topResult =
                BrowserItemViewModel.Wrap(Model.AddRootCategoryToStart("Top Result")) as
                    BrowserRootElementViewModel;
            
            // When LibraryUpdated fires, sync up
            Model.LibraryUpdated += ModelOnRequestSync;

            // Delete all empty categories.
            Model.RemoveEmptyCategories();

            // Mirror BrowserRootCategories from the Model to the ViewModel
            foreach (BrowserRootElement item in Model.BrowserRootCategories)
            {
                BrowserRootCategories.Add(BrowserItemViewModel.WrapExplicit(item));
            }

            // Respond to collection changes in BrowserRootCategories
            Model.BrowserRootCategories.CollectionChanged += BrowserRootCategoriesOnCollectionChanged;

            //Sort all categories alphabetically, recursively
            SortCategoryChildren();
        }

        /// <summary>
        /// Helper method for synchronization of the Model observable collections
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BrowserRootCategoriesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (BrowserRootElement item in e.NewItems.OfType<BrowserRootElement>())
                    {
                        BrowserRootCategories.Add(BrowserItemViewModel.WrapExplicit(item));
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    BrowserRootCategories.Clear();
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (
                        var vm in
                            from object item in e.OldItems
                            select BrowserRootCategories.First(x => x.Model == item))
                    {
                        BrowserRootCategories.Remove(vm);
                    }
                    break;
            }
        }

        #endregion

        #region Search

        internal void SortCategoryChildren()
        {
            foreach (var item in BrowserRootCategories)
            {
                item.RecursivelySort();
            }
        }

        private void ModelOnRequestSync(object sender, EventArgs eventArgs)
        {
            SearchAndUpdateResults();
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
        ///     Performs a search and updates the observable SearchResults property.
        /// </summary>
        /// <param name="query"> The search query </param>
        public void SearchAndUpdateResults(string query)
        {
            if (Visible != true)
                return;

            List<SearchElementBase> result = Model.Search(query).ToList();

            // Remove old execute handler from old top result
            if (topResult.Items.Any())
            {
                var oldTopResult = topResult.Items.First() as NodeSearchElementViewModel;
                if (oldTopResult != null)
                    oldTopResult.Model.Executed -= ExecuteElement;
            }

            // deselect the last selected item
            if (visibleSearchResults.Count > SelectedIndex)
                visibleSearchResults[SelectedIndex].Model.IsSelected = false;

            // clear visible results list
            visibleSearchResults.Clear();

            // if the search query is empty, go back to the default treeview
            if (string.IsNullOrEmpty(query))
            {
                foreach (var ele in Model.BrowserRootCategories)
                {
                    ele.CollapseToLeaves();
                    ele.SetVisibilityToLeaves(true);
                }

                // hide the top result
                topResult.Model.Visibility = false;
                return;
            }

            // otherwise, first collapse all
            foreach (var root in Model.BrowserRootCategories)
            {
                root.CollapseToLeaves();
                root.SetVisibilityToLeaves(false);
            }

            // if there are any results, add the top result 
            if (result.Any())
            {
                var firstRes = (result.First() as NodeModelSearchElement);
                if (firstRes != null)
                {
                    topResult.Model.Items.Clear();

                    var copy = firstRes.Copy();
                    copy.Executed += ExecuteElement;

                    var catName = MakeShortCategoryString(firstRes.FullCategoryName);

                    var breadCrumb = new BrowserInternalElement(catName, topResult.Model);
                    breadCrumb.AddChild(copy);
                    topResult.Model.AddChild(breadCrumb);

                    topResult.Model.SetVisibilityToLeaves(true);
                    copy.ExpandToRoot();
                }
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

            var vl = new List<BrowserItem>();
            baseBrowserItem.GetVisibleLeaves(ref vl);

            visibleSearchResults = vl.Select(BrowserItemViewModel.Wrap).ToList();

            if (visibleSearchResults.Any())
            {
                SelectedIndex = 0;
                visibleSearchResults[0].Model.IsSelected = true;
            }

            SearchResults.Clear();

            foreach (var x in visibleSearchResults.OfType<NodeSearchElementViewModel>())
            {
                SearchResults.Add(x);
            }
        }

        private static string MakeShortCategoryString(string fullCategoryName)
        {
            var catName = fullCategoryName.Replace(".", " > ");

            if (catName.Length <= 50) 
                return catName;

            // if the category name is too long, we strip off the interior categories
            var s = catName.Split('>').Select(x => x.Trim()).ToList();
            if (s.Count() <= 4) 
                return catName;
            
            s = new List<string>
            {
                s[0],
                "...",
                s[s.Count - 3],
                s[s.Count - 2],
                s[s.Count - 1]
            };
            catName = string.Join(" > ", s);

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
        // TODO(Steve): This looks really inefficient
        public static string RemoveLastPartOfText(string text)
        {
            while (true)
            {
                if (string.IsNullOrEmpty(text))
                    return text;

                var matches = Regex.Matches(text, Regex.Escape("."));

                // no period
                if (matches.Count == 0)
                    return "";

                if (matches[matches.Count - 1].Index + 1 != text.Length)
                    return text.Substring(0, matches[matches.Count - 1].Index + 2);
                
                // if period is in last position, remove that period and recurse
                text = text.Substring(0, text.Length - 1);
            }
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

            SearchText = SearchResults[SelectedIndex].Model.Name;
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

            if (!(visibleSearchResults[SelectedIndex].Model is SearchElementBase)) return;

            ExecuteElement(visibleSearchResults[SelectedIndex].Model as SearchElementBase);
        }

        private void ExecuteElement(BrowserItem searchElement)
        {
            dynamic ele = searchElement;
            ExecuteElementDynamically(ele);
        }

        private void ExecuteElementDynamically(CategorySearchElement searchElement)
        {
            SearchText = searchElement.Name + ".";
        }

        private void ExecuteElementDynamically(DSFunctionNodeSearchElement element)
        {
            // create node
            var guid = Guid.NewGuid();
            dynamoViewModel.ExecuteCommand(
                new DynamoModel.CreateNodeCommand(guid, element.FunctionDescriptor.MangledName, 0, 0, true, true));

            // select node
            var placedNode = dynamoViewModel.Model.CurrentWorkspace.Nodes.FirstOrDefault(node => node.GUID == guid);
            if (placedNode != null)
            {
                DynamoSelection.Instance.ClearSelection();
                DynamoSelection.Instance.Selection.Add(placedNode);
            }
        }

        private void ExecuteElementDynamically(CustomNodeSearchElement element)
        {
            string name = element.Guid.ToString();

            // create node
            var guid = Guid.NewGuid();
            dynamoViewModel.ExecuteCommand(
                new DynamoModel.CreateNodeCommand(guid, name, 0, 0, true, true));

            // select node
            var placedNode = dynamoViewModel.Model.CurrentWorkspace.Nodes.FirstOrDefault(node => node.GUID == guid);
            if (placedNode != null)
            {
                DynamoSelection.Instance.ClearSelection();
                DynamoSelection.Instance.Selection.Add(placedNode);
            }
        }

        private void ExecuteElementDynamically(NodeModelSearchElement element)
        {
            // create node
            var guid = Guid.NewGuid();
            dynamoViewModel.ExecuteCommand(
                new DynamoModel.CreateNodeCommand(guid, element.FullName, 0, 0, true, true));

            // select node
            var placedNode = dynamoViewModel.Model.CurrentWorkspace.Nodes.FirstOrDefault(node => node.GUID == guid);
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
            SearchAndUpdateResults();
        }

        internal bool CanSearch(object parameter)
        {
            return true;
        }

        internal void HideSearch(object parameter)
        {
            Visible = false;
        }

        internal bool CanHideSearch(object parameter)
        {
            return Visible;
        }

        public void ShowSearch(object parameter)
        {
            Visible = true;
        }

        internal bool CanShowSearch(object parameter)
        {
            return !Visible;
        }

        public void FocusSearch(object parameter)
        {
            OnRequestFocusSearch(dynamoViewModel, EventArgs.Empty);
        }

        internal bool CanFocusSearch(object parameter)
        {
            return true;
        }

        #endregion
    }
}
