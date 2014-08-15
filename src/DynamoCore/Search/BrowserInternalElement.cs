using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
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
        /// Specifies whether or not BrowserInternalElement is container for leaves.
        /// </summary>
        public bool IsPlaceHolder { get; set; }

        public BrowserInternalElement()
        {
            this._name = "Default";
            this.Parent = null;
            this.OldParent = null;
        }

        public BrowserInternalElement(string name, BrowserItem parent)
        {
            this._name = name;
            this.Parent = parent;
            this.OldParent = null;
        }

        public string FullCategoryName { get; set; }
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
        public Visibility SPVisibility { get; set; }

        private ObservableCollection<BrowserInternalElement> createMembers;
        public ObservableCollection<BrowserInternalElement> CreateMembers
        {
            get { return this.createMembers; }
        }

        private ObservableCollection<BrowserInternalElement> actionMembers;
        public ObservableCollection<BrowserInternalElement> ActionMembers
        {
            get { return this.actionMembers; }
        }

        private ObservableCollection<BrowserInternalElement> queryMembers;
        public ObservableCollection<BrowserInternalElement> QueryMembers
        {
            get { return this.queryMembers; }
        }

        public ClassInformation()
        {
            SPVisibility = System.Windows.Visibility.Collapsed;
        }

        public void PopulateMemberCollections(BrowserInternalElement element)
        {
            createMembers = new ObservableCollection<BrowserInternalElement>();
            actionMembers = new ObservableCollection<BrowserInternalElement>();
            queryMembers = new ObservableCollection<BrowserInternalElement>();

            foreach (var subelement in element.Items)
            {
                var nodeSearchEle = subelement as NodeSearchElement;
                // nodeSearchEle is null means that our subelement is not a leaf of nodes tree.
                // Normally we shouldn't have this situations. Should be clarified
                // with project management.
                if (nodeSearchEle == null)
                    continue;

                switch (nodeSearchEle.Group)
                {
                    case SearchElementGroup.Create:
                        createMembers.Add(subelement as BrowserInternalElement);
                        break;

                    case SearchElementGroup.Action:
                        actionMembers.Add(subelement as BrowserInternalElement);
                        break;

                    case SearchElementGroup.Query:
                        queryMembers.Add(subelement as BrowserInternalElement);
                        break;
                }
            }
        }
    }
}