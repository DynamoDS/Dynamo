using System.Collections.ObjectModel;
using Dynamo.DSEngine;
using Dynamo.UI;

namespace Dynamo.Search
{
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

        public override void Execute()
        {
            var endState = !this.IsExpanded;

            foreach (var ele in this.Siblings)
                ele.IsExpanded = false;

            // Collapse all expanded items on next level.
            if (endState)
            {
                foreach (var ele in this.Items)
                    ele.IsExpanded = false;
            }

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

        public string FullCategoryName { get; set; }
    }
}
