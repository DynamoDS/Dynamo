using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using Dynamo.Nodes;
using Dynamo.Utilities;
using Greg.Requests;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.ViewModel;
using Newtonsoft.Json;

namespace Dynamo.PackageManager
{

    public class Package : NotificationObject
    {

        public string Name { get; set; }

        public string CustomNodeDirectory
        {
            get { return Path.Combine(this.RootDirectory, "dyf"); }
        }

        public string BinaryDirectory
        {
            get { return Path.Combine(this.RootDirectory, "bin"); }
        }

        public string ExtraDirectory
        {
            get { return Path.Combine(this.RootDirectory, "extra"); }
        }

        public bool Loaded { get; set; }

        private bool _typesVisibleInManager;
        public bool TypesVisibleInManager { get { return _typesVisibleInManager; } set { _typesVisibleInManager = value; RaisePropertyChanged("TypesVisibleInManager"); } }

        private string _rootDirectory;
        public string RootDirectory { get { return _rootDirectory; } set { _rootDirectory = value; RaisePropertyChanged("RootDirectory"); } }

        private string _description = "";
        public string Description { get { return _description; } set { _description = value; RaisePropertyChanged("Description"); } }

        private string _versionName = "";
        public string VersionName { get { return _versionName; } set { _versionName = value; RaisePropertyChanged("VersionName"); } }

        private string _license = "";
        public string License { get { return _license; } set { _license = value; RaisePropertyChanged("License"); } }

        private string _contents = "";
        public string Contents { get { return _contents; } set { _contents = value; RaisePropertyChanged("Contents"); } }

        private IEnumerable<string> _keywords = new List<string>();
        public IEnumerable<string> Keywords { get { return _keywords; } set { _keywords = value; RaisePropertyChanged("Keywords"); } }

        private string _group = "";
        public string Group { get { return _group; } set { _group = value; RaisePropertyChanged("Group"); } }

        public PackageUploadRequestBody Header { get { return PackageUploadBuilder.NewPackageHeader(this);  } }

        public DelegateCommand ToggleTypesVisibleInManagerCommand { get; set; }
        public DelegateCommand GetLatestVersionCommand { get; set; }
        public DelegateCommand PublishNewPackageVersionCommand { get; set; }
        public DelegateCommand UninstallCommand { get; set; }
        public DelegateCommand PublishNewPackageCommand { get; set; }

        public ObservableCollection<Type> LoadedTypes { get; set; }
        public ObservableCollection<CustomNodeInfo> LoadedCustomNodes { get; set; }
        public ObservableCollection<PackageDependency> Dependencies { get; set; }

        public Package(string directory, string name, string versionName)
        {
            this.Loaded = false;
            this.RootDirectory = directory;
            this.Name = name;
            this.VersionName = versionName;
            this.LoadedTypes = new ObservableCollection<Type>();
            this.Dependencies = new ObservableCollection<PackageDependency>();
            this.LoadedCustomNodes = new ObservableCollection<CustomNodeInfo>();

            ToggleTypesVisibleInManagerCommand = new DelegateCommand(ToggleTypesVisibleInManager, CanToggleTypesVisibleInManager);
            GetLatestVersionCommand = new DelegateCommand(GetLatestVersion, CanGetLatestVersion);
            PublishNewPackageVersionCommand = new DelegateCommand(PublishNewPackageVersion, CanPublishNewPackageVersion);
            PublishNewPackageCommand = new DelegateCommand(PublishNewPackage, CanPublishNewPackage);
            UninstallCommand = new DelegateCommand(Uninstall, CanUninstall);

            dynSettings.Controller.DynamoModel.NodeAdded += (node) => UninstallCommand.RaiseCanExecuteChanged();
            dynSettings.Controller.DynamoModel.NodeDeleted += (node) => UninstallCommand.RaiseCanExecuteChanged();
            dynSettings.Controller.DynamoModel.WorkspaceHidden += (ws) => UninstallCommand.RaiseCanExecuteChanged();
            dynSettings.Controller.DynamoModel.Workspaces.CollectionChanged += (sender, args) => UninstallCommand.RaiseCanExecuteChanged();
        }

        public static Package FromDirectory(string rootPath)
        {
            return Package.FromJson(Path.Combine(rootPath, "pkg.json"));
        }
      
        public static Package FromJson(string headerPath)
        {

            try
            {
                var pkgHeader = File.ReadAllText(headerPath);
                var body = JsonConvert.DeserializeObject<PackageUploadRequestBody>(pkgHeader);

                if (body.name == null || body.version == null)
                {
                    throw new Exception("The header is missing a name or version field.");
                }

                var pkg = new Package(Path.GetDirectoryName(headerPath), body.name, body.version);
                pkg.Group = body.group;
                pkg.Description = body.description;
                pkg.Keywords = body.keywords;
                pkg.VersionName = body.version;
                pkg.License = body.license;
                pkg.Contents = body.contents;
                body.dependencies.ToList().ForEach(pkg.Dependencies.Add);

                return pkg;
            }
            catch (Exception e)
            {
                DynamoLogger.Instance.Log("Failed to form package from json header.");
                DynamoLogger.Instance.Log(e.GetType() + ": " + e.Message);
                return null;
            }

        }

        public void Load()
        {

            try
            {
                GetAssemblies().Select(DynamoLoader.LoadNodesFromAssembly).SelectMany(x => x).ToList().ForEach(x => LoadedTypes.Add(x));
                DynamoLoader.LoadCustomNodes(CustomNodeDirectory).ForEach(x => LoadedCustomNodes.Add(x));

                Loaded = true;
            }
            catch (Exception e)
            {
                DynamoLogger.Instance.Log("Exception when attempting to load package " + this.Name + " from " + this.RootDirectory);
                DynamoLogger.Instance.Log(e.GetType() + ": " + e.Message);
            }

        }

        public List<Assembly> GetAssemblies()
        {
            if (!Directory.Exists(BinaryDirectory)) 
                return new List<Assembly>();

            return
                (new DirectoryInfo(BinaryDirectory))
                    .EnumerateFiles("*.dll")
                    .Select((fileInfo) => Assembly.LoadFrom(fileInfo.FullName)).ToList();
        }

        public bool ContainsFile(string path)
        {
            if (String.IsNullOrEmpty(RootDirectory) || !Directory.Exists(RootDirectory)) return false;
            return Directory.EnumerateFiles(RootDirectory, "*", SearchOption.AllDirectories).Any(s => s == path);
        }

        public bool InUse()
        {
            // get all of the function ids from the custom nodes in this package
            var guids = LoadedCustomNodes.Select(x => x.Guid);

            // check if any of the custom nodes is in a workspace
            var customNodeInUse =  dynSettings.Controller.DynamoModel.AllNodes.Where(x => x is dynFunction)
                                   .Cast<dynFunction>()
                                   .Any(x => guids.Contains(x.Definition.FunctionId));

            return (LoadedTypes.Any() || customNodeInUse) && Loaded;
        }

        private void Uninstall()
        {

            var res = MessageBox.Show("Are you sure you want to uninstall " + this.Name + "?  This will delete the packages root directory.\n\n You can always redownload the package.", "Uninstalling Package", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (res == MessageBoxResult.No) return;

            try
            {
                LoadedCustomNodes.ToList().ForEach(x => dynSettings.CustomNodeManager.Remove(x.Guid));
                dynSettings.PackageLoader.LocalPackages.Remove(this);
                Directory.Delete(this.RootDirectory, true);
            }
            catch( Exception e )
            {
                MessageBox.Show("Dynamo failed to uninstall the package.  You may need to delete the package's root directory manually.", "Uninstall Failure", MessageBoxButton.OK, MessageBoxImage.Error);
                DynamoLogger.Instance.Log("Exception when attempting to uninstall the package " + this.Name + " from " + this.RootDirectory);
                DynamoLogger.Instance.Log(e.GetType() + ": " + e.Message);
            }
            
        }

        private bool CanUninstall()
        {
            return !InUse();
        }

        private void RefreshCustomNodesFromDirectory()
        {
            this.LoadedCustomNodes.Clear();
            dynSettings.CustomNodeManager
                        .GetInfosFromFolder(this.CustomNodeDirectory)
                        .ToList()
                        .ForEach(x => this.LoadedCustomNodes.Add(x));
        }

        private void PublishNewPackageVersion()
        {
            this.RefreshCustomNodesFromDirectory();
            var vm = PublishPackageViewModel.FromLocalPackage(this);
            vm.IsNewVersion = true;

            dynSettings.PackageManagerClient.OnShowPackagePublishUIRequested(vm);
        }

        private bool CanPublishNewPackageVersion()
        {
            return true;
        }

        private void PublishNewPackage()
        {
            this.RefreshCustomNodesFromDirectory();
            var vm = PublishPackageViewModel.FromLocalPackage(this);
            vm.IsNewVersion = false;

            dynSettings.PackageManagerClient.OnShowPackagePublishUIRequested(vm);
        }

        private bool CanPublishNewPackage()
        {
            return true;
        }

        private void GetLatestVersion()
        {
            throw new NotImplementedException();
        }

        private bool CanGetLatestVersion()
        {
            return false;
        }

        private void ToggleTypesVisibleInManager()
        {
            this.TypesVisibleInManager = !this.TypesVisibleInManager;
        }

        private bool CanToggleTypesVisibleInManager()
        {
            return true;
        }
        
    }
}
