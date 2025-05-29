using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using Dynamo.Extensions;
using Dynamo.Graph.Workspaces;
using Dynamo.Interfaces;
using Dynamo.Logging;
using Dynamo.Models;
using Dynamo.Session;
using Greg;
using Greg.Responses;

namespace Dynamo.PackageManager
{
    public class PackageManagerExtension : IExtension, ILogSource, IExtensionSource 
    {
        #region Fields & Properties

        private Action<Assembly> RequestLoadNodeLibraryHandler;
        //TODO should we add a new handler specifically for packages? this is the package manager afterall so maybe not.
        private event Func<string, PackageInfo, IEnumerable<CustomNodeInfo>> RequestLoadCustomNodeDirectoryHandler;
        private Action<IEnumerable<Assembly>> LoadPackagesHandler;
       
        public event Func<string, IExtension> RequestLoadExtension;
        public event Action<IExtension> RequestAddExtension;
        public event Action<ILogMessage> MessageLogged;

        private IWorkspaceModel currentWorkspace;

        private ReadyParams ReadyParams;
        private Core.CustomNodeManager customNodeManager;

        /// <summary>
        /// Dictionary mapping a custom node functionID to the package that contains it.
        /// Used for package dependency serialization.
        /// </summary>
        private Dictionary<Guid, List<PackageInfo>> CustomNodePackageDictionary;

        /// <summary>
        /// Dictionary mapping the AssemblyName.FullName of an assembly to the package that contains it.
        /// Used for package dependency serialization.
        /// </summary>
        private Dictionary<string, List<PackageInfo>> NodePackageDictionary;

        private bool noNetworkMode;

        public string Name { get { return "DynamoPackageManager"; } }

        public string UniqueId
        {
            get { return "FCABC211-D56B-4109-AF18-F434DFE48139"; }
        }

        // Current host, empty if sandbox, null when running tests
        internal virtual string Host => DynamoModel.HostAnalyticsInfo.HostProductName;

        /// <summary>
        ///     Manages loading of packages (property meant solely for tests)
        /// </summary>
        public PackageLoader PackageLoader { get; private set; }

        public IEnumerable<IExtension> RequestedExtensions => this.PackageLoader.RequestedExtensions;

        /// <summary>
        ///     Dynamo Package Manager Instance.
        /// </summary>
        public virtual PackageManagerClient PackageManagerClient { get; private set; }


        #endregion

        #region IExtension members

        public void Dispose()
        {
            PackageLoader.MessageLogged -= OnMessageLogged;
            PackageLoader.PackgeLoaded -= OnPackageLoaded;
            PackageLoader.PackageRemoved -= OnPackageRemoved;

            if (LoadPackagesHandler != null)
            {
                PackageLoader.PackagesLoaded -= LoadPackagesHandler;
            }

            if (RequestLoadNodeLibraryHandler != null)
            {
                PackageLoader.RequestLoadNodeLibrary -= RequestLoadNodeLibraryHandler;
            }

            if (RequestLoadCustomNodeDirectoryHandler != null)
            {
                PackageLoader.RequestLoadCustomNodeDirectory -=
                    RequestLoadCustomNodeDirectoryHandler;
            }
            if (RequestLoadExtension != null)
            {
                PackageLoader.RequestLoadExtension -=
                RequestLoadExtension;
            }
            if (RequestAddExtension != null)
            {
                PackageLoader.RequestAddExtension -=
                RequestAddExtension;
            }
            if (currentWorkspace != null)
            {
                (currentWorkspace as WorkspaceModel).CollectingCustomNodePackageDependencies -= GetCustomNodePackageFromID;
                (currentWorkspace as WorkspaceModel).CollectingNodePackageDependencies -= GetNodePackageFromAssemblyName;
            }
            if (customNodeManager != null)
            {
                customNodeManager.RequestCustomNodeOwner -= handleCustomNodeOwnerQuery;
            }
            ReadyParams.CurrentWorkspaceChanged -= OnCurrentWorkspaceChanged;
        }

        /// <summary>
        ///     Validate the package manager url and initialize the PackageManagerClient object
        /// </summary>
        public void Startup(StartupParams startupParams)
        {
            string url = DynamoUtilities.PathHelper.GetServiceBackendAddress(this, "packageManagerAddress");

            OnMessageLogged(LogMessage.Info("Dynamo will use the package manager server at : " + url));

            PackageLoader = new PackageLoader(startupParams.PathManager);
            PackageLoader.MessageLogged += OnMessageLogged;
            PackageLoader.PackgeLoaded += OnPackageLoaded;
            PackageLoader.PackageRemoved += OnPackageRemoved;
            RequestLoadNodeLibraryHandler = (startupParams.LibraryLoader as ExtensionLibraryLoader).LoadLibraryAndSuppressZTSearchImport;
            //TODO: Add LoadPackages to ILibraryLoader interface in 3.0
            LoadPackagesHandler = (startupParams.LibraryLoader as ExtensionLibraryLoader).LoadPackages;
            customNodeManager = (startupParams.CustomNodeManager as Core.CustomNodeManager);

            //TODO - in 3.0 we can add the other overload of AddUninitializedCustomNodesInPath to the ICustomNodeManager interface.
            RequestLoadCustomNodeDirectoryHandler = (dir,pkgInfo) => customNodeManager
                    .AddUninitializedCustomNodesInPath(dir, DynamoModel.IsTestMode, pkgInfo);

            //when the customNodeManager requests to know the owner of a customNode handle this query.
            customNodeManager.RequestCustomNodeOwner += handleCustomNodeOwnerQuery;

            //raise the public events on this extension when the package loader requests.
            PackageLoader.RequestLoadExtension += RequestLoadExtension;
            PackageLoader.RequestAddExtension += RequestAddExtension;
            PackageLoader.PackagesLoaded += LoadPackagesHandler;
            PackageLoader.RequestLoadNodeLibrary += RequestLoadNodeLibraryHandler;
            PackageLoader.RequestLoadCustomNodeDirectory += RequestLoadCustomNodeDirectoryHandler;
            PythonServices.PythonEngineManager.Instance.AvailableEngines.CollectionChanged += PythonEngineAdded;
                
            var dirBuilder = new PackageDirectoryBuilder(
                new MutatingFileSystem(),
                new CustomNodePathRemapper(startupParams.CustomNodeManager, DynamoModel.IsTestMode));

            PackageUploadBuilder.SetEngineVersion(startupParams.DynamoVersion);
            var uploadBuilder = new PackageUploadBuilder(dirBuilder, new MutatingFileCompressor());

            var packageUploadDirectory = startupParams.PathManager.DefaultPackagesDirectory;

            noNetworkMode = startupParams.NoNetworkMode;

            var client = noNetworkMode ? new GregClient(startupParams.AuthProvider, url,
                new HttpClient(new NoNetworkModeHandler())) : new GregClient(startupParams.AuthProvider, url);
            PackageManagerClient = new PackageManagerClient(client, uploadBuilder, packageUploadDirectory, noNetworkMode);


            //we don't ask dpm for the compatibility map in offline mode.
            if (!noNetworkMode)
            {
                // Load the compatibility map
                PackageManagerClient.LoadCompatibilityMap();
            }

            LoadPackages(startupParams.Preferences, startupParams.PathManager);
        }

        /// <summary>
        /// When a new engine is added from a package, add its dependency to the respective package in the dictionary.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void PythonEngineAdded(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                try
                {
                    var assem = e.NewItems[0]?.GetType().Assembly;
                    if (assem == null) return;

                    var assemLoc = assem.Location;
                    foreach (var pkg in PackageLoader.LocalPackages)
                    {
                        if (assemLoc.StartsWith(pkg.RootDirectory))
                        {
                            if (NodePackageDictionary.ContainsKey(assem.FullName))
                            {
                                var assemName = AssemblyName.GetAssemblyName(assem.Location);
                                OnMessageLogged(LogMessage.Info(
                                    string.Format("{0} contains the python engine library {1}, which has already been loaded " +
                                    "by another package. This may cause inconsistent results when determining which " +
                                    "python engine the nodes are dependent on.", pkg.Name, assemName.Name)
                                    ));
                            }
                            else
                            {
                                NodePackageDictionary[assem.FullName] = new List<PackageInfo>();
                            }
                            NodePackageDictionary[assem.FullName].Add(new PackageInfo(pkg.Name, new Version(pkg.VersionName)));
                        }
                    }
                }
                catch(Exception ex)
                {
                    OnMessageLogged(LogMessage.Info("Error occurred while recording python engine and package mapping. " + ex.Message));
                }
            }
        }

        private PackageInfo handleCustomNodeOwnerQuery(Guid customNodeFunctionID)
        {
            return GetCustomNodePackageFromID(customNodeFunctionID);
        }

        public void Ready(ReadyParams sp)
        {
            ReadyParams = sp;
            sp.CurrentWorkspaceChanged += OnCurrentWorkspaceChanged;

            (sp.CurrentWorkspaceModel as WorkspaceModel).CollectingCustomNodePackageDependencies += GetCustomNodePackageFromID;
            (sp.CurrentWorkspaceModel as WorkspaceModel).CollectingNodePackageDependencies += GetNodePackageFromAssemblyName;
            currentWorkspace = (sp.CurrentWorkspaceModel as WorkspaceModel);
        }

        public void Shutdown()
        {
            PythonServices.PythonEngineManager.Instance.AvailableEngines.CollectionChanged -= PythonEngineAdded;
            //this.Dispose();
        }

        #endregion

        #region Private helper methods

        private void LoadPackages(IPreferences preferences, IPathManager pathManager)
        {
            // Load Packages
            PackageLoader.DoCachedPackageUninstalls(preferences);
            PackageLoader.LoadAll(new LoadPackageParams
            {
                Preferences = preferences,
            });
        }

        private void OnMessageLogged(ILogMessage msg)
        {
            if (this.MessageLogged != null)
            {
                this.MessageLogged(msg);
            }
        }
        
        private void OnCurrentWorkspaceChanged(IWorkspaceModel ws)
        {
            if (ws is WorkspaceModel)
            {
                if (currentWorkspace != null)
                {
                    (currentWorkspace as WorkspaceModel).CollectingCustomNodePackageDependencies -= GetCustomNodePackageFromID;
                    (currentWorkspace as WorkspaceModel).CollectingNodePackageDependencies -= GetNodePackageFromAssemblyName;
                }
                
                (ws as WorkspaceModel).CollectingCustomNodePackageDependencies += GetCustomNodePackageFromID;
                (ws as WorkspaceModel).CollectingNodePackageDependencies += GetNodePackageFromAssemblyName;
                currentWorkspace = ws;
            }
        }
        
        private PackageInfo GetNodePackageFromAssemblyName(AssemblyName assemblyName)
        {
            if (NodePackageDictionary?.TryGetValue(assemblyName.FullName, out var packages) == true)
            {
                return packages.Last();
            }

            return null;
        }

        private PackageInfo GetCustomNodePackageFromID(Guid functionID)
        {
            if (CustomNodePackageDictionary != null && CustomNodePackageDictionary.ContainsKey(functionID))
            {
                return CustomNodePackageDictionary[functionID].Last();
            }
            return null;
        }

        private void OnPackageLoaded(Package package)
        {
            // Create NodePackageDictionary if it doesn't exist
            if (NodePackageDictionary == null)
            {
                NodePackageDictionary = new Dictionary<string, List<PackageInfo>>();
            }
            // Add new assemblies to NodePackageDictionary
            var nodeLibraries = package.LoadedAssemblies.Where(a => a.IsNodeLibrary);
            foreach (var assembly in nodeLibraries.Select(a => AssemblyName.GetAssemblyName(a.Assembly.Location)))
            {
                if (NodePackageDictionary.ContainsKey(assembly.FullName))
                {
                    OnMessageLogged(LogMessage.Info(
                        string.Format("{0} contains the node library {1}, which has already been loaded " +
                        "by another package. This may cause inconsistent results when determining which " +
                        "package nodes from this node library are dependent on.", package.Name, assembly.Name)
                        ));
                }
                else
                {
                    NodePackageDictionary[assembly.FullName] = new List<PackageInfo>();
                }
                NodePackageDictionary[assembly.FullName].Add(new PackageInfo(package.Name, new Version(package.VersionName)));
            }

            // Create CustomNodePackageDictionary if it doesn't exist
            if (CustomNodePackageDictionary == null)
            {
                CustomNodePackageDictionary = new Dictionary<Guid, List<PackageInfo>>();
            }
            // Add new custom nodes to CustomNodePackageDictionary
            foreach (var cn in package.LoadedCustomNodes)
            {
                if (CustomNodePackageDictionary.ContainsKey(cn.FunctionId))
                {
                    OnMessageLogged(LogMessage.Info(
                        string.Format("{0} contains the custom node {1}, which has already been loaded " +
                        "by another package. This may cause inconsistent results when determining which " +
                        "package instances of this custom node are dependent on.", package.Name, cn.Name)
                        ));
                }
                else
                {
                    CustomNodePackageDictionary[cn.FunctionId] = new List<PackageInfo>();
                }
                CustomNodePackageDictionary[cn.FunctionId].Add(new PackageInfo(package.Name, new Version(package.VersionName)));
            }
            Dynamo.Logging.Analytics.TrackEvent(
                Actions.Load,
                Categories.PackageManagerOperations,
                package.Name);
        }

        private void OnPackageRemoved(Package package)
        {
            var pInfo = new PackageInfo(package.Name, new Version(package.VersionName));

            // Remove package references from NodePackageDictionary
            var nodeLibraries = package.LoadedAssemblies.Where(a => a.IsNodeLibrary);
            foreach (var assembly in nodeLibraries.Select(a => AssemblyName.GetAssemblyName(a.Assembly.Location)))
            {
                // If multiple packages contain this assembly, only remove the reference to this package
                if (NodePackageDictionary[assembly.FullName].Count > 1)
                {
                    NodePackageDictionary[assembly.FullName].Remove(pInfo);
                }
                // Otherwise just remove the whole dictionary entry
                else
                {
                    NodePackageDictionary.Remove(assembly.FullName);
                }
            }
            // Remove package references from CustomNodePackageDictionary
            foreach (var cn in package.LoadedCustomNodes)
            {
                // If multiple packages contain this custom node, only remove the reference to this package
                if (CustomNodePackageDictionary[cn.FunctionId].Count > 1)
                {
                    CustomNodePackageDictionary[cn.FunctionId].Remove(pInfo);
                }
                // Otherwise just remove the whole dictionary entry
                else
                {
                    CustomNodePackageDictionary.Remove(cn.FunctionId);
                }
            }

            Dynamo.Logging.Analytics.TrackEvent(
                Actions.Delete,
                Categories.PackageManagerOperations,
                package.Name);
        }

        #endregion
        internal bool CheckIfPackagesTargetOtherHosts(IEnumerable<PackageVersion> newPackageHeaders)
        {
            // determine if any of the packages are targeting other hosts
            var containsPackagesThatTargetOtherHosts = false;
            //fallback list of hosts as of 9/8/23
            IEnumerable<string> knownHosts =  new List<string> { "Revit", "Civil 3D", "Alias", "Advance Steel", "FormIt" };

            //we don't ask dpm for known hosts in offline mode.
            if (!noNetworkMode)
            {
                // Known hosts
                knownHosts = PackageManagerClient.GetKnownHosts();
            }

            // Sandbox, special case: Warn if any package targets only one known host
            if (String.IsNullOrEmpty(Host))
            {
                containsPackagesThatTargetOtherHosts =
                    newPackageHeaders.Any(y => y.host_dependencies != null && y.host_dependencies.Intersect(knownHosts).Count() == 1);
            }
            else
            {
                // Warn if there are packages targeting other hosts but not our host
                var otherHosts = knownHosts.Except(new List<string>() { Host });
                containsPackagesThatTargetOtherHosts = newPackageHeaders.Any(x =>
                {
                    // Is our host in the list?
                    // If not, is any other host in the list?
                    // Also, check if any dependency contains the hostname or vice-versa.
                    return x.host_dependencies != null && !x.host_dependencies.Contains(Host) && !x.host_dependencies.Any(y => Host.Contains(y)) && otherHosts.Any(y => x.host_dependencies.Contains(y));
                });
            }

            return containsPackagesThatTargetOtherHosts;
        }


    }
    


    public static class DynamoModelExtensions
    {
        public static PackageManagerExtension GetPackageManagerExtension(this DynamoModel model)
        {
            var extensions = model.ExtensionManager.Extensions.OfType<PackageManagerExtension>();
            if (extensions.Any())
            {
                return extensions.First();
            }

            return null;
        }
    }
}
