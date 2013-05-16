using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Dynamo.Commands;
using Dynamo.Nodes;
using Dynamo.Nodes.Search;
using Dynamo.Search.Regions;
using Dynamo.Search.SearchElements;
using Dynamo.Utilities;
using Greg.Responses;
using Microsoft.Practices.Prism.ViewModel;

namespace Dynamo.Search
{
    /// <summary>
    ///     This is the core ViewModel for searching
    /// </summary>
    public class SearchViewModel : NotificationObject
    {
        #region Properties

        /// <summary>
        ///     A helper dictionary to keep track of currently added 
        ///     categories.
        /// </summary>
        private Dictionary<string, BrowserItem> _browserCategoryDict = new Dictionary<string, BrowserItem>();

        /// <summary>
        ///     Indicates whether the node browser is visible or not
        /// </summary>
        private Visibility _browserVisibility = Visibility.Visible;
        public Visibility BrowserVisibility
        {
            get { return _browserVisibility; }
            set { _browserVisibility = value; RaisePropertyChanged("BrowserVisibility"); }
        }

        /// <summary>
        ///     Indicates whether the node browser is visible or not
        /// </summary>
        private Visibility _searchVisibility = Visibility.Collapsed;
        public Visibility SearchVisibility
        {
            get { return _searchVisibility; }
            set { _searchVisibility = value; RaisePropertyChanged("SearchVisibility"); }
        }

        /// <summary>
        ///     Regions property
        /// </summary>
        /// <value>
        ///     Specifies different regions to search over.  The command toggles whether searching
        ///     over that field or not.
        /// </value>
        public ObservableDictionary<string, RegionBase> Regions { get; set; }

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
        private Visibility _visible;

        public Visibility Visible
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
            Visible = Visibility.Collapsed;
            _SearchText = "";
            IncludeRevitAPIElements = false; // revit api
            Regions = new ObservableDictionary<string, RegionBase>();
            //Regions.Add("Include Nodes from Package Manager", DynamoCommands.PackageManagerRegionCommand );
            Regions.Add("Include Experimental Revit API Nodes", new RevitAPIRegion());

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


        public BrowserRootElement AddRootCategory(string name)
        {
            var ele = new BrowserRootElement(name, BrowserRootCategories);
            BrowserRootCategories.Add(ele);
            this._browserCategoryDict.Add(name, ele);
            return ele;
        }

        /// <summary>
        ///     Adds the Home Workspace to search.
        /// </summary>
        private void AddHomeToSearch()
        {
            SearchDictionary.Add(new WorkspaceSearchElement("Home", "Navigate to Home Workspace"), "Home");
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

        /// <summary>
        ///     Remove a specific category from browser and search
        /// </summary>
        /// <param name="categoryName">The category name, including delimiters</param>
        public void RemoveCategory( string categoryName )
        {
            var splitCat = new List<string>();

            if (categoryName.Contains(CATEGORY_DELIMITER))
            {
                splitCat = categoryName.Split(CATEGORY_DELIMITER).ToList();
            }
            else
            {
                splitCat.Add(categoryName);
            }

            var currentCat = (BrowserItem)BrowserRootCategories.FirstOrDefault((x) => x.Name == splitCat[0]);

            if (currentCat == null)
            {
                return;
            }

            // if were looking to remove a root element, simply do that
            if (splitCat.Count == 1)
            {
                BrowserRootCategories.Remove( (BrowserRootElement) currentCat);
                return;
            }

            for (var i = 1; i < splitCat.Count; i++ ){
                
                var matchingCat = currentCat.Items.FirstOrDefault((x) => x.Name == splitCat[i]);

                if (matchingCat == null || i == splitCat.Count-1)
                {
                    if (i == splitCat.Count - 1 && matchingCat == null)
                    {
                        break;
                    }

                    if (i == splitCat.Count - 1 && matchingCat != null)
                    {
                        currentCat = matchingCat;
                    }

                    // remove current cat from its siblings list
                    if (currentCat is BrowserRootElement)
                    {
                        (currentCat as BrowserRootElement).Siblings.Remove((currentCat as BrowserRootElement));
                    }
                    else if (currentCat is BrowserInternalElement)
                    {
                        (currentCat as BrowserInternalElement).Siblings.Remove(currentCat);
                    }
                    break;
                }

                currentCat = matchingCat;

            }

            if (_browserCategoryDict.ContainsKey(categoryName))
            {
                _browserCategoryDict.Remove(categoryName);
            }

        }

        /// <summary>
        ///     Add a category, given a delimited name
        /// </summary>
        /// <param name="categoryName">The comma delimited name </param>
        /// <returns>The newly created item</returns>
        public BrowserItem AddCategory(string categoryName)
        {
            // if already added, return immediately
            if (_browserCategoryDict.ContainsKey(categoryName) )
            {
                return _browserCategoryDict[categoryName];
            }

            // otherwise split the categoryname
            var splitCat = new List<string>();
            if (categoryName.Contains(CATEGORY_DELIMITER))
            {
                splitCat = categoryName.Split(CATEGORY_DELIMITER).ToList();
            }
            else
            {
                splitCat.Add(categoryName);
            }

            // attempt to add root element
            if (splitCat.Count == 1)
            {
                return this.AddRootCategory(categoryName);
            }

            var currentCatName = splitCat[0];

            // attempt to add all other categoires
            var currentCat = (BrowserItem) BrowserRootCategories.FirstOrDefault((x) => x.Name == splitCat[0]);
            if (currentCat == null)
            {
                currentCat = AddRootCategory(splitCat[0]);
            }            

            for (var i = 1; i < splitCat.Count; i++)
            {
                currentCatName = currentCatName + CATEGORY_DELIMITER + splitCat[i];

                var tempCat = currentCat.Items.FirstOrDefault((x) => x.Name == splitCat[i]);
                if (tempCat == null)
                {
                    tempCat = new BrowserInternalElement(splitCat[i], currentCat);
                    currentCat.AddChild( (BrowserInternalElement) tempCat);
                    _browserCategoryDict.Add(currentCatName, tempCat);
                }

                currentCat = tempCat;

            }

            return currentCat;

        }

        public bool ContainsCategory(string categoryName)
        {
            return _browserCategoryDict.ContainsKey(categoryName);
        }


        /// <summary>
        ///     Asynchronously performs a search and updates the observable SearchResults property.
        /// </summary>
        /// <param name="query"> The search query </param>
        internal void SearchAndUpdateResults(string query)
        {
            if (Visible != Visibility.Visible)
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
                                    ele.SetVisibilityToLeaves(Visibility.Visible);
                                }

                                // hide the top result
                                _topResult.Visibility = Visibility.Collapsed;

                                return;
                            }
                            
                            // otherwise, first collapse all
                            foreach (var root in BrowserRootCategories)
                            {
                                root.CollapseToLeaves();
                                root.SetVisibilityToLeaves(Visibility.Collapsed);
                            }

                            //// if there are any results, add the top result 
                            if (t.Result.Any() && t.Result.ElementAt(0) is NodeSearchElement)
                            {
                                _topResult.Items.Clear();
                                _topResult.AddChild( new TopSearchElement( t.Result.ElementAt(0) ) );

                                _topResult.SetVisibilityToLeaves(Visibility.Visible);
                                _topResult.IsExpanded = true;
                            }
                            
                            // for all of the other results, show them in their category
                            foreach (var ele in _browserLeaves)
                            {
                                if (t.Result.Contains(ele))
                                {
                                    ele.Visibility = Visibility.Visible;
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
        internal void SearchAndUpdateResultsSync()
        {
            SearchAndUpdateResultsSync(SearchText);
        }

        /// <summary>
        ///     Synchronously Performs a search and updates the observable SearchResults property
        ///     on the current thread.
        /// </summary>
        /// <param name="query"> The search query </param>
        internal void SearchAndUpdateResultsSync(string query)
        {
            if (Visible != Visibility.Visible)
                return;

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
            else if (e.Key == Key.Tab)
            {
                PopulateSearchTextWithSelectedResult();
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
        public void Add(string name, string category, Guid functionId)
        {
            if (name == "Home")
                return;

            // create the node in search
            var nodeEle = new NodeSearchElement(name, functionId);
            SearchDictionary.Add(nodeEle, nodeEle.Name);
            SearchDictionary.Add(nodeEle, category + "." + nodeEle.Name);

            TryAddCategoryAndItem(category, nodeEle);

            NodeCategories[category].NumElements++;

        }

        /// <summary>
        ///     Attempt to add a new category to the browser and an item as one of its children
        /// </summary>
        /// <param name="category">The name of the category - a string possibly separated with one period </param>
        /// <param name="item">The item to add as a child of that category</param>
        public void TryAddCategoryAndItem( string category, BrowserInternalElement item )
        {

            if (!NodeCategories.ContainsKey(category))
            {
                NodeCategories.Add(category, new CategorySearchElement(category));
            }

            var cat = this.AddCategory(category);
            cat.AddChild(item);

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


        /// <summary>
        ///     Rename a workspace that is currently part of the SearchDictionary
        /// </summary>
        /// <param name="def">The FunctionDefinition whose name must change</param>
        /// <param name="newName">The new name to assign to the workspace</param>
        public void Refactor(FunctionDefinition def, string oldName, string newName)
        {
            SearchDictionary.Remove((ele) => (ele).Name == oldName);
        }
    }
}