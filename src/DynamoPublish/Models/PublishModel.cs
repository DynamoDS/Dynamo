using Dynamo.Core;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Wpf.Authentication;
using Greg;
using Greg.AuthProviders;
using Reach;
using Reach.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Dynamo.Publish.Models
{
    public class PublishModel
    {
        private readonly IAuthProvider authenticationProvider;
        private readonly ICustomNodeManager customNodeManager;

        private readonly string serverUrl;
        private readonly string port;
        private readonly string page;

        private IWorkspaceStorageClient reachClient;

        public bool IsLoggedIn
        {
            get
            {
                return authenticationProvider.LoginState == LoginState.LoggedIn;
            }
        }

        public HomeWorkspaceModel HomeWorkspace
        {
            get;
            private set;
        }

        public List<CustomNodeWorkspaceModel> CustomNodeWorkspaces
        {
            get;
            private set;
        }

        public bool HasAuthProvider
        {
            get
            {
                return authenticationProvider != null;
            }
        }

        #region Initialization

        internal PublishModel(IAuthProvider dynamoAuthenticationProvider, ICustomNodeManager dynamoCustomNodeManager)
        {
            // Open the configuration file using the dll location.
            var config = ConfigurationManager.OpenExeConfiguration(this.GetType().Assembly.Location);
            // Get the appSettings section.
            var appSettings = (AppSettingsSection)config.GetSection("appSettings");

            serverUrl = appSettings.Settings["ServerUrl"].Value;
            if (String.IsNullOrWhiteSpace(serverUrl))
                throw new Exception(Resource.ServerErrorMessage);

            port = appSettings.Settings["Port"].Value;
            if (String.IsNullOrWhiteSpace(port))
                throw new Exception(Resource.PortErrorMessage);

            page = appSettings.Settings["Page"].Value;
            if (String.IsNullOrWhiteSpace(page))
                throw new Exception(Resource.PageErrorMessage);

            authenticationProvider = dynamoAuthenticationProvider;
            customNodeManager = dynamoCustomNodeManager;
        }

        internal PublishModel(IAuthProvider provider, ICustomNodeManager manager, IWorkspaceStorageClient client) :
            this(provider, manager)
        {
            reachClient = client;
        }

        #endregion

        internal void Authenticate()
        {
            // Manager must be initialized in constructor.
            if (authenticationProvider == null)
                throw new Exception(Resource.AuthenticationErrorMessage);

            authenticationProvider.Login();
        }

        /// <summary>
        /// Sends workspace and its' dependencies to Flood.
        /// </summary>
        internal void Send(IEnumerable<IWorkspaceModel> workspaces)
        {
            if (String.IsNullOrWhiteSpace(serverUrl) || String.IsNullOrWhiteSpace(authenticationProvider.Username))
                throw new Exception(Resource.ServerErrorMessage);

            if (authenticationProvider == null)
                throw new Exception(Resource.AuthenticationErrorMessage);

            string fullServerAdress = serverUrl + ":" + port;

            if (reachClient == null)
                reachClient = new WorkspaceStorageClient(authenticationProvider, fullServerAdress);

            HomeWorkspace = workspaces.OfType<HomeWorkspaceModel>().First();
            var functionNodes = HomeWorkspace.Nodes.OfType<Function>();

            List<CustomNodeDefinition> dependencies = new List<CustomNodeDefinition>();
            foreach (var node in functionNodes)
            {
                dependencies.AddRange(node.Definition.Dependencies);
            }

            CustomNodeWorkspaces = new List<CustomNodeWorkspaceModel>();
            foreach (var dependency in dependencies)
            {
                CustomNodeWorkspaceModel customNodeWs;
                var isWorkspaceCreated = customNodeManager.TryGetFunctionWorkspace(dependency.FunctionId, false, out customNodeWs);
                if (isWorkspaceCreated && !CustomNodeWorkspaces.Contains(customNodeWs))
                    CustomNodeWorkspaces.Add(customNodeWs);
            }

            var result = reachClient.Send(HomeWorkspace, CustomNodeWorkspaces);
        }
    }
}
