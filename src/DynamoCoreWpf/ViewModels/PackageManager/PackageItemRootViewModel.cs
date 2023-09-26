using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace Dynamo.PackageManager.UI
{
    public enum DependencyType
    {
        CustomNode, Assembly, File, Folder
    }

    public class PackageItemRootViewModel : PackageItemViewModel
    {
        /// <summary>
        ///     The items inside of the browser item
        /// </summary>
        private ObservableCollection<PackageItemViewModel> _items = new ObservableCollection<PackageItemViewModel>();
        private ObservableCollection<PackageItemRootViewModel> _childitems = new ObservableCollection<PackageItemRootViewModel>();
        public override ObservableCollection<PackageItemViewModel> Items { get { return _items; } set { _items = value; } }
        public ObservableCollection<PackageItemRootViewModel> ChildItems { get { return _childitems; } set { _childitems = value; } }

        /// <summary>
        /// The name of this item, regardless of which constructor was used.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// The file path of this item (if any), regardless of which constructor was used.
        /// </summary>
        public string FilePath { get; }
        public string DirectoryName { get; private set; }
        public string PathName { get; private set; }
        internal bool isChild;

        public PackageItemRootViewModel(CustomNodeDefinition def)
        {
            this.Height = 32;
            this.DependencyType = DependencyType.CustomNode;
            this.Definition = def;
            this.DisplayName = def.DisplayName;
            this.FilePath = String.Empty;
            this.DirectoryName = String.Empty;
            this.BuildDependencies(new HashSet<object>());
        }

        public PackageItemRootViewModel(PackageAssembly assembly)
        {
            this.Height = 32;
            this.DependencyType = DependencyType.Assembly;
            this.Assembly = assembly;
            this.DisplayName = assembly.Name;
            this.FilePath = assembly.Assembly.Location;
            this.DirectoryName = Path.GetDirectoryName(this.FilePath);
            this.BuildDependencies(new HashSet<object>());
            this.isChild = true;
        }

        public PackageItemRootViewModel(System.IO.FileInfo fileInfo)
        {
            this.Height = 32;
            this.DependencyType = DependencyType.File;
            this.FileInfo = fileInfo;
            this.DisplayName = fileInfo.Name;
            this.FilePath = fileInfo.FullName;
            this.DirectoryName = Path.GetDirectoryName(fileInfo.FullName);
            this.BuildDependencies(new HashSet<object>());
            this.isChild = true;
        }

        public PackageItemRootViewModel(string folderName)
        {
            this.DependencyType = DependencyType.Folder;
            this.DisplayName = Path.GetFileName(folderName);
            this.DirectoryName = folderName;
        }

        internal void AddChildren(List<PackageItemRootViewModel> items)
        {
            foreach(var item in items)
            {
                AddChildren(item);
            }
        }

        internal void AddChildren(PackageItemRootViewModel item)
        {
            if (this.ChildItems.Contains(item)) return;
            this.ChildItems.Add(item);
        }

        internal void AddChild(PackageItemRootViewModel elem)
        {
            var di = new DirectoryInfo(elem.DirectoryName);
            PackageItemRootViewModel subFolder;

            Dictionary<string, PackageItemRootViewModel> existingSubFolders = GetAllSubfolderItems(this);


            while (di.Parent != null)
            {
                // if we already have a subfodler item with that name, add this element to it instead of creating a new subfolder branch
                if(existingSubFolders.Keys.Contains(elem.DirectoryName))
                {
                    existingSubFolders[elem.DirectoryName].ChildItems.Add(elem);
                    return;
                }
                if (di.Parent.FullName == this.DirectoryName)
                {
                    this.ChildItems.Add(elem);
                    return;
                }
                subFolder = new PackageItemRootViewModel(di.Parent.FullName);
                subFolder.ChildItems.Add(elem);
                elem = subFolder;
                di = di.Parent;
            }
        }

        private Dictionary<string, PackageItemRootViewModel> GetAllSubfolderItems(PackageItemRootViewModel elem)
        {
            if(elem.ChildItems.Count == 0) return null;

            var existingSubFolders = new Dictionary<string, PackageItemRootViewModel>();
            foreach (var child in elem.ChildItems)
            {
                if (child.DependencyType != DependencyType.Folder) continue;
                existingSubFolders[child.DirectoryName] = child;
                existingSubFolders = existingSubFolders.Concat(GetAllSubfolderItems(child))
                    .ToDictionary(x => x.Key, x => x.Value);
            }
            return existingSubFolders;
        }
    }
}
