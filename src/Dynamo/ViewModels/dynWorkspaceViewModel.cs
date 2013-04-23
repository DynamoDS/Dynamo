using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Dynamo.Connectors;
using Dynamo.Controls;
using Dynamo.Nodes;
using Dynamo.Selection;
using Dynamo.Utilities;
using Microsoft.Practices.Prism.Commands;

namespace Dynamo
{
    public class dynWorkspaceViewModel: dynViewModelBase
    {
        public dynWorkspace _workspace;

        ObservableCollection<dynConnectorViewModel> _connectors = new ObservableCollection<dynConnectorViewModel>();
        ObservableCollection<dynNodeViewModel> _nodes = new ObservableCollection<dynNodeViewModel>();
        ObservableCollection<dynNoteViewModel> _notes = new ObservableCollection<dynNoteViewModel>(); 

        public ObservableCollection<dynConnectorViewModel> Connectors
        {
            get { return _connectors; }
            set { 
                _connectors = value;
                RaisePropertyChanged("Connectors");
            }
        }
        public ObservableCollection<dynNodeViewModel> Nodes
        {
            get { return _nodes; }
            set
            {
                _nodes = value;
                RaisePropertyChanged("Nodes");
            }
        }
        public ObservableCollection<dynNoteViewModel> Notes
        {
            get { return _notes; }
            set
            {
                _notes = value;
                RaisePropertyChanged("Notes");
            }
        }

        public string Name
        {
            get { return _workspace.Name; }
        }

        public dynWorkspaceViewModel(dynWorkspace workspace)
        {
            _workspace = workspace;
            _workspace.NodeAdded += _workspace_NodeAdded;
            _workspace.ConnectorAdded += _workspace_ConnectorAdded;
            _workspace.NoteAdded += _workspace_NoteAdded;

            _workspace.PropertyChanged += Workspace_PropertyChanged;

        }

        void Workspace_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "Name")
                RaisePropertyChanged("Name");
        }

        void _workspace_NoteAdded(object sender, EventArgs e)
        {
            Notes.Add(new dynNoteViewModel(sender as dynNote));
        }

        void _workspace_ConnectorAdded(object sender, EventArgs e)
        {
            Connectors.Add(new dynConnectorViewModel(sender as dynNode));
        }

        void _workspace_NodeAdded(object sender, EventArgs e)
        {
            Nodes.Add(new dynNodeViewModel(sender as dynNode));
        }

        
    }
}
