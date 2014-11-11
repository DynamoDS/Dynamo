using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;

using Dynamo.Core;
using Dynamo.DSEngine;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Utilities;
using Greg.Requests;

using Newtonsoft.Json;
using String = System.String;

namespace Dynamo.PackageManager
{
    public class PackageAssembly
    {
        public bool IsNodeLibrary { get; set; }
        public Assembly Assembly { get; set; }

        public string Name
        {
            get { return Assembly.GetName().Name; }
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

        public bool Loaded { get; private set; }

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


        /// <summary>
        ///     Determines if there are binaries in the package
        /// </summary>
        internal bool ContainsBinaries
        {
            get { return this.LoadedAssemblies.Any(); }
        }

        /// <summary>
        ///     List the LoadedAssemblies whose IsNodeLibrary attribute is true
        /// </summary>
        internal IEnumerable<Assembly> NodeLibraries
        {
            get { return this.LoadedAssemblies.Where(x => x.IsNodeLibrary).Select(x => x.Assembly); }
        } 

        public String SiteUrl { get; set; }
        public String RepositoryUrl { get; set; }

        public ObservableCollection<Type> LoadedTypes { get; private set; }
        public ObservableCollection<PackageAssembly> LoadedAssemblies { get; private set; }
        public ObservableCollection<CustomNodeInfo> LoadedCustomNodes { get; private set; }
        public ObservableCollection<PackageDependency> Dependencies { get; private set; }
        public ObservableCollection<PackageFileInfo> AdditionalFiles { get; private set; }

        /// <summary>
        ///     A header used to create the package, this data does not reflect runtime
        ///     changes to the package, but instead reflects how the package was formed.
        /// </summary>
        public PackageUploadRequestBody Header { get; internal set; }

        #endregion

        public Package(string directory, string name, string versionName, string license)
        {
            this.RootDirectory = directory;
            this.Name = name;
            this.License = license;
            this.VersionName = versionName;
            this.LoadedTypes = new ObservableCollection<Type>();
            this.LoadedAssemblies = new ObservableCollection<PackageAssembly>();
            this.Dependencies = new ObservableCollection<PackageDependency>();
            this.LoadedCustomNodes = new ObservableCollection<CustomNodeInfo>();
            this.AdditionalFiles = new ObservableCollection<PackageFileInfo>();
            this.Header = PackageUploadBuilder.NewPackageHeader(this);
        }

        public static Package FromDirectory(string rootPath, ILogger logger)
        {
            return FromJson(Path.Combine(rootPath, "pkg.json"), logger);
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

                var pkg = new Package(Path.GetDirectoryName(headerPath), body.name, body.version, body.license);
                pkg.Group = body.group;
                pkg.Description = body.description;
                pkg.Keywords = body.keywords;
                pkg.VersionName = body.version;
                pkg.EngineVersion = body.engine_version;
                pkg.Contents = body.contents;
                pkg.SiteUrl = body.site_url;
                pkg.RepositoryUrl = body.repository_url;
                body.dependencies.ToList().ForEach(pkg.Dependencies.Add);
                pkg.Header = body;

                return pkg;
            }
            catch (Exception e)
            {
                logger.Log("Failed to form package from json header.");
                logger.Log(e.GetType() + ": " + e.Message);
                return null;
            }

        }

        /// <summary>
        /// Load the Package into Dynamo.  
        /// </summary>
        /// <param name="loader"></param>
        /// <param name="logger"></param>
        /// <param name="libraryServices"></param>
        public void LoadIntoDynamo( DynamoLoader loader, ILogger logger, LibraryServices libraryServices)
        {
            // Prevent duplicate loads
            if (Loaded) return;

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

        public IEnumerable<string> EnumerateAssemblyFilesInBinDirectory()
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
            var assemblies = LoadAssembliesInBinDirectory();

            // filter the assemblies
            var zeroTouchAssemblies = new List<Assembly>();
            var nodeModelAssemblies = new List<Assembly>();

            // categorize the assemblies to load, skipping the ones that are not identified as node libraries
            foreach (var assem in assemblies.Where(x => x.IsNodeLibrary).Select(x => x.Assembly))
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

        /// <summary>
        ///     Add assemblies at runtime to the package.  Does not load the assembly into the node library.
        ///     If the package is already present in LoadedAssemblies, this will mutate it's IsNodeLibrary property.
        /// </summary>
        /// <param name="assems">A list of assemblies</param>
        internal void AddAssemblies(IEnumerable<PackageAssembly> assems)
        {
            foreach (var assem in assems)
            {
                var existingAssem = LoadedAssemblies.FirstOrDefault(x => x.Assembly.FullName == assem.Assembly.FullName);
                if (existingAssem != null)
                {
                    existingAssem.IsNodeLibrary = assem.IsNodeLibrary;
                }
                else
                {
                    this.LoadedAssemblies.Add(assem);
                }
            }
        }

        /// <summary>
        /// Loads all possible assemblies in node library and returns the list of loaded node library assemblies
        /// </summary>
        /// <returns>The list of all node library assemblies</returns>
        private IEnumerable<PackageAssembly> LoadAssembliesInBinDirectory()
        {
            var assemblies = new List<PackageAssembly>();

            if (!Directory.Exists(BinaryDirectory))
                return assemblies;

            // use the pkg header to determine which assemblies to load
            var nodeLibraries = this.Header.node_libraries;

            foreach (var assemFile in (new DirectoryInfo(BinaryDirectory)).EnumerateFiles("*.dll"))
            {
                Assembly assem;

                // dll files may be un-managed, skip those
                var result = PackageLoader.TryLoadFrom(assemFile.FullName, out assem);
                if (result)
                {
                    assemblies.Add(new PackageAssembly()
                    {
                        Assembly = assem,
                        IsNodeLibrary = (nodeLibraries == null || nodeLibraries.Contains(assem.FullName))
                    });
                }
            }

            foreach (var assem in assemblies)
            {
                this.LoadedAssemblies.Add( assem );
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
