using System.Collections.ObjectModel;
using System.Linq;

namespace Dynamo.Search
{
    public class BrowserInternalElementForClasses //: BrowserItem
    {
#if false
        private string name;
        public override string Name
        {
            get { return name; }
        }

        /// <summary>
        ///     The classes inside of the browser item
        /// </summary>
        private ObservableCollection<BrowserItem> classesItems = new ObservableCollection<BrowserItem>();
        public override ObservableCollection<BrowserItem> Items
        {
            get { return classesItems; }
            set { classesItems = value; }
        }

        public BrowserItem Parent { get; set; }

        public BrowserInternalElementForClasses(string name, BrowserItem parent)
        {
            this.name = name;
            this.Parent = parent;
        }

        public bool ContainsClass(string className)
        {
            var searchedClass = Items.FirstOrDefault(x => x.Name == className);
            return searchedClass != null;
        }


        /// <summary>
        /// Tries  to get child category, in fact child class.
        /// If class was not found, then creates it.
        /// </summary>
        /// <param name="childCategoryName">Name of searched class</param>
        /// <param name="resourceAssembly">Assembly with icons</param>
        /// <returns></returns>
        public BrowserItem GetChildCategory(string childCategoryName, string resourceAssembly)
        {
            // Find among all presented classes requested class.
            var allPresentedClasses = Items;
            var requestedClass = allPresentedClasses.FirstOrDefault(x => x.Name == childCategoryName);
            if (requestedClass != null) return requestedClass;

            //  Add new class, if it wasn't found.
            var tempClass = new BrowserInternalElement(childCategoryName, this, resourceAssembly);
            tempClass.FullCategoryName = Parent.Name + childCategoryName;
            Items.Add(tempClass);
            return tempClass;
        }

        public override void Execute() { }
#endif
    }
}
