using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media.Imaging;
using Dynamo.DSEngine;
using Dynamo.Search;
using Dynamo.Search.SearchElements;
using Dynamo.UI;

namespace Dynamo.Nodes.Search
{
    public class BrowserRootElement : BrowserItem
    {

        /// <summary>
        ///     The items inside of the browser item
        /// </summary>
        private ObservableCollection<BrowserItem> _items = new ObservableCollection<BrowserItem>();
        public override ObservableCollection<BrowserItem> Items { get { return _items; } set { _items = value; } }

        public ObservableCollection<BrowserRootElement> Siblings { get; set; }

        /// <summary>
        /// Name property </summary>
        /// <value>
        /// The name of the node </value>
        private string _name;
        public override string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// Property specifies if BrowserItem has members only as children. No any subcategories.
        /// </summary>        
        public bool IsPlaceholder
        {
            get
            {
                // If all childs are derived from NodeSearchElement they all are members
                // not subcategories.
                return Items.Count > 0 && !Items.Any(it => !(it is NodeSearchElement));
            }
        }

        /// <summary>
        /// Specifies whether or not Root Category package / packages need update.
        /// Makes sense for packages only.
        /// TODO: implement as design clarifications are provided.
        /// </summary>
        public bool IsUpdateAvailable { get; set; }

        /// <summary>
        /// Specifies the type of library under the category. Can be Regular, Package, CustomDll 
        /// and CustomNode.
        /// TODO: implement as design clarifications are provided. 
        /// </summary>
        public SearchModel.ElementType ElementType { get; set; }

        private ClassInformation classDetails;
        public ClassInformation ClassDetails
        {
            get
            {
                if (classDetails == null && IsPlaceholder)
                {
                    classDetails = new ClassInformation();
                    classDetails.IsRootCategoryDetails = true;
                    classDetails.PopulateMemberCollections(this);
                }

                return classDetails;
            }
        }

        public BrowserRootElement(string name, ObservableCollection<BrowserRootElement> siblings)
        {
            this.Height = 32;
            this.Siblings = siblings;
            this._name = name;
        }

        public BrowserRootElement(string name)
        {
            this.Height = 32;
            this.Siblings = null;
            this._name = name;
        }

        public void SortChildren()
        {
            this.Items = new ObservableCollection<BrowserItem>(this.Items.OrderBy(x => x.Name));
        }
    }

    public class BrowserInternalElement : BrowserItem
    {

        /// <summary>
        ///     The items inside of the browser item
        /// </summary>
        private ObservableCollection<BrowserItem> _items = new ObservableCollection<BrowserItem>();
        public override ObservableCollection<BrowserItem> Items { get { return _items; } set { _items = value; } }

        public ObservableCollection<BrowserItem> Siblings { get { return this.Parent.Items; } }

        public BrowserItem Parent { get; set; }
        public BrowserItem OldParent { get; set; }

        protected enum ResourceType
        {
            SmallIcon, LargeIcon
        }

        ///<summary>
        /// Small icon for class and method buttons.
        ///</summary>
        public BitmapSource SmallIcon
        {
            get
            {
                var name = GetResourceName(ResourceType.SmallIcon, false);
                BitmapSource icon = GetIcon(name + Configurations.SmallIconPostfix);

                if (icon == null)
                {
                    // Get dis-ambiguous resource name and try again.
                    name = GetResourceName(ResourceType.SmallIcon, true);
                    icon = GetIcon(name + Configurations.SmallIconPostfix);

                    // If there is no icon, use default.
                    if (icon == null)
                        icon = LoadDefaultIcon(ResourceType.SmallIcon);
                }
                return icon;
            }
        }

        ///<summary>
        /// Large icon for tooltips.
        ///</summary>
        public BitmapSource LargeIcon
        {
            get
            {
                var name = GetResourceName(ResourceType.LargeIcon, false);
                BitmapSource icon = GetIcon(name + Configurations.LargeIconPostfix);

                if (icon == null)
                {
                    // Get dis-ambiguous resource name and try again.
                    name = GetResourceName(ResourceType.LargeIcon, true);
                    icon = GetIcon(name + Configurations.LargeIconPostfix);

                    // If there is no icon, use default.
                    if (icon == null)
                        icon = LoadDefaultIcon(ResourceType.LargeIcon);
                }
                return icon;
            }
        }

        public void ReturnToOldParent()
        {
            if (this.OldParent == null) return;

            this.OldParent.AddChild(this);
        }

        public void ExpandToRoot()
        {
            if (this.Parent == null)
                return;

            this.Parent.IsExpanded = true;
            this.Parent.Visibility = true;

            var parent = Parent as BrowserInternalElement;
            if (parent != null)
            {
                parent.ExpandToRoot();
            }
        }

        /// <summary>
        /// Name property </summary>
        /// <value>
        /// The name of the node </value>
        private string _name;
        public override string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// Assembly, from which we can get icon for class button.
        /// </summary>
        private string assembly;
        public string Assembly
        {
            get
            {
                if (!string.IsNullOrEmpty(assembly))
                    return assembly;

                // If there wasn't any assembly, then it's buildin function or operator.
                // Icons for these members are in DynamoCore project.
                return "DynamoCore";
            }

            // Note: we need setter, when we set resource assembly in NodeSearchElement.
            set { assembly = value; }
        }

        public BrowserInternalElement()
        {
            this._name = "Default";
            this.Parent = null;
            this.OldParent = null;
        }

        public BrowserInternalElement(string name, BrowserItem parent, string _assembly = "")
        {
            this._name = name;
            this.assembly = _assembly;
            this.Parent = parent;
            this.OldParent = null;
        }

        public string FullCategoryName { get; set; }

        protected virtual string GetResourceName(
            ResourceType resourceType, bool disambiguate = false)
        {
            if (resourceType == ResourceType.SmallIcon)
                return disambiguate ? String.Empty : this.Name;

            return string.Empty;
        }

        protected BitmapSource GetIcon(string fullNameOfIcon)
        {
            if (string.IsNullOrEmpty(this.Assembly))
                return null;

            var cust = LibraryCustomizationServices.GetForAssembly(this.Assembly);
            BitmapSource icon = null;
            if (cust != null)
                icon = cust.LoadIconInternal(fullNameOfIcon);
            return icon;
        }

        protected virtual BitmapSource LoadDefaultIcon(ResourceType resourceType)
        {
            if (resourceType == ResourceType.LargeIcon)
                return null;

            var cust = LibraryCustomizationServices.GetForAssembly(Configurations.DefaultAssembly);
            return cust.LoadIconInternal(Configurations.DefaultIcon);
        }
    }

    public class BrowserInternalElementForClasses : BrowserItem
    {
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
    }

    public class ClassInformation : BrowserItem
    {
        #region BrowserItem abstract members implementation

        public override ObservableCollection<BrowserItem> Items
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
                throw new System.NotImplementedException();
            }
        }

        public override string Name
        {
            get { throw new System.NotImplementedException(); }
        }

        #endregion

        private bool hideClassDetails = false;

        /// <summary>
        /// Specifies whether or not parent is root category (BrowserRootElement).
        /// </summary>
        public bool IsRootCategoryDetails { get; set; }

        /// <summary>
        /// Specifies whether or not instance should be shown as StandardPanel.
        /// </summary>
        public bool ClassDetailsVisibility
        {
            set
            {
                // If a caller sets the 'ClassDetailsVisibility' to 'false',
                // then it is intended that we hide away the class details.
                hideClassDetails = !value;
                RaisePropertyChanged("ClassDetailsVisibility");
            }

            get
            {
                if (hideClassDetails)
                    return false;

                // If we don't forcefully hide the class detail, then the overall 
                // visibility is dependent on the availability of the following lists.
                return createMembers.Any() || actionMembers.Any() || queryMembers.Any();
            }
        }

        public string PrimaryHeaderText { get; set; }
        public string SecondaryHeaderLeftText { get; set; }
        public string SecondaryHeaderRightText { get; set; }
        public bool IsPrimaryHeaderVisible { get; set; }
        public bool IsSecondaryHeaderLeftVisible { get; set; }
        public bool IsSecondaryHeaderRightVisible { get; set; }

        public enum DisplayMode { None, Query, Action };

        /// <summary>
        /// Specifies which of QueryMembers of ActionMembers list is active for the moment.
        /// If any of CreateMembers, ActionMembers or QueryMembers lists is empty
        /// it returns 'None'.
        /// </summary>
        private DisplayMode currentDisplayMode;
        public DisplayMode CurrentDisplayMode
        {
            get
            {
                return currentDisplayMode;
            }
            set
            {
                currentDisplayMode = value;
                RaisePropertyChanged("CurrentDisplayMode");
            }
        }

        private int hiddenSecondaryMembersCount;
        public int HiddenSecondaryMembersCount
        {
            get { return hiddenSecondaryMembersCount; }
            set
            {
                hiddenSecondaryMembersCount = value;
                RaisePropertyChanged("HiddenSecondaryMembersCount");
                RaisePropertyChanged("MoreButtonText");
            }
        }

        public string MoreButtonText
        {
            get
            {
                var count = HiddenSecondaryMembersCount;
                return string.Format(Configurations.MoreButtonTextFormat, count);
            }
        }

        public ClassInformation()
            : base()
        {
            createMembers = new List<BrowserInternalElement>();
            actionMembers = new List<BrowserInternalElement>();
            queryMembers = new List<BrowserInternalElement>();
        }

        private List<BrowserInternalElement> createMembers;
        public IEnumerable<BrowserInternalElement> CreateMembers
        {
            get { return this.createMembers; }
        }

        private List<BrowserInternalElement> actionMembers;
        public IEnumerable<BrowserInternalElement> ActionMembers
        {
            get { return this.actionMembers; }
        }

        private List<BrowserInternalElement> queryMembers;
        public IEnumerable<BrowserInternalElement> QueryMembers
        {
            get { return this.queryMembers; }
        }

        public void PopulateMemberCollections(BrowserItem element)
        {
            createMembers.Clear();
            actionMembers.Clear();
            queryMembers.Clear();

            foreach (var subElement in element.Items)
            {
                var nodeSearchEle = subElement as NodeSearchElement;
                // nodeSearchEle is null means that our subelement 
                // is not a leaf of nodes tree.
                // Normally we shouldn't have this situation.
                // TODO: discuss with product management.
                if (nodeSearchEle == null)
                    continue;

                switch (nodeSearchEle.Group)
                {
                    case SearchElementGroup.Create:
                        createMembers.Add(subElement as BrowserInternalElement);
                        break;

                    case SearchElementGroup.Action:
                        actionMembers.Add(subElement as BrowserInternalElement);
                        break;

                    case SearchElementGroup.Query:
                        queryMembers.Add(subElement as BrowserInternalElement);
                        break;
                }
            }
        }
    }
}
