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
            Height = 32;
            Siblings = siblings;
            _name = name;
        }

        public void SortChildren()
        {
            Items = new ObservableCollection<BrowserItem>(Items.OrderBy(x => x.Name));
        }

        public BrowserRootElement(string name)
        {
            Height = 32;
            Siblings = null;
            _name = name;
        }

    }

    public class BrowserInternalElement : BrowserItem
    {

        /// <summary>
        ///     The items inside of the browser item
        /// </summary>
        private ObservableCollection<BrowserItem> _items = new ObservableCollection<BrowserItem>();
        public override ObservableCollection<BrowserItem> Items { get { return _items; } set { _items = value; } }

        public ObservableCollection<BrowserItem> Siblings { get { return Parent.Items; } }

        public BrowserItem Parent { get; set; }
        public BrowserItem OldParent { get; set; }

        public void ReturnToOldParent()
        {
            if (OldParent == null) return;

            OldParent.AddChild(this);
        }

        public void ExpandToRoot()
        {
            if (Parent == null)
                return;

            Parent.IsExpanded = true;
            Parent.Visibility = true;

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
            _name = "Default";
            Parent = null;
            OldParent = null;
        }

        public BrowserInternalElement(string name, BrowserItem parent)
        {
            _name = name;
            Parent = parent;
            OldParent = null;
        }


        public string FullCategoryName { get; set; }
    }
}

