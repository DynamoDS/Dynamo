using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using Dynamo.Nodes.Search;

namespace Dynamo.PackageManager.UI
{
    public class PackageDependencyElement : PackageDependencyItem
    {
        /// <summary>
        ///     The items inside the package
        /// </summary>
        private ObservableCollection<PackageDependencyItem> _items = new ObservableCollection<PackageDependencyItem>();
        public override ObservableCollection<PackageDependencyItem> Items { get { return _items; } set { _items = value; } }

        public PackageDependencyItem Parent { get; set; }

        public void ExpandToRoot()
        {
            if (this.Parent == null)
                return;

            this.Parent.IsExpanded = true;
            this.Parent.Visibility = Visibility.Visible;

            var parent = Parent as PackageDependencyElement;
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

        public PackageDependencyElement()
        {
            this._name = "Default";
            this.Parent = null;
        }

        public PackageDependencyElement(string name, PackageDependencyItem parent)
        {
            this._name = name;
            this.Parent = parent;
        }

    }
}

