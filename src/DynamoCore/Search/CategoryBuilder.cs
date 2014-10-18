using System.Collections.ObjectModel;
using System.Linq;
using Dynamo.Nodes.Search;
using Dynamo.Search.SearchElements;
using Dynamo.UI;

namespace Dynamo.Search
{
    class CategoryBuilder
    {
        private ObservableCollection<BrowserRootElement> rootCategories;
        private SearchModel searchModel;

        internal CategoryBuilder(SearchModel searchModel, ObservableCollection<BrowserRootElement> rootCategories)
        {
            this.searchModel = searchModel;
            this.rootCategories = rootCategories;
        }

        internal void RemoveEmptyCategories()
        {
            rootCategories = new ObservableCollection<BrowserRootElement>(rootCategories.Where(x => x.Items.Any()));
        }

        internal void SortCategoryChildren()
        {
            rootCategories.ToList().ForEach(x => x.RecursivelySort());
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

            rootCategories.Remove(rootEle);
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

            nodes.Select(x => x.Name).ToList().ForEach(searchModel.RemoveNode);
            cats.ToList().ForEach(RemoveCategory);

            ele.Items.Clear();

            if (ele is BrowserRootElement)
            {
                rootCategories.Remove(ele as BrowserRootElement);
            }
            else if (ele is BrowserInternalElement)
            {
                (ele as BrowserInternalElement).Parent.Items.Remove(ele);
            }
        }

        /// <summary>
        ///     Add a category, given a delimited name
        /// </summary>
        /// <param name="categoryName">The comma delimited name </param>
        /// <returns>The newly created item</returns>
        internal BrowserItem AddCategory(string categoryName, string resourceAssembly = "",
            SearchModel.ElementType nodeType = SearchModel.ElementType.Regular)
        {
            if (string.IsNullOrEmpty(categoryName))
            {
                return this.TryAddRootCategory("Uncategorized");
            }

            if (ContainsCategory(categoryName))
            {
                return GetCategoryByName(categoryName);
            }

            if (!searchModel.NodeCategories.ContainsKey(categoryName))
            {
                var cat = new CategorySearchElement(categoryName);
                cat.Executed += searchModel.OnExecuted;

                searchModel.NodeCategories.Add(categoryName, cat);
            }

            // otherwise split the category name
            var splitCat = SearchModel.SplitCategoryName(categoryName);

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

            // If splitCat.Count equals 2, then we try to add not class.
            // That means root category is full of methods, not classes.
            // E.g. Operators, BuiltinFunctions.
            // Rootcategory has property IsPlaceholder, that indicates of 
            // which members category contains.
            // So, just add method in category and do nothing.
            // This situation is true for Regular nodes. For other element types we work
            // with all pieces of category.
            var count = nodeType == SearchModel.ElementType.Regular ? splitCat.Count - 1 : splitCat.Count;

            for (var i = 1; i < count; i++)
            {
                // All next members are namespaces.
                currentCat = TryAddChildCategory(currentCat, splitCat[i], resourceAssembly);
            }

            // We sure, that the last member is class.
            if (nodeType == SearchModel.ElementType.Regular)
                currentCat = TryAddChildClass(currentCat, splitCat[splitCat.Count - 1],
                    resourceAssembly);

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
        /// <param name="resourceAssembly">Assembly, where icon for class button can be found.</param>
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
            var ele = new BrowserRootElement(name, rootCategories);
            rootCategories.Add(ele);

            return ele;
        }

        /// <summary>
        /// Add a root category, assuming it doesn't already exist
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        internal BrowserRootElement AddRootCategoryToStart(string name)
        {
            var ele = new BrowserRootElement(name, rootCategories);
            rootCategories.Insert(0, ele);
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
            var split = SearchModel.SplitCategoryName(categoryName);
            if (!split.Any())
                return null;

            var cat = (BrowserItem)rootCategories.FirstOrDefault(x => x.Name == split[0]);

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
    }
}
