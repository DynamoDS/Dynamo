using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using Dynamo.Core;

namespace Dynamo.PackageManager.UI
{

    public enum DependencyType
    {
        CustomNode, Assembly
    }

    public class PackageItemRootViewModel : PackageItemViewModel
    {
        /// <summary>
        ///     The items inside of the browser item
        /// </summary>
        private ObservableCollection<PackageItemViewModel> _items = new ObservableCollection<PackageItemViewModel>();
        public override ObservableCollection<PackageItemViewModel> Items { get { return _items; } set { _items = value; } }

        public PackageItemRootViewModel(CustomNodeDefinition def)
        {
            Height = 32;
            DependencyType = DependencyType.CustomNode;
            Definition = def;
            BuildDependencies(new HashSet<object>());
        }

        public PackageItemRootViewModel(Assembly assembly)
        {
            Height = 32;
            DependencyType = DependencyType.Assembly;
            Assembly = assembly;
            BuildDependencies(new HashSet<object>());
        }

    }

}
