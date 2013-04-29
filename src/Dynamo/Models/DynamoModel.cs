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
        private ObservableCollection<dynWorkspace> _workSpaces = new ObservableCollection<dynWorkspace>();
        private dynWorkspace _cspace;

        public dynWorkspace CurrentSpace
        {
            get { return _cspace; }
            internal set
            {
                _cspace = value;
                //Bench.CurrentX = _cspace.PositionX;
                //Bench.CurrentY = _cspace.PositionY;

                //if (Bench != null)
                //    Bench.CurrentOffset = new Point(_cspace.PositionX, _cspace.PositionY);
                
                //TODO: Also set the name here.
                RaisePropertyChanged("CurrentSpace");
            }
        }

        public dynWorkspace HomeSpace { get; protected set; }

        /// <summary>
        /// A collection of workspaces in the dynamo model.
        /// </summary>
        public ObservableCollection<dynWorkspace> Workspaces
        {
            get { return _workSpaces; }
            set 
            { 
                _workSpaces = value;
            }
        }

        public List<dynNode> Nodes
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
        public void RemoveWorkspace(dynWorkspace workspace)
        {
            _workSpaces.Remove(workspace);
        }

        //MVVM : visibility should be bound to current space
        public static void hideWorkspace(dynWorkspace ws)
        {
            //foreach (dynNode e in ws.Nodes)
            //    e.NodeUI.Visibility = Visibility.Collapsed;
            //foreach (dynConnector c in ws.Connectors)
            //    c.Visible = false;
            //foreach (dynNote n in ws.Notes)
            //    n.Visibility = Visibility.Hidden;
            //throw new NotImplementedException("Verify that workspace visbility is now bound.");
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
