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
using RestSharp;

namespace Dynamo.Publish.Models
{
    public class InviteModel
    {
        private readonly IAuthProvider authenticationProvider;       

        private readonly string serverUrl;
        private readonly string port;
        private readonly string page;
        
        private RestClient restClient;

        public bool IsLoggedIn
        {
            get
            {
                return authenticationProvider.LoginState == LoginState.LoggedIn;
            }
        }
       
        public bool HasAuthProvider
        {
            get
            {
                return authenticationProvider != null;
            }
        }

        #region Initialization

        internal InviteModel(IAuthProvider dynamoAuthenticationProvider)
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

            page = appSettings.Settings["Invite"].Value;
            if (String.IsNullOrWhiteSpace(page))
                throw new Exception(Resource.PageErrorMessage);

            authenticationProvider = dynamoAuthenticationProvider;            
        }

        internal InviteModel(IAuthProvider provider,  RestClient client) :
            this(provider)
        {
            restClient = client;
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
        internal void Send()
        {
            //if (String.IsNullOrWhiteSpace(serverUrl) || String.IsNullOrWhiteSpace(authenticationProvider.Username))
            //    throw new Exception(Resource.ServerErrorMessage);

            //if (authenticationProvider == null)
            //    throw new Exception(Resource.AuthenticationErrorMessage);

            string fullServerAdress = serverUrl + ":" + port;

            //Construct the client here
            if (restClient == null)
            {
                restClient = new RestClient(fullServerAdress);
            }
            
            var  request = new RestRequest(page, Method.POST);
            
            //authenticationProvider.SignRequest(ref request,restClient);

            var response = restClient.Execute(request);
        }
    }
}
