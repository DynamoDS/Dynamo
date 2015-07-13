using Dynamo.Core;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Wpf.Authentication;
using Greg;
using Greg.AuthProviders;
using Reach;
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

        private readonly string serverUrl;
        private readonly string port;
        private readonly string page;

        public bool IsLoggedIn
        {
            get
            {
                return authenticationProvider.LoginState == LoginState.LoggedIn;
            }
        }

        #region Initialization

        public PublishModel(IAuthProvider dynamoAuthenticationProvider)
        {
            // Open the configuration file using the dll location.
            Configuration config = ConfigurationManager.OpenExeConfiguration(this.GetType().Assembly.Location);
            // Get the appSettings section.
            AppSettingsSection appSettings = (AppSettingsSection)config.GetSection("appSettings");

            serverUrl = appSettings.Settings["ServerUrl"].Value;
            if (String.IsNullOrWhiteSpace(serverUrl))
                throw new ArgumentException();

            port = appSettings.Settings["Port"].Value;
            if (String.IsNullOrWhiteSpace(port))
                throw new ArgumentException();

            page = appSettings.Settings["Page"].Value;
            if (String.IsNullOrWhiteSpace(page))
                throw new ArgumentException();

            authenticationProvider = dynamoAuthenticationProvider;
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
            var reachClient = new WorkspaceStorageClient(authenticationProvider, fullServerAdress);

            var homeWorkspace = workspaces.OfType<HomeWorkspaceModel>().First();
            var functionNodes = homeWorkspace.Nodes.OfType<Function>();

            List<CustomNodeDefinition> dependencies = new List<CustomNodeDefinition>();
            foreach (var node in functionNodes)
            {
                dependencies.AddRange(node.Definition.Dependencies);
            }

            var result = reachClient.Send(homeWorkspace, null/*dependencies*/);
        }
    }
}
