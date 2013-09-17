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
        private bool _browserVisibility = true;
        public bool BrowserVisibility
        {
            get { return _browserVisibility; }
            set { _browserVisibility = value; RaisePropertyChanged("BrowserVisibility"); }
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
        public bool IncludeRevitApiElements;
        public bool IncludeRevitAPIElements
        {
            get { return IncludeRevitApiElements; }
            set
            {
                IncludeRevitApiElements = value;
                RaisePropertyChanged("IncludeRevitAPIElements");
                ToggleIncludingRevitAPIElements();
            }
        }

        /// <summary>
        /// Leaves of the browser - used for navigation
        /// </summary>
        private List<SearchElementBase> _browserLeaves = new List<SearchElementBase>(); 

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
        private int _selectedIndex;
        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                if (_selectedIndex != value)
                {
                    if (_visibleSearchResults.Count > _selectedIndex)
                        _visibleSearchResults[_selectedIndex].IsSelected = false;
                    _selectedIndex = value;
                    if (_visibleSearchResults.Count > _selectedIndex)
                        _visibleSearchResults[_selectedIndex].IsSelected = true;
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
            _SearchText = "";
            IncludeRevitAPIElements = false; // revit api

            //Regions = new ObservableDictionary<string, RegionBase<object>>();
            ////Regions.Add("Include Nodes from Package Manager", DynamoCommands.PackageManagerRegionCommand );
            //var region = new RevitAPIRegion<object>(RevitAPIRegionExecute, RevitAPIRegionCanExecute);
            //region.RaiseCanExecuteChanged();
            //Regions.Add("Include Experimental Revit API Nodes", new RevitAPIRegion<object>(RevitAPIRegionExecute, RevitAPIRegionCanExecute));

            _topResult = this.AddRootCategory("Top Result");
            this.AddRootCategory(BuiltinNodeCategories.CORE);
            this.AddRootCategory(BuiltinNodeCategories.LOGIC);
            this.AddRootCategory(BuiltinNodeCategories.CREATEGEOMETRY);
            this.AddRootCategory(BuiltinNodeCategories.MODIFYGEOMETRY);
            this.AddRootCategory(BuiltinNodeCategories.REVIT);
            this.AddRootCategory(BuiltinNodeCategories.IO);
            this.AddRootCategory(BuiltinNodeCategories.SCRIPTING);
            this.AddRootCategory(BuiltinNodeCategories.ANALYZE);
        }

        public static void RevitAPIRegionExecute(object parameter)
        {
            dynSettings.Controller.SearchViewModel.IncludeRevitAPIElements = !dynSettings.Controller.SearchViewModel.IncludeRevitAPIElements;
            dynSettings.ReturnFocusToSearch();
        }

        internal static bool RevitAPIRegionCanExecute(object parameter)
        {
            return true;
        }

        public static void PackageManagerRegionExecute(object parameters)
        {
            //if (Loaded)
            //{
            //    //DynamoCommands.RefreshRemotePackagesCmd.Execute(null);
            //}
            //else
            //{
                dynSettings.Controller.SearchViewModel.SearchDictionary.Remove((value) => value is PackageManagerSearchElement);
                dynSettings.Controller.SearchViewModel.SearchAndUpdateResultsSync(dynSettings.Controller.SearchViewModel.SearchText);
            //}

            dynSettings.ReturnFocusToSearch();
        }

        internal static bool PackageManagerRegionCanExecute(object parameters)
        {
            return true;
        }

        /// <summary>
        ///     If Revit API elements are shown, hides them.  Otherwise,
        ///     shows them.  Update search when done with either.
        /// </summary>
        public void ToggleIncludingRevitAPIElements()
        {
            if (!IncludeRevitAPIElements)
            {
                this.RemoveCategory(BuiltinNodeCategories.REVIT_API);

                foreach (var ele in RevitApiSearchElements)
                {
                    SearchDictionary.Remove(ele, ele.Name);
                    if (!(ele is CategorySearchElement))
                        SearchDictionary.Remove(ele, BuiltinNodeCategories.REVIT_API + "." + ele.Name);
                }
            }
            else
            {
                var revitCat = this.AddCategory(BuiltinNodeCategories.REVIT_API);
                bool addToCat = !revitCat.Items.Any();

                // add elements to search
                foreach (var ele in RevitApiSearchElements)
                {
                    if (addToCat)
                        revitCat.Items.Add(ele);
                    SearchDictionary.Add(ele, ele.Name);
                    if (!(ele is CategorySearchElement))
                        SearchDictionary.Add(ele, BuiltinNodeCategories.REVIT_API + "." + ele.Name);
                }
            }

        }

        private const char CATEGORY_DELIMITER = '.';

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
                            if (string.IsNullOrEmpty(query) || query == "Search...")
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
                                _topResult.AddChild( new TopSearchElement( t.Result.ElementAt(0) ) );

                                _topResult.SetVisibilityToLeaves(true);
                                _topResult.IsExpanded = true;
                            }
                            
                            // for all of the other results, show them in their category
                            foreach (var ele in _browserLeaves)
                            {
                                if (t.Result.Contains(ele))
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
                return _browserLeaves;
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
            SearchAndUpdateResultsSync(SearchText);
        }

        /// <summary>
        ///     Add a custom node to search.
        /// </summary>
        /// <param name="workspace">A dynWorkspace to add</param>
        /// <param name="name">The name to use</param>
        public void Add(string name, string category, string description, Guid functionId)
        {
            if (name == "Home")
                return;

            // create the node in search
            var nodeEle = new NodeSearchElement(name, description, functionId);
            nodeEle.FullCategoryName = category;

            if (SearchDictionary.Contains(nodeEle))
                return;

            SearchDictionary.Add(nodeEle, nodeEle.Name);
            SearchDictionary.Add(nodeEle, category + "." + nodeEle.Name);

            TryAddCategoryAndItem(category, nodeEle);

            

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
                _browserLeaves.Add(searchEleItem);

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
                SearchDictionary.Add(searchEle, tags);
            }
            SearchDictionary.Add(searchEle, description);

        }

        public void RemoveNode(string nodeName)
        {
            // remove from search dictionary
            SearchDictionary.Remove((ele) => (ele).Name == nodeName);
            SearchDictionary.Remove((ele) => (ele).Name.EndsWith("." + nodeName));

            // remove from browser leaves
            _browserLeaves.Where(x => x.Name == nodeName).ToList().ForEach(x => _browserLeaves.Remove(x));
        }

        /// <summary>
        /// Removes a node from search and all empty parent categories
        /// </summary>
        /// <param name="nodeName">The name of the node</param>
        public void RemoveNodeAndEmptyParentCategory(string nodeName)
        {
            var nodes = _browserLeaves.Where(x => x.Name == nodeName).ToList();
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

        public void Add(CustomNodeInfo nodeInfo)
        {
            this.Add(nodeInfo.Name, nodeInfo.Category, nodeInfo.Description, nodeInfo.Guid);
        }

        internal void Refactor(CustomNodeInfo nodeInfo)
        {
            this.RemoveNodeAndEmptyParentCategory(nodeInfo.Name);
            this.Add(nodeInfo);
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
    }
}