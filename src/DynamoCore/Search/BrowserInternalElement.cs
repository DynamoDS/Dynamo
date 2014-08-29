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

        ///<summary>
        /// Small icon for class and method buttons.
        ///</summary>
        public BitmapImage SmallIcon
        {
            get
            {
                return GetIcon(this.GetResourceName(ResourceType.SmallIcon)
                              + Dynamo.UI.Configurations.SmallIconPostfix);
            }
        }

        ///<summary>
        /// Large icon for tooltips.
        ///</summary>
        public BitmapImage LargeIcon
        {
            get
            {
                return GetIcon(this.GetResourceName(ResourceType.LargeIcon)
                              + Dynamo.UI.Configurations.LargeIconPostfix);
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
            get { return assembly; }

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

        protected virtual string GetResourceName(ResourceType resourceType)
        {
            if (resourceType == ResourceType.SmallIcon)
                return this.Name;
            else
                return string.Empty;
        }

        private BitmapImage GetIcon(string fullNameOfIcon)
        {
            if (string.IsNullOrEmpty(this.Assembly))
                return null;

            var cust = LibraryCustomizationServices.GetForAssembly(this.Assembly);
            return (cust != null) ? cust.LoadIconInternal(fullNameOfIcon) : null;
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