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
using System.Threading.Tasks;

namespace Dynamo.Publish.Models
{
    public class PublishModel
    {

        public enum UploadState
        {
            Uninitialized,
            Succeeded,
            Uploading,
            Failed
        }

        public enum UploadErrorType
        {
            None,
            AuthenticationFailed,
            ServerNotFound,
            AuthProviderNotFound,
            UnknownServerError
        }

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

        public List<ICustomNodeWorkspaceModel> CustomNodeWorkspaces
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


        private UploadState state;
        /// <summary>
        /// Indicates the state of workspace publishing.
        /// </summary>
        public UploadState State
        {
            get
            {
                return state;
            }
            private set
            {
                if (state != value)
                {
                    state = value;
                    OnUploadStateChanged(state);
                }
            }
        }

        private UploadErrorType errorType;
        /// <summary>
        /// Indicates the type of error.
        /// </summary>
        public UploadErrorType Error
        {
            get
            {
                return errorType;
            }
            private set
            {
                errorType = value;

                if (errorType != UploadErrorType.None)
                    State = UploadState.Failed;
            }
        }

        internal event Action<UploadState> UploadStateChanged;
        private void OnUploadStateChanged(UploadState state)
        {
            if (UploadStateChanged != null)
                UploadStateChanged(state);
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
                throw new Exception(Resource.ServerNotFoundMessage);

            port = appSettings.Settings["Port"].Value;
            if (String.IsNullOrWhiteSpace(port))
                throw new Exception(Resource.PortErrorMessage);

            page = appSettings.Settings["Page"].Value;
            if (String.IsNullOrWhiteSpace(page))
                throw new Exception(Resource.PageErrorMessage);

            authenticationProvider = dynamoAuthenticationProvider;
            customNodeManager = dynamoCustomNodeManager;

            State = UploadState.Uninitialized;
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
            {
                Error = UploadErrorType.AuthProviderNotFound;
                return;
            }

            var loggedIn = authenticationProvider.Login();

            if (!loggedIn)
            {
                Error = UploadErrorType.AuthenticationFailed;
            }
        }

        internal void SendAsynchronously(IEnumerable<IWorkspaceModel> workspaces)
        {
            State = UploadState.Uploading;

            Task.Factory.StartNew(() =>
                {
                    var result = this.Send(workspaces);

                    if (result == Resource.WorkspacesSendSucceededServerResponse)
                    {
                        State = UploadState.Succeeded;
                        Error = UploadErrorType.None;
                    }
                    else
                    {
                        // If there wasn't any error during uploading, 
                        // that means it's some error on the server side.
                        Error = UploadErrorType.UnknownServerError;
                    }
                });

        }

        /// <summary>
        /// Sends workspace and its' dependencies to Flood.
        /// </summary>
        /// <returns>String which is response from server.</returns>
        internal string Send(IEnumerable<IWorkspaceModel> workspaces)
        {
            if (String.IsNullOrWhiteSpace(serverUrl))
            {
                Error = UploadErrorType.ServerNotFound;
                return Resource.FailedMessage;
            }

            if (String.IsNullOrWhiteSpace(authenticationProvider.Username))
            {
                Error = UploadErrorType.AuthenticationFailed;
                return Resource.FailedMessage;
            }

            if (authenticationProvider == null)
            {
                Error = UploadErrorType.AuthProviderNotFound;
                return Resource.FailedMessage;
            }

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

            CustomNodeWorkspaces = new List<ICustomNodeWorkspaceModel>();
            foreach (var dependency in dependencies)
            {
                ICustomNodeWorkspaceModel customNodeWs;
                var isWorkspaceCreated = customNodeManager.TryGetFunctionWorkspace(dependency.FunctionId, false, out customNodeWs);
                if (isWorkspaceCreated && !CustomNodeWorkspaces.Contains(customNodeWs))
                    CustomNodeWorkspaces.Add(customNodeWs);
            }

            string result;
            try
            {
                result = reachClient.Send(HomeWorkspace, CustomNodeWorkspaces.OfType<CustomNodeWorkspaceModel>());
            }
            catch
            {
                result = Resource.FailedMessage;
            }
            return result;
        }
    }

}
