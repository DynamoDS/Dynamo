using System.Windows.Controls;

using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.ViewModels;
using Dynamo.Wpf;

namespace Dynamo.Wpf
{
    public class FunctionNodeViewCustomization : INodeViewCustomization<Dynamo.Nodes.Function>
    {
        private Dynamo.Nodes.Function functionNodeModel;

        public void CustomizeView(Dynamo.Nodes.Function function, NodeView nodeView)
        {
            this.functionNodeModel = function;

            nodeView.MainContextMenu.Items.Add(new Separator());

            // edit contents
            var editItem = new MenuItem
            {
                Header = "Edit Custom Node...",
                IsCheckable = false
            };
            nodeView.MainContextMenu.Items.Add(editItem);
            editItem.Click += (sender, args) => GoToWorkspace(nodeView.ViewModel);

            // edit properties
            var editPropertiesItem = new MenuItem
            {
                Header = "Edit Custom Node Properties...",
                IsCheckable = false
            };
            nodeView.MainContextMenu.Items.Add(editPropertiesItem);
            editPropertiesItem.Click += (sender, args) => EditCustomNodeProperties();

            // publish
            var publishCustomNodeItem = new MenuItem
            {
                Header = "Publish This Custom Node...",
                IsCheckable = false
            };
            nodeView.MainContextMenu.Items.Add(publishCustomNodeItem);
            publishCustomNodeItem.Click += (sender, args) =>
            {
                GoToWorkspace(nodeView.ViewModel);

                if (nodeView.ViewModel.DynamoViewModel.PublishCurrentWorkspaceCommand.CanExecute(null))
                {
                    nodeView.ViewModel.DynamoViewModel.PublishCurrentWorkspaceCommand.Execute(null);
                }
            };

            nodeView.UpdateLayout();
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

            functionNodeModel.Workspace.DynamoModel.OnRequestsFunctionNamePrompt(this.functionNodeModel, args);

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
        }
    }
}