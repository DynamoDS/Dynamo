using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using Dynamo.Core;
using Dynamo.Exceptions;
using Dynamo.Graph.Nodes.CustomNodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Interfaces;
using Dynamo.Logging;
using Dynamo.Models;
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

    /// <summary>
    /// Describes a package's load state
    /// </summary>
    public class PackageLoadState
    {
        /// <summary>
        /// The current load state of a package
        /// </summary>
        public enum StateTypes
        {
            /// <summary>
            /// Invalid state. The initial state for every package, before it is interpreted by Dynamo
            /// </summary>
            None,
            /// <summary>
            /// The package is fully loaded and is ready to be used
            /// </summary>
            Loaded,
            /// <summary>
            /// The package is not loaded in Dynamo and not deleted from package locations
            /// </summary>
            Unloaded,
            /// <summary>
            /// The package was not loaded in Dynamo because of an error. See the Error property for more information
            /// </summary>
            Error
        }

        /// <summary>
        /// The scheduled load state of a package
        /// </summary>
        public enum ScheduledTypes
        {
            /// <summary>
            /// Invalid scheduled state. The initial state for every package, before it is interpreted by Dynamo
            /// </summary>
            None,
            /// <summary>
            /// The package is scheduled to be unloaded. After the next Dynamo restart, the package will be in an Unloaded state
            /// </summary>
            ScheduledForUnload,
            /// <summary>
            /// The package is scheduled to be deleted. After the next Dynamo restart, the package will deleted from the package locations
            /// </summary>
            ScheduledForDeletion
        }

        private string errorMessage;
        private ScheduledTypes scheduledState = ScheduledTypes.None;
        private StateTypes state = StateTypes.None;// Default to None type.

        internal void SetAsLoaded() { state = StateTypes.Loaded; errorMessage = ""; }
        internal void SetAsError(string msg = "") { state = StateTypes.Error; errorMessage = msg; }
        internal void SetAsUnloaded() { state = StateTypes.Unloaded; errorMessage = ""; }
        internal void ResetState() { state = StateTypes.None; }

        internal void SetScheduledForDeletion() { scheduledState = ScheduledTypes.ScheduledForDeletion; }
        internal void SetScheduledForUnload() { scheduledState = ScheduledTypes.ScheduledForUnload; }
        internal void ResetScheduledState() { scheduledState = ScheduledTypes.None; }

        // The current load state of the Package.
        public StateTypes State { get { return state; } }

        // The scheduled (or desired) state of the Package.
        public ScheduledTypes ScheduledState { get { return scheduledState; } }
        
        // The error message associated with the current State of the Package. Valid only if the State is of type StateTypes.Error
        public string ErrorMessage { get { return errorMessage; } }
    }

    public class Package : NotificationObject, ILogSource
    {
        #region Properties/Fields

        public string Name { get; set; }

        public string ID { get; set; }

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

        /// <summary>
        /// Directory path to where node documentation markdown files should be placed.
        /// </summary>
        public string NodeDocumentaionDirectory
        {
            get { return Path.Combine(RootDirectory, "doc"); }
        }

        [Obsolete("This property will be removed in 3.0. Please use the LoadState property instead.")]
        public bool Loaded {
            get {
                return LoadState.State == PackageLoadState.StateTypes.Loaded;
            }
        }

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

        private IEnumerable<string> hostDependencies = new List<string>();
        /// <summary>
        /// Package Host Dependencies, e.g. specifying "Revit" in the list means this package can be guaranteed to work in this host environment only
        /// </summary>
        public IEnumerable<string> HostDependencies { get { return hostDependencies; } set { hostDependencies = value; RaisePropertyChanged("HostDependencies"); } }

        private string copyrightHolder = "";
        public string CopyrightHolder { get { return copyrightHolder; } set { copyrightHolder = value; RaisePropertyChanged("CopyrightHolder"); } }

        private string copyrightYear = "";
        public string CopyrightYear { get { return copyrightYear; } set { copyrightYear = value; RaisePropertyChanged("CopyrightYear"); } }

        internal bool BuiltInPackage
        {
            get { return RootDirectory.StartsWith(PathManager.BuiltinPackagesDirectory); }
        }

        [Obsolete("This property will be removed in Dynamo 3.0. Use LoadState.ScheduledState instead")]
        public bool MarkedForUninstall
        {
            get {
                return BuiltInPackage ? LoadState.ScheduledState == PackageLoadState.ScheduledTypes.ScheduledForUnload :
                  LoadState.ScheduledState == PackageLoadState.ScheduledTypes.ScheduledForDeletion;
            }
        }

        public PackageLoadState LoadState = new PackageLoadState();

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

        /// <summary>
        /// Is set to true if the Package is located in a directory that requires certificate verification of its node library dlls.
        /// </summary>
        internal bool RequiresSignedEntryPoints { get; set; }

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
            Header = PackageUploadBuilder.NewRequestBody(this);
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
                    HostDependencies = body.host_dependencies,
                    CopyrightHolder = body.copyright_holder,
                    CopyrightYear = body.copyright_year,
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

        public void EnumerateAdditionalFiles()
        {
            if (String.IsNullOrEmpty(RootDirectory) || !Directory.Exists(RootDirectory)) return;

            var backupFolderName = @"\" + Configuration.Configurations.BackupFolderName + @"\";

            var nonDyfDllFiles = Directory.EnumerateFiles(
                RootDirectory,
                "*",
                SearchOption.AllDirectories)
                .Where(x => !x.ToLower().EndsWith(".dyf") && !x.ToLower().EndsWith(".dll") &&
                    !x.ToLower().EndsWith("pkg.json") && !x.ToLower().EndsWith(".backup") &&
                    !x.ToLower().Contains(backupFolderName))
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
        ///     Enumerates all assemblies in the package. This method currently has the side effect that it will
        ///     load all binaries in the package bin folder unless the package is loaded from a special package path
        ///     I.E. Builtin Packages.
        /// </summary>
        /// <returns>The list of all node library assemblies</returns>
        internal IEnumerable<PackageAssembly> EnumerateAndLoadAssembliesInBinDirectory()
        {
            var assemblies = new List<PackageAssembly>();

            if (!Directory.Exists(BinaryDirectory))
                return assemblies;

            // Use the pkg header to determine which assemblies to load and prevent multiple enumeration
            // In earlier packages, this field could be null, which is correctly handled by IsNodeLibrary
            var nodeLibraries = Header.node_libraries;
            
            foreach (var assemFile in new DirectoryInfo(BinaryDirectory).EnumerateFiles("*.dll"))
            {
                Assembly assem;
                //TODO when can we make this false. 3.0?
                bool shouldLoadFile = true;
                if (this.RequiresSignedEntryPoints)
                {
                    shouldLoadFile = IsFileSpecifiedInPackageJsonManifest(nodeLibraries, assemFile.Name, BinaryDirectory);
                }

                if (shouldLoadFile)
                {
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
            }

            foreach (var assem in assemblies)
            {
                LoadedAssemblies.Add( assem );
            }

            return assemblies;
        }

        /// <summary>
        /// Checks if a specific file is specified in the Node Libraries section of the package manifest json.
        /// </summary>
        /// <param name="nodeLibraries">node libraries defined in package manifest json.</param>
        /// <param name="filename">filename of dll file to check</param>
        /// <param name="path">path of the packages</param>
        /// <returns></returns>
        private static bool IsFileSpecifiedInPackageJsonManifest(IEnumerable<string> nodeLibraries, string filename, string path)
        {
            foreach (var nodeLibraryAssembly in nodeLibraries)
            {
                try
                {
                    var name = new AssemblyName(nodeLibraryAssembly).Name + ".dll";
                    if (name == filename)
                    {
                        return true;
                    }
                }
                catch
                {
                    throw new LibraryLoadFailedException(path, Resources.IncorrectlyFormattedNodeLibraryWarning);
                }
            }

            return false;
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
            //TODO I'm guessing this was added for legacy or handbuilt packages - all assemblies are treated as node libraries
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
                catch (Exception)
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

        // Checks if the package is used in the Dynamo model.
        // The check does not take into account the package load state.
        internal bool InUse(DynamoModel dynamoModel)
        {
            return (LoadedAssemblies.Any() || IsWorkspaceFromPackageOpen(dynamoModel) || 
                IsCustomNodeFromPackageInUse(dynamoModel));
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

        /// <summary>
        /// Marks built-in package for unload.
        /// Any other custom package will be marked for deletion.
        /// </summary>
        /// <param name="prefs"></param>
        internal void MarkForUninstall(IPreferences prefs)
        {
            if (BuiltInPackage) 
            {
                Analytics.TrackEvent(Actions.BuiltInPackageConflict, Categories.PackageManagerOperations, $"{Name } {versionName} marked to be unloaded");
                LoadState.SetScheduledForUnload();
            } 
            else
            {
                LoadState.SetScheduledForDeletion();
            }

            if (!prefs.PackageDirectoriesToUninstall.Contains(RootDirectory))
            {
                prefs.PackageDirectoriesToUninstall.Add(RootDirectory);
            }
            RaisePropertyChanged(nameof(LoadState));
        }

        /// <summary>
        /// Resets scheduled state to 'None' for given package.
        /// Custom package will no longer be uninstalled.
        /// Package load state will remain unaffected.
        /// </summary>
        /// <param name="prefs"></param>
        internal void UnmarkForUninstall(IPreferences prefs)
        {
            LoadState.ResetScheduledState();

            prefs.PackageDirectoriesToUninstall.RemoveAll(x => x.Equals(RootDirectory));
            RaisePropertyChanged(nameof(LoadState));
        }

        /// <summary>
        /// Marks any given package for unload.
        /// The package will not be marked for deletion.
        /// </summary>
        internal void MarkForUnload()
        {
            LoadState.SetScheduledForUnload();
            RaisePropertyChanged(nameof(LoadState));
        }

        /// <summary>
        /// Resets scheduled state to 'None' for given package.
        /// Package will no longer be unloaded.
        /// Package load state will remain unaffected.
        /// </summary>
        internal void UnmarkForUnload()
        {
            LoadState.ResetScheduledState();
            RaisePropertyChanged(nameof(LoadState));
        }

        internal void SetAsLoaded()
        {
            LoadState.SetAsLoaded();
            RaisePropertyChanged(nameof(LoadState));
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
                if (BuiltInPackage)
                {
                    LoadState.SetAsUnloaded();
                    Analytics.TrackEvent(Actions.BuiltInPackageConflict, Categories.PackageManagerOperations, $"{Name } {versionName} set unloaded");

                    RaisePropertyChanged(nameof(LoadState));

                    if (!prefs.PackageDirectoriesToUninstall.Contains(RootDirectory))
                    {
                        prefs.PackageDirectoriesToUninstall.Add(RootDirectory);
                    }
                }
                else
                {
                    packageLoader.Remove(this);
                    Directory.Delete(RootDirectory, true);
                }
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

            var reloadedCustomNodes = customNodeManager.AddUninitializedCustomNodesInPath(
                CustomNodeDirectory,
                isTestMode,
                new PackageInfo(Name, new Version(versionName)));

            foreach (var x in reloadedCustomNodes)
            {
                LoadedCustomNodes.Add(x);
            }
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
