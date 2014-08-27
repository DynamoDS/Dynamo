using System.Windows.Controls;

using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.ViewModels;
using Dynamo.Wpf;

namespace Dynamo.Wpf
{
    public class Function : INodeViewInjection
    {
        private Dynamo.Nodes.Function functionNodeModel;

        public void SetupCustomUIElements(dynNodeView nodeUI)
        {
            this.functionNodeModel = nodeUI.ViewModel.NodeModel as Dynamo.Nodes.Function;

            nodeUI.MainContextMenu.Items.Add(new Separator());

            // edit contents
            var editItem = new MenuItem
            {
                Header = "Edit Custom Node...",
                IsCheckable = false
            };
            nodeUI.MainContextMenu.Items.Add(editItem);
            editItem.Click += (sender, args) => GoToWorkspace(nodeUI.ViewModel);

            // edit properties
            var editPropertiesItem = new MenuItem
            {
                Header = "Edit Custom Node Properties...",
                IsCheckable = false
            };
            nodeUI.MainContextMenu.Items.Add(editPropertiesItem);
            editPropertiesItem.Click += (sender, args) => EditCustomNodeProperties();

            // publish
            var publishCustomNodeItem = new MenuItem
            {
                Header = "Publish This Custom Node...",
                IsCheckable = false
            };
            nodeUI.MainContextMenu.Items.Add(publishCustomNodeItem);
            publishCustomNodeItem.Click += (sender, args) =>
            {
                GoToWorkspace(nodeUI.ViewModel);

                if (nodeUI.ViewModel.DynamoViewModel.PublishCurrentWorkspaceCommand.CanExecute(null))
                {
                    nodeUI.ViewModel.DynamoViewModel.PublishCurrentWorkspaceCommand.Execute(null);
                }
            };

            nodeUI.UpdateLayout();
        }

        private void EditCustomNodeProperties()
        {
            var workspace = functionNodeModel.Definition.WorkspaceModel;

            // copy these strings
            var newName = workspace.Name.Substring(0);
            var newCategory = workspace.Category.Substring(0);
            var newDescription = workspace.Description.Substring(0);

            var args = new FunctionNamePromptEventArgs
            {
                Name = newName,
                Description = newDescription,
                Category = newCategory,
                CanEditName = false
            };

            functionNodeModel.Workspace.DynamoModel.OnRequestsFunctionNamePrompt(this, args);

            if (args.Success)
            {
                if (workspace is CustomNodeWorkspaceModel)
                {
                    var def = (workspace as CustomNodeWorkspaceModel).CustomNodeDefinition;
                    functionNodeModel.Workspace.DynamoModel.CustomNodeManager.Refactor(def.FunctionId, args.CanEditName ? args.Name : workspace.Name, args.Category, args.Description);
                }

                if (args.CanEditName) workspace.Name = args.Name;
                workspace.Description = args.Description;
                workspace.Category = args.Category;

                if (workspace.FileName != null)
                    workspace.Save();
            }
        }

        private static void GoToWorkspace(NodeViewModel viewModel)
        {
            if (viewModel == null) return;

            if (viewModel.GotoWorkspaceCommand.CanExecute(null))
            {
                viewModel.GotoWorkspaceCommand.Execute(null);
            }
        }

        public void Dispose()
        {
            throw new System.NotImplementedException();
        }
    }
}