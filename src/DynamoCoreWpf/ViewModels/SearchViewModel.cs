using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using Dynamo.Models;
using Dynamo.Search;
using Dynamo.Search.SearchElements;
using Dynamo.Selection;
using Dynamo.UI;
using Dynamo.Wpf.ViewModels;
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

        public event EventHandler SearchTextChanged;
        public void OnSearchTextChanged(object sender, EventArgs e)
        {
            if (SearchTextChanged != null)
                SearchTextChanged(this, e);
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
                OnSearchTextChanged(this, EventArgs.Empty);
                RaisePropertyChanged("SearchText");
                RaisePropertyChanged("CurrentMode");
            }
        }

        private SearchMemberGroup topResult;
        public SearchMemberGroup TopResult
        {
            get { return topResult; }
            set
            {
                topResult = value;
                RaisePropertyChanged("TopResult");
            }
        }

        /// <summary>
        ///     SearchIconAlignment property
        /// </summary>
        /// <value>
        ///     This is used for aligment search icon and text.
        /// </value>
        private System.Windows.HorizontalAlignment searchIconAlignment;
        public System.Windows.HorizontalAlignment SearchIconAlignment
        {
            get { return searchIconAlignment; }
            set
            {
                searchIconAlignment = value;
                RaisePropertyChanged("SearchIconAlignment");
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

        public bool SearchAddonsVisibility
        {
            get
            {
                return Model.AddonRootCategories.Any(cat => cat.Visibility);
            }
        }

        public enum ViewMode { LibraryView, LibrarySearchView };

        /// <summary>
        /// The property specifies which View is active now.
        /// </summary>
        public ViewMode CurrentMode
        {
            get
            {
                return string.IsNullOrEmpty(SearchText) ? ViewMode.LibraryView :
                    ViewMode.LibrarySearchView;
            }
        }

        /// <summary>
        ///     SearchResults property
        /// </summary>
        /// <value>
        ///     This property is observed by SearchView to see the search results
        /// </value>
        public ObservableCollection<SearchElementBaseViewModel> SearchResults { get; private set; }

        private bool searchScrollBarVisibility = true;
        public bool SearchScrollBarVisibility
        {
            get { return searchScrollBarVisibility; }
            set { searchScrollBarVisibility = value; RaisePropertyChanged("SearchScrollBarVisibility"); }
        }

        public Typeface RegularTypeface { get; private set; }

        public ObservableCollection<BrowserRootElementViewModel> BrowserRootCategories { get;
            private set; }

        public ObservableCollection<SearchCategory> SearchRootCategories
        {
            get { return Model.SearchRootCategories; }
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
            SearchResults = new ObservableCollection<SearchElementBaseViewModel>();
            BrowserRootCategories = new ObservableCollection<BrowserRootElementViewModel>();

            Visible = false;
            searchText = "";
            searchIconAlignment = System.Windows.HorizontalAlignment.Left;

            var fontFamily = new FontFamily(SharedDictionaryManager.DynamoModernDictionaryUri, "../../Fonts/#Open Sans");
            RegularTypeface = new Typeface(fontFamily, FontStyles.Normal, FontWeights.Normal,
                FontStretches.Normal);

            this.Model.RequestSync += ModelOnRequestSync;
            this.Model.Executed += ExecuteElement;

            this.Model.RemoveEmptyCategories();

            foreach (BrowserRootElement item in this.Model.BrowserRootCategories)
            {
                BrowserRootCategories.Add(BrowserItemViewModel.WrapExplicit(item));
            }

            this.Model.BrowserRootCategoriesCollectionChanged += BrowserRootCategoriesOnCollectionChanged;

            this.SortCategoryChildren();
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
                    foreach (var item in e.OldItems)
                    {
                        var vm = BrowserRootCategories.First(x => x.Model == item);
                        BrowserRootCategories.Remove(vm);
                    }
                    break;
            }
        }

        #endregion

        #region Search

        internal void SortCategoryChildren()
        {
            foreach (var item in this.BrowserRootCategories)
            {
                item.RecursivelySort();
            }
        }

        private void ModelOnRequestSync(object sender, EventArgs eventArgs)
        {
            this.SearchAndUpdateResults();
        }

        /// <summary>
        ///     Performs a search using the internal SearcText as the query.
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

            var foundNodes = this.Model.Search(query);
            this.UpdateTopResult();

            //sw.Stop();

            //this.dynamoViewModel.Model.Logger.Log(String.Format("Search complete in {0}", sw.Elapsed));

            RaisePropertyChanged("SearchAddonsVisibility");
            RaisePropertyChanged("SearchRootCategories");

            // SearchResults doesn't used everywhere.
            // It is populated for making connected tests as successful.
            SearchResults = new ObservableCollection<SearchElementBaseViewModel>(foundNodes.Select(node => new NodeSearchElementViewModel(node as NodeSearchElement)));
        }

        private void UpdateTopResult()
        {
            if (!Model.SearchRootCategories.Any())
            {
                TopResult = null;
                return;
            }

            // If SearchRootCategories has at least 1 element, it has at least 1 member. 
            var firstMemberGroup = Model.SearchRootCategories.First().
                MemberGroups.First();

            var topMemberGroup = new SearchMemberGroup(firstMemberGroup.FullyQualifiedName);
            topMemberGroup.AddMember(firstMemberGroup.Members.First());

            TopResult = topMemberGroup;
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
            // TODO (Vladimir): implement it for new navigation system

            //if (SearchResults.Count == 0) return;

            //// none of the elems are selected, return 
            //if (SelectedIndex == -1)
            //    return;

            //SearchText = SearchResults[SelectedIndex].Model.Name;
        }

        #endregion

        #region Execution

        /// <summary>
        ///     Runs the Execute() method of the current selected SearchElementBase object.
        /// </summary>
        public void Execute()
        {
            // TODO (Vladimir): implement it for new navigation system

            //// none of the elems are selected, return 
            //if (SelectedIndex == -1)
            //    return;

            //if (visibleSearchResults.Count <= SelectedIndex)
            //    return;

            //if (!(visibleSearchResults[SelectedIndex].Model is SearchElementBase)) return;

            //ExecuteElement(visibleSearchResults[SelectedIndex].Model as SearchElementBase);
        }

        private void ExecuteElement(BrowserItem searchElement)
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
