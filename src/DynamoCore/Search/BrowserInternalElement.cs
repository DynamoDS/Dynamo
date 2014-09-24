using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media.Imaging;
using Dynamo.DSEngine;
using Dynamo.Search;
using Dynamo.Search.SearchElements;

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

        private ClassInformation classDetails;
        public ClassInformation ClassDetails
        {
            get
            {
                if (classDetails == null && IsPlaceholder)
                {
                    classDetails = new ClassInformation();
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
                BitmapSource icon = GetIcon(name + Dynamo.UI.Configurations.SmallIconPostfix);

                if (icon == null)
                {
                    // Get dis-ambiguous resource name and try again.
                    name = GetResourceName(ResourceType.SmallIcon, true);
                    icon = GetIcon(name + Dynamo.UI.Configurations.SmallIconPostfix);
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
                BitmapSource icon = GetIcon(name + Dynamo.UI.Configurations.LargeIconPostfix);

                if (icon == null)
                {
                    // Get dis-ambiguous resource name and try again.
                    name = GetResourceName(ResourceType.LargeIcon, true);
                    icon = GetIcon(name + Dynamo.UI.Configurations.LargeIconPostfix);
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
            this.Focusable = true;
        }

        public BrowserInternalElement(string name, BrowserItem parent, string _assembly = "")
        {
            this._name = name;
            this.assembly = _assembly;
            this.Parent = parent;
            this.OldParent = null;
            this.Focusable = true;
        }

        public string FullCategoryName { get; set; }

        protected virtual string GetResourceName(
            ResourceType resourceType, bool disambiguate = false)
        {
            if (resourceType == ResourceType.SmallIcon)
                return disambiguate ? String.Empty : this.Name;

            return string.Empty;
        }

        private BitmapSource GetIcon(string fullNameOfIcon)
        {
            if (string.IsNullOrEmpty(this.Assembly))
                return null;

            var cust = LibraryCustomizationServices.GetForAssembly(this.Assembly);
            BitmapSource icon = null;
            if (cust != null)
                icon = cust.LoadIconInternal(fullNameOfIcon);
            return icon;
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
        /// Specifies whether or not instance should be shown as StandardPanel.
        /// </summary>
        public bool ClassDetailsVisibility
        {
            set
            {
                // If a caller sets the 'ClassDetailsVisibility' to 'false',
                // then it is intended that we hide away the class details.
                hideClassDetails = !value;
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

        private bool isMoreButtonVisible;
        public bool IsMoreButtonVisible
        {
            get
            { return isMoreButtonVisible; }
            set
            {
                isMoreButtonVisible = value;
                RaisePropertyChanged("IsMoreButtonVisible");
            }
        }

        public ClassInformation()
            : base()
        {
            createMembers = new List<BrowserInternalElement>();
            actionMembers = new List<BrowserInternalElement>();
            queryMembers = new List<BrowserInternalElement>();
            Focusable = false;
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
