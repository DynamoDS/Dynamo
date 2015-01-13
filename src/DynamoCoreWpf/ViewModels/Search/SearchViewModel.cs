using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using Dynamo.Nodes;
using Dynamo.Search;
using Dynamo.Search.SearchElements;
using Dynamo.UI;
using Dynamo.Utilities;
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
                RaisePropertyChanged("BrowserRootCategories");
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
        private HorizontalAlignment searchIconAlignment;
        public HorizontalAlignment SearchIconAlignment
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
                // TODO(Vladimir): uncomment when Addons are shown.
                return false;//AddonRootCategories.Any(cat => cat.Visibility);
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

        private bool searchScrollBarVisibility = true;
        public bool SearchScrollBarVisibility
        {
            get { return searchScrollBarVisibility; }
            set { searchScrollBarVisibility = value; RaisePropertyChanged("SearchScrollBarVisibility"); }
        }

        public Typeface RegularTypeface { get; private set; }

        private ObservableCollection<SearchCategory> searchRootCategories;
        public ObservableCollection<SearchCategory> SearchRootCategories
        {
            get { return searchRootCategories; }
        }

        public ObservableCollection<NodeCategoryViewModel> LibraryRootCategories
        {
            get { return libraryRoot.SubCategories; }
        }

        private readonly NodeCategoryViewModel libraryRoot = new NodeCategoryViewModel("");

        public ObservableCollection<NodeCategoryViewModel> BrowserRootCategories
        {
            get { return LibraryRootCategories; }
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
            SearchResults = new ObservableCollection<NodeSearchElementViewModel>();
            searchRootCategories = new ObservableCollection<SearchCategory>();

            Visible = false;
            searchText = "";
            searchIconAlignment = System.Windows.HorizontalAlignment.Left;

            var fontFamily = new FontFamily(SharedDictionaryManager.DynamoModernDictionaryUri, "../../Fonts/#Open Sans");
            RegularTypeface = new Typeface(fontFamily, FontStyles.Normal, FontWeights.Normal,
                FontStretches.Normal);

            // When Library changes, sync up
            Model.EntryAdded += entry =>
            {
                InsertEntry(MakeNodeSearchElementVM(entry), entry.Categories);
            };
            Model.EntryRemoved += RemoveEntry;

            libraryRoot.PropertyChanged += LibraryRootOnPropertyChanged;
            LibraryRootCategories.AddRange(CategorizeEntries(Model.SearchEntries, false));

            InsertClassesIntoTree(LibraryRootCategories);
            DefineCategoriesFullCategoryName(LibraryRootCategories, "");

            ChangeCategoryExpandState(BuiltinNodeCategories.GEOMETRY, true);
        }

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
            return result.OrderBy(cat => cat.Name);
        }

        private void InsertClassesIntoTree(ObservableCollection<NodeCategoryViewModel> tree)
        {
            foreach (var item in tree)
            {
                var classes = item.SubCategories.Where(cat => cat.SubCategories.Count == 0).ToList();

                foreach (var item2 in classes)
                    item.SubCategories.Remove(item2);

                InsertClassesIntoTree(item.SubCategories);

                var container = new ClassesNodeCategoryViewModel();
                container.SubCategories.AddRange(classes);

                item.SubCategories.Insert(0, container);
            }
        }

        private void DefineCategoriesFullCategoryName(ObservableCollection<NodeCategoryViewModel> tree, string path)
        {
            foreach (var item in tree)
            {
                item.FullCategoryName = AddToFullPath(path, item.Name);

                DefineCategoriesFullCategoryName(item.SubCategories, item.FullCategoryName);
            }
        }

        private void ChangeCategoryExpandState(string categoryName, bool isExpanded)
        {
            var category = LibraryRootCategories.FirstOrDefault(cat => cat.Name == categoryName);
            if (category != null && category.IsExpanded != isExpanded)
                category.IsExpanded = isExpanded;
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

            while (!target.Items.Any() && treeStack.Any())
            {
                var parent = treeStack.Pop();
                parent.SubCategories.Remove(target);
                target = parent;
            }
        }

        private void InsertEntry(NodeSearchElementViewModel entry, IEnumerable<string> categoryNames)
        {
            var nameStack = new Stack<string>(categoryNames.Reverse());
            var target = libraryRoot;
            bool isRoot = true;
            string path = "";
            while (nameStack.Any())
            {
                var next = nameStack.Pop();
                path = AddToFullPath(path, next);

                var categories = target.SubCategories;
                NodeCategoryViewModel targetClassSuccessor = null;
                var newTarget = categories.FirstOrDefault(c =>
                {
                    if (c is ClassesNodeCategoryViewModel)
                    {
                        targetClassSuccessor = c.SubCategories.FirstOrDefault(c2 => c2.Name == next);
                        return targetClassSuccessor != null;
                    }

                    return c.Name == next;
                });
                if (newTarget == default(NodeCategoryViewModel))
                {
                    newTarget = isRoot ? new RootNodeCategoryViewModel(next) : new NodeCategoryViewModel(next);
                    newTarget.FullCategoryName = path;
                    if (nameStack.Count == 0 && target.SubCategories[0] is ClassesNodeCategoryViewModel)
                    {
                        target.SubCategories[0].SubCategories.Add(newTarget);
                        newTarget.Entries.Add(entry);
                        return;
                    }
                    target.SubCategories.Add(newTarget);
                    PlaceInNewCategory(entry, newTarget, nameStack);
                    return;
                }
                if (targetClassSuccessor != null)
                    target = targetClassSuccessor;
                else
                    target = newTarget;

                isRoot = false;
            }
            target.Entries.Add(entry);
        }

        private static void PlaceInNewCategory(
            NodeSearchElementViewModel entry, NodeCategoryViewModel target,
            IEnumerable<string> categoryNames)
        {
            var path = target.FullCategoryName;
            var newTargets = categoryNames.Select(name =>
            {
                path = AddToFullPath(path, name);

                var cat = new NodeCategoryViewModel(name);
                cat.FullCategoryName = path;
                return cat;
            }).ToList();

            int indexToInsertClass = newTargets.Count - 1 > 0 ? newTargets.Count - 1 : 0;
            newTargets.Insert(indexToInsertClass, new ClassesNodeCategoryViewModel());
            newTargets[indexToInsertClass].FullCategoryName = Configurations.ClassesDefaultName;

            foreach (var newTarget in newTargets)
            {
                target.SubCategories.Add(newTarget);
                target = newTarget;
            }

            target.Entries.Add(entry);
        }

        private static string AddToFullPath(string path, string addition)
        {
            return string.IsNullOrEmpty(path) ? addition :
                path + Configurations.CategoryDelimiter + addition;
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


            // if the search query is empty, go back to the default treeview
            if (string.IsNullOrEmpty(query))
                return;

            var foundNodes = Search(query);

            UpdateTopResult();
            RaisePropertyChanged("SearchRootCategories");
            //SearchRootCategories.Clear();

            //if (string.IsNullOrEmpty(query))
            //    return;

            //var result =
            //    Model.Search(query).Where(r => r.IsVisibleInSearch).Take(MaxNumSearchResults).ToList();

            //// Add top result
            //var firstRes = result.FirstOrDefault();
            //if (firstRes == null)
            //    return; //No results

            // TODO(Vladimir): master implementation
#if false
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
#endif
            // TODO(Vladimir): Sitrus implementation.
#if false
            //var sw = new Stopwatch();

            //sw.Start();

            var foundNodes = this.Model.Search(query);
            
            //sw.Stop();

            //this.dynamoViewModel.Model.Logger.Log(String.Format("Search complete in {0}", sw.Elapsed));

            RaisePropertyChanged("SearchAddonsVisibility");
            RaisePropertyChanged("SearchRootCategories");

            // SearchResults doesn't used everywhere.
            // It is populated for making connected tests as successful.
            SearchResults = new ObservableCollection<SearchElementBaseViewModel>(foundNodes.Select(node => new NodeSearchElementViewModel(node as NodeSearchElement)));
#endif
        }


        /// <summary>
        ///     Performs a search using the given string as query.
        /// </summary>
        /// <returns> Returns a list with a maximum MaxNumSearchResults elements.</returns>
        /// <param name="search"> The search query </param>
        internal IEnumerable<NodeSearchElementViewModel> Search(string search)
        {
            if (string.IsNullOrEmpty(search))
            {
                return SearchResults;
            }

            return Search(search, MaxNumSearchResults);
        }

        private IEnumerable<NodeSearchElementViewModel> Search(string search, int maxNumSearchResults)
        {
            var foundNodes = Model.Search(search).Take(15);

            ClearSearchCategories();
            PopulateSearchCategories(foundNodes);

            return foundNodes.Select(MakeNodeSearchElementVM);
        }

        private void PopulateSearchCategories(IEnumerable<NodeSearchElement> nodes)
        {
            foreach (NodeSearchElement node in nodes)
            {
                var rootCategoryName = NodeSearchElement.SplitCategoryName(node.FullCategoryName).FirstOrDefault();

                var category = searchRootCategories.FirstOrDefault(sc => sc.Name == rootCategoryName);
                if (category == null)
                {
                    category = new SearchCategory(rootCategoryName);
                    searchRootCategories.Add(category);
                }

                category.AddMemberToGroup(node);
            }

            // Order found categories by name.
            searchRootCategories = new ObservableCollection<SearchCategory>(searchRootCategories.OrderBy(x => x.Name));

            SortSearchCategoriesChildren();
        }

        private void SortSearchCategoriesChildren()
        {
            searchRootCategories.ToList().ForEach(x => x.SortChildren());
        }

        private void ClearSearchCategories()
        {
            searchRootCategories.Clear();
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

        }

        private void UpdateTopResult()
        {
            if (!SearchRootCategories.Any())
            {
                TopResult = null;

                return;
            }

            // If SearchRootCategories has at least 1 element, it has at least 1 member. 
            var firstMemberGroup = SearchRootCategories.First().MemberGroups.First();

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

            //TODO(Vladimir): master line
            // visibleSearchResults[SelectedIndex].Model.ProduceNode();

            //if (!(visibleSearchResults[SelectedIndex].Model is SearchElementBase)) return;

            //ExecuteElement(visibleSearchResults[SelectedIndex].Model as SearchElementBase);
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
