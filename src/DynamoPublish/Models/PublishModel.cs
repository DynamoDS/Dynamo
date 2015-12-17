using Dynamo.Publish.Properties;
using Greg;
using Greg.AuthProviders;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dynamo.Graph.Nodes.CustomNodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Logging;
using Reach.Data;
using Reach.Exceptions;
using Reach.Upload;
using Dynamo.Graph.Nodes.ZeroTouch;
using Reach.Messages.Data;
using RestSharp;
using System.Windows;
using Newtonsoft.Json;

namespace Dynamo.Publish.Models
{
    /// <summary>
    /// Core data model for publishing a customizer
    /// </summary>
    public class PublishModel : LogSourceBase
    {
        /// <summary>
        /// A Workspace and its dependencies
        /// </summary>
        internal sealed class WorkspaceDependencies
        {
            /// <summary>
            /// The Workspace for which the dependencies are to be collected.
            /// </summary>
            public readonly HomeWorkspaceModel HomeWorkspace;

            /// <summary>
            /// The full collection of workspaces representing the dependencies
            /// </summary>
            public readonly IEnumerable<CustomNodeWorkspaceModel> CustomNodeWorkspaces;

            private WorkspaceDependencies(HomeWorkspaceModel homeWorkspace, IEnumerable<ICustomNodeWorkspaceModel> customNodeWorkspaces)
            {
                this.HomeWorkspace = homeWorkspace;
                this.CustomNodeWorkspaces = customNodeWorkspaces.OfType<CustomNodeWorkspaceModel>();
            }

            /// <summary>
            /// Get all of the dependencies from a workspace
            /// </summary>
            /// <param name="workspace">The workspace to read the dependencies from</param>
            /// <param name="customNodeManager">A custom node manager to look up dependencies</param>
            /// <returns>A WorkspaceDependencies object containing the workspace and its CustomNodeWorkspaceModel dependencies</returns>
            public static WorkspaceDependencies Collect(HomeWorkspaceModel workspace, ICustomNodeManager customNodeManager)
            {
                if (workspace == null) throw new ArgumentNullException("workspace");
                if (customNodeManager == null) throw new ArgumentNullException("customNodeManager");

                // collect all dependencies
                var dependencies = new HashSet<CustomNodeDefinition>();
                foreach (var node in workspace.Nodes.OfType<Function>())
                {
                    dependencies.Add(node.Definition);
                    foreach (var dep in node.Definition.Dependencies)
                    {
                        dependencies.Add(dep);
                    }
                }

                var customNodeWorkspaces = new List<ICustomNodeWorkspaceModel>();
                foreach (var dependency in dependencies)
                {
                    ICustomNodeWorkspaceModel customNodeWs;
                    var workspaceExists = customNodeManager.TryGetFunctionWorkspace(dependency.FunctionId, false, out customNodeWs);

                    if (!workspaceExists)
                    {
                        // dependency.FunctionName won't be informative for a user, so DisplayName is passed
                        throw new UnresolvedFunctionException(dependency.DisplayName);
                    }

                    if (!customNodeWorkspaces.Contains(customNodeWs))
                    {
                        customNodeWorkspaces.Add(customNodeWs);
                    }
                }

                return new WorkspaceDependencies(workspace, customNodeWorkspaces);
            }
        }

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
            Unauthorized,
            ServerNotFound,
            AuthProviderNotFound,
            EmptyWorkspace,
            InvalidNodes,
            CustomNodeNotFound,
            GetWorkspacesError,
            UnknownServerError
        }

        private readonly IAuthProvider authenticationProvider;
        private readonly ICustomNodeManager customNodeManager;
        private readonly IWorkspaceStorageClient reachClient;

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

        /// <summary>
        /// List of node names which are not allowed to be published
        /// </summary>
        public IEnumerable<string> InvalidNodeNames { get; private set; }

        /// <summary>
        /// Name of custom node which is not available to be published
        /// </summary>
        public string NotFoundCustomNodeName { get; private set; }

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
            reachClient = reachClient ?? new WorkspaceStorageClient(authenticationProvider, serverUrl);
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

        internal async void SendAsync(HomeWorkspaceModel workspace, WorkspaceProperties workspaceProperties = null)
        {
            if (String.IsNullOrWhiteSpace(serverUrl))
            {
                Error = UploadErrorType.ServerNotFound;
                return;
            }

            if (String.IsNullOrWhiteSpace(authenticationProvider.Username))
            {
                Error = UploadErrorType.AuthenticationFailed;
                return;
            }

            if (workspace.Nodes.Count() == 0)
            {
                Error = UploadErrorType.EmptyWorkspace;
                return;
            }

            if (authenticationProvider == null)
            {
                Error = UploadErrorType.AuthProviderNotFound;
                return;
            }

            State = UploadState.Uploading;

            IReachHttpResponse result = null;
            IEnumerable<Workspace> wss = null;

            try
            {
                wss = (await this.GetWorkspaces()).Where(x => x.Name == workspaceProperties.Name) ?? new List<Workspace>();
                
            }
            catch (GetWorkspacesException)
            {
                Error = UploadErrorType.GetWorkspacesError;
                return;
            }

            var publishWorkspace = true;
            if (wss.Any((w) =>  w.Name == workspaceProperties.Name ))
            {
                MessageBoxResult decision =
                        MessageBox.Show(Resources.CustomizerOverrideContent,
                            Resources.CustomizerOverrideHeader,
                            MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (decision == MessageBoxResult.No)
                {
                    publishWorkspace = false;
                }
            }

            if (!publishWorkspace)
            {
                // user doesn't want to override existing customizer
                State = UploadState.Uninitialized;
                Error = UploadErrorType.None;
                return;
            }

            try
            {
                result = await this.Send(workspace, workspaceProperties);
            }
            catch (InvalidNodesException ex)
            {
                InvalidNodeNames = ex.InvalidNodeNames;
                Error = UploadErrorType.InvalidNodes;
                return;
            }
            catch (UnresolvedFunctionException ex)
            {
                NotFoundCustomNodeName = ex.FunctionName;
                Error = UploadErrorType.CustomNodeNotFound;
                return;
            }

            if (result.StatusCode == HttpStatusCode.Unauthorized) {
                Error = UploadErrorType.Unauthorized;
                return;
            }

            var serverResponce = serverResponceRegex.Match(result.Content);

            if (serverResponce.Success)
            {
                State = UploadState.Succeeded;
                Error = UploadErrorType.None;
                CustomizerURL = String.Concat(serverUrl, serverResponce.Value);
            }
            else
            {
                // If there wasn't any error during uploading, 
                // that means it's some error on the server side.
                Error = UploadErrorType.UnknownServerError;
            }
        }

        /// <summary>
        /// Sends workspace and its' dependencies to Flood.
        /// </summary>
        /// <returns>String which is response from server.</returns>
        private Task<IReachHttpResponse> Send(HomeWorkspaceModel workspace, WorkspaceProperties workspaceProperties = null)
        {
            NotFoundCustomNodeName = null;
            var dependencies = WorkspaceDependencies.Collect(workspace, customNodeManager);

            InvalidNodeNames = null;

            return reachClient.Send(
                    workspace,
                    dependencies.CustomNodeWorkspaces,
                    workspaceProperties);
        }

        private Task<IEnumerable<Workspace>> GetWorkspaces()
        {
            return reachClient.GetWorkspaces();
        }

        internal void ClearState()
        {
            State = UploadState.Uninitialized;
            Error = UploadErrorType.None;
        }
    }
}