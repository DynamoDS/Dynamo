using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

namespace Dynamo.PackageManager.UI
{
    public enum DependencyType
    {
        CustomNode, Assembly, File
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
            this.Height = 32;
            this.DependencyType = DependencyType.CustomNode;
            this.Definition = def;
            this.BuildDependencies(new HashSet<object>());
        }

        public PackageItemRootViewModel(PackageAssembly assembly)
        {
            this.Height = 32;
            this.DependencyType = DependencyType.Assembly;
            this.Assembly = assembly;
            this.BuildDependencies(new HashSet<object>());
        }

        public PackageItemRootViewModel(System.IO.FileInfo fileInfo)
        {
            this.Height = 32;
            this.DependencyType = DependencyType.File;
            this.FileInfo = fileInfo;
            this.BuildDependencies(new HashSet<object>());
        }

    }

}
