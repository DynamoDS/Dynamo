using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
<<<<<<< HEAD:src/DynamoCoreWpf/ViewModels/Search/SearchViewModel.cs

using Dynamo.Search;
using Dynamo.Search.SearchElements;
using Dynamo.Utilities;
=======
using System.Windows;
using System.Windows.Media;
using Dynamo.Models;
using Dynamo.Search;
using Dynamo.Search.SearchElements;
using Dynamo.Selection;
using Dynamo.UI;
>>>>>>> Sitrus2:src/DynamoCoreWpf/ViewModels/SearchViewModel.cs
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
        ///     Maximum number of items to show in search.
        /// </summary>
        public int MaxNumSearchResults { get; set; }

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
<<<<<<< HEAD:src/DynamoCoreWpf/ViewModels/Search/SearchViewModel.cs
                RaisePropertyChanged("BrowserRootCategories");
=======
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
>>>>>>> Sitrus2:src/DynamoCoreWpf/ViewModels/SearchViewModel.cs
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
<<<<<<< HEAD:src/DynamoCoreWpf/ViewModels/Search/SearchViewModel.cs
                if (selectedIndex != value)
                {
                    if (visibleSearchResults.Count > selectedIndex)
                        visibleSearchResults[selectedIndex].IsSelected = false;
                    selectedIndex = value;
                    if (visibleSearchResults.Count > selectedIndex)
                        visibleSearchResults[selectedIndex].IsSelected = true;
                    RaisePropertyChanged("SelectedIndex");
                }
=======
                searchIconAlignment = value;
                RaisePropertyChanged("SearchIconAlignment");
>>>>>>> Sitrus2:src/DynamoCoreWpf/ViewModels/SearchViewModel.cs
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
        public ObservableCollection<NodeSearchElementViewModel> SearchResults { get; private set; }

<<<<<<< HEAD:src/DynamoCoreWpf/ViewModels/Search/SearchViewModel.cs
        /// <summary>
        /// A category representing the "Top Result"
        /// </summary>
        //private RootNodeCategoryViewModel topResult;

        /// <summary>
        ///     An ordered list representing all of the visible items in the browser.
        ///     This is used to manage up-down navigation through the menu.
        /// </summary>
        private readonly List<NodeSearchElementViewModel> visibleSearchResults =
            new List<NodeSearchElementViewModel>();

=======
>>>>>>> Sitrus2:src/DynamoCoreWpf/ViewModels/SearchViewModel.cs
        private bool searchScrollBarVisibility = true;
        public bool SearchScrollBarVisibility
        {
            get { return searchScrollBarVisibility; }
            set { searchScrollBarVisibility = value; RaisePropertyChanged("SearchScrollBarVisibility"); }
        }
<<<<<<< HEAD:src/DynamoCoreWpf/ViewModels/Search/SearchViewModel.cs
=======

        public Typeface RegularTypeface { get; private set; }

        public ObservableCollection<BrowserRootElementViewModel> BrowserRootCategories { get;
            private set; }

        public ObservableCollection<SearchCategory> SearchRootCategories
        {
            get { return Model.SearchRootCategories; }
        }

        public SearchModel Model { get; private set; }
        private readonly DynamoViewModel dynamoViewModel;
>>>>>>> Sitrus2:src/DynamoCoreWpf/ViewModels/SearchViewModel.cs

        public ObservableCollection<NodeCategoryViewModel> SearchRootCategories { get; private set; }

        public ObservableCollection<NodeCategoryViewModel> LibraryRootCategories
        {
            get { return libraryRoot.SubCategories; }
        }

        private readonly NodeCategoryViewModel libraryRoot = new NodeCategoryViewModel("");

        public ObservableCollection<NodeCategoryViewModel> BrowserRootCategories
        {
            get { return string.IsNullOrWhiteSpace(SearchText) ? LibraryRootCategories : SearchRootCategories; }
        }

        public NodeSearchModel Model { get; private set; }
        private readonly DynamoViewModel dynamoViewModel;
        #endregion

        #region Initialization

        internal SearchViewModel(DynamoViewModel dynamoViewModel, NodeSearchModel model)
        {
            Model = model;
            this.dynamoViewModel = dynamoViewModel;

            MaxNumSearchResults = 15;

            InitializeCore();
        }

        private void InitializeCore()
        {
<<<<<<< HEAD:src/DynamoCoreWpf/ViewModels/Search/SearchViewModel.cs
            SelectedIndex = 0;
            SearchResults = new ObservableCollection<NodeSearchElementViewModel>();
            SearchRootCategories = new ObservableCollection<NodeCategoryViewModel>();
=======
            SearchResults = new ObservableCollection<SearchElementBaseViewModel>();
            BrowserRootCategories = new ObservableCollection<BrowserRootElementViewModel>();

>>>>>>> Sitrus2:src/DynamoCoreWpf/ViewModels/SearchViewModel.cs
            Visible = false;
            searchText = "";
            searchIconAlignment = System.Windows.HorizontalAlignment.Left;

            var fontFamily = new FontFamily(SharedDictionaryManager.DynamoModernDictionaryUri, "../../Fonts/#Open Sans");
            RegularTypeface = new Typeface(fontFamily, FontStyles.Normal, FontWeights.Normal,
                FontStretches.Normal);

<<<<<<< HEAD:src/DynamoCoreWpf/ViewModels/Search/SearchViewModel.cs
            // When Library changes, sync up
            Model.EntryAdded += entry =>
            {
                InsertEntry(MakeNodeSearchElementVM(entry), entry.Categories);
            };
            Model.EntryRemoved += RemoveEntry;
            
            libraryRoot.PropertyChanged += LibraryRootOnPropertyChanged;
            LibraryRootCategories.AddRange(CategorizeEntries(Model.SearchEntries, false));
        }
=======
            this.Model.RequestSync += ModelOnRequestSync;
            this.Model.Executed += ExecuteElement;
>>>>>>> Sitrus2:src/DynamoCoreWpf/ViewModels/SearchViewModel.cs

        private void LibraryRootOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "Visibility")
                SearchAndUpdateResults();
        }

        private static IEnumerable<RootNodeCategoryViewModel> CategorizeEntries(IEnumerable<NodeSearchElement> entries, bool expanded)
        {
            var tempRoot = 
                entries.GroupByRecursive<NodeSearchElement, string, NodeCategoryViewModel>(
                    element => element.Categories,
                    (name, subs, es) =>
                        new NodeCategoryViewModel(name, es.Select(MakeNodeSearchElementVM), subs)
                        {
                            IsExpanded = expanded
                        },
                    "");
            var result =
                tempRoot.SubCategories.Select(
                    cat =>
                        new RootNodeCategoryViewModel(cat.Name, cat.Entries, cat.SubCategories)
                        {
                            IsExpanded = expanded
                        });
            tempRoot.Dispose();
            return result;
        }

        private void RemoveEntry(NodeSearchElement entry)
        {
            var treeStack = new Stack<NodeCategoryViewModel>();
            var nameStack = new Stack<string>(entry.Categories);
            var target = libraryRoot;
            while (nameStack.Any())
            {
                var next = nameStack.Pop();
                var categories = target.SubCategories;
                var newTarget = categories.FirstOrDefault(c => c.Name == next);
                if (newTarget == default(NodeCategoryViewModel))
                {
                    return;
                }
                treeStack.Push(target);
                target = newTarget;
            }
            var location = target.Entries.Select((e, i) => new { e.Model, i })
                .FirstOrDefault(x => entry == x.Model);
            if (location == null)
                return;
            target.Entries.RemoveAt(location.i);

<<<<<<< HEAD:src/DynamoCoreWpf/ViewModels/Search/SearchViewModel.cs
            while (!target.Items.Any() && treeStack.Any())
            {
                var parent = treeStack.Pop();
                parent.SubCategories.Remove(target);
                target = parent;
            }
=======
            this.Model.BrowserRootCategoriesCollectionChanged += BrowserRootCategoriesOnCollectionChanged;

            this.SortCategoryChildren();
>>>>>>> Sitrus2:src/DynamoCoreWpf/ViewModels/SearchViewModel.cs
        }

        private void InsertEntry(NodeSearchElementViewModel entry, IEnumerable<string> categoryNames)
        {
            var nameStack = new Stack<string>(categoryNames.Reverse());
            var target = libraryRoot;
            bool isRoot = true;
            while (nameStack.Any())
            {
                var next = nameStack.Pop();
                var categories = target.SubCategories;
                var newTarget = categories.FirstOrDefault(c => c.Name == next);
                if (newTarget == default(NodeCategoryViewModel))
                {
                    newTarget = isRoot ? new RootNodeCategoryViewModel(next) : new NodeCategoryViewModel(next);
                    target.SubCategories.Add(newTarget);
                    PlaceInNewCategory(entry, newTarget, nameStack);
                    return;
                }
                target = newTarget;
                isRoot = false;
            }
            target.Entries.Add(entry);
        }

        private static void PlaceInNewCategory(
            NodeSearchElementViewModel entry, NodeCategoryViewModel target,
            IEnumerable<string> categoryNames)
        {
            var newTargets =
                categoryNames.Select(name => new NodeCategoryViewModel(name));

            foreach (var newTarget in newTargets)
            {
                target.SubCategories.Add(newTarget);
                target = newTarget;
            }

            target.Entries.Add(entry);
        }

        #endregion

        #region Search

        /// <summary>
        ///     Performs a search using the internal SearcText as the query.
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

<<<<<<< HEAD:src/DynamoCoreWpf/ViewModels/Search/SearchViewModel.cs
            // deselect the last selected item
            if (visibleSearchResults.Count > SelectedIndex)
                visibleSearchResults[SelectedIndex].IsSelected = false;

            // if the search query is empty, go back to the default treeview
            if (string.IsNullOrEmpty(query))
                return;

            // clear visible results list
            visibleSearchResults.Clear();

            foreach (var category in SearchRootCategories)
                category.DisposeTree();

            SearchRootCategories.Clear();

            if (string.IsNullOrEmpty(query))
                return;

            var result =
                Model.Search(query).Where(r => r.IsVisibleInSearch).Take(MaxNumSearchResults).ToList();

            // Add top result
            var firstRes = result.FirstOrDefault();
            if (firstRes == null)
                return; //No results

            var topResultCategory = new RootNodeCategoryViewModel("Top Result");
            SearchRootCategories.Add(topResultCategory);

            var copy = MakeNodeSearchElementVM(firstRes);
            var catName = MakeShortCategoryString(firstRes.FullCategoryName);

            var breadCrumb = new NodeCategoryViewModel(catName) { IsExpanded = true };
            breadCrumb.Entries.Add(copy);
            topResultCategory.SubCategories.Add(breadCrumb);
            topResultCategory.Visibility = true;
            topResultCategory.IsExpanded = true;

            SearchRootCategories.AddRange(CategorizeEntries(result, true));

            visibleSearchResults.AddRange(SearchRootCategories.SelectMany(GetVisibleSearchResults));

            if (visibleSearchResults.Any())
            {
                SelectedIndex = 0;
                visibleSearchResults[0].IsSelected = true;
            }

            SearchResults.Clear();
            foreach (var x in visibleSearchResults)
                SearchResults.Add(x);
        }

        private static IEnumerable<NodeSearchElementViewModel> GetVisibleSearchResults(NodeCategoryViewModel category)
        {
            foreach (var item in category.Items)
            {
                var sub = item as NodeCategoryViewModel;
                if (sub != null)
                {
                    foreach (var visible in GetVisibleSearchResults(sub))
                        yield return visible;
                }
                else
                    yield return (NodeSearchElementViewModel)item;
            }
        }

        private static NodeSearchElementViewModel MakeNodeSearchElementVM(NodeSearchElement entry)
        {
            var element = entry as CustomNodeSearchElement;
            return element != null
                ? new CustomNodeSearchElementViewModel(element)
                : new NodeSearchElementViewModel(entry);
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
=======
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
>>>>>>> Sitrus2:src/DynamoCoreWpf/ViewModels/SearchViewModel.cs
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

<<<<<<< HEAD:src/DynamoCoreWpf/ViewModels/Search/SearchViewModel.cs
            visibleSearchResults[SelectedIndex].Model.ProduceNode();
=======
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
>>>>>>> Sitrus2:src/DynamoCoreWpf/ViewModels/SearchViewModel.cs
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
