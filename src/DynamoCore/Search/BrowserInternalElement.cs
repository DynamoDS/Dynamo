using System.Collections.ObjectModel;

namespace Dynamo.Search
{
    /// <summary>
    ///     This class represents internal elements of browser item
    /// </summary>
    public class BrowserInternalElement : BrowserItem
    {
        private ObservableCollection<BrowserItem> _items = new ObservableCollection<BrowserItem>();
        
        /// <summary>
        ///     The items inside of the browser item
        /// </summary>
        public override ObservableCollection<BrowserItem> Items { get { return _items; } set { _items = value; } }

        /// <summary>
        ///     Returns items which are in the same category as the browser item
        /// </summary>
        public ObservableCollection<BrowserItem> Siblings { get { return this.Parent.Items; } }

        /// <summary>
        ///     Returns browser item representing category which this element belongs to
        /// </summary>
        public BrowserItem Parent { get; set; }

        /// <summary>
        ///     Returns previous parent item 
        /// </summary>
        public BrowserItem OldParent { get; set; }

        /// <summary>
        ///     The framework element hosting the extension
        /// </summary>
        public object UIParent { get; set; }

        internal void ReturnToOldParent()
        {
            if (this.OldParent == null) return;

            this.OldParent.AddChild(this);
        }

        internal void ExpandToRoot()
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

        private string _name;
        
        /// <summary>
        ///     Returns name of the node
        /// </summary>
        public override string Name
        {
            get { return _name; }
        }

        /// <summary>
        ///     Returns full category name consisting of root and all sub categories names
        /// </summary>
        public string FullCategoryName { get; set; }

        internal BrowserInternalElement()
        {
            this._name = "Default";
            this.Parent = null;
            this.OldParent = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BrowserInternalElement"/> class.
        /// </summary>
        /// <param name="name">Name of element.</param>
        /// <param name="parent">Parent element.</param>
        internal BrowserInternalElement(string name, BrowserItem parent)
        {
            this._name = name;
            this.Parent = parent;
            this.OldParent = null;
        }

        internal override void Execute()
        {
            var endState = !this.IsExpanded;

            foreach (var ele in this.Siblings)
                ele.IsExpanded = false;

            //Walk down the tree expanding anything nested one layer deep
            //this can be removed when we have the hierachy implemented properly
            if (this.Items.Count == 1)
            {
                BrowserItem subElement = this.Items[0];

                while (subElement.Items.Count == 1)
                {
                    subElement.IsExpanded = true;
                    subElement = subElement.Items[0];
                }

                subElement.IsExpanded = true;
            }

            this.IsExpanded = endState;
        }
    }
}

