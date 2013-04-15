using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dynamo
{
    /// <summary>
    /// A singleton object representing a dynamo model.
    /// </summary>
    public class DynamoModel
    {
        private List<dynWorkspaceModel> _workSpaces = new List<dynWorkspaceModel>();
        private static DynamoModel _instance;

        public event EventHandler WorkspaceAdded;
        public event EventHandler WorkspaceRemoved;

        protected virtual void OnWorkspaceAdded(object sender, DynamoModelUpdateArgs e)
        {
            if (WorkspaceAdded != null)
                WorkspaceAdded(this, e);
        }
        protected virtual void OnWorkspaceRemoved(object sender, DynamoModelUpdateArgs e)
        {
            if (WorkspaceRemoved != null)
                WorkspaceRemoved(this, e);
        }

        public static DynamoModel Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DynamoModel();
                }
                return _instance;
            }
        }

        /// <summary>
        /// A collection of workspaces in the dynamo model.
        /// </summary>
        public List<dynWorkspaceModel> Workspaces
        {
            get { return _workSpaces; }
            set 
            { 
                _workSpaces = value;
            }
        }
    
        /// <summary>
        /// Default constructor.
        /// </summary>
        private DynamoModel()
        {
            
        }

        /// <summary>
        /// Add a workspace to the dynamo model.
        /// </summary>
        /// <param name="workspace"></param>
        public void AddWorkspace()
        {
            dynWorkspaceModel workspace = new dynWorkspaceModel();
            _workSpaces.Add(workspace);

            //Fire an event and send along the workspace
            OnWorkspaceAdded(this, new DynamoModelUpdateArgs(workspace));
        }
    
        /// <summary>
        /// Remove a workspace from the dynamo model.
        /// </summary>
        /// <param name="workspace"></param>
        public void RemoveWorkspace(dynWorkspaceModel workspace)
        {
            _workSpaces.Remove(workspace);
            OnWorkspaceRemoved(this, new DynamoModelUpdateArgs(workspace));
        }
    
        public void SaveWorkspace()
        {
            throw new NotImplementedException();
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
