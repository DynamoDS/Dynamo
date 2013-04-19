using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamo.Nodes;
using Dynamo.Utilities;

namespace Dynamo
{
    /// <summary>
    /// A singleton object representing a dynamo model.
    /// </summary>
    public class DynamoModel
    {
        private List<dynWorkspace> _workSpaces = new List<dynWorkspace>();
        private static DynamoModel _instance;
        private dynWorkspace _cspace;

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

        public dynWorkspace CurrentSpace
        {
            get { return _cspace; }
            internal set
            {
                _cspace = value;
                //Bench.CurrentX = _cspace.PositionX;
                //Bench.CurrentY = _cspace.PositionY;
                if (Bench != null)
                    Bench.CurrentOffset = new Point(_cspace.PositionX, _cspace.PositionY);

                //TODO: Also set the name here.
            }
        }

        public dynWorkspace HomeSpace { get; protected set; }

        public IEnumerable<dynNode> AllNodes
        {
            get
            {
                return HomeSpace.Nodes.Concat(
                    dynSettings.FunctionDict.Values.Aggregate(
                        (IEnumerable<dynNode>)new List<dynNode>(),
                        (a, x) => a.Concat(x.Workspace.Nodes)
                        )
                    );
            }
        }

        /// <summary>
        /// A collection of workspaces in the dynamo model.
        /// </summary>
        public List<dynWorkspace> Workspaces
        {
            get { return _workSpaces; }
            set 
            { 
                _workSpaces = value;
            }
        }

        public List<dynNode> Nodes
        {
            get { return CurrentSpace.Nodes; }
        }

        /// <summary>
        /// Construct a Dynamo Model and create a home space.
        /// </summary>
        public DynamoModel()
        {
            HomeSpace = CurrentSpace = new HomeWorkspace();
            _workSpaces.Add(HomeSpace);
            OnWorkspaceAdded(this, new DynamoModelUpdateArgs(HomeSpace));
        }

        /// <summary>
        /// Add a workspace to the dynamo model.
        /// </summary>
        /// <param name="workspace"></param>
        public void AddHomeWorkspace()
        {
            dynHomeWorkspace workspace = new dynHomeWorkspace();
            _workSpaces.Add(workspace);

            //Fire an event and send along the workspace
            OnWorkspaceAdded(this, new DynamoModelUpdateArgs(workspace));
        }

        /// <summary>
        /// Remove a workspace from the dynamo model.
        /// </summary>
        /// <param name="workspace"></param>
        public void RemoveWorkspace(dynWorkspace workspace)
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
