using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using Dynamo.Connectors;
using Dynamo.Nodes;
using Microsoft.Practices.Prism.ViewModel;

namespace Dynamo
{
    /// <summary>
    /// The Dynamo model.
    /// </summary>
    public class DynamoModel:dynModelBase
    {
        private ObservableCollection<dynWorkspaceModel> _workSpaces = new ObservableCollection<dynWorkspaceModel>();
        private dynWorkspaceModel _cspace;

        public dynWorkspaceModel CurrentSpace
        {
            get { return _cspace; }
            internal set
            {
                _cspace = value;
                RaisePropertyChanged("CurrentSpace");
            }
        }

        public dynWorkspaceModel HomeSpace { get; protected set; }

        /// <summary>
        /// A collection of workspaces in the dynamo model.
        /// </summary>
        public ObservableCollection<dynWorkspaceModel> Workspaces
        {
            get { return _workSpaces; }
            set 
            { 
                _workSpaces = value;
            }
        }

        public List<dynNodeModel> Nodes
        {
            get { return CurrentSpace.Nodes.ToList(); }
        }

        /// <summary>
        /// Construct a Dynamo Model and create a home space.
        /// </summary>
        public DynamoModel()
        {          
        }

        /// <summary>
        /// Add a workspace to the dynamo model.
        /// </summary>
        /// <param name="workspace"></param>
        public void AddHomeWorkspace()
        {
            var workspace = new HomeWorkspace();
            HomeSpace = workspace;
            _workSpaces.Add(workspace);
        }

        /// <summary>
        /// Remove a workspace from the dynamo model.
        /// </summary>
        /// <param name="workspace"></param>
        public void RemoveWorkspace(dynWorkspaceModel workspace)
        {
            _workSpaces.Remove(workspace);
        }

    }

    public class DynamoModelUpdateArgs : EventArgs
    {
        public object Item { get; set; }

        public DynamoModelUpdateArgs(object item)
        {
            Item = item;
        }
    }
}
