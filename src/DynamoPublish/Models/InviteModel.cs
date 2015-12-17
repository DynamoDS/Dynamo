using Dynamo.Core;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Logging;
using Dynamo.Wpf.Authentication;
using Greg;
using Greg.AuthProviders;
using Reach;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using Dynamo.Publish.Properties;
using RestSharp;
using Newtonsoft.Json;

namespace Dynamo.Publish.Models
{
    public class InviteModel : ILogSource
    {
        private readonly IAuthProvider authenticationProvider;       

        private readonly string serverUrl;       
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
                throw new Exception(Resources.ServerNotFoundMessage);
            
            invite = appSettings.Settings["Invite"].Value;
            if (String.IsNullOrWhiteSpace(invite))
                throw new Exception(Resources.PageErrorMessage);

            authenticationProvider = dynamoAuthenticationProvider;
        }

        internal string GetInvitationStatus() 
        {
            var req = new RestRequest(invite, Method.GET);

            restClient = new RestClient(serverUrl);

            if (authenticationProvider.LoginState != LoginState.LoggedIn) 
            {
                return String.Empty;
            }
            authenticationProvider.SignRequest(ref req, restClient);

            var res = restClient.Execute(req).Content;

            return JsonConvert.DeserializeObject<dynamic>(res)["status"];
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
                OnUpdateStatusMessage(Resources.AuthManagerNotFoundMessage, true);
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
                OnUpdateStatusMessage(Resources.ServerNotFoundMessage, true);
                return false;
            }

            if (authenticationProvider == null)
            {
                OnUpdateStatusMessage(Resources.AuthenticationFailedMessage, true);
                return false;
            }
                         
            if (restClient == null)
            {
                restClient = new RestClient(serverUrl);
            }

            try
            {
                var request = new RestRequest(invite, Method.POST);

                authenticationProvider.SignRequest(ref request, restClient);

                var response = restClient.Execute(request);

                if (response.ErrorException == null && response.StatusCode == HttpStatusCode.OK)
                {
                    var target = JsonConvert.DeserializeObject<dynamic>(response.Content)["target"].Value;
                    if (!String.IsNullOrEmpty(target))
                    {
                        OnUpdateStatusMessage(Resources.InviteRequestSuccess + target, false);
                    }                    
                }
                else
                {
                    OnUpdateStatusMessage(Resources.InviteRequestFailed, true);
                    if (response.ErrorException != null)
                    {
                        Log(LogMessage.Error(response.ErrorException));
                    }

                    return false;
                }
            }
            catch (Exception ex)
            {
                OnUpdateStatusMessage(Resources.InviteRequestFailed, true);
                Log(ex.Message, WarningLevel.Error);
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

        protected void Log(string msg, WarningLevel severity)
        {
            switch (severity)
            {
                case WarningLevel.Error:
                    Log(LogMessage.Error(msg));
                    break;
                default:
                    Log(LogMessage.Warning(msg, severity));
                    break;
            }
        }
        #endregion
    }
}