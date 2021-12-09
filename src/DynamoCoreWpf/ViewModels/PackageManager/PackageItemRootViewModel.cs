using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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

        /// <summary>
        /// The name of this item, regardless of which constructor was used.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// The file path of this item (if any), regardless of which constructor was used.
        /// </summary>
        public string FilePath { get; }
        
        public PackageItemRootViewModel(CustomNodeDefinition def)
        {
            this.Height = 32;
            this.DependencyType = DependencyType.CustomNode;
            this.Definition = def;
            this.DisplayName = def.DisplayName;
            this.FilePath = String.Empty;
            this.BuildDependencies(new HashSet<object>());
        }

        public PackageItemRootViewModel(PackageAssembly assembly)
        {
            this.Height = 32;
            this.DependencyType = DependencyType.Assembly;
            this.Assembly = assembly;
            this.DisplayName = assembly.Name;
            this.FilePath = assembly.Assembly.Location;
            this.BuildDependencies(new HashSet<object>());
        }

        public PackageItemRootViewModel(System.IO.FileInfo fileInfo)
        {
            this.Height = 32;
            this.DependencyType = DependencyType.File;
            this.FileInfo = fileInfo;
            this.DisplayName = fileInfo.Name;
            this.FilePath = fileInfo.FullName;
            this.BuildDependencies(new HashSet<object>());
        }
    }
}
