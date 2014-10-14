using System;
using System.Collections.Generic;
using System.Linq;

using System.Collections.ObjectModel;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Nodes.Search;
using Dynamo.Search.SearchElements;
using Dynamo.Utilities;
using Dynamo.DSEngine;
using Dynamo.UI;

namespace Dynamo.Search
{
    public class SearchModel
    {
        #region Events

        /// <summary>
        /// Can be invoked in order to signify that the UI should be updated after
        /// the search elements have been modified
        /// </summary>
        public event EventHandler RequestSync;
        public virtual void OnRequestSync(object sender = null, EventArgs e = null)
        {
            if (RequestSync != null)
            {
                RequestSync(sender ?? this, e ?? new EventArgs());
            }
        }

        #endregion

        #region Properties/Fields

        /// <summary>
        /// Leaves of the browser - used for navigation
        /// </summary>
        private List<SearchElementBase> _searchElements = new List<SearchElementBase>();
        internal List<SearchElementBase> SearchElements { get { return _searchElements; } }

        /// <summary>
        ///     Categories property
        /// </summary>
        /// <value>
        ///     A set of categories
        /// </value>
        internal IEnumerable<string> Categories
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
        /// Enum represents loading type of element.
        /// </summary>
        public enum ElementType
        {
            // Element is part of core libraries.
            Regular,
            // Element is part of package.
            Package,
            // Element is custom node created by user but not part of package.
            CustomNode,
            // Element is part of custom DLL. 
            CustomDll
        };

        /// <summary>
        /// The root elements for the browser
        /// </summary>
        private ObservableCollection<BrowserRootElement> _browserRootCategories = new ObservableCollection<BrowserRootElement>();
        public ObservableCollection<BrowserRootElement> BrowserRootCategories
        {
            get { return _browserRootCategories; }
            set { _browserRootCategories = value; }
        }

        /// <summary>
        /// The root elements for custom nodes tree.
        /// </summary>
        private ObservableCollection<BrowserRootElement> _addonRootCategories = new ObservableCollection<BrowserRootElement>();
        public ObservableCollection<BrowserRootElement> AddonRootCategories
        {
            get { return _addonRootCategories; }
            set { _addonRootCategories = value; }
        }

        private ObservableCollection<SearchCategory> _searchRootCategories = new ObservableCollection<SearchCategory>();
        public ObservableCollection<SearchCategory> SearchRootCategories
        {
            get { return _searchRootCategories; }
            set { _searchRootCategories = value; }
        }

        /// <summary>
        ///     SearchDictionary property
        /// </summary>
        /// <value>
        ///     This is the dictionary used to search
        /// </value>
        internal SearchDictionary<SearchElementBase> SearchDictionary { get; private set; }

        /// <summary>
        ///     MaxNumSearchResults property
        /// </summary>
        /// <value>
        ///     Internal limit on the number of search results returned by SearchDictionary
        /// </value>
        internal int MaxNumSearchResults { get; set; }

        private readonly DynamoModel DynamoModel;

        #endregion

        #region Initialization

        /// <summary>
        /// Constructor without any arguments.  Does not initialize the DynamoModel
        /// field.
        /// </summary>
        internal SearchModel()
        {
            InitializeCore();
        }

        internal SearchModel(DynamoModel model)
        {
            DynamoModel = model;
            DynamoModel.CurrentWorkspaceChanged += RevealWorkspaceSpecificNodes;

            InitializeCore();
        }

        private void InitializeCore()
        {
            NodeCategories = new Dictionary<string, CategorySearchElement>();
            SearchDictionary = new SearchDictionary<SearchElementBase>();
            MaxNumSearchResults = 15;

            // pre-populate the search categories
            this.AddRootCategory(BuiltinNodeCategories.CORE);
            this.AddRootCategory(LibraryServices.Categories.BuiltIns);
            this.AddRootCategory(LibraryServices.Categories.Operators);
            this.AddRootCategory(BuiltinNodeCategories.GEOMETRY);
            this.AddRootCategory(BuiltinNodeCategories.REVIT);
            this.AddRootCategory(BuiltinNodeCategories.ANALYZE);
            this.AddRootCategory("Units");
            this.AddRootCategory("Office");
            this.AddRootCategory("Migration");
        }

        #endregion

        #region Destructor

        ~SearchModel()
        {
            if (DynamoModel != null)
            {
                DynamoModel.CurrentWorkspaceChanged -= RevealWorkspaceSpecificNodes;
            }
        }

        #endregion

        #region Context-specific hiding

        private List<NodeSearchElement> NodesHiddenInHomeWorkspace = new List<NodeSearchElement>();
        private List<NodeSearchElement> NodesHiddenInCustomNodeWorkspace = new List<NodeSearchElement>();

        /// <summary>
        /// Show or reveal workspace specific search elements.  These are usually declared using an attribute
        /// on a subclass of NodeModel.
        /// </summary>
        /// <param name="workspace"></param>
        private void RevealWorkspaceSpecificNodes(WorkspaceModel workspace)
        {
            var isCustomNodeWorkspace = workspace is CustomNodeWorkspaceModel;
            var updateSearch = false;

            foreach (var ele in NodesHiddenInHomeWorkspace)
            {
                updateSearch = true;
                ele.SetSearchable(isCustomNodeWorkspace);
            }

            foreach (var ele in NodesHiddenInCustomNodeWorkspace)
            {
                updateSearch = true;
                ele.SetSearchable(!isCustomNodeWorkspace);
            }

            if (updateSearch) this.OnRequestSync();
        }

        #endregion

        #region Search

        /// <summary>
        ///     Performs a search using the given string as query, but does not update
        ///     the SearchResults object.
        /// </summary>
        /// <returns> Returns a list with a maximum MaxNumSearchResults elements.</returns>
        /// <param name="search"> The search query </param>
        internal IEnumerable<SearchElementBase> Search(string search)
        {
            if (string.IsNullOrEmpty(search))
            {
                return _searchElements;
            }

            return Search(search, MaxNumSearchResults);
        }

        private IEnumerable<SearchElementBase> Search(string search, int maxNumSearchResults)
        {
            var foundNodes = SearchDictionary.Search(search, maxNumSearchResults);

            ClearSearchCategories();
            PopulateSearchCategories(foundNodes);

            return foundNodes;
        }

        private void PopulateSearchCategories(IEnumerable<SearchElementBase> nodes)
        {
            foreach (var node in nodes)
            {
                var rootCategoryName = SplitCategoryName(node.FullCategoryName).FirstOrDefault();

                var category = _searchRootCategories.FirstOrDefault(sc => sc.Name == rootCategoryName);
                if (category == null)
                {
                    category = new SearchCategory(rootCategoryName);
                    _searchRootCategories.Add(category);
                }

                category.AddMemberToGroup(node as NodeSearchElement);
                category.AddClassToGroup(node);
            }
        }

        private void ClearSearchCategories()
        {
            _searchRootCategories.Clear();
        }

        #endregion

        #region Categories

        /// <summary>
        ///     Attempt to add a new category to the browser and an item as one of its children
        /// </summary>
        /// <param name="category">The name of the category - a string possibly separated with one period </param>
        /// <param name="item">The item to add as a child of that category</param>
        internal void TryAddCategoryAndItem(string category, BrowserInternalElement item)
        {
            // When create category, give not only category name, 
            // but also assembly, where icon for category could be found.
            var cat = this.AddCategory(category, GetElementType(item),
                (item as NodeSearchElement).Assembly);

            cat.AddChild(item);

            item.FullCategoryName = category;

            var searchEleItem = item as SearchElementBase;
            if (searchEleItem != null)
                _searchElements.Add(searchEleItem);

        }

        internal ElementType GetElementType(BrowserInternalElement item)
        {
            //TODO: Add check if item is loaded as part of package
            if (item is CustomNodeSearchElement)
                return ElementType.CustomNode;

            if (item is DSFunctionNodeSearchElement)
                return (item as DSFunctionNodeSearchElement).ElementType;

            return ElementType.Regular;
        }

        internal void RemoveEmptyCategories()
        {
            this.BrowserRootCategories = new ObservableCollection<BrowserRootElement>(BrowserRootCategories.Where(x => x.Items.Any()));
        }

        internal void SortCategoryChildren()
        {
            this.BrowserRootCategories.ToList().ForEach(x => x.RecursivelySort());
        }

        internal void RemoveEmptyRootCategory(string categoryName)
        {
            if (categoryName.Contains(Configurations.CategoryDelimiter))
            {
                RemoveEmptyCategory(categoryName);
                return;
            }

            var cat = GetCategoryByName(categoryName);
            if (cat == null)
            {
                return;
            }

            RemoveEmptyRootCategory((BrowserRootElement)cat);
        }

        internal void RemoveEmptyRootCategory(BrowserRootElement rootEle)
        {
            if (!ContainsCategory(rootEle.Name))
                return;

            BrowserRootCategories.Remove(rootEle);
        }

        /// <summary>
        /// Remove and empty category from browser and search by name. Useful when a single item is removed.
        /// </summary>
        /// <param name="categoryName">The category name, including delimiters</param>
        internal void RemoveEmptyCategory(string categoryName)
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
        internal void RemoveEmptyCategory(BrowserItem ele)
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
        internal void RemoveCategory(string categoryName)
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
        internal void RemoveCategory(BrowserItem ele)
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
            if (categoryName.Contains(Configurations.CategoryDelimiter))
            {
                splitCat =
                    categoryName.Split(Configurations.CategoryDelimiter)
                                .Where(x => x != Configurations.CategoryDelimiter.ToString() && !System.String.IsNullOrEmpty(x))
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
        internal BrowserItem AddCategory(string categoryName, ElementType nodeType = ElementType.Regular, string resourceAssembly = "")
        {
            if (string.IsNullOrEmpty(categoryName))
            {
                return this.TryAddRootCategory("Uncategorized", nodeType);
            }

            if (ContainsCategory(categoryName))
            {
                return GetCategoryByName(categoryName);
            }

            if (!NodeCategories.ContainsKey(categoryName))
            {
                var cat = new CategorySearchElement(categoryName);
                cat.Executed += this.OnExecuted;

                NodeCategories.Add(categoryName, cat);
            }

            // otherwise split the category name
            var splitCat = SplitCategoryName(categoryName);

            // attempt to add root element
            if (splitCat.Count == 1)
            {
                return this.TryAddRootCategory(categoryName, nodeType);
            }

            if (splitCat.Count == 0)
            {
                return null;
            }

            // attempt to add root category
            var currentCat = TryAddRootCategory(splitCat[0], nodeType);

            // If splitCat.Count equals 2, then we try to add not class.
            // That means root category is full of methods, not classes.
            // E.g. Operators, BuiltinFunctions.
            // Rootcategory has property IsPlaceholder, that indicates of 
            // which members category contains.
            // So, just add method in category and do nothing.

            for (var i = 1; i < splitCat.Count-1; i++)
            {
                // All next members are namespaces.
                currentCat = TryAddChildCategory(currentCat, splitCat[i], resourceAssembly);
            }

            // We sure, that the last member is class.
            if(nodeType == ElementType.Regular)
            currentCat = TryAddChildClass(currentCat, splitCat[splitCat.Count-1], resourceAssembly);

            return currentCat;
        }

        /// <summary>
        /// Add a single category as a child of a category.  If the category already exists, just return that one.
        /// </summary>
        /// <param name="parent">The parent category </param>
        /// <param name="childCategoryName">The name of the child category (can't be nested)</param>
        /// <param name="assembly">Assembly, where icon for class button can be found</param>
        /// <returns>The newly created category</returns>
        internal BrowserItem TryAddChildCategory(BrowserItem parent, string childCategoryName,
                                                 string resourceAssembly = "")
        {
            var newCategoryName = parent.Name + Configurations.CategoryDelimiter + childCategoryName;

            // support long nested categories like Math.Math.StaticMembers.Abs
            var parentItem = parent as BrowserInternalElement;
            while (parentItem != null)
            {
                var grandParent = parentItem.Parent;
                if (null == grandParent)
                    break;

                newCategoryName = grandParent.Name + Configurations.CategoryDelimiter + newCategoryName;
                parentItem = grandParent as BrowserInternalElement;
            }

            if (ContainsCategory(newCategoryName))
            {
                return GetCategoryByName(newCategoryName);
            }

            var tempCat = new BrowserInternalElement(childCategoryName, parent, resourceAssembly);
            tempCat.FullCategoryName = newCategoryName;
            parent.AddChild(tempCat);

            return tempCat;
        }

        /// <summary>
        ///  Add in browserInternalElementForClasses new class or gets this class, if it alredy exists.
        /// </summary>
        /// <param name="parent">Root category or namespace(nested class)</param>
        /// <param name="childCategoryName">Name of class</param>
        /// <param name="resourceAssembly">Assembly, where icon for class button can be found.]</param>
        /// <returns>Class, in which insert methods</returns>
        internal BrowserItem TryAddChildClass(BrowserItem parent, string childCategoryName,
                                                 string resourceAssembly = "")
        {
            // Find in this category BrowserInternalElementForClasses, if it's not presented,
            // create it.
            if (parent.Items.OfType<BrowserInternalElementForClasses>().FirstOrDefault() == null)
            {
                parent.Items.Insert(0, new BrowserInternalElementForClasses("Classes", parent));
            }

            // BIEFC is used to store all classes together. So that, they can be easily shown in treeview.
            var element = parent.Items[0] as BrowserInternalElementForClasses;

            return element.GetChildCategory(childCategoryName, resourceAssembly);    
        }

        /// <summary>
        ///     
        /// </summary>
        /// <returns>The newly added category or the existing one.</returns>
        internal BrowserItem TryAddRootCategory(string categoryName, ElementType nodeType = ElementType.Regular)
        {
            return ContainsCategory(categoryName) ? GetCategoryByName(categoryName) : AddRootCategory(categoryName, nodeType);
        }

        /// <summary>
        /// Add a root category, assuming it doesn't already exist
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        internal BrowserRootElement AddRootCategory(string name, ElementType nodeType = ElementType.Regular)
        {
            BrowserRootElement ele = null;
            if (nodeType == ElementType.Regular)
            {
                ele = new BrowserRootElement(name, BrowserRootCategories);
                BrowserRootCategories.Add(ele);
            }
            else
            {
                ele = new BrowserRootElement(name, AddonRootCategories);
                AddonRootCategories.Add(ele);
            }

            return ele;
        }

        /// <summary>
        /// Add a root category, assuming it doesn't already exist
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        internal BrowserRootElement AddRootCategoryToStart(string name)
        {
            var ele = new BrowserRootElement(name, BrowserRootCategories);
            BrowserRootCategories.Insert(0, ele);
            return ele;
        }

        /// <summary>
        /// Determine whether a category exists in search
        /// </summary>
        /// <param name="categoryName"></param>
        /// <returns></returns>
        internal bool ContainsCategory(string categoryName)
        {
            return GetCategoryByName(categoryName) != null;
        }

        internal BrowserItem GetCategoryByName(string categoryName)
        {
            var split = SplitCategoryName(categoryName);
            if (!split.Any())
                return null;

            var cat = (BrowserItem)BrowserRootCategories.FirstOrDefault(x => x.Name == split[0]);
            if (cat == null)
                cat = (BrowserItem)AddonRootCategories.FirstOrDefault(x => x.Name == split[0]);

            foreach (var splitName in split.GetRange(1, split.Count - 1))
            {
                if (cat == null)
                    return cat;
                cat = TryGetSubCategory(cat, splitName);
            }
            return cat;
        }

        internal BrowserItem TryGetSubCategory(BrowserItem category, string catName)
        {
            return category.Items.FirstOrDefault(x => x.Name == catName);
        }

        internal bool ContainsClass(string categoryName, string className)
        {
            var category = GetCategoryByName(categoryName);
            if (category == null) 
                return false;

            // Find in some category BrowserInternalElementForClasses, that is full of classes.
            var classes = category.Items.OfType<BrowserInternalElementForClasses>();
            if (!classes.Any()) 
                return false;

            BrowserInternalElementForClasses element = classes.ElementAt(0);
            return element.ContainsClass(className);
        }

        #endregion

        #region Add

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
                    //Don't add the functions that are not visible in library.
                    if (!function.IsVisibleInLibrary)
                        continue;

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
                    var group = SearchElementGroup.None;
                    var category = ProcessNodeCategory(function.Category, ref group);

                    // do not add GetType method names to search
                    if (displayString.Contains("GetType"))
                    {
                        continue;
                    }

                    if (isOverloaded)
                    {
                        var args = string.Join(", ", function.Parameters.Select(p => p.ToString()));

                        if (!string.IsNullOrEmpty(args))
                            displayString = displayString + "(" + args + ")";
                    }

                    var searchElement = new DSFunctionNodeSearchElement(displayString, function, group);
                    searchElement.SetSearchable(true);
                    searchElement.FullCategoryName = category;
                    searchElement.Executed += this.OnExecuted;
                    searchElement.ElementType = functionGroup.ElementType;

                    // Add this search eleemnt to the search view
                    TryAddCategoryAndItem(category, searchElement);

                    // function.QualifiedName is the search string for this
                    // element
                    SearchDictionary.Add(searchElement, function.QualifiedName);

                    // add all search tags
                    function.GetSearchTags().ToList().ForEach(x => SearchDictionary.Add(searchElement, x));


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
            var attribs = t.GetCustomAttributes(typeof(NodeNameAttribute), false);
            var name = "";
            if (attribs.Length > 0)
            {
                name = (attribs[0] as NodeNameAttribute).Name;
            }

            attribs = t.GetCustomAttributes(typeof(NodeCategoryAttribute), false);
            var group = SearchElementGroup.None;
            var cat = "";
            if (attribs.Length > 0)
            {
                cat = (attribs[0] as NodeCategoryAttribute).ElementCategory;
                cat = ProcessNodeCategory(cat, ref group);
            }

            attribs = t.GetCustomAttributes(typeof(NodeSearchTagsAttribute), false);
            var tags = new List<string>();
            if (attribs.Length > 0)
            {
                tags = (attribs[0] as NodeSearchTagsAttribute).Tags;
            }

            attribs = t.GetCustomAttributes(typeof(NodeDescriptionAttribute), false);
            var description = "";
            if (attribs.Length > 0)
            {
                description = (attribs[0] as NodeDescriptionAttribute).ElementDescription;
            }

            var searchEle = new NodeSearchElement(name, description, tags, group, t.FullName,
                                                  t.Assembly.GetName().Name + ".dll");
            searchEle.Executed += this.OnExecuted;

            attribs = t.GetCustomAttributes(typeof(NodeSearchableAttribute), false);
            bool searchable = true;
            if (attribs.Length > 0)
            {
                searchable = (attribs[0] as NodeSearchableAttribute).IsSearchable;
            }

            searchEle.SetSearchable(searchable);

            attribs = t.GetCustomAttributes(typeof(NotSearchableInHomeWorkspace), false);
            if (attribs.Length > 0)
            {
                this.NodesHiddenInHomeWorkspace.Add(searchEle);
                if (this.DynamoModel != null && this.DynamoModel.CurrentWorkspace != null &&
                    this.DynamoModel.CurrentWorkspace is HomeWorkspaceModel)
                {
                    searchEle.SetSearchable(false);
                }
            }

            attribs = t.GetCustomAttributes(typeof(NotSearchableInCustomNodeWorkspace), false);
            if (attribs.Length > 0)
            {
                this.NodesHiddenInCustomNodeWorkspace.Add(searchEle);
                if (this.DynamoModel != null && this.DynamoModel.CurrentWorkspace != null &&
                    this.DynamoModel.CurrentWorkspace is CustomNodeWorkspaceModel)
                {
                    searchEle.SetSearchable(false);
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

        public bool Add(CustomNodeInfo nodeInfo)
        {
            var group = SearchElementGroup.None;
            nodeInfo.Category = ProcessNodeCategory(nodeInfo.Category, ref group);

            var nodeEle = new CustomNodeSearchElement(nodeInfo, group);
            nodeEle.Executed += this.OnExecuted;

            if (SearchDictionary.Contains(nodeEle))
            {
                return this.Refactor(nodeInfo);
            }

            SearchDictionary.Add(nodeEle, nodeEle.Name);
            SearchDictionary.Add(nodeEle, nodeInfo.Category + "." + nodeEle.Name);

            TryAddCategoryAndItem(nodeInfo.Category, nodeEle);

            return true;
        }

        #endregion

        #region Execution

        internal event SearchElementBase.SearchElementHandler Executed;
        protected void OnExecuted(SearchElementBase element)
        {
            if (Executed != null)
            {
                Executed(element);
            }
        }

        #endregion

        #region Remove

        internal void RemoveNode(string nodeName)
        {
            // remove from search dictionary
            SearchDictionary.Remove((ele) => (ele).Name == nodeName);
            SearchDictionary.Remove((ele) => (ele).Name.EndsWith("." + nodeName));

            // remove from browser leaves
            _searchElements.Where(x => x.Name == nodeName).ToList().ForEach(x => _searchElements.Remove(x));
        }

        internal void RemoveNode(Guid funcId)
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

        #endregion

        #region Refactoring

        internal bool Refactor(CustomNodeInfo nodeInfo)
        {
            this.RemoveNodeAndEmptyParentCategory(nodeInfo.Guid);
            return this.Add(nodeInfo);
        }

        #endregion

        /// <summary>
        /// Call this method to assign a default grouping information if a given category 
        /// does not have any. A node category's group can either be "Create", "Query" or
        /// "Actions". If none of the group names above is assigned to the category, it 
        /// will be assigned a default one that is "Actions".
        /// 
        /// For examples:
        /// 
        ///     "Core.Evaluate" will be renamed as "Core.Evaluate.Actions"
        ///     "Core.List.Create" will remain as "Core.List.Create"
        /// 
        /// </summary>
        public string ProcessNodeCategory(string category, ref SearchElementGroup group)
        {
            if (string.IsNullOrEmpty(category))
                return category;

            int index = category.LastIndexOf(Configurations.CategoryDelimiter);

            // If "index" is "-1", then the whole "category" will be used as-is.            
            switch (category.Substring(index + 1))
            {
                case Configurations.CategoryGroupAction:
                    group = SearchElementGroup.Action;
                    break;
                case Configurations.CategoryGroupCreate:
                    group = SearchElementGroup.Create;
                    break;
                case Configurations.CategoryGroupQuery:
                    group = SearchElementGroup.Query;
                    break;
                default:
                    group = SearchElementGroup.Action;
                    return category;
            }

            return category.Substring(0, index);
        }
    }
}

