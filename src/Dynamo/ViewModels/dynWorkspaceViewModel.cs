using System;
using System.Collections.ObjectModel;
using Microsoft.Practices.Prism.Commands;

namespace Dynamo
{
    class dynWorkspaceViewModel: dynViewModelBase
    {
        private dynWorkspaceModel _workspace;

        ObservableCollection<dynWorkspaceViewModel> _workspaces = new ObservableCollection<dynWorkspaceViewModel>(); 
        public ObservableCollection<dynWorkspaceViewModel> Workspaces
        {
            get { return _workspaces; }
            set { 
                _workspaces = value;
                RaisePropertyChanged("Workspaces");
            }
        }

        public DelegateCommand AddNodeCommand { get; private set; }

        public DelegateCommand AddConnectorCommand { get; private set; }

        public DelegateCommand AddNoteCommand { get; private set; }

        public dynWorkspaceViewModel(string name, dynWorkspaceModel workspace):base(name)
        {
            _workspace = workspace;
            _workspace.NodeAdded += new EventHandler(_workspace_NodeAdded);
            _workspace.ConnectorAdded += new EventHandler(_workspace_ConnectorAdded);
            _workspace.NoteAdded += new EventHandler(_workspace_NoteAdded);

            AddNodeCommand = new DelegateCommand(new Action(AddNode), CanAddNode);
            AddConnectorCommand = new DelegateCommand(new Action(AddConnector), CanAddConnector);
            AddNoteCommand = new DelegateCommand(new Action(AddNote), CanAddNote);
        }

        void _workspace_NoteAdded(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        void _workspace_ConnectorAdded(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        void _workspace_NodeAdded(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private bool CanAddNode()
        {
            return true;
        }

        private void AddNode()
        {
            _workspace.AddNode();
        }

        private bool CanAddConnector()
        {
            return true;
        }

        private void AddConnector()
        {
            _workspace.AddConnector();
        }

        private bool CanAddNote()
        {
            return true;
        }

        private void AddNote()
        {
            _workspace.AddNote();
        }
    }
}
