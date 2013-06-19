using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Dynamo.PackageManager.UI
{

    public enum DependencyType
    {
        CustomNode, Assembly
    }

    public class PackageDependencyRootViewModel : PackageDependencyViewModel
    {
        /// <summary>
        ///     The items inside of the browser item
        /// </summary>
        private ObservableCollection<PackageDependencyViewModel> _items = new ObservableCollection<PackageDependencyViewModel>();
        public override ObservableCollection<PackageDependencyViewModel> Items { get { return _items; } set { _items = value; } }

        public PackageDependencyRootViewModel(FunctionDefinition def)
        {
            this.Height = 32;
            this.DependencyType = DependencyType.CustomNode;
            this.Definition = def;
            this.BuildDependencies(new HashSet<object>());
        }

        public PackageDependencyRootViewModel(Assembly assembly)
        {
            this.Height = 32;
            this.DependencyType = DependencyType.Assembly;
            this.Assembly = assembly;
            this.BuildDependencies(new HashSet<object>());
        }

    }

}
