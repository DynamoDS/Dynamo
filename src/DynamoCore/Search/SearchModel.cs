﻿using System;
using System.Collections.Generic;
using System.Linq;

using System.Collections.ObjectModel;
using Dynamo.Core;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Search.SearchElements;
using Dynamo.DSEngine;

using System.Xml;
using DynamoUtilities;


namespace Dynamo.Search
{
    [Obsolete("No longer used.", true)]
    public class SearchModel : NotificationObject
    {
        #region Events

        /// <summary>
        /// Can be invoked in order to signify that the UI should be updated after
        /// the search elements have been modified
        /// </summary>
        public event EventHandler LibraryUpdated;
        protected virtual void OnLibraryUpdated()
        {
            if (LibraryUpdated != null)
                LibraryUpdated(this, EventArgs.Empty);
        }

        #endregion

        #region Properties/Fields
        internal List<SearchElementBase> SearchElements { get; private set; }

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
        /// The root elements for the browser
        /// </summary>
        public ObservableCollection<BrowserRootElement> BrowserRootCategories { get; set; }

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

        #endregion

        #region Initialization

        /// <summary>
        /// Constructor without any arguments.  Does not initialize the DynamoModel
        /// field.
        /// </summary>
        internal SearchModel()
        {
            BrowserRootCategories = new ObservableCollection<BrowserRootElement>();
            SearchElements = new List<SearchElementBase>();
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

        #region Context-specific hiding

        private readonly List<SearchElements.NodeModelSearchElement> nodesHiddenInHomeWorkspace = new List<SearchElements.NodeModelSearchElement>();
        private readonly List<SearchElements.NodeModelSearchElement> nodesHiddenInCustomNodeWorkspace = new List<SearchElements.NodeModelSearchElement>();

        /// <summary>
        /// Show or reveal workspace specific search elements.  These are usually declared using an attribute
        /// on a subclass of NodeModel.
        /// </summary>
        /// <param name="workspace"></param>
        public void RevealWorkspaceSpecificNodes(WorkspaceModel workspace)
        {
            var isCustomNodeWorkspace = workspace is CustomNodeWorkspaceModel;
            var updateSearch = false;

            foreach (var ele in nodesHiddenInHomeWorkspace)
            {
                updateSearch = true;
                ele.SetSearchable(isCustomNodeWorkspace);
            }

            foreach (var ele in nodesHiddenInCustomNodeWorkspace)
            {
                updateSearch = true;
                ele.SetSearchable(!isCustomNodeWorkspace);
            }

            if (updateSearch) OnLibraryUpdated();
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
            return string.IsNullOrEmpty(search)
                ? SearchElements
                : SearchDictionary.Search(search); //, MaxNumSearchResults);
        }

        #endregion

        #region Categories

        private const char CATEGORY_DELIMITER = '.';

        /// <summary>
        ///     Attempt to add a new category to the browser and an item as one of its children
        /// </summary>
        /// <param name="category">The name of the category - a string possibly separated with one period </param>
        /// <param name="item">The item to add as a child of that category</param>
        internal void TryAddCategoryAndItem(string category, BrowserInternalElement item)
        {

            var cat = AddCategory(category);
            cat.AddChild(item);

            item.FullCategoryName = category;

            var searchEleItem = item as SearchElementBase;
            if (searchEleItem != null)
                SearchElements.Add(searchEleItem);

        }

        internal void RemoveEmptyCategories()
        {
            BrowserRootCategories = new ObservableCollection<BrowserRootElement>(BrowserRootCategories.Where(x => x.Items.Any() || x.Name == "Top Result"));
        }

        internal void RemoveEmptyRootCategory(string categoryName)
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
            var nodes = ele.Items.Where(x => x is NodeModelSearchElement)
                           .Cast<NodeModelSearchElement>().ToList();

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
            if (String.IsNullOrEmpty(categoryName))
                return new List<string>();

            var splitCat = new List<string>();
            if (categoryName.Contains(CATEGORY_DELIMITER))
            {
                splitCat =
                    categoryName.Split(CATEGORY_DELIMITER)
                                .Where(x => x != CATEGORY_DELIMITER.ToString() && !String.IsNullOrEmpty(x))
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
        internal BrowserItem AddCategory(string categoryName)
        {
            if (string.IsNullOrEmpty(categoryName))
            {
                return TryAddRootCategory("Uncategorized");
            }

            if (ContainsCategory(categoryName))
            {
                return GetCategoryByName(categoryName);
            }

            if (!NodeCategories.ContainsKey(categoryName))
            {
                var cat = new CategorySearchElement(categoryName);

                NodeCategories.Add(categoryName, cat);
            }

            // otherwise split the category name
            var splitCat = SplitCategoryName(categoryName);

            // attempt to add root element
            if (splitCat.Count == 1)
            {
                return TryAddRootCategory(categoryName);
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
        internal BrowserItem TryAddChildCategory(BrowserItem parent, string childCategoryName)
        {
            var newCategoryName = parent.Name + CATEGORY_DELIMITER + childCategoryName;

            // support long nested categories like Math.Math.StaticMembers.Abs
            var parentItem = parent as BrowserInternalElement;
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
        internal BrowserItem TryAddRootCategory(string categoryName)
        {
            return ContainsCategory(categoryName) ? GetCategoryByName(categoryName) : AddRootCategory(categoryName);
        }

        /// <summary>
        /// Add a root category, assuming it doesn't already exist
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        internal BrowserRootElement AddRootCategory(string name)
        {
            var ele = new BrowserRootElement(name, BrowserRootCategories);
            BrowserRootCategories.Add(ele);
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

            foreach (var splitName in split.GetRange(1, split.Count - 1))
            {
                if (cat == null)
                    return null;
                cat = TryGetSubCategory(cat, splitName);
            }
            return cat;
        }

        internal BrowserItem TryGetSubCategory(BrowserItem category, string catName)
        {
            return category.Items.FirstOrDefault(x => x.Name == catName);
        }

        #endregion

        #region Add
        /// <summary>
        ///     Adds DesignScript function groups
        /// </summary>
        /// <param name="functionGroups"></param>
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
                    var category = function.Category;

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

                    var searchElement = new DSFunctionNodeSearchElement(displayString, function);
                    searchElement.SetSearchable(true);
                    searchElement.FullCategoryName = category;

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
        public void Add(Type t, WorkspaceModel currentWorkspace)
        {
            // get name, category, attributes (this is terribly ugly...)
            var attribs = t.GetCustomAttributes(typeof(NodeNameAttribute), false);
            var name = "";
            if (attribs.Length > 0)
            {
                name = (attribs[0] as NodeNameAttribute).Name;
            }

            attribs = t.GetCustomAttributes(typeof(NodeCategoryAttribute), false);
            var cat = "";
            if (attribs.Length > 0)
            {
                cat = (attribs[0] as NodeCategoryAttribute).ElementCategory;
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

            var searchEle = new SearchElements.NodeModelSearchElement(name, description, tags, t.FullName);

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
                nodesHiddenInHomeWorkspace.Add(searchEle);
                if (currentWorkspace is HomeWorkspaceModel)
                {
                    searchEle.SetSearchable(false);
                }
            }

            attribs = t.GetCustomAttributes(typeof(NotSearchableInCustomNodeWorkspace), false);
            if (attribs.Length > 0)
            {
                nodesHiddenInCustomNodeWorkspace.Add(searchEle);
                if (currentWorkspace is CustomNodeWorkspaceModel)
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
            var nodeEle = new SearchElements.CustomNodeSearchElement(nodeInfo);
            if (SearchDictionary.Contains(nodeEle))
            {
                // Second node with the same GUID should rewrite the original node. 
                // Original node is removed from tree.
                return this.Refactor(nodeInfo);
            }

            SearchDictionary.Add(nodeEle, nodeEle.Name);
            SearchDictionary.Add(nodeEle, nodeInfo.Category + "." + nodeEle.Name);

            TryAddCategoryAndItem(nodeInfo.Category, nodeEle);

            OnLibraryUpdated();

            return true;
        }

        #endregion
        
        #region Remove

        internal void RemoveNode(string nodeName)
        {
            // remove from search dictionary
            SearchDictionary.Remove((ele) => (ele).Name == nodeName);
            SearchDictionary.Remove((ele) => (ele).Name.EndsWith("." + nodeName));

            // remove from browser leaves
            SearchElements.Where(x => x.Name == nodeName).ToList().ForEach(x => SearchElements.Remove(x));
        }

        internal void RemoveNode(Guid funcId)
        {
            // remove from search dictionary
            SearchDictionary.Remove((x) => x is SearchElements.CustomNodeSearchElement && ((SearchElements.CustomNodeSearchElement)x).Guid == funcId);

            // remove from browser leaves
            SearchElements.Where(x => x is SearchElements.CustomNodeSearchElement && ((SearchElements.CustomNodeSearchElement)x).Guid == funcId).ToList().ForEach(x => SearchElements.Remove(x));
        }

        /// <summary>
        /// Removes a node from search and all empty parent categories
        /// </summary>
        /// <param name="nodeName">The name of the node</param>
        public void RemoveNodeAndEmptyParentCategory(string nodeName)
        {
            var nodes = SearchElements.Where(x => x.Name == nodeName).ToList();
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
        public void RemoveNodeAndEmptyParentCategory(Guid customNodeFunctionId)
        {
            var nodes =
                SearchElements.OfType<SearchElements.CustomNodeSearchElement>().Where(x => x.Guid == customNodeFunctionId);

            if (!nodes.Any())
                return;

            foreach (var node in nodes)
            {
                RemoveNode(node.Guid);
                RemoveEmptyCategory(node);
            }

            OnLibraryUpdated();
        }

        #endregion

        #region Refactoring

        /// <summary>
        ///  TODO
        /// </summary>
        /// <param name="nodeInfo"></param>
        /// <returns></returns>
        //TODO(Steve): Remove all custom-node-specific stuff from Search.
        internal bool Refactor(CustomNodeInfo nodeInfo)
        {
            RemoveNodeAndEmptyParentCategory(nodeInfo.FunctionId);
            return Add(nodeInfo);
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

        private static void AddChildrenToXml(XmlNode parent, IEnumerable<BrowserItem> children)
        {
            foreach (var child in children)
            {
                var element = XmlHelper.AddNode(parent, child.GetType().ToString());

                if (child is SearchElements.NodeModelSearchElement)
                {
                    var castedChild = child as SearchElements.NodeModelSearchElement;
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
    }
}

