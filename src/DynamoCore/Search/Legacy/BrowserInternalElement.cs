using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
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

        protected virtual string GetResourceName(
            ResourceType resourceType, bool disambiguate = false)
        {
            if (resourceType == ResourceType.SmallIcon)
                return disambiguate ? string.Empty : this.Name;

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
}
