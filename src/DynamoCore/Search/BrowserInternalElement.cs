using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
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

        public BrowserRootElement(string name, ObservableCollection<BrowserRootElement> siblings)
        {
            this.Height = 32;
            this.Siblings = siblings;
            this._name = name;
        }

        public void SortChildren()
        {
            this.Items = new ObservableCollection<BrowserItem>(this.Items.OrderBy(x => x.Name));
        }

        public BrowserRootElement(string name)
        {
            this.Height = 32;
            this.Siblings = null;
            this._name = name;
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

        public BitmapImage SmallIcon
        {
            get { return GetSmallIcon(this); }
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
        /// Assembly, where icon for class button can be found.
        /// </summary>
        private string resourceAssembly;
        public string ResourceAssembly
        {
            get { return resourceAssembly; }
        }

        public BrowserInternalElement()
        {
            this._name = "Default";
            this.Parent = null;
            this.OldParent = null;
        }

        public BrowserInternalElement(string name, BrowserItem parent, string resAssembly = "")
        {
            this._name = name;
            this.resourceAssembly = resAssembly;
            this.Parent = parent;
            this.OldParent = null;
        }

        public string FullCategoryName { get; set; }

        protected virtual string GetResourceName(ResourceType resourceType)
        {
            if (resourceType == ResourceType.SmallIcon)
                return this.Name;
            else
                return string.Empty;
        }

        private BitmapImage GetSmallIcon(BrowserInternalElement member)
        {
            if (string.IsNullOrEmpty(member.ResourceAssembly))
                return null;

            LibraryCustomization cust = LibraryCustomizationServices.GetForAssembly(member.ResourceAssembly);
            return (cust != null) ? cust.GetSmallIcon(member.GetResourceName(ResourceType.SmallIcon)) : null;
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

        /// <summary>
        /// Specifies whether or not instance should be shown as StandardPanel.
        /// </summary>
        public bool ClassDetailsVisibility { get; set; }

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

        public void PopulateMemberCollections(BrowserInternalElement element)
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