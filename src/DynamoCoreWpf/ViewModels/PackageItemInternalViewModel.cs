using System.Collections.ObjectModel;
using System.Reflection;

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

        public PackageItemInternalViewModel(CustomNodeDefinition def, PackageItemViewModel parent)
        {
            this.DependencyType = DependencyType.CustomNode;
            this.Definition = def;
            this.Parent = parent;
        }

    }
}
