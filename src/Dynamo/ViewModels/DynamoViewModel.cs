using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Dynamo.Nodes;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.ViewModel;

namespace Dynamo
{
    /// <summary>
    /// DynamoViewModel
    /// </summary>
    class DynamoViewModel:dynViewModelBase
    {
        private ObservableCollection<dynWorkspaceViewModel> _workspaces = new ObservableCollection<dynWorkspaceViewModel>();

        public ObservableCollection<dynWorkspaceViewModel> Workspaces
        {
            get { return _workspaces; }
            set
            {
                _workspaces = value;
                RaisePropertyChanged("Workspaces");
            }
        }

        public DelegateCommand AddWorkspaceCommand { get; set; }

        public bool ViewingHomespace
        {
            get { return DynamoModel.Instance.CurrentSpace == DynamoModel.Instance.HomeSpace; }
        }

        public DynamoViewModel(DynamoModel model)
        {
            DynamoModel.Instance.WorkspaceAdded += new EventHandler(Instance_WorkspaceAdded);
            DynamoModel.Instance.WorkspaceRemoved+=new EventHandler(Instance_WorkspaceRemoved);   

            AddWorkspaceCommand = new DelegateCommand(new Action(AddFunctionWorkspace), CanAddFunctionWorkspace);
        }

        void Instance_WorkspaceAdded(object sender, EventArgs e)
        {
            //TODO: What do we do in the view model when we add a workspace?
        }

        void Instance_WorkspaceRemoved(object sender, EventArgs e)
        {
            //TODO: What do we do in the view model when we remove a workspace?
        }

        private void AddFunctionWorkspace()
        {
            DynamoModel.Instance.Workspaces.Add(new FuncWorkspace());
        }

        private bool CanAddFunctionWorkspace()
        {
            return true;
        }
    }
}
