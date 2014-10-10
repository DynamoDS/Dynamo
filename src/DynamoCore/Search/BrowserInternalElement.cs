using System.Collections.ObjectModel;
using System.Linq;

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
}

