using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamo.Core;
using Dynamo.Interfaces;
using Dynamo.Utilities;
using Greg;

namespace Dynamo.PackageManager
{
    class PackageManagerExtension : Dynamo.Extensions.IExtension
    {
        public string Name { get { return "DynamoPackageManager";  }}

        public string Id
        {
            get { return "FCABC211-D56B-4109-AF18-F434DFE48139"; }
        }

        /// <summary>
        ///     Validate the package manager url and initialize the PackageManagerClient object
        /// </summary>
        /// <param name="provider">A possibly null IAuthProvider</param>
        /// <param name="url">The end point for the package manager server</param>
        /// <param name="rootDirectory">The root directory for the package manager</param>
        /// <param name="customNodeManager">A valid CustomNodeManager object</param>
        /// <returns>Newly created object</returns>
        public PackageManagerExtension(IAuthProvider provider, string url, string rootDirectory, CustomNodeManager customNodeManager)
        {
            url = url ?? AssemblyConfiguration.Instance.GetAppSetting("packageManagerAddress");

            OnMessageLogged(LogMessage.Info("Dynamo will use the package manager server at : " + url));

            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                throw new ArgumentException("Incorrectly formatted URL provided for Package Manager address.", "url");
            }

            this.PackageLoader = new PackageLoader(rootDirectory);
            this.PackageLoader.MessageLogged += OnMessageLogged;

            this.PackageManagerClient = new PackageManagerClient(
                new GregClient(provider, url),
                rootDirectory,
                customNodeManager);
        }

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

        /// <summary>
        ///     Manages loading of packages.
        /// </summary>
        public readonly PackageLoader PackageLoader;

        /// <summary>
        ///     Dynamo Package Manager Instance.
        /// </summary>
        public readonly PackageManagerClient PackageManagerClient;

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public event Action<Interfaces.ILogMessage> MessageLogged;

        public void OnMessageLogged(ILogMessage msg)
        {
            if (this.MessageLogged != null)
            {
                this.MessageLogged(msg);
            }
        }


        public event Action<System.Reflection.Assembly> RequestLoadNodeLibrary;

        public event Func<string, IEnumerable<CustomNodeInfo>> RequestLoadCustomNodeDirectory;

    }
}
