using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;

using Dynamo.DSEngine;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Utilities;
using Greg.Requests;

using Microsoft.Practices.Prism;
using Microsoft.Practices.Prism.ViewModel;
using Newtonsoft.Json;
using String = System.String;

namespace Dynamo.PackageManager
{
    public class PackageFileInfo
    {
        public FileInfo Model { get; private set; }
        private readonly string packageRoot;

        /// <summary>
        /// Filename relative to the package root directory
        /// </summary>
        public string RelativePath
        {
            get
            {
                return Model.FullName.Substring(packageRoot.Length);
            }
        }

        public PackageFileInfo(string packageRoot, string filename)
        {
            this.packageRoot = packageRoot;
            this.Model = new FileInfo(filename);
        }
    }

    public class Package : NotificationObject
    {

        #region Properties/Fields

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

        private bool typesVisibleInManager;
        public bool TypesVisibleInManager
        {
            get
            {
                return typesVisibleInManager;
            }
            set
            {
                // this implies the user would like to rescan additional files
                this.EnumerateAdditionalFiles();
                typesVisibleInManager = value;
                RaisePropertyChanged("TypesVisibleInManager");
            }
        }

        private string rootDirectory;
        public string RootDirectory { get { return rootDirectory; } set { rootDirectory = value; RaisePropertyChanged("RootDirectory"); } }

        private string description = "";
        public string Description { get { return description; } set { description = value; RaisePropertyChanged("Description"); } }

        private string versionName = "";
        public string VersionName { get { return versionName; } set { versionName = value; RaisePropertyChanged("VersionName"); } }

        private string engineVersion = "";
        public string EngineVersion { get { return engineVersion; } set { engineVersion = value; RaisePropertyChanged("EngineVersion"); } }

        private string license = "";
        public string License { get { return license; } set { license = value; RaisePropertyChanged("License"); } }

        private string contents = "";
        public string Contents { get { return contents; } set { contents = value; RaisePropertyChanged("Contents"); } }

        private IEnumerable<string> _keywords = new List<string>();
        public IEnumerable<string> Keywords { get { return _keywords; } set { _keywords = value; RaisePropertyChanged("Keywords"); } }

        private bool markedForUninstall;
        public bool MarkedForUninstall
        {
            get { return markedForUninstall; }
            private set { markedForUninstall = value; RaisePropertyChanged("MarkedForUninstall"); }
        }

        private string _group = "";
        public string Group { get { return _group; } set { _group = value; RaisePropertyChanged("Group"); } }

        public PackageUploadRequestBody Header { get { return PackageUploadBuilder.NewPackageHeader(this);  } }

        public ObservableCollection<Type> LoadedTypes { get; private set; }
        public ObservableCollection<Assembly> LoadedAssemblies { get; private set; }
        public ObservableCollection<AssemblyName> LoadedAssemblyNames { get; private set; }
        public ObservableCollection<CustomNodeInfo> LoadedCustomNodes { get; private set; }
        public ObservableCollection<PackageDependency> Dependencies { get; private set; }
        public ObservableCollection<PackageFileInfo> AdditionalFiles { get; private set; }

        #endregion

        public Package(string directory, string name, string versionName)
        {
            this.Loaded = false;
            this.RootDirectory = directory;
            this.Name = name;
            this.VersionName = versionName;
            this.LoadedTypes = new ObservableCollection<Type>();
            this.LoadedAssemblies = new ObservableCollection<Assembly>();
            this.LoadedAssemblyNames = new ObservableCollection<AssemblyName>();
            this.Dependencies = new ObservableCollection<PackageDependency>();
            this.LoadedCustomNodes = new ObservableCollection<CustomNodeInfo>();
            this.AdditionalFiles = new ObservableCollection<PackageFileInfo>();

            this.LoadedAssemblies.CollectionChanged += LoadedAssembliesOnCollectionChanged;

        }

        private void LoadedAssembliesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            this.LoadedAssemblyNames.Clear();
            foreach (var ass in LoadedAssemblies)
            {
                this.LoadedAssemblyNames.Add(ass.GetName());
            }
        }

        public static Package FromDirectory(string rootPath, ILogger logger)
        {
            return Package.FromJson(Path.Combine(rootPath, "pkg.json"), logger);
        }

        public static Package FromJson(string headerPath, ILogger logger)
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
                pkg.EngineVersion = body.engine_version;
                pkg.Contents = body.contents;
                body.dependencies.ToList().ForEach(pkg.Dependencies.Add);

                return pkg;
            }
            catch (Exception e)
            {
                logger.Log("Failed to form package from json header.");
                logger.Log(e.GetType() + ": " + e.Message);
                return null;
            }

        }

        public void LoadIntoDynamo( DynamoLoader loader, ILogger logger, LibraryServices libraryServices)
        {
            try
            {
                this.LoadAssembliesIntoDynamo(loader, logger, libraryServices);
                this.LoadCustomNodesIntoDynamo( loader );
                this.EnumerateAdditionalFiles();
                
                Loaded = true;
            }
            catch (Exception e)
            {
                logger.Log("Exception when attempting to load package " + this.Name + " from " + this.RootDirectory);
                logger.Log(e.GetType() + ": " + e.Message);
            }

        }

        public void EnumerateAdditionalFiles()
        {
            if (String.IsNullOrEmpty(RootDirectory) || !Directory.Exists(RootDirectory)) return;

            var nonDyfDllFiles = Directory.EnumerateFiles(
                RootDirectory,
                "*",
                SearchOption.AllDirectories)
                .Where(x => !x.ToLower().EndsWith(".dyf") && !x.ToLower().EndsWith(".dll") && !x.ToLower().EndsWith("pkg.json") && !x.ToLower().EndsWith(".backup"))
                .Select(x => new PackageFileInfo(this.RootDirectory, x));

            this.AdditionalFiles.Clear();
            this.AdditionalFiles.AddRange( nonDyfDllFiles );
        }

        public IEnumerable<string> EnumerateAssemblyFiles()
        {
            if (String.IsNullOrEmpty(RootDirectory) || !Directory.Exists(RootDirectory)) return new List<string>();

            return Directory.EnumerateFiles(
                RootDirectory,
                "*.dll",
                SearchOption.AllDirectories);
        }

        private void LoadCustomNodesIntoDynamo( DynamoLoader loader)
        {
            loader.LoadCustomNodes(CustomNodeDirectory).ForEach(x => LoadedCustomNodes.Add(x));
        }

        private void LoadAssembliesIntoDynamo( DynamoLoader loader, ILogger logger, LibraryServices libraryServices)
        {
            var assemblies = LoadAssembliesInBinDirectory(logger);

            // filter the assemblies
            var zeroTouchAssemblies = new List<Assembly>();
            var nodeModelAssemblies = new List<Assembly>();

            foreach (var assem in assemblies)
            {
                if (loader.ContainsNodeModelSubType(assem))
                {
                    nodeModelAssemblies.Add(assem);
                }
                else
                {
                    zeroTouchAssemblies.Add(assem);
                }
            }

            // load the zero touch assemblies
            foreach (var zeroTouchAssem in zeroTouchAssemblies)
            {
                libraryServices.ImportLibrary(zeroTouchAssem.Location, logger);
            }

            // load the node model assemblies
            foreach (var nodeModelAssem in nodeModelAssemblies)
            {
                var nodes = loader.LoadNodesFromAssembly(nodeModelAssem);
                nodes.ForEach(x => LoadedTypes.Add(x));
            }
        }

        private IEnumerable<Assembly> LoadAssembliesInBinDirectory(ILogger logger)
        {
            var assemblies = new List<Assembly>();

            if (!Directory.Exists(BinaryDirectory))
                return assemblies;

            foreach (var assemFile in (new DirectoryInfo(BinaryDirectory)).EnumerateFiles("*.dll"))
            {
                Assembly assem;

                // dll files may be un-managed, skip those
                var result = PackageLoader.TryLoadFrom(assemFile.FullName, out assem);
                if (result) assemblies.Add(assem);
            }

            foreach (var assem in assemblies)
            {
                this.LoadedAssemblies.Add(assem);
            }

            return assemblies;
        }

        internal bool ContainsFile(string path)
        {
            if (String.IsNullOrEmpty(RootDirectory) || !Directory.Exists(RootDirectory)) return false;
            return Directory.EnumerateFiles(RootDirectory, "*", SearchOption.AllDirectories).Any(s => s == path);
        }

        internal bool InUse( DynamoModel dynamoModel )
        {
            return (LoadedAssemblies.Any() || IsWorkspaceFromPackageOpen(dynamoModel) || IsCustomNodeFromPackageInUse(dynamoModel)) && Loaded;
        }

        private bool IsCustomNodeFromPackageInUse(DynamoModel dynamoModel)
        {
            // get all of the function ids from the custom nodes in this package
            var guids = LoadedCustomNodes.Select(x => x.Guid);

            // check if any of the custom nodes is in a workspace
            return dynamoModel.AllNodes.Where(x => x is Function)
                                   .Cast<Function>()
                                   .Any(x => guids.Contains(x.Definition.FunctionId));

        }

        private bool IsWorkspaceFromPackageOpen(DynamoModel dynamoModel)
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

        internal void MarkForUninstall(IPreferences prefs)
        {
            this.MarkedForUninstall = true;

            if (!prefs.PackageDirectoriesToUninstall.Contains(this.RootDirectory))
            {
                prefs.PackageDirectoriesToUninstall.Add(this.RootDirectory);
            }
        }

        internal void UnmarkForUninstall(IPreferences prefs)
        {
            this.MarkedForUninstall = false;
            prefs.PackageDirectoriesToUninstall.RemoveAll(x => x.Equals(this.RootDirectory));
        }

        internal void UninstallCore( CustomNodeManager customNodeManager, PackageLoader packageLoader, IPreferences prefs, ILogger logger )
        {
            if (this.LoadedAssemblies.Any())
            {
                this.MarkForUninstall(prefs);
                return;
            }

            try
            {
                LoadedCustomNodes.ToList().ForEach(x => customNodeManager.RemoveFromDynamo(x.Guid));
                packageLoader.LocalPackages.Remove(this);
                Directory.Delete(this.RootDirectory, true);
            }
            catch (Exception e)
            {
                logger.Log("Exception when attempting to uninstall the package " + this.Name + " from " + this.RootDirectory);
                logger.Log(e.GetType() + ": " + e.Message);
                throw e;
            }
        }

        internal void RefreshCustomNodesFromDirectory(CustomNodeManager customNodeManager)
        {
            this.LoadedCustomNodes.Clear();
            customNodeManager
                        .GetInfosFromFolder(this.CustomNodeDirectory)
                        .ToList()
                        .ForEach(x => this.LoadedCustomNodes.Add(x));
        }

    }
}
