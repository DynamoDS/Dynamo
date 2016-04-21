using Dynamo.Interfaces;
using Dynamo.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Dynamo.Graph.Workspaces;

namespace Dynamo.Extensions
{
    /// <summary>
    /// Application-level handles provided to an extension when 
    /// Dynamo has started and is ready for interaction
    /// </summary>
    public class ReadyParams
    {
        private readonly DynamoModel dynamoModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadyParams"/> class.
        /// </summary>
        /// <param name="dynamoM">Dynamo model.</param>
        internal ReadyParams(DynamoModel dynamoM)
        {
            dynamoModel = dynamoM;
            dynamoModel.PropertyChanged += OnDynamoModelPropertyChanged;
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
