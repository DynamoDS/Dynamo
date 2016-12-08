using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

using Dynamo.Extensions;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.Logging;

using Greg;
using System.Reflection;

namespace Dynamo.PackageManager
{
    public class PackageManagerExtension : IExtension, ILogSource
    {
        #region Fields & Properties

        private Action<Assembly> RequestLoadNodeLibraryHandler;
        private Func<string, IEnumerable<CustomNodeInfo>> RequestLoadCustomNodeDirectoryHandler; 

        public event Action<ILogMessage> MessageLogged;

        public string Name { get { return "DynamoPackageManager"; } }

        public string UniqueId
        {
            get { return "FCABC211-D56B-4109-AF18-F434DFE48139"; }
        }

        /// <summary>
        ///     Manages loading of packages (property meant solely for tests)
        /// </summary>
        public PackageLoader PackageLoader { get; private set; }

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

        public void Ready(ReadyParams sp) { }

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
