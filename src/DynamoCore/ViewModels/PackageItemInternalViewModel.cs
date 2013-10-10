using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using Dynamo.Nodes;
using Dynamo.Nodes.Search;

namespace Dynamo.PackageManager.UI
{
    public class PackageItemInternalViewModel : PackageItemViewModel
    {
        /// <summary>
        ///     The items inside the package
        /// </summary>
        private ObservableCollection<PackageItemViewModel> _items = new ObservableCollection<PackageItemViewModel>();
        public override ObservableCollection<PackageItemViewModel> Items { get { return _items; } set { _items = value; } }

        public PackageItemViewModel Parent { get; set; }

        public PackageItemInternalViewModel(Assembly assembly, PackageItemViewModel parent)
        {
            this.DependencyType = DependencyType.Assembly;
            this.Assembly = assembly;
            this.Parent = parent;
        }

        public PackageItemInternalViewModel(FunctionDefinition def, PackageItemViewModel parent)
        {
            this.DependencyType = DependencyType.CustomNode;
            this.Definition = def;
            this.Parent = parent;
        }

    }
}

