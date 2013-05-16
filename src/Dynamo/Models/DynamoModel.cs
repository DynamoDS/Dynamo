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
        private ObservableCollection<dynWorkspaceModel> _hiddenWorkspaces = new ObservableCollection<dynWorkspaceModel>();

        private dynWorkspaceModel _cspace;

        public dynWorkspaceModel CurrentSpace
        {
            get { return _cspace; }
            internal set
            {
                if (_cspace != null)
                    _cspace.IsCurrentSpace = false;
                _cspace = value;
                _cspace.IsCurrentSpace = true;
                RaisePropertyChanged("CurrentSpace");
            }
        }

        public dynWorkspaceModel HomeSpace { get; protected set; }

        /// <summary>
        ///     The collection of visible workspaces in Dynamo
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

        public void HideWorkspace(dynWorkspaceModel workspace)
        {
            this.CurrentSpace = _workSpaces[0];  // typically the home workspace
            _workSpaces.Remove(workspace);
            _hiddenWorkspaces.Add(workspace);
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


        public static bool RunEnabled { get; set; }

        public static bool RunInDebug { get; set; }
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
