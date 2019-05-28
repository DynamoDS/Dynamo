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
                (currentWorkspace as WorkspaceModel).CollectingCustomNodePackageDependencies -= GetCustomNodesPackagesFromGuids;
                (currentWorkspace as WorkspaceModel).CollectingNodePackageDependencies -= GetNodesPackagesFromAssemblyNames;
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
                    (currentWorkspace as WorkspaceModel).CollectingCustomNodePackageDependencies -= GetCustomNodesPackagesFromGuids;
                    (currentWorkspace as WorkspaceModel).CollectingNodePackageDependencies -= GetNodesPackagesFromAssemblyNames;
                }
                
                (ws as WorkspaceModel).CollectingCustomNodePackageDependencies += GetCustomNodesPackagesFromGuids;
                (ws as WorkspaceModel).CollectingNodePackageDependencies += GetNodesPackagesFromAssemblyNames;
                currentWorkspace = ws;
            }
        }

        private IEnumerable<PackageInfo> GetNodesPackagesFromAssemblyNames(IEnumerable<AssemblyName> assemblyNames)
        {
            // Create a dictionary that maps assembly names to the package they are contained in
            var assemblyPackageDict = new Dictionary<string, PackageInfo>();
            foreach (var package in PackageLoader.LocalPackages)
            {
                foreach (var assemblyName in package.LoadedAssemblies.Select(a => AssemblyName.GetAssemblyName(a.Assembly.Location)))
                {
                    assemblyPackageDict[assemblyName.FullName] = new PackageInfo(package.Name, package.VersionName);
                }
            }

            // Create a list of packages
            var packageDependencies = new HashSet<PackageInfo>();
            foreach (var assemblyName in assemblyNames)
            {
                if (assemblyPackageDict.ContainsKey(assemblyName.FullName))
                {
                    packageDependencies.Add(assemblyPackageDict[assemblyName.FullName]);
                }
            }
            return packageDependencies;
        }

        private IEnumerable<PackageInfo> GetCustomNodesPackagesFromGuids(IEnumerable<Guid> functionIDs)
        {
            // Create dictionary mapping Guids to packages
            var guidPackageDictionary = new Dictionary<Guid, PackageInfo>();
            {
                foreach(var p in PackageLoader.LocalPackages)
                {
                    foreach(var cn in p.LoadedCustomNodes)
                    {
                        var pInfo = new PackageInfo(p.Name, p.VersionName);
                        guidPackageDictionary[cn.FunctionId] = pInfo;
                    }
                }
            }

            // Create set of packages containing the custom nodes with the given function IDs
            var customNodePackageDependencies = new HashSet<PackageInfo>();
            foreach(var fID in functionIDs)
            {
                if (guidPackageDictionary.ContainsKey(fID))
                {
                    customNodePackageDependencies.Add(guidPackageDictionary[fID]);
                }
            }
            return customNodePackageDependencies;
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
