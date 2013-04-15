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
    class DynamoViewModel:NotificationObject
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

        public DelegateCommand SaveWorkspaceCommand { get; set; }

        public DynamoViewModel()
        {
            SaveWorkspaceCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(new Action(SaveWorkspace), CanSaveWorkspace);

            DynamoModel.Instance.WorkspaceAdded += new EventHandler(Instance_WorkspaceAdded);
            DynamoModel.Instance.WorkspaceAdded += new EventHandler(Instance_WorkspaceAdded);
        }

        void Instance_WorkspaceAdded(object sender, EventArgs e)
        {
            dynWorkspaceViewModel wvm = new dynWorkspaceViewModel("", ((DynamoModelUpdateArgs)e).Item as dynWorkspaceModel);
            Workspaces.Add(wvm);
        }

        /// <summary>
        /// Can we add a workspace?
        /// </summary>
        /// <returns></returns>
        public bool CanSaveWorkspace()
        {
            return true;
        }

        /// <summary>
        /// Add a workspace in the model.
        /// </summary>
        public void SaveWorkspace()
        {
            DynamoModel.Instance.SaveWorkspace();
        }
    }
}
