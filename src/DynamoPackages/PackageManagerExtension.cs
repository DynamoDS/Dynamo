using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using Dynamo.Extensions;
using Dynamo.Graph.Workspaces;
using Dynamo.Interfaces;
using Dynamo.Logging;
using Dynamo.Models;
using Greg;

namespace Dynamo.PackageManager
{
    public class PackageManagerExtension : IExtension, ILogSource, IExtensionSource
    {
        #region Fields & Properties

        private Action<Assembly> RequestLoadNodeLibraryHandler;
        private event Func<string, IEnumerable<CustomNodeInfo>> RequestLoadCustomNodeDirectoryHandler;
       
        public event Func<string, IExtension> RequestLoadExtension;
        public event Action<IExtension> RequestAddExtension;

        public event Action<ILogMessage> MessageLogged;

        private IWorkspaceModel currentWorkspace;

        /// <summary>
        /// Dictionary mapping a custom node functionID to the package that contains it.
        /// Used for package dependency serialization.
        /// </summary>
        private Dictionary<Guid, PackageDependencyInfo> CustomNodePackageDictionary;

        /// <summary>
        /// Dictionary mapping the AssemblyName.FullName of an assembly to the package that contains it.
        /// Used for package dependency serialization.
        /// </summary>
        private Dictionary<string, PackageDependencyInfo> NodePackageDictionary;

        public string Name { get { return "DynamoPackageManager"; } }

        public string UniqueId
        {
            get { return "FCABC211-D56B-4109-AF18-F434DFE48139"; }
        }

        /// <summary>
        ///     Manages loading of packages (property meant solely for tests)
        /// </summary>
        public PackageLoader PackageLoader { get; private set; }

        public IEnumerable<IExtension> RequestedExtensions => this.PackageLoader.RequestedExtensions;

        /// <summary>
        ///     Dynamo Package Manager Instance.
        /// </summary>
        public PackageManagerClient PackageManagerClient { get; private set; }

        #endregion

        #region IExtension members

        public void Dispose()
        {
            PackageLoader.MessageLogged -= OnMessageLogged;
            PackageLoader.PackgeLoaded -= OnPackageLoaded;
            PackageLoader.PackageRemoved -= OnPackageRemoved;

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
        }

        /// <summary>
        ///     Validate the package manager url and initialize the PackageManagerClient object
        /// </summary>
        public void Startup(StartupParams startupParams)
        {
            var path = this.GetType().Assembly.Location;
            var config = ConfigurationManager.OpenExeConfiguration(path);
            var key = config.AppSettings.Settings["packageManagerAddress"];
            string url = null;
            if (key != null)
            {
                url = key.Value;
            }

            OnMessageLogged(LogMessage.Info("Dynamo will use the package manager server at : " + url));

            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                throw new ArgumentException("Incorrectly formatted URL provided for Package Manager address.", "url");
            }

            PackageLoader = new PackageLoader(startupParams.PathManager.PackagesDirectories);
            PackageLoader.MessageLogged += OnMessageLogged;
            PackageLoader.PackgeLoaded += OnPackageLoaded;
            PackageLoader.PackageRemoved += OnPackageRemoved;
            RequestLoadNodeLibraryHandler = startupParams.LibraryLoader.LoadNodeLibrary;
            RequestLoadCustomNodeDirectoryHandler = (dir) => startupParams.CustomNodeManager
                    .AddUninitializedCustomNodesInPath(dir, DynamoModel.IsTestMode, true);

            //raise the public events on this extension when the package loader requests.
            PackageLoader.RequestLoadExtension += RequestLoadExtension;
            PackageLoader.RequestAddExtension += RequestAddExtension;
            
            PackageLoader.RequestLoadNodeLibrary += RequestLoadNodeLibraryHandler;
            PackageLoader.RequestLoadCustomNodeDirectory += RequestLoadCustomNodeDirectoryHandler;
                
            var dirBuilder = new PackageDirectoryBuilder(
                new MutatingFileSystem(),
                new CustomNodePathRemapper(startupParams.CustomNodeManager, DynamoModel.IsTestMode));

            PackageUploadBuilder.SetEngineVersion(startupParams.DynamoVersion);
            var uploadBuilder = new PackageUploadBuilder(dirBuilder, new MutatingFileCompressor());

            PackageManagerClient = new PackageManagerClient(
                new GregClient(startupParams.AuthProvider, url),
                uploadBuilder, PackageLoader.DefaultPackagesDirectory);

            LoadPackages(startupParams.Preferences, startupParams.PathManager);
        }

        public void Ready(ReadyParams sp)
        {
            sp.CurrentWorkspaceChanged += OnCurrentWorkspaceChanged;
        }

        public void Shutdown()
        {
            this.Dispose();
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
                PathManager = pathManager
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

        private PackageDependencyInfo GetNodePackageFromAssemblyName(AssemblyName assemblyName)
        {
            if (NodePackageDictionary.ContainsKey(assemblyName.FullName))
            {
                return NodePackageDictionary[assemblyName.FullName];
            }
            return null;
        }

        private PackageDependencyInfo GetCustomNodePackageFromID(Guid functionID)
        {
            if (CustomNodePackageDictionary.ContainsKey(functionID))
            {
                return CustomNodePackageDictionary[functionID];
            }
            return null;
        }

        private void OnPackageLoaded(Package package)
        {
            // Create NodePackageDictionary if it doesn't exist
            if (NodePackageDictionary == null)
            {
                NodePackageDictionary = new Dictionary<string, PackageDependencyInfo>();
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
                NodePackageDictionary[assembly.FullName] = new PackageDependencyInfo(package.Name, new Version(package.VersionName));
            }

            // Create CustomNodePackageDictionary if it doesn't exist
            if (CustomNodePackageDictionary == null)
            {
                CustomNodePackageDictionary = new Dictionary<Guid, PackageDependencyInfo>();
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
                var pInfo = new PackageDependencyInfo(package.Name, new Version(package.VersionName));
                CustomNodePackageDictionary[cn.FunctionId] = pInfo;
            }
        }

        private void OnPackageRemoved(Package package)
        {
            var pInfo = new PackageDependencyInfo(package.Name, new Version(package.VersionName));
            // Remove package references from NodePackageDictionary
            foreach (var key in NodePackageDictionary.Keys)
            {
                if (NodePackageDictionary[key].FullName == pInfo.FullName)
                {
                    NodePackageDictionary.Remove(key);
                }
            }
            // Remove package references from CustomNodePackageDictionary
            foreach (var key in CustomNodePackageDictionary.Keys)
            {
                if (CustomNodePackageDictionary[key].FullName == pInfo.FullName)
                {
                    CustomNodePackageDictionary.Remove(key);
                }
            }
        }

        #endregion
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
