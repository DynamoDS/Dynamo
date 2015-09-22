using Dynamo.Core;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Publish.Properties;
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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Reach.Data;
using Reach.Exceptions;
using Reach.Upload;

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
            InvalidNodes,
            UnknownServerError
        }

        private readonly IAuthProvider authenticationProvider;
        private readonly ICustomNodeManager customNodeManager;
        private IWorkspaceStorageClient reachClient;

        private static string serverUrl;
        private static string page;
        private readonly Regex serverResponceRegex;

        private static string managerURL;
        public static string ManagerURL
        {
            get
            {
                return managerURL;
            }
        }

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

        public IEnumerable<string> InvalidNodeNames { get; private set; }

        private string customizerURL;
        /// <summary>
        /// URL sent by server.
        /// </summary>
        public string CustomizerURL
        {
            get
            {
                return customizerURL;
            }
            private set
            {
                customizerURL = value;
                OnCustomizerURLChanged(customizerURL);
            }
        }

        internal event Action<UploadState> UploadStateChanged;
        private void OnUploadStateChanged(UploadState state)
        {
            if (UploadStateChanged != null)
                UploadStateChanged(state);
        }

        internal event Action<string> CustomizerURLChanged;
        private void OnCustomizerURLChanged(string url)
        {
            if (CustomizerURLChanged != null)
                CustomizerURLChanged(url);
        }

        #region Initialization

        static PublishModel()
        {
            // Open the configuration file using the dll location.
            var config = ConfigurationManager.OpenExeConfiguration(typeof(PublishModel).Assembly.Location);
            // Get the appSettings section.
            var appSettings = (AppSettingsSection)config.GetSection("appSettings");

            // set the static fields
            serverUrl = appSettings.Settings["ServerUrl"].Value;
            page = appSettings.Settings["Page"].Value;
            managerURL = appSettings.Settings["ManagerPage"].Value;
        }

        internal PublishModel(IAuthProvider dynamoAuthenticationProvider, ICustomNodeManager dynamoCustomNodeManager)
        {
            // Here we throw exceptions if any of the required static fields are not set
            // This prevents these exceptions from being thrown in the static constructor.
            if (String.IsNullOrWhiteSpace(serverUrl))
                throw new Exception(Resources.ServerNotFoundMessage);

            if (String.IsNullOrWhiteSpace(page))
                throw new Exception(Resources.PageErrorMessage);

            if (String.IsNullOrWhiteSpace(managerURL))
                throw new Exception(Resources.ManagerErrorMessage);

            authenticationProvider = dynamoAuthenticationProvider;
            customNodeManager = dynamoCustomNodeManager;

            serverResponceRegex = new Regex(Resources.WorkspacesSendSucceededServerResponse, RegexOptions.IgnoreCase);

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

        internal void SendAsynchronously(IEnumerable<IWorkspaceModel> workspaces, WorkspaceProperties workspaceProperties = null)
        {
            State = UploadState.Uploading;

            Task.Factory.StartNew(() =>
                {
                    var result = this.Send(workspaces, workspaceProperties);
                    var serverResponce = serverResponceRegex.Match(result);

                    if (serverResponce.Success)
                    {
                        State = UploadState.Succeeded;
                        Error = UploadErrorType.None;
                        CustomizerURL = String.Concat(serverUrl, serverResponce.Value);
                    }
                    else if (InvalidNodeNames != null)
                    {
                        Error = UploadErrorType.InvalidNodes;
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
        internal string Send(IEnumerable<IWorkspaceModel> workspaces, WorkspaceProperties workspaceProperties = null)
        {
            if (String.IsNullOrWhiteSpace(serverUrl))
            {
                Error = UploadErrorType.ServerNotFound;
                return Resources.FailedMessage;
            }

            if (String.IsNullOrWhiteSpace(authenticationProvider.Username))
            {
                Error = UploadErrorType.AuthenticationFailed;
                return Resources.FailedMessage;
            }

            if (authenticationProvider == null)
            {
                Error = UploadErrorType.AuthProviderNotFound;
                return Resources.FailedMessage;
            }

            if (reachClient == null)
                reachClient = new WorkspaceStorageClient(authenticationProvider, serverUrl);

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
                result = reachClient.Send(
                    HomeWorkspace,
                    CustomNodeWorkspaces.OfType<CustomNodeWorkspaceModel>(), 
                    workspaceProperties);
                InvalidNodeNames = null;
            }
            catch (InvalidNodesException ex)
            {
                InvalidNodeNames = ex.InvalidNodeNames;
                result = Resources.FailedMessage;
            }
            catch
            {
                result = Resources.FailedMessage;
            }

            return result;
        }

        internal void ClearState()
        {
            State = UploadState.Uninitialized;
            Error = UploadErrorType.None;
        }
    }

}
