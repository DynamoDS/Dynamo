using Dynamo.Core;
using Dynamo.DSEngine;
using Dynamo.Extensions;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.Utilities;
using Greg;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dynamo.PackageManager
{
    public class PackageManagerExtension : IExtension, ILogSource
    {
        #region Fields & Properties

        private PackageLoader packageLoader;
        private PackageManagerClient packageManagerClient;

        public event Action<ILogMessage> MessageLogged;
        public event Action<Assembly> RequestLoadNodeLibrary;

        public string Name { get { return "DynamoPackageManager"; } }

        public string UniqueId
        {
            get { return "FCABC211-D56B-4109-AF18-F434DFE48139"; }
        }

        /// <summary>
        ///     Manages loading of packages.
        /// </summary>
        public PackageLoader PackageLoader
        {
            get { return packageLoader; }
        }

        /// <summary>
        ///     Dynamo Package Manager Instance.
        /// </summary>
        public PackageManagerClient PackageManagerClient
        {
            get { return packageManagerClient; }
        }

        #endregion

        #region IExtension members

        public void Load(IPreferences preferences, IPathManager pathManager)
        {
            // Load Packages
            PackageLoader.DoCachedPackageUninstalls(preferences);
            PackageLoader.LoadAll(new LoadPackageParams
            {
                Preferences = preferences,
                PathManager = pathManager
            });
        }

        public void Dispose()
        {
            this.packageLoader.MessageLogged -= OnMessageLogged;
            this.packageLoader.RequestLoadNodeLibrary -= OnRequestLoadNodeLibrary;
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

            this.packageLoader = new PackageLoader(startupParams.PathManager.PackagesDirectory);
            this.packageLoader.MessageLogged += OnMessageLogged;
            this.packageLoader.RequestLoadNodeLibrary += OnRequestLoadNodeLibrary;
            this.packageLoader.RequestLoadCustomNodeDirectory +=
                (dir) => startupParams.CustomNodeManager
                    .AddUninitializedCustomNodesInPath(dir, DynamoModel.IsTestMode, true);

            var dirBuilder = new PackageDirectoryBuilder(
                new MutatingFileSystem(),
                new CustomNodePathRemapper(startupParams.CustomNodeManager, DynamoModel.IsTestMode));

            var uploadBuilder = new PackageUploadBuilder(dirBuilder, new MutatingFileCompressor());

            this.packageManagerClient = new PackageManagerClient(new GregClient(startupParams.AuthProvider, url), 
                uploadBuilder, PackageLoader.RootPackagesDirectory);
        }

        public void Ready(ReadyParams sp) { }

        public void Shutdown()
        {
            this.Dispose();
        }

        #endregion

        #region Private helper methods

        private void OnMessageLogged(ILogMessage msg)
        {
            if (this.MessageLogged != null)
            {
                this.MessageLogged(msg);
            }
        }

        private void OnRequestLoadNodeLibrary(Assembly assembly)
        {
            if (RequestLoadNodeLibrary != null)
            {
                RequestLoadNodeLibrary(assembly);
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
