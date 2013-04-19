using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using Dynamo.Connectors;
using Dynamo.Nodes;
using Dynamo.PackageManager;
using Dynamo.Utilities;
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

        /// <summary>
        ///     Change the currently visible workspace to the home workspace
        /// </summary>
        /// <param name="symbol">The function definition for the custom node workspace to be viewed</param>
        internal void ViewHomeWorkspace()
        {
            //Step 1: Make function workspace invisible
            foreach (dynNode ele in Nodes)
            {
                ele.NodeUI.Visibility = Visibility.Collapsed;
            }
            foreach (dynConnector con in DynamoModel.Instance.CurrentSpace.Connectors)
            {
                con.Visible = false;
            }
            foreach (dynNote note in DynamoModel.Instance.CurrentSpace.Notes)
            {
                note.Visibility = Visibility.Hidden;
            }

            //Step 3: Save function
            SaveFunction(dynSettings.FunctionDict.Values.FirstOrDefault(x => x.Workspace == DynamoModel.Instance.CurrentSpace));

            //Step 4: Make home workspace visible
            DynamoModel.Instance.CurrentSpace = DynamoModel.Instance.HomeSpace;

            foreach (dynNode ele in Nodes)
            {
                ele.NodeUI.Visibility = Visibility.Visible;
            }
            foreach (dynConnector con in DynamoModel.Instance.CurrentSpace.Connectors)
            {
                con.Visible = true;
            }
            foreach (dynNote note in DynamoModel.Instance.CurrentSpace.Notes)
            {
                note.Visibility = Visibility.Visible;
            }

            Bench.homeButton.IsEnabled = false;

            // TODO: get this out of here
            PackageManagerClient.HidePackageControlInformation();

            Bench.workspaceLabel.Content = "Home";
            Bench.editNameButton.Visibility = Visibility.Collapsed;
            Bench.editNameButton.IsHitTestVisible = false;

            Bench.setHomeBackground();

            DynamoModel.Instance.CurrentSpace.OnDisplayed();
        }
    }
}
