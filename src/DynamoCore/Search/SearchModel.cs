using System;
using System.Collections.Generic;
using System.Linq;

using System.Collections.ObjectModel;
using Dynamo.Core;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Search.SearchElements;
using Dynamo.Utilities;
using Dynamo.DSEngine;

using System.Xml;
using DynamoUtilities;
using Dynamo.UI;
using System.Collections.Specialized;

namespace Dynamo.Search
{
    public class SearchModel : NotificationObject
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

        private CategoryBuilder browserCategoriesBuilder;
        internal CategoryBuilder BrowserCategoriesBuilder { get { return browserCategoriesBuilder; } }

        private CategoryBuilder addonCategoriesBuilder;
        internal CategoryBuilder AddonCategoriesBuilder { get { return addonCategoriesBuilder; } }

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
        internal Dictionary<string, CategorySearchElement> NodeCategories { get; set; }

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
        public ObservableCollection<BrowserRootElement> BrowserRootCategories
        {
            get { return browserCategoriesBuilder.RootCategories; }
        }

        public event NotifyCollectionChangedEventHandler BrowserRootCategoriesCollectionChanged
        {
            add { browserCategoriesBuilder.RootCategoriesCollectionChanged += value; }
            remove { browserCategoriesBuilder.RootCategoriesCollectionChanged -= value; }
        }

        /// <summary>
        /// The root elements for custom nodes tree.
        /// </summary>        
        public ObservableCollection<BrowserRootElement> AddonRootCategories
        {
            get { return addonCategoriesBuilder.RootCategories; }
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
            browserCategoriesBuilder = new CategoryBuilder(this, false);
            addonCategoriesBuilder = new CategoryBuilder(this, true);

            NodeCategories = new Dictionary<string, CategorySearchElement>();
            SearchDictionary = new SearchDictionary<SearchElementBase>();
            MaxNumSearchResults = 15;

            // pre-populate the search categories
            browserCategoriesBuilder.AddRootCategory(BuiltinNodeCategories.CORE);
            browserCategoriesBuilder.AddRootCategory(LibraryServices.Categories.BuiltIns);
            browserCategoriesBuilder.AddRootCategory(LibraryServices.Categories.Operators);
            browserCategoriesBuilder.AddRootCategory(BuiltinNodeCategories.GEOMETRY);
            browserCategoriesBuilder.AddRootCategory(BuiltinNodeCategories.REVIT);
            browserCategoriesBuilder.AddRootCategory(BuiltinNodeCategories.ANALYZE);
            browserCategoriesBuilder.AddRootCategory("Units");
            browserCategoriesBuilder.AddRootCategory("Office");
            browserCategoriesBuilder.AddRootCategory("Migration");
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
        ///     Performs a search using the given string as query.
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
            foreach (NodeSearchElement node in nodes)
            {
                var rootCategoryName = SplitCategoryName(node.FullCategoryName).FirstOrDefault();

                var category = _searchRootCategories.FirstOrDefault(sc => sc.Name == rootCategoryName);
                if (category == null)
                {
                    category = new SearchCategory(rootCategoryName);
                    _searchRootCategories.Add(category);
                }

                category.AddMemberToGroup(node);
                category.AddClassToGroup(node);
            }

            // Order found categories by name.
            _searchRootCategories = new ObservableCollection<SearchCategory>(_searchRootCategories.OrderBy(x => x.Name));
            
            SortSearchCategoriesChildren();
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
            ElementType nodeType = GetElementType(item);

            // When create category, give not only category name, 
            // but also assembly, where icon for category could be found.

            BrowserItem cat = browserCategoriesBuilder.AddCategory(category, (item as NodeSearchElement).Assembly);

            cat.AddChild(item);

            item.FullCategoryName = category;

            var searchEleItem = item as SearchElementBase;
            if (searchEleItem != null)
                _searchElements.Add(searchEleItem);
        }

        internal ElementType GetElementType(BrowserInternalElement item)
        {
            if (item is NodeSearchElement)
                return (item as NodeSearchElement).ElementType;

            return ElementType.Regular;
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
                                .Where(x => x != Configurations.CategoryDelimiter.ToString() && !string.IsNullOrEmpty(x))
                                .ToList();
            }
            else
            {
                splitCat.Add(categoryName);
            }

            return splitCat;
        }

        internal void RemoveEmptyCategories()
        {
            browserCategoriesBuilder.RemoveEmptyCategories();
            addonCategoriesBuilder.RemoveEmptyCategories();
        }

        internal void SortRootCategories()
        {
            browserCategoriesBuilder.SortCategoryItems();
            addonCategoriesBuilder.SortCategoryItems();
        }

        internal void SortCategoryChildren()
        {
            browserCategoriesBuilder.SortCategoryChildren();
            addonCategoriesBuilder.SortCategoryChildren();
        }

        internal void SortSearchCategoriesChildren()
        {
            _searchRootCategories.ToList().ForEach(x => x.SortChildren());
        }

        internal static string ShortenCategoryName(string fullCategoryName)
        {
            if (string.IsNullOrEmpty(fullCategoryName))
                return string.Empty;

            var catName = fullCategoryName.Replace(Configurations.CategoryDelimiter.ToString(), " " + Configurations.ShortenedCategoryDelimiter + " ");

            // if the category name is too long, we strip off the interior categories
            if (catName.Length > 50)
            {
                var s = catName.Split(Configurations.ShortenedCategoryDelimiter).Select(x => x.Trim()).ToList();
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
                    catName = String.Join(" " + Configurations.ShortenedCategoryDelimiter + " ", s);
                }
            }

            return catName;
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

                    // Rename category (except for custom nodes, imported libraries).
                    string category = ProcessNodeCategory(function.Category, ref group);

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
        public void Add(Type t, ElementType nodeType = ElementType.Regular)
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
                var catCandidate = (attribs[0] as NodeCategoryAttribute).ElementCategory;
                cat = ProcessNodeCategory(catCandidate, ref group);
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
            searchEle.ElementType = nodeType;

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
                SearchDictionary.Add(searchEle, cat + Configurations.CategoryDelimiter + searchEle.Name);
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
            if (SearchDictionary.Contains(nodeEle))
            {
                // Second node with the same GUID should rewrite the original node. 
                // Original node is removed from tree.
                return this.Refactor(nodeInfo);
            }

            nodeEle.ElementType = nodeInfo.ElementType;
            nodeEle.Executed += this.OnExecuted;

            SearchDictionary.Add(nodeEle, nodeEle.Name);
            SearchDictionary.Add(nodeEle, nodeInfo.Category + "." + nodeEle.Name);

            TryAddCategoryAndItem(nodeInfo.Category, nodeEle);

            return true;
        }

        #endregion

        #region Execution

        internal event SearchElementBase.SearchElementHandler Executed;
        internal void OnExecuted(SearchElementBase element)
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
                browserCategoriesBuilder.RemoveEmptyCategory(node);
                addonCategoriesBuilder.RemoveEmptyCategory(node);
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
                browserCategoriesBuilder.RemoveEmptyCategory(node);
                addonCategoriesBuilder.RemoveEmptyCategory(node);
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

        internal void DumpLibraryToXml(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return;

            var document = ComposeXmlForLibrary();
            document.Save(fileName);
        }

        internal XmlDocument ComposeXmlForLibrary()
        {
            var document = XmlHelper.CreateDocument("LibraryTree");

            foreach (var category in BrowserRootCategories)
            {
                var element = XmlHelper.AddNode(document.DocumentElement, category.GetType().ToString());
                XmlHelper.AddAttribute(element, "Name", category.Name);

                AddChildrenToXml(element, category.Items);

                document.DocumentElement.AppendChild(element);
            }

            return document;
        }

        private void AddChildrenToXml(XmlNode parent, ObservableCollection<BrowserItem> children)
        {
            foreach (var child in children)
            {
                var element = XmlHelper.AddNode(parent, child.GetType().ToString());

                if (child is NodeSearchElement)
                {
                    var castedChild = child as NodeSearchElement;
                    XmlHelper.AddNode(element, "FullCategoryName", castedChild.FullCategoryName);
                    XmlHelper.AddNode(element, "FullName", castedChild.FullName);
                    XmlHelper.AddNode(element, "Name", castedChild.Name);
                    XmlHelper.AddNode(element, "Description", castedChild.Description);
                }
                else
                {
                    XmlHelper.AddAttribute(element, "Name", child.Name);
                    AddChildrenToXml(element, child.Items);
                }

                parent.AppendChild(element);
            }
        }

        internal void ChangeCategoryExpandState(string categoryName, bool isExpanded)
        {
            BrowserItem category = BrowserCategoriesBuilder.GetCategoryByName(categoryName);
            if (category == null)
                category = AddonCategoriesBuilder.GetCategoryByName(categoryName);

            if (category != null && category.IsExpanded != isExpanded)
                category.IsExpanded = isExpanded;
        }

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
