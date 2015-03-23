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
using Dynamo.Properties;
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

    public class Package : NotificationObject, ILogSource
    {
        #region Properties/Fields

        public string Name { get; set; }

        public string CustomNodeDirectory
        {
            get { return Path.Combine(RootDirectory, "dyf"); }
        }

        public string BinaryDirectory
        {
            get { return Path.Combine(RootDirectory, "bin"); }
        }

        public string ExtraDirectory
        {
            get { return Path.Combine(RootDirectory, "extra"); }
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
                EnumerateAdditionalFiles();
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
            get { return LoadedAssemblies.Any(); }
        }

        /// <summary>
        ///     List the LoadedAssemblies whose IsNodeLibrary attribute is true
        /// </summary>
        internal IEnumerable<Assembly> NodeLibraries
        {
            get { return LoadedAssemblies.Where(x => x.IsNodeLibrary).Select(x => x.Assembly); }
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
            RootDirectory = directory;
            Name = name;
            License = license;
            VersionName = versionName;
            LoadedTypes = new ObservableCollection<Type>();
            LoadedAssemblies = new ObservableCollection<PackageAssembly>();
            Dependencies = new ObservableCollection<PackageDependency>();
            LoadedCustomNodes = new ObservableCollection<CustomNodeInfo>();
            AdditionalFiles = new ObservableCollection<PackageFileInfo>();
            Header = PackageUploadBuilder.NewPackageHeader(this);
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
                    throw new Exception("The header is missing a name or version field.");

                var pkg = new Package(
                    Path.GetDirectoryName(headerPath),
                    body.name,
                    body.version,
                    body.license)
                {
                    Group = body.@group,
                    Description = body.description,
                    Keywords = body.keywords,
                    VersionName = body.version,
                    EngineVersion = body.engine_version,
                    Contents = body.contents,
                    SiteUrl = body.site_url,
                    RepositoryUrl = body.repository_url,
                    Header = body
                };
                
                foreach (var dep in body.dependencies)
                    pkg.Dependencies.Add(dep);

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
        /// <param name="loadPackageParams"></param>
        /// <param name="logger"></param>
        public void LoadIntoDynamo(LoadPackageParams loadPackageParams, ILogger logger)
        {
            // Prevent duplicate loads
            if (Loaded) return;

            try
            {
                LoadAssembliesIntoDynamo(loadPackageParams);
                LoadCustomNodesIntoDynamo(loadPackageParams);
                EnumerateAdditionalFiles();
                Loaded = true;
            }
            catch (Exception e)
            {
                Log("Exception when attempting to load package " + Name + " from " + RootDirectory);
                Log(e.GetType() + ": " + e.Message);
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
                .Select(x => new PackageFileInfo(RootDirectory, x));

            AdditionalFiles.Clear();
            AdditionalFiles.AddRange(nonDyfDllFiles);
        }

        public IEnumerable<string> EnumerateAssemblyFilesInBinDirectory()
        {
            if (String.IsNullOrEmpty(RootDirectory) || !Directory.Exists(RootDirectory)) 
                return new List<string>();

            return Directory.EnumerateFiles(RootDirectory, "*.dll", SearchOption.AllDirectories);
        }

        private void LoadCustomNodesIntoDynamo(LoadPackageParams loadPackageParams)
        {
            var loader = loadPackageParams.CustomNodeManager;
            var isTestMode = loadPackageParams.IsTestMode;
            foreach (var info in loader.AddUninitializedCustomNodesInPath(CustomNodeDirectory, isTestMode))
                LoadedCustomNodes.Add(info);
        }

        private void LoadAssembliesIntoDynamo(LoadPackageParams loadPackageParams)
        {
            var loader = loadPackageParams.Loader;
            var assemblies = LoadAssembliesInBinDirectory();

            // filter the assemblies
            var zeroTouchAssemblies = new List<Assembly>();
            var nodeModelAssemblies = new List<Assembly>();

            // categorize the assemblies to load, skipping the ones that are not identified as node libraries
            foreach (var assem in assemblies.Where(x => x.IsNodeLibrary).Select(x => x.Assembly))
            {
                if (loader.ContainsNodeModelSubType(assem))
                    nodeModelAssemblies.Add(assem);
                else
                    zeroTouchAssemblies.Add(assem);
            }

            // load the zero touch assemblies
            var libraryServices = loadPackageParams.LibraryServices;
            foreach (var zeroTouchAssem in zeroTouchAssemblies)
                libraryServices.ImportLibrary(zeroTouchAssem.Location);

            // load the node model assemblies
            var context = loadPackageParams.Context;
            var nodes = nodeModelAssemblies.SelectMany(
                assem =>
                {
                    var assemblyNodes = new List<TypeLoadData>();
                    loader.LoadNodesFromAssembly(assem, context, assemblyNodes, new List<TypeLoadData>());
                    return assemblyNodes;
                });

            foreach (var node in nodes)
                LoadedTypes.Add(node.Type);
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
                var existingAssem = LoadedAssemblies.FirstOrDefault(x => x.Assembly.GetName().Name == assem.Assembly.GetName().Name);
                if (existingAssem != null)
                {
                    existingAssem.IsNodeLibrary = assem.IsNodeLibrary;
                }
                else
                {
                    LoadedAssemblies.Add(assem);
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

            // Use the pkg header to determine which assemblies to load and prevent multiple enumeration
            // In earlier packages, this field could be null, which is correctly handled by IsNodeLibrary
            var nodeLibraries = Header.node_libraries;
            
            foreach (var assemFile in (new DirectoryInfo(BinaryDirectory)).EnumerateFiles("*.dll"))
            {
                Assembly assem;

                // dll files may be un-managed, skip those
                var result = PackageLoader.TryLoadFrom(assemFile.FullName, out assem);
                if (result)
                {
                    // IsNodeLibrary may fail, we store the warnings here and then show
                    IList<ILogMessage> warnings = new List<ILogMessage>();

                    assemblies.Add(new PackageAssembly()
                    {
                        Assembly = assem,
                        IsNodeLibrary = IsNodeLibrary(nodeLibraries, assem.GetName(), ref warnings)
                    });

                    warnings.ToList().ForEach(this.Log);
                }
            }

            foreach (var assem in assemblies)
            {
                LoadedAssemblies.Add( assem );
            }

            return assemblies;
        }

        /// <summary>
        ///     Determine if an assembly is in the "node_libraries" list for the package.
        /// 
        ///     This algorithm accepts assemblies that don't have the same version, but the same name.
        ///     This is important when a package author has updated a dll in their package.  
        /// 
        ///     This algorithm assumes all of the entries in nodeLibraryFullNames are properly formatted
        ///     as returned by the Assembly.FullName property.  If they are not, it ignores the entry.
        /// </summary>
        internal static bool IsNodeLibrary(IEnumerable<string> nodeLibraryFullNames, AssemblyName name, ref IList<ILogMessage> messages)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            if (nodeLibraryFullNames == null)
            {
                return true;
            }

            foreach (var n in nodeLibraryFullNames)
            {
                try
                {
                    // The AssemblyName constructor throws an exception for an improperly formatted string
                    if (new AssemblyName(n).Name == name.Name)
                    {
                        return true;
                    }
                }
                catch (Exception _)
                {
                    if (messages != null)
                    {
                        messages.Add(LogMessage.Warning(Resources.IncorrectlyFormattedNodeLibraryWarning, WarningLevel.Mild));
                        messages.Add(LogMessage.Warning(String.Format(Resources.IncorrectlyFormattedNodeLibraryDisplay + " {0}", n), WarningLevel.Mild));
                    }
                }
            }
            return false;
        }

        internal bool ContainsFile(string path)
        {
            if (String.IsNullOrEmpty(RootDirectory) || !Directory.Exists(RootDirectory)) return false;
            return Directory.EnumerateFiles(RootDirectory, "*", SearchOption.AllDirectories).Any(s => s == path);
        }

        internal bool InUse(DynamoModel dynamoModel)
        {
            return (LoadedAssemblies.Any() || IsWorkspaceFromPackageOpen(dynamoModel) || IsCustomNodeFromPackageInUse(dynamoModel)) && Loaded;
        }

        private bool IsCustomNodeFromPackageInUse(DynamoModel dynamoModel)
        {
            // get all of the function ids from the custom nodes in this package
            var guids = LoadedCustomNodes.Select(x => x.FunctionId);

            // check if any of the custom nodes is in a workspace
            return
                dynamoModel.Workspaces.SelectMany(ws => ws.Nodes.OfType<Function>())
                    .Any(x => guids.Contains(x.Definition.FunctionId));

        }

        private bool IsWorkspaceFromPackageOpen(DynamoModel dynamoModel)
        {
            // get all of the function ids from the custom nodes in this package
            var guids = new HashSet<Guid>(LoadedCustomNodes.Select(x => x.FunctionId));

            return
                dynamoModel.Workspaces.OfType<CustomNodeWorkspaceModel>()
                    .Select(x => x.CustomNodeId)
                    .Any(guids.Contains);
        }

        internal void MarkForUninstall(IPreferences prefs)
        {
            MarkedForUninstall = true;

            if (!prefs.PackageDirectoriesToUninstall.Contains(RootDirectory))
            {
                prefs.PackageDirectoriesToUninstall.Add(RootDirectory);
            }
        }

        internal void UnmarkForUninstall(IPreferences prefs)
        {
            MarkedForUninstall = false;
            prefs.PackageDirectoriesToUninstall.RemoveAll(x => x.Equals(RootDirectory));
        }

        internal void UninstallCore(CustomNodeManager customNodeManager, PackageLoader packageLoader, IPreferences prefs)
        {
            if (LoadedAssemblies.Any())
            {
                MarkForUninstall(prefs);
                return;
            }

            try
            {
                LoadedCustomNodes.ToList().ForEach(x => customNodeManager.Remove(x.FunctionId));
                packageLoader.LocalPackages.Remove(this);
                Directory.Delete(RootDirectory, true);
            }
            catch (Exception e)
            {
                Log("Exception when attempting to uninstall the package " + Name + " from " + RootDirectory);
                Log(e.GetType() + ": " + e.Message);
                throw;
            }
        }

        internal void RefreshCustomNodesFromDirectory(CustomNodeManager customNodeManager, bool isTestMode)
        {
            LoadedCustomNodes.Clear();

            foreach (var x in customNodeManager.AddUninitializedCustomNodesInPath(CustomNodeDirectory, isTestMode))
                LoadedCustomNodes.Add(x);
        }

        public event Action<ILogMessage> MessageLogged;

        protected virtual void Log(ILogMessage obj)
        {
            var handler = MessageLogged;
            if (handler != null) handler(obj);
        }

        protected virtual void Log(string s)
        {
            Log(LogMessage.Info(s));
        }
    }
}
