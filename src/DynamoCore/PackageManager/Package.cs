using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;

using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Utilities;
using Greg.Requests;

using Microsoft.Practices.Prism.ViewModel;
using Newtonsoft.Json;
using String = System.String;

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

        private string _engineVersion = "";
        public string EngineVersion { get { return _engineVersion; } set { _engineVersion = value; RaisePropertyChanged("EngineVersion"); } }

        private string _license = "";
        public string License { get { return _license; } set { _license = value; RaisePropertyChanged("License"); } }

        private string _contents = "";
        public string Contents { get { return _contents; } set { _contents = value; RaisePropertyChanged("Contents"); } }

        private IEnumerable<string> _keywords = new List<string>();
        public IEnumerable<string> Keywords { get { return _keywords; } set { _keywords = value; RaisePropertyChanged("Keywords"); } }

        private string _group = "";
        public string Group { get { return _group; } set { _group = value; RaisePropertyChanged("Group"); } }

        public PackageUploadRequestBody Header { get { return PackageUploadBuilder.NewPackageHeader(this);  } }

        public ObservableCollection<Type> LoadedTypes { get; set; }
        public ObservableCollection<CustomNodeInfo> LoadedCustomNodes { get; set; }
        public ObservableCollection<PackageDependency> Dependencies { get; set; }

        private readonly DynamoModel dynamoModel;

        public Package(DynamoModel dynamoModel, string directory, string name, string versionName)
        {
            this.dynamoModel = dynamoModel;
            this.Loaded = false;
            this.RootDirectory = directory;
            this.Name = name;
            this.VersionName = versionName;
            this.LoadedTypes = new ObservableCollection<Type>();
            this.Dependencies = new ObservableCollection<PackageDependency>();
            this.LoadedCustomNodes = new ObservableCollection<CustomNodeInfo>();
        }

        public static Package FromDirectory(string rootPath, DynamoModel dynamoModel)
        {
            return Package.FromJson(Path.Combine(rootPath, "pkg.json"), dynamoModel);
        }

        public static Package FromJson(string headerPath, DynamoModel dynamoModel)
        {
            try
            {
                var pkgHeader = File.ReadAllText(headerPath);
                var body = JsonConvert.DeserializeObject<PackageUploadRequestBody>(pkgHeader);

                if (body.name == null || body.version == null)
                {
                    throw new Exception("The header is missing a name or version field.");
                }

                var pkg = new Package(dynamoModel, Path.GetDirectoryName(headerPath), body.name, body.version);
                pkg.Group = body.group;
                pkg.Description = body.description;
                pkg.Keywords = body.keywords;
                pkg.VersionName = body.version;
                pkg.License = body.license;
                pkg.EngineVersion = body.engine_version;
                pkg.Contents = body.contents;
                body.dependencies.ToList().ForEach(pkg.Dependencies.Add);

                return pkg;
            }
            catch (Exception e)
            {
                dynamoModel.Logger.Log("Failed to form package from json header.");
                dynamoModel.Logger.Log(e.GetType() + ": " + e.Message);
                return null;
            }

        }

        public void Load()
        {
            try
            {
                GetAssemblies().Select(dynamoModel.Loader.LoadNodesFromAssembly).SelectMany(x => x).ToList().ForEach(x => LoadedTypes.Add(x));
                dynamoModel.Loader.LoadCustomNodes(CustomNodeDirectory).ForEach(x => LoadedCustomNodes.Add(x));

                Loaded = true;
            }
            catch (Exception e)
            {
                dynamoModel.Logger.Log("Exception when attempting to load package " + this.Name + " from " + this.RootDirectory);
                dynamoModel.Logger.Log(e.GetType() + ": " + e.Message);
            }

        }

        internal List<Assembly> GetAssemblies()
        {
            if (!Directory.Exists(BinaryDirectory)) 
                return new List<Assembly>();

            return
                (new DirectoryInfo(BinaryDirectory))
                    .EnumerateFiles("*.dll")
                    .Select((fileInfo) => Assembly.LoadFrom(fileInfo.FullName)).ToList();
        }

        internal bool ContainsFile(string path)
        {
            if (String.IsNullOrEmpty(RootDirectory) || !Directory.Exists(RootDirectory)) return false;
            return Directory.EnumerateFiles(RootDirectory, "*", SearchOption.AllDirectories).Any(s => s == path);
        }

        internal bool InUse()
        {
            return (LoadedTypes.Any() || WorkspaceOpen() || CustomNodeInWorkspace() ) && Loaded;
        }

        private bool CustomNodeInWorkspace()
        {
            // get all of the function ids from the custom nodes in this package
            var guids = LoadedCustomNodes.Select(x => x.Guid);

            // check if any of the custom nodes is in a workspace
            return dynamoModel.AllNodes.Where(x => x is Function)
                                   .Cast<Function>()
                                   .Any(x => guids.Contains(x.Definition.FunctionId));

        }

        private bool WorkspaceOpen()
        {
            // get all of the function ids from the custom nodes in this package
            var guids = LoadedCustomNodes.Select(x => x.Guid);

            return
                dynamoModel.Workspaces.Any(
                    x =>
                        {
                            var def = dynamoModel.CustomNodeManager.GetDefinitionFromWorkspace(x);
                            return def != null && guids.Contains(def.FunctionId);
                        });
        }

        internal void UninstallCore()
        {
            try
            {
                LoadedCustomNodes.ToList().ForEach(x => dynamoModel.CustomNodeManager.RemoveFromDynamo(x.Guid));
                dynamoModel.Loader.PackageLoader.LocalPackages.Remove(this);
                Directory.Delete(this.RootDirectory, true);
            }
            catch (Exception e)
            {
                dynamoModel.Logger.Log("Exception when attempting to uninstall the package " + this.Name + " from " + this.RootDirectory);
                dynamoModel.Logger.Log(e.GetType() + ": " + e.Message);
                throw e;
            }
        }

        internal void RefreshCustomNodesFromDirectory()
        {
            this.LoadedCustomNodes.Clear();
            dynamoModel.CustomNodeManager
                        .GetInfosFromFolder(this.CustomNodeDirectory)
                        .ToList()
                        .ForEach(x => this.LoadedCustomNodes.Add(x));
        }

        
    }
}
