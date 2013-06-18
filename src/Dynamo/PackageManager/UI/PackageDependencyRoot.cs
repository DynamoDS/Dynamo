using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Dynamo.PackageManager.UI
{
    public class PackageDependencyRoot : PackageDependencyItem
    {
        /// <summary>
        ///     The items inside of the browser item
        /// </summary>
        private ObservableCollection<PackageDependencyItem> _items = new ObservableCollection<PackageDependencyItem>();
        public override ObservableCollection<PackageDependencyItem> Items { get { return _items; } set { _items = value; } }

        /// <summary>
        /// Name property </summary>
        /// <value>
        /// The name of the node </value>
        private string _name;
        public override string Name
        {
            get { return _name; }
        }

        public PackageDependencyRoot(string name)
        {
            this.Height = 32;
            this._name = name;
        }

    }

}
