using System.Collections.ObjectModel;
using System.Reflection;
using Dynamo.Core;

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
            DependencyType = DependencyType.Assembly;
            Assembly = assembly;
            Parent = parent;
        }

        public PackageItemInternalViewModel(CustomNodeDefinition def, PackageItemViewModel parent)
        {
            DependencyType = DependencyType.CustomNode;
            Definition = def;
            Parent = parent;
        }

    }
}

