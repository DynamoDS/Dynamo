using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Dynamo.Search.SearchElements;
using Dynamo.UI;

namespace Dynamo.Search
{
#if false
    class CategoryBuilder
    {
        private ObservableCollection<BrowserRootElement> rootCategories;
        private SearchModel searchModel;
        private bool isAddons;

        internal ObservableCollection<BrowserRootElement> RootCategories
        {
            get { return rootCategories; }
        }

        public event NotifyCollectionChangedEventHandler RootCategoriesCollectionChanged;
        public void OnRootCategoriesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (RootCategoriesCollectionChanged != null)
                RootCategoriesCollectionChanged(sender, e);
        }

        internal CategoryBuilder(SearchModel searchModel, bool isAddons)
        {
            this.searchModel = searchModel;
            this.isAddons = isAddons;
            this.rootCategories = new ObservableCollection<BrowserRootElement>();
        }

        internal void SortCategoryItems()
        {
            rootCategories.CollectionChanged -= OnRootCategoriesCollectionChanged;
            rootCategories = new ObservableCollection<BrowserRootElement>(rootCategories.OrderBy(x => x.Name));
            rootCategories.CollectionChanged += OnRootCategoriesCollectionChanged;
        }

        internal void SortCategoryChildren()
        {
            rootCategories.ToList().ForEach(x => x.RecursivelySort());
        }

        internal void RemoveEmptyCategories()
        {
            rootCategories.CollectionChanged -= OnRootCategoriesCollectionChanged;
            rootCategories = new ObservableCollection<BrowserRootElement>(rootCategories.Where(x => x.Items.Any()));
            rootCategories.CollectionChanged += OnRootCategoriesCollectionChanged;
        }

        internal void RemoveEmptyRootCategory(string categoryName)
        {
            if (categoryName.Contains(Configurations.CategoryDelimiter))
            {
                RemoveEmptyCategory(categoryName);
                return;
            }

            var category = GetCategoryByName(categoryName);
            if (category == null)
            {
                return;
            }

            RemoveEmptyRootCategory((BrowserRootElement)category);
        }

        internal void RemoveEmptyRootCategory(BrowserRootElement rootElement)
        {
            if (!ContainsCategory(rootElement.Name))
                return;

            rootCategories.Remove(rootElement);
        }

        /// <summary>
        /// Remove and empty category from browser and search by name. Useful when a single item is removed.
        /// </summary>
        /// <param name="categoryName">The category name, including delimiters</param>
        internal void RemoveEmptyCategory(string categoryName)
        {
            var currentCategory = GetCategoryByName(categoryName);
            if (currentCategory == null)
            {
                return;
            }

            RemoveEmptyCategory(currentCategory);
        }

        /// <summary>
        /// Remove an empty category from browser and search.  Useful when a single item is removed.
        /// </summary>
        /// <param name="element"></param>
        internal void RemoveEmptyCategory(BrowserItem element)
        {
            if (element.Items.Count != 0)
                return;

            if (element is BrowserRootElement)
            {
                RemoveEmptyRootCategory(element as BrowserRootElement);
                return;
            }

            if (element is BrowserInternalElement)
            {
                var internalElement = element as BrowserInternalElement;

                internalElement.Parent.Items.Remove(internalElement);
                RemoveEmptyCategory(internalElement.Parent);
            }

            if (element is BrowserInternalElementForClasses)
            {
                var internalElement = element as BrowserInternalElementForClasses;

                internalElement.Parent.Items.Remove(internalElement);
                RemoveEmptyCategory(internalElement.Parent);
            }
        }

        /// <summary>
        /// Remove a category and all its children from the browser and search.  The category does not
        /// have to be empty.
        /// </summary>
        /// <param name="categoryName"></param>
        internal void RemoveCategory(string categoryName)
        {
            var currentCategory = GetCategoryByName(categoryName);
            if (currentCategory == null) return;

            RemoveCategory(currentCategory);

        }

        /// <summary>
        /// Remove a category and all its children from the browser and search.  The category does
        /// not have to be empty.
        /// </summary>
        /// <param name="element"></param>
        internal void RemoveCategory(BrowserItem element)
        {
            var nodes = element.Items.Where(x => x is NodeSearchElement)
                           .Cast<NodeSearchElement>().ToList();

            var cats = element.Items.Where(x => x is BrowserInternalElement)
                           .Cast<BrowserInternalElement>().ToList();

            nodes.Select(x => x.Name).ToList().ForEach(searchModel.RemoveNode);
            cats.ToList().ForEach(RemoveCategory);

            element.Items.Clear();

            if (element is BrowserRootElement)
            {
                rootCategories.Remove(element as BrowserRootElement);
            }
            else if (element is BrowserInternalElement)
            {
                (element as BrowserInternalElement).Parent.Items.Remove(element);
            }
        }

        /// <summary>
        ///     Add a category, given a delimited name
        /// </summary>
        /// <param name="categoryName">The comma delimited name </param>
        /// <returns>The newly created item</returns>
        internal BrowserItem AddCategory(string categoryName, string resourceAssembly = "")
        {
            if (string.IsNullOrEmpty(categoryName))
            {
                return this.TryAddRootCategory("Uncategorized");
            }

            // If the category is already created just return it.
            if (ContainsCategory(categoryName))
            {
                return GetCategoryByName(categoryName);
            }

            if (!searchModel.NodeCategories.ContainsKey(categoryName))
            {
                var category = new CategorySearchElement(categoryName);
                category.Executed += searchModel.OnExecuted;

                searchModel.NodeCategories.Add(categoryName, category);
            }

            // Otherwise split the category name.
            var splitCategory = SearchModel.SplitCategoryName(categoryName);

            // Attempt to add root element
            if (splitCategory.Count == 1)
            {
                return this.TryAddRootCategory(categoryName);
            }

            if (splitCategory.Count == 0)
            {
                return null;
            }

            // Attempt to add root category
            var currentCategory = TryAddRootCategory(splitCategory[0]);

            // If splitCat.Count equals 2, then we try to add not class.
            // That means root category is full of methods, not classes.
            // E.g. Operators, BuiltinFunctions.
            // Rootcategory has property IsPlaceholder, that indicates of 
            // which members category contains.
            // So, just add method in category and do nothing.
            // This situation is true for Regular nodes. For other element types we work
            // with all pieces of category.
            var count = isAddons ? splitCategory.Count : splitCategory.Count - 1;

            for (var i = 1; i < count; i++)
            {
                // All next members are namespaces.
                currentCategory = TryAddChildCategory(currentCategory, splitCategory[i], resourceAssembly);
            }

            // That's all for Addons builder.
            if (isAddons)
                return currentCategory;

            // We sure, that the last member is class.

            // There are possible situations when one category is handled as namespace
            // and class (created special BrowserInternalElement as container). For these
            // situations will be created two instances. Here we fix the case.
            // For example: Analyze.Render namespace and Analyze.Classes.Render as class.
            // We need to move children of Analyze.Classes.Render to Analyze.Render.
            // Category Analyze.Classes.Render (and Analyze.Classes if empty) should be removed.
            var currentFullCategory = string.Join(Configurations.CategoryDelimiter.ToString(),
                splitCategory.ToArray(), 0, splitCategory.Count - 1);
            var index = currentFullCategory.LastIndexOf(Configurations.CategoryDelimiter);
            // Supposed that top categories can't have a duality.
            if (index > -1)
            {
                var classesCategory = currentFullCategory.Insert(index,
                    Configurations.CategoryDelimiter + Configurations.ClassesDefaultName);

                // currentCategory represents context of namespace. For example: Analyze.Render.
                // Searching for Classes element. For example: Analyze.Classes.Render.                    
                var classesElement = GetCategoryByName(classesCategory);
                if (classesElement != null)
                {
                    // Both entries are presented in the tree.                    
                    MoveElementChildren(classesElement, currentCategory);
                    RemoveCategory(classesElement);
                    RemoveEmptyCategory((classesElement as BrowserInternalElement).Parent);
                }
            }

            currentCategory = TryAddChildClass(currentCategory, splitCategory[splitCategory.Count - 1],
                resourceAssembly);

            return currentCategory;
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

            var tempCategory = new BrowserInternalElement(childCategoryName, parent, resourceAssembly);
            tempCategory.FullCategoryName = newCategoryName;
            parent.AddChild(tempCategory);

            return tempCategory;
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
                parent.Items.Insert(0,
                    new BrowserInternalElementForClasses(Configurations.ClassesDefaultName, parent));
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
            var element = new BrowserRootElement(name, rootCategories);
            rootCategories.Add(element);

            return element;
        }

        /// <summary>
        /// Add a root category, assuming it doesn't already exist
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        internal BrowserRootElement AddRootCategoryToStart(string name)
        {
            var element = new BrowserRootElement(name, rootCategories);
            rootCategories.Insert(0, element);
            return element;
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

            var category = (BrowserItem)rootCategories.FirstOrDefault(x => x.Name == split[0]);

            foreach (var splitName in split.GetRange(1, split.Count - 1))
            {
                if (category == null)
                    return category;
                category = TryGetSubCategory(category, splitName);
            }
            return category;
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

        internal void MoveElementChildren(BrowserItem source, BrowserItem destination)
        {
            if (source == null || destination == null) return;
            if (source.Equals(destination)) return;

            while (source.Items.Count != 0)
            {
                destination.Items.Add(source.Items[0]);
                source.Items.RemoveAt(0);
            }
        }
    }
#endif
}
