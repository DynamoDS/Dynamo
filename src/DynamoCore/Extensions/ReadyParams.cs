using System;
using System.Collections.Generic;
using System.ComponentModel;
using Dynamo.Graph.Workspaces;
using Dynamo.Linting;
using Dynamo.Models;

namespace Dynamo.Extensions
{
    /// <summary>
    /// Application-level handles provided to an extension when 
    /// Dynamo has started and is ready for interaction
    /// </summary>
    public class ReadyParams : IDisposable
    {
        private readonly DynamoModel dynamoModel;
        private readonly StartupParams startupParams;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadyParams"/> class.
        /// </summary>
        /// <param name="dynamoM">Dynamo model.</param>
        internal ReadyParams(DynamoModel dynamoM)
        {
            dynamoModel = dynamoM;
            dynamoModel.PropertyChanged += OnDynamoModelPropertyChanged;
            dynamoModel.WorkspaceOpened += OnCurrentWorkspaceModelOpened;
            dynamoModel.WorkspaceClearingStarted += OnCurrentWorkspaceModelClearingStarted;
            dynamoModel.WorkspaceCleared += OnCurrentWorkspaceModelCleared;
            dynamoModel.WorkspaceRemoveStarted += OnCurrentWorkspaceRemoveStarted;
            dynamoM.Logger.NotificationLogged += OnNotificationRecieved;
            startupParams = new StartupParams(dynamoModel);
        }

        /// <summary>
        /// A reference to the <see cref="StartupParams"/> class.
        /// Useful if this extension will be loaded from a package as its startup method, will not be called.
        /// </summary>
        public StartupParams StartupParams
        {
            get
            {
                return startupParams;
            }
        }

        /// <summary>
        /// Returns list of workspaces
        /// </summary>
        public IEnumerable<IWorkspaceModel> WorkspaceModels
        {
            get
            {
                return dynamoModel.Workspaces;
            }
        }

        /// <summary>
        /// Returns current workspace
        /// </summary>
        public IWorkspaceModel CurrentWorkspaceModel
        {
            get
            {
                return dynamoModel.CurrentWorkspace;
            }
        }

        private ICommandExecutive commandExecutive;
        /// <summary>
        /// Extension specific implementation to execute Recordable commands on DynamoModel
        /// </summary>
        public virtual ICommandExecutive CommandExecutive
        {
            get { return commandExecutive ?? (commandExecutive = new ExtensionCommandExecutive(dynamoModel)); }
        }

        /// <summary>
        /// HostInfo object, Useful to determine what host context Dynamo is running in.
        /// </summary>
        internal HostAnalyticsInfo HostInfo => DynamoModel.HostAnalyticsInfo;


        /// <summary>
        /// Event that is raised when the Dynamo Logger logs a notification.
        /// This event passes the notificationMessage to any subscribers
        /// </summary>
        public event Action<Logging.NotificationMessage> NotificationRecieved;
        private void OnNotificationRecieved(Logging.NotificationMessage notification)
        {
            NotificationRecieved?.Invoke(notification);
        }

        /// <summary>
        /// Occurs when current workspace is changed
        /// </summary>
        public event Action<IWorkspaceModel> CurrentWorkspaceChanged;
        private void OnCurrentWorkspaceModelChanged(IWorkspaceModel ws)
        {
            CurrentWorkspaceChanged?.Invoke(ws);
        }

        /// <summary>
        /// Occurs when current workspace is cleared
        /// </summary>
        public event Action<IWorkspaceModel> CurrentWorkspaceCleared;
        private void OnCurrentWorkspaceModelCleared(IWorkspaceModel ws)
        {
            CurrentWorkspaceCleared?.Invoke(ws);
        }

        /// <summary>
        /// Occurs when current workspace is clearing
        /// </summary>
        public event Action<IWorkspaceModel> CurrentWorkspaceClearingStarted;
        private void OnCurrentWorkspaceModelClearingStarted(IWorkspaceModel ws)
        {
            CurrentWorkspaceClearingStarted?.Invoke(ws);
        }

        /// <summary>
        /// Occurs when current workspace has finished opening
        /// </summary>
        public event Action<IWorkspaceModel> CurrentWorkspaceOpened;
        private void OnCurrentWorkspaceModelOpened(IWorkspaceModel ws)
        {
            CurrentWorkspaceOpened?.Invoke(ws);
        }

        /// <summary>
        /// Occurs when a worspace is about to be removed
        /// </summary>
        public event Action<IWorkspaceModel> CurrentWorkspaceRemoveStarted;
        private void OnCurrentWorkspaceRemoveStarted(IWorkspaceModel ws)
        {
            CurrentWorkspaceRemoveStarted?.Invoke(ws);
        }

        private void OnDynamoModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DynamoModel.CurrentWorkspace))
                OnCurrentWorkspaceModelChanged((sender as DynamoModel).CurrentWorkspace);
        }

        /// <summary>
        /// This method clears event handlers from the DynamoModel that the extension framework setup 
        /// when the model was first loaded.
        /// </summary>
        public void Dispose()
        {
            dynamoModel.PropertyChanged -= OnDynamoModelPropertyChanged;
            dynamoModel.WorkspaceOpened -= OnCurrentWorkspaceModelOpened;
            dynamoModel.WorkspaceClearingStarted -= OnCurrentWorkspaceModelClearingStarted;
            dynamoModel.WorkspaceCleared -= OnCurrentWorkspaceModelCleared;
            dynamoModel.WorkspaceRemoveStarted -= OnCurrentWorkspaceRemoveStarted;
            dynamoModel.Logger.NotificationLogged -= OnNotificationRecieved;
        }
    }
}
