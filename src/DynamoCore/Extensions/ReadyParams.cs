using System;
using System.Collections.Generic;
using System.ComponentModel;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;

namespace Dynamo.Extensions
{
    /// <summary>
    /// Application-level handles provided to an extension when 
    /// Dynamo has started and is ready for interaction
    /// </summary>
    public class ReadyParams
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
            dynamoM.Logger.NotificationLogged += OnNotificationRecieved;
            startupParams = new StartupParams(dynamoModel.AuthenticationManager.AuthProvider,
                dynamoModel.PathManager, new ExtensionLibraryLoader(dynamoModel), dynamoModel.CustomNodeManager,
                new Version(dynamoModel.Version), dynamoModel.PreferenceSettings);
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
        /// Event that is raised when the Dynamo Logger logs a notification.
        /// This event passes the notificationMessage to any subscribers
        /// </summary>
        public event Action<Logging.NotificationMessage> NotificationRecieved;
        private void OnNotificationRecieved(Logging.NotificationMessage notification)
        {
            if (NotificationRecieved != null)
                NotificationRecieved(notification);
        }

        /// <summary>
        /// Occurs when current workspace is changed
        /// </summary>
        public event Action<IWorkspaceModel> CurrentWorkspaceChanged;
        private void OnCurrentWorkspaceModelChanged(IWorkspaceModel ws)
        {
            if (CurrentWorkspaceChanged != null)
                CurrentWorkspaceChanged(ws);
        }

        private void OnDynamoModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentWorkspace")
                OnCurrentWorkspaceModelChanged((sender as DynamoModel).CurrentWorkspace);
        }
    }
}
