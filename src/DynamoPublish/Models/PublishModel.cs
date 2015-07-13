using Dynamo.Core;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.Wpf.Authentication;
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
        private readonly AuthenticationManager manager;
        public LoginService LoginService;

        private readonly string serverUrl;
        private readonly string port;
        private readonly string page;
        private readonly string provider;

        public bool IsLoggedIn
        {
            get
            {
                return manager.LoginState == LoginState.LoggedIn;
            }
        }

        #region Initialization

        public PublishModel()
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

            provider = appSettings.Settings["Provider"].Value;
            if (String.IsNullOrWhiteSpace(provider))
                throw new ArgumentException();

            if (!String.IsNullOrWhiteSpace(provider))
                manager = new AuthenticationManager(new OxygenProvider(provider));
        }

        #endregion

        internal void Authenticate()
        {
            if (manager == null)
                throw new Exception(Resource.AuthenticationErrorMessage);

            if (manager.HasAuthProvider && LoginService != null)
            {
                manager.AuthProvider.RequestLogin += LoginService.ShowLogin;
                manager.AuthProvider.Login();
            }
        }

        /// <summary>
        /// Sends workspace and its' dependencies to Flood.
        /// </summary>
        internal void Send(IEnumerable<IWorkspaceModel> workspaces)
        {
            if (String.IsNullOrWhiteSpace(serverUrl) || String.IsNullOrWhiteSpace(manager.Username))
                return;

            string fullServerAdress = serverUrl + ":" + port;
            var reachClient = new WorkspaceStorageClient(manager.AuthProvider, fullServerAdress);
            var result = reachClient.Send(workspaces.OfType<HomeWorkspaceModel>().First(), workspaces.OfType<CustomNodeWorkspaceModel>());
        }
    }
}
