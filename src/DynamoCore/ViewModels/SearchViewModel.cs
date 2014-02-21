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
using Dynamo.PackageManager;
using Dynamo.Search.SearchElements;
using Dynamo.Utilities;
using Greg.Responses;
using Microsoft.Practices.Prism.ViewModel;
using Dynamo.DSEngine;

namespace Dynamo.ViewModels
{
    /// <summary>
    ///     This is the core ViewModel for searching
    /// </summary>
    public partial class SearchViewModel : NotificationObject
    {
        #region Properties

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
        ///     Indicates whether the node browser is visible or not
        /// </summary>
        //private Visibility _searchVisibility = Visibility.Collapsed;
        //public Visibility SearchVisibility
        //{
        //    get { return _searchVisibility; }
        //    set { _searchVisibility = value; RaisePropertyChanged("SearchVisibility"); }
        //}

        /// <summary>
        ///     IncludeRevitAPIElements property
        /// </summary>
        /// <value>
        ///     Specifies whether we are including Revit API elements in search.
        /// </value>
        private bool includeRevitApiElements;
        public bool IncludeRevitAPIElements
        {
            get { return includeRevitApiElements; }
            set
            {
                includeRevitApiElements = value;
                RaisePropertyChanged("IncludeRevitAPIElements");
                //ToggleIncludingRevitAPIElements();
            }
        }

        /// <summary>
        /// Leaves of the browser - used for navigation
        /// </summary>
        private List<SearchElementBase> _searchElements = new List<SearchElementBase>();
        public List<SearchElementBase> SearchElements { get { return _searchElements; } }

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
                //DynamoCommands.SearchCommand.Execute();
                SearchAndUpdateResults();
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
                    if (_visibleSearchResults.Count > selectedIndex)
                        _visibleSearchResults[selectedIndex].IsSelected = false;
                    selectedIndex = value;
                    if (_visibleSearchResults.Count > selectedIndex)
                        _visibleSearchResults[selectedIndex].IsSelected = true;
                    RaisePropertyChanged("SelectedIndex");
                }
            }
        }

        /// <summary>
        ///     Categories property
        /// </summary>
        /// <value>
        ///     A set of categories
        /// </value>
        public IEnumerable<string> Categories
        {
            get { return NodeCategories.Keys; }
        }

        /// <summary>
        ///     NodeCategories property
        /// </summary>
        /// <value>
        ///     A set of categories
        /// </value>
        private Dictionary<string, CategorySearchElement> NodeCategories { get; set; }

        /// <summary>
        ///     RevitApiSearchElements property
        /// </summary>
        /// <value>
        ///     A collection of elements corresponding to the auto-generated Revit
        ///     API elements
        /// </value>
        public List<SearchElementBase> RevitApiSearchElements { get; private set; }

        /// <summary>
        ///     Visible property
        /// </summary>
        /// <value>
        ///     Tells whether the View is visible or not
        /// </value>
        private bool _visible;
        public bool Visible
        {
            get { return _visible; }
            set
            {
                _visible = value;
                RaisePropertyChanged("Visible");
            }
        }

        /// <summary>
        ///     SearchDictionary property
        /// </summary>
        /// <value>
        ///     This is the dictionary used to search
        /// </value>
        public SearchDictionary<SearchElementBase> SearchDictionary { get; private set; }

        /// <summary>
        ///     SearchResults property
        /// </summary>
        /// <value>
        ///     This property is observed by SearchView to see the search results
        /// </value>
        public ObservableCollection<SearchElementBase> SearchResults { get; private set; }

        /// <summary>
        ///     MaxNumSearchResults property
        /// </summary>
        /// <value>
        ///     Internal limit on the number of search results returned by SearchDictionary
        /// </value>
        public int MaxNumSearchResults { get; set; }

        /// <summary>
        /// The root elements for the browser
        /// </summary>
        private ObservableCollection<BrowserRootElement> _browserRootCategories = new ObservableCollection<BrowserRootElement>();
        public ObservableCollection<BrowserRootElement> BrowserRootCategories { get { return _browserRootCategories; } set { _browserRootCategories = value; } }

        /// <summary>
        /// A category represent the "Top Result"
        /// </summary>
        private BrowserRootElement _topResult;

        /// <summary>
        ///     An ordered list representing all of the visible items in the browser.
        ///     This is used to manage up-down navigation through the menu.
        /// </summary>
        private List<BrowserItem> _visibleSearchResults = new List<BrowserItem>();

        private bool searchScrollBarVisibility = true;
        public bool SearchScrollBarVisibility
        {
            get { return searchScrollBarVisibility; }
            set { searchScrollBarVisibility = value; RaisePropertyChanged("SearchScrollBarVisibility"); }
        }

        #endregion

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

        /// <summary>
        ///     The class constructor.
        /// </summary>
        /// <param name="bench"> Reference to dynBench object for logging </param>
        public SearchViewModel()
        {
            SelectedIndex = 0;
            RevitApiSearchElements = new List<SearchElementBase>();
            NodeCategories = new Dictionary<string, CategorySearchElement>();
            SearchDictionary = new SearchDictionary<SearchElementBase>();
            SearchResults = new ObservableCollection<SearchElementBase>();
            MaxNumSearchResults = 20;
            Visible = false;
            searchText = "";
            IncludeRevitAPIElements = true; // revit api

            _topResult = this.AddRootCategory("Top Result");
            this.AddRootCategory(BuiltinNodeCategories.CORE);
            this.AddRootCategory(BuiltinNodeCategories.LOGIC);
            this.AddRootCategory(BuiltinNodeCategories.GEOMETRY);
            this.AddRootCategory(BuiltinNodeCategories.REVIT);
            this.AddRootCategory(BuiltinNodeCategories.ANALYZE);
            this.AddRootCategory(BuiltinNodeCategories.IO);
            
        }

        private const char CATEGORY_DELIMITER = '.';

        public void RemoveEmptyCategories()
        {
            this.BrowserRootCategories = new ObservableCollection<BrowserRootElement>(BrowserRootCategories.Where(x => x.Items.Any() || x.Name == "Top Result"));
        }

        public void SortCategoryChildren()
        {
            dynSettings.Controller.SearchViewModel.BrowserRootCategories.ToList().ForEach(x => x.RecursivelySort());
        }

        public void RemoveEmptyRootCategory(string categoryName)
        {
            if (categoryName.Contains(CATEGORY_DELIMITER))
            {
                RemoveEmptyCategory(categoryName);
                return;
            }

            var cat = GetCategoryByName(categoryName);
            if (cat == null)
            {
                return;
            }

            RemoveEmptyRootCategory((BrowserRootElement) cat);
        }

        public void RemoveEmptyRootCategory(BrowserRootElement rootEle)
        {
            if (!ContainsCategory(rootEle.Name))
                return;
            
            BrowserRootCategories.Remove(rootEle);
        }

        /// <summary>
        /// Remove and empty category from browser and search by name. Useful when a single item is removed.
        /// </summary>
        /// <param name="categoryName">The category name, including delimiters</param>
        public void RemoveEmptyCategory( string categoryName )
        {
            var currentCat = GetCategoryByName(categoryName);
            if (currentCat == null)
            {
                return;
            }

            RemoveEmptyCategory(currentCat);
        }

        /// <summary>
        /// Remove an empty category from browser and search.  Useful when a single item is removed.
        /// </summary>
        /// <param name="ele"></param>
        public void RemoveEmptyCategory(BrowserItem ele)
        {
            if (ele is BrowserRootElement && ele.Items.Count == 0)
            {
                RemoveEmptyRootCategory(ele as BrowserRootElement);
                return;
            }

            if (ele is BrowserInternalElement && ele.Items.Count == 0)
            {
                var internalEle = ele as BrowserInternalElement;
                
                internalEle.Parent.Items.Remove(internalEle);
                RemoveEmptyCategory(internalEle.Parent);
            }
        }

        /// <summary>
        /// Remove a category and all its children from the browser and search.  The category does not
        /// have to be empty.
        /// </summary>
        /// <param name="categoryName"></param>
        public void RemoveCategory(string categoryName)
        {
            var currentCat = GetCategoryByName(categoryName);
            if (currentCat == null) return;

            RemoveCategory(currentCat);
            
        }

        /// <summary>
        /// Remove a category and all its children from the browser and search.  The category does
        /// not have to be empty.
        /// </summary>
        /// <param name="ele"></param>
        public void RemoveCategory(BrowserItem ele)
        {
            var nodes = ele.Items.Where(x => x is NodeSearchElement)
                           .Cast<NodeSearchElement>().ToList();

            var cats = ele.Items.Where(x => x is BrowserInternalElement)
                           .Cast<BrowserInternalElement>().ToList();

            nodes.Select(x => x.Name).ToList().ForEach(RemoveNode);
            cats.ToList().ForEach(RemoveCategory);

            ele.Items.Clear();

            if (ele is BrowserRootElement)
            {
                BrowserRootCategories.Remove(ele as BrowserRootElement);
            }
            else if (ele is BrowserInternalElement)
            {
                (ele as BrowserInternalElement).Parent.Items.Remove(ele);
            }
        }

        /// <summary>
        /// Split a category name into individual category names splitting be DEFAULT_DELIMITER
        /// </summary>
        /// <param name="categoryName">The name</param>
        /// <returns>A list of output</returns>
        public static List<string> SplitCategoryName(string categoryName)
        {
            if (System.String.IsNullOrEmpty(categoryName))
                return new List<string>();

            var splitCat = new List<string>();
            if (categoryName.Contains(CATEGORY_DELIMITER))
            {
                splitCat =
                    categoryName.Split(CATEGORY_DELIMITER)
                                .Where(x => x != CATEGORY_DELIMITER.ToString() && !System.String.IsNullOrEmpty(x))
                                .ToList();
            }
            else
            {
                splitCat.Add(categoryName);
            }

            return splitCat;
        } 

        /// <summary>
        ///     Add a category, given a delimited name
        /// </summary>
        /// <param name="categoryName">The comma delimited name </param>
        /// <returns>The newly created item</returns>
        public BrowserItem AddCategory(string categoryName)
        {
            if (string.IsNullOrEmpty(categoryName))
            {
                return this.TryAddRootCategory("Uncategorized");
            }

            if ( ContainsCategory(categoryName) )
            {
                return GetCategoryByName(categoryName);
            }

            if (!NodeCategories.ContainsKey(categoryName))
            {
                NodeCategories.Add(categoryName, new CategorySearchElement(categoryName));
            }

            // otherwise split the category name
            var splitCat = SplitCategoryName(categoryName);

            // attempt to add root element
            if (splitCat.Count == 1)
            {
                return this.TryAddRootCategory(categoryName);
            }

            if (splitCat.Count == 0)
            {
                return null;
            }

            // attempt to add root category
            var currentCat = TryAddRootCategory(splitCat[0]);    

            for (var i = 1; i < splitCat.Count; i++)
            {
                currentCat = TryAddChildCategory(currentCat, splitCat[i]);
            }

            return currentCat;

        }

        /// <summary>
        /// Add a single category as a child of a category.  If the category already exists, just return that one.
        /// </summary>
        /// <param name="parent">The parent category </param>
        /// <param name="childCategoryName">The name of the child category (can't be nested)</param>
        /// <returns>The newly created category</returns>
        public BrowserItem TryAddChildCategory(BrowserItem parent, string childCategoryName)
        {
            var newCategoryName = parent.Name + CATEGORY_DELIMITER + childCategoryName;

            // support long nested categories like Math.Math.StaticMembers.Abs
            var parentItem  = parent as BrowserInternalElement;
            while (parentItem != null)
            {
                var grandParent = parentItem.Parent;
                if (null == grandParent)
                    break;

                newCategoryName = grandParent.Name + CATEGORY_DELIMITER + newCategoryName;
                parentItem = grandParent as BrowserInternalElement;
            }

            if (ContainsCategory(newCategoryName))
            {
                return GetCategoryByName(newCategoryName);
            }

            var tempCat = new BrowserInternalElement(childCategoryName, parent);
            parent.AddChild(tempCat);

            return tempCat;
        }

        /// <summary>
        ///     
        /// </summary>
        /// <returns>The newly added category or the existing one.</returns>
        public BrowserItem TryAddRootCategory(string categoryName)
        {
            return ContainsCategory(categoryName) ? GetCategoryByName(categoryName) : AddRootCategory(categoryName);
        }

        /// <summary>
        /// Add a root category, assuming it doesn't already exist
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private BrowserRootElement AddRootCategory(string name)
        {
            var ele = new BrowserRootElement(name, BrowserRootCategories);
            BrowserRootCategories.Add(ele);
            return ele;
        }

        /// <summary>
        /// Determine whether a category exists in search
        /// </summary>
        /// <param name="categoryName"></param>
        /// <returns></returns>
        public bool ContainsCategory(string categoryName)
        {
            return GetCategoryByName(categoryName) != null;
        }

        public BrowserItem GetCategoryByName(string categoryName)
        {
            var split = SplitCategoryName(categoryName);
            if (!split.Any())
                return null;

            var cat = (BrowserItem) BrowserRootCategories.FirstOrDefault(x => x.Name == split[0]);

            foreach (var splitName in split.GetRange(1, split.Count - 1))
            {
                if (cat == null)
                    return cat;
                cat = TryGetSubCategory(cat, splitName);
            }
            return cat;
        }

        public BrowserItem TryGetSubCategory(BrowserItem category, string catName)
        {
            return category.Items.FirstOrDefault(x => x.Name == catName);
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
                    lock (SearchDictionary)
                    {
                        return Search(query);
                    }
                }).ContinueWith((t) =>
                    {

                        lock (_visibleSearchResults)
                        {

                            // deselect the last selected item
                            if (_visibleSearchResults.Count > SelectedIndex)
                            {
                                _visibleSearchResults[SelectedIndex].IsSelected = false;
                            }

                            // clear visible results list
                            _visibleSearchResults.Clear();

                            // if the search query is empty, go back to the default treeview
                            if (string.IsNullOrEmpty(query))
                            {

                                foreach (var ele in BrowserRootCategories)
                                {
                                    ele.CollapseToLeaves();
                                    ele.SetVisibilityToLeaves(true);
                                }

                                // hide the top result
                                _topResult.Visibility = false;

                                return;
                            }
                            
                            // otherwise, first collapse all
                            foreach (var root in BrowserRootCategories)
                            {
                                root.CollapseToLeaves();
                                root.SetVisibilityToLeaves(false);
                            }

                            //// if there are any results, add the top result 
                            if (t.Result.Any() && t.Result.ElementAt(0) is NodeSearchElement)
                            {
                                _topResult.Items.Clear();

                                var copy = (t.Result.ElementAt(0) as NodeSearchElement).Copy();

                                _topResult.AddChild(copy);

                                _topResult.SetVisibilityToLeaves(true);
                                _topResult.IsExpanded = true;
                            }

                            // for all of the other results, show them in their category
                            foreach (var ele in _searchElements)
                            {
                                if ( t.Result.Contains(ele) )
                                {
                                    ele.Visibility = true;
                                    ele.ExpandToRoot();
                                }
                            }

                            // create an ordered list of visible search results
                            var baseBrowserItem = new BrowserRootElement("root");
                            foreach (var root in BrowserRootCategories)
                            {
                                baseBrowserItem.Items.Add(root);
                            }

                            baseBrowserItem.GetVisibleLeaves(ref _visibleSearchResults);

                            if (_visibleSearchResults.Any())
                            {
                                this.SelectedIndex = 0;
                                _visibleSearchResults[0].IsSelected = true;
                            }

                            SearchResults.Clear();
                            _visibleSearchResults.ToList().ForEach(x => SearchResults.Add( (NodeSearchElement) x));
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
            var result = Search(query);

            SearchResults.Clear();
            foreach (var node in result)
            {
                SearchResults.Add(node);
            }
            SelectedIndex = 0;
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
        internal List<SearchElementBase> Search(string search)
        {
            if (string.IsNullOrEmpty(search) || search == "Search...")
            {
                return _searchElements;
            }

            return SearchDictionary.Search(search, MaxNumSearchResults);

        }

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

        /// <summary>
        ///     Runs the Execute() method of the current selected SearchElementBase object
        ///     amongst the SearchResults.
        /// </summary>
        public void ExecuteSelected()
        {

            // none of the elems are selected, return 
            if (SelectedIndex == -1)
                return;

            if (_visibleSearchResults.Count <= SelectedIndex)
                return;

            if (_visibleSearchResults[SelectedIndex] is SearchElementBase)
            {
                ( (SearchElementBase) _visibleSearchResults[SelectedIndex]).Execute();
            }

        }

        /// <summary>
        ///     Adds a PackageHeader, recently downloaded from the Package Manager, to Search
        /// </summary>
        /// <param name="packageHeader">A PackageHeader object</param>
        public void Add(PackageHeader packageHeader)
        {
            var searchEle = new PackageManagerSearchElement(packageHeader);
            SearchDictionary.Add(searchEle, searchEle.Name);
            if (packageHeader.keywords != null && packageHeader.keywords.Count > 0)
                SearchDictionary.Add(searchEle, packageHeader.keywords);
            SearchDictionary.Add(searchEle, searchEle.Description);
            //SearchAndUpdateResultsSync(SearchText);
        }

        /// <summary>
        ///     Attempt to add a new category to the browser and an item as one of its children
        /// </summary>
        /// <param name="category">The name of the category - a string possibly separated with one period </param>
        /// <param name="item">The item to add as a child of that category</param>
        public void TryAddCategoryAndItem( string category, BrowserInternalElement item )
        {

            var cat = this.AddCategory(category);
            cat.AddChild(item);

            item.FullCategoryName = category;

            var searchEleItem = item as SearchElementBase;
            if (searchEleItem != null)
                _searchElements.Add(searchEleItem);

        }

        /// <summary>
        ///     Adds DesignScript function groups
        /// </summary>
        /// <param name="func"></param>
        public void Add(IEnumerable<FunctionGroup> functionGroups)
        {
            if (null == functionGroups)
                return;

            foreach (var functionGroup in functionGroups)
            {
                var functions = functionGroup.Functions.ToList();
                if (!functions.Any())
                    continue;

                bool isOverloaded = functions.Count > 1;

                foreach (var function in functions)
                {
                    // For overloaded functions, only parameters are displayed
                    // for this item. E.g, for Count(), on UI it is:
                    //
                    // -> Abs
                    //      +----------------+
                    //      | dValue: double |
                    //      +----------------+
                    //      | nValue: int    |
                    //      +----------------+
                    var displayString = function.UserFriendlyName;
                    var category = function.Category;

                    if (isOverloaded)
                    {
                        displayString = string.Join(", ", function.Parameters.Select(p => p.ToString()));
                        if (string.IsNullOrEmpty(displayString))
                            displayString = "void";
                        category = category + "." + function.UserFriendlyName; 
                    }

                    var searchElement = new DSFunctionNodeSearchElement(displayString, function);
                    searchElement.SetSearchable(true);
                    
                    // Add this search eleemnt to the search view
                    TryAddCategoryAndItem(category, searchElement);

                    // function.QualifiedName is the search string for this
                    // element
                    SearchDictionary.Add(searchElement, function.QualifiedName);
                }
            }

        }

        /// <summary>
        ///     Adds a local DynNode to search
        /// </summary>
        /// <param name="dynNode">A Dynamo node object</param>
        public void Add(Type t)
        {
            // get name, category, attributes (this is terribly ugly...)
            var attribs = t.GetCustomAttributes(typeof (NodeNameAttribute), false);
            var name = "";
            if (attribs.Length > 0)
            {
                name = (attribs[0] as NodeNameAttribute).Name;
            }

            attribs = t.GetCustomAttributes(typeof (NodeCategoryAttribute), false);
            var cat = "";
            if (attribs.Length > 0)
            {
                cat = (attribs[0] as NodeCategoryAttribute).ElementCategory;
            }

            attribs = t.GetCustomAttributes(typeof (NodeSearchTagsAttribute), false);
            var tags = new List<string>();
            if (attribs.Length > 0)
            {
                tags = (attribs[0] as NodeSearchTagsAttribute).Tags;
            }

            attribs = t.GetCustomAttributes(typeof (NodeDescriptionAttribute), false);
            var description = "";
            if (attribs.Length > 0)
            {
                description = (attribs[0] as NodeDescriptionAttribute).ElementDescription;
            }

            var searchEle = new NodeSearchElement(name, description, tags);

            attribs = t.GetCustomAttributes(typeof(NodeSearchableAttribute), false);
            bool searchable = true;
            if (attribs.Length > 0)
            {
                searchable = (attribs[0] as NodeSearchableAttribute).IsSearchable;
            }

            searchEle.SetSearchable(searchable);

            // if it's a revit search element, keep track of it
            if ( cat.Equals(BuiltinNodeCategories.REVIT_API) )
            {
                this.RevitApiSearchElements.Add(searchEle);
                if (!IncludeRevitAPIElements)
                {
                    return;
                }
            }

            if (!string.IsNullOrEmpty(cat))
            {
                SearchDictionary.Add(searchEle, cat + "." + searchEle.Name);
            }

            TryAddCategoryAndItem(cat, searchEle);

            SearchDictionary.Add(searchEle, searchEle.Name);
            if (tags.Count > 0)
            {
                // reduce the weight in search by adding white space
                tags.ForEach(x => SearchDictionary.Add(searchEle, x + "++++++++"));
            }
            SearchDictionary.Add(searchEle, description);

        }

        public void RemoveNode(string nodeName)
        {
            // remove from search dictionary
            SearchDictionary.Remove((ele) => (ele).Name == nodeName);
            SearchDictionary.Remove((ele) => (ele).Name.EndsWith("." + nodeName));

            // remove from browser leaves
            _searchElements.Where(x => x.Name == nodeName).ToList().ForEach(x => _searchElements.Remove(x));
        }

        public void RemoveNode(Guid funcId)
        {
            // remove from search dictionary
            SearchDictionary.Remove((x) => x is CustomNodeSearchElement && ((CustomNodeSearchElement)x).Guid == funcId);

            // remove from browser leaves
            _searchElements.Where(x => x is CustomNodeSearchElement && ((CustomNodeSearchElement)x).Guid == funcId).ToList().ForEach(x => _searchElements.Remove(x));
        }

        /// <summary>
        /// Removes a node from search and all empty parent categories
        /// </summary>
        /// <param name="nodeName">The name of the node</param>
        public void RemoveNodeAndEmptyParentCategory(string nodeName)
        {
            var nodes = _searchElements.Where(x => x.Name == nodeName).ToList();
            if (!nodes.Any())
            {
                return;
            }

            foreach (var node in nodes)
            {
                RemoveNode(nodeName);
                RemoveEmptyCategory(node);
            }

        }

        /// <summary>
        /// Removes a node from search and all empty parent categories
        /// </summary>
        /// <param name="nodeName">The name of the node</param>
        public void RemoveNodeAndEmptyParentCategory(Guid customNodeFunctionId)
        {
            var nodes = _searchElements
                .Where(x => x is CustomNodeSearchElement)
                .Cast<CustomNodeSearchElement>()
                .Where(x => x.Guid == customNodeFunctionId)
                .ToList();

            if (!nodes.Any())
            {
                return;
            }

            foreach (var node in nodes)
            {
                RemoveNode(node.Guid);
                RemoveEmptyCategory(node);
            }

        }

        public bool Add(CustomNodeInfo nodeInfo)
        {
            var nodeEle = new CustomNodeSearchElement(nodeInfo);

            if (SearchDictionary.Contains(nodeEle))
            {
                return this.Refactor(nodeInfo);
            }

            SearchDictionary.Add(nodeEle, nodeEle.Name);
            SearchDictionary.Add(nodeEle, nodeInfo.Category + "." + nodeEle.Name);

            TryAddCategoryAndItem(nodeInfo.Category, nodeEle);

            return true;
        }

        public bool Refactor(CustomNodeInfo nodeInfo)
        {
            this.RemoveNodeAndEmptyParentCategory(nodeInfo.Guid);
            return this.Add(nodeInfo);
        }

        public void Search(object parameter)
        {
            if (dynSettings.Controller != null)
            {
                dynSettings.Controller.SearchViewModel.SearchAndUpdateResults();
            }
        }

        internal bool CanSearch(object parameter)
        {
            return true;
        }

        internal void HideSearch(object parameter)
        {
            dynSettings.Controller.SearchViewModel.Visible = false;
        }

        internal bool CanHideSearch(object parameter)
        {
            if (dynSettings.Controller.SearchViewModel.Visible == true)
                return true;
            return false;
        }

        public void ShowSearch(object parameter)
        {
            dynSettings.Controller.SearchViewModel.Visible = true;
        }

        internal bool CanShowSearch(object parameter)
        {
            if (dynSettings.Controller.SearchViewModel.Visible == false)
                return true;
            return false;
        }

        public void FocusSearch(object parameter)
        {
            dynSettings.Controller.SearchViewModel.OnRequestFocusSearch(dynSettings.Controller.DynamoViewModel, EventArgs.Empty);
        }

        internal bool CanFocusSearch(object parameter)
        {
            return true;
        }

        public void ShowLibItemInfoBubble(object parameter)
        {
            dynSettings.Controller.DynamoViewModel.ShowInfoBubble(parameter);
        }

        internal bool CanShowLibItemInfoBubble(object parameter)
        {
            return true;
        }

        public void HideLibItemInfoBubble(object parameter)
        {
            dynSettings.Controller.DynamoViewModel.HideInfoBubble(parameter);
        }

        internal bool CanHideLibItemInfoBubble(object parameter)
        {
            return true;
        }
    }
}
