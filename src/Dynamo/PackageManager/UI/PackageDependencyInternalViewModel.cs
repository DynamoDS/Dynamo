using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using Dynamo.Nodes.Search;

namespace Dynamo.PackageManager.UI
{
    public class PackageDependencyInternalViewModel : PackageDependencyViewModel
    {
        /// <summary>
        ///     The items inside the package
        /// </summary>
        private ObservableCollection<PackageDependencyViewModel> _items = new ObservableCollection<PackageDependencyViewModel>();
        public override ObservableCollection<PackageDependencyViewModel> Items { get { return _items; } set { _items = value; } }

        public PackageDependencyViewModel Parent { get; set; }

        public PackageDependencyInternalViewModel(Assembly assembly, PackageDependencyViewModel parent)
        {
            this.DependencyType = DependencyType.Assembly;
            this.Assembly = assembly;
            this.Parent = parent;
        }

        public PackageDependencyInternalViewModel(FunctionDefinition def, PackageDependencyViewModel parent)
        {
            this.DependencyType = DependencyType.CustomNode;
            this.Definition = def;
            this.Parent = parent;
        }

    }
}

