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
    public class InviteModel : ILogSource
    {
        private readonly IAuthProvider authenticationProvider;       

        private readonly string serverUrl;
        private readonly string port;
        private readonly string invite;
        
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

        internal event Action<string,bool> UpdateStatusMessage;
        private void OnUpdateStatusMessage(String text, bool hasError)
        {
            if (UpdateStatusMessage != null)
                UpdateStatusMessage(text, hasError);
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
                throw new Exception(Resource.ServerNotFoundMessage);

            port = appSettings.Settings["Port"].Value;
            if (String.IsNullOrWhiteSpace(port))
                throw new Exception(Resource.PortErrorMessage);

            invite = appSettings.Settings["Invite"].Value;
            if (String.IsNullOrWhiteSpace(invite))
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
            {
                OnUpdateStatusMessage(Resource.AuthManagerNotFoundMessage,true);
                return;
            }

            authenticationProvider.Login();
        }


        /// <summary>
        /// Sends request to flood. Returns true if success.
        /// </summary>
        /// <returns></returns>
        internal bool  Send()
        {
            if (String.IsNullOrWhiteSpace(serverUrl) || String.IsNullOrWhiteSpace(authenticationProvider.Username))
            {
                OnUpdateStatusMessage(Resource.ServerNotFoundMessage, true);
                return false;
            }

            if (authenticationProvider == null)
            {                
                OnUpdateStatusMessage(Resource.AuthenticationFailedMessage, true);
                return false;
            }

            string fullServerAdress = serverUrl + ":" + port;
            
            if (restClient == null)
            {
                restClient = new RestClient(fullServerAdress);
            }
            
            var  request = new RestRequest(invite, Method.POST);
            
            authenticationProvider.SignRequest(ref request,restClient);
        
            var response = restClient.Execute(request);      
      
            if(response.ErrorException == null)
                OnUpdateStatusMessage(Resource.InviteRequestSuccess, false);
            else
            {
                OnUpdateStatusMessage(Resource.InviteRequestFailed, true);
                Log(LogMessage.Error(response.ErrorException));
                return false;
            }

            return true;
        }

        #region ILogSource implementation
        public event Action<ILogMessage> MessageLogged;

        protected void Log(ILogMessage obj)
        {
            var handler = MessageLogged;
            if (handler != null) handler(obj);
        }

        protected void Log(string msg)
        {
            Log(LogMessage.Info(msg));
        }       
        #endregion
    }
}
