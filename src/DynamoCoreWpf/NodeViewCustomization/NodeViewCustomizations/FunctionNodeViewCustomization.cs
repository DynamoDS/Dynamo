using System.Windows.Controls;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.ViewModels;

namespace Dynamo.Wpf
{
    public class FunctionNodeViewCustomization : INodeViewCustomization<Function>
    {
        private Function functionNodeModel;
        private DynamoViewModel dynamoViewModel;

        public void CustomizeView(Function function, NodeView nodeView)
        {
            functionNodeModel = function;
            dynamoViewModel = nodeView.ViewModel.DynamoViewModel;

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
            CustomNodeInfo info;
            dynamoViewModel.Model.CustomNodeManager.TryGetNodeInfo(functionNodeModel.Definition.FunctionId, out info);
            
            // copy these strings
            var newName = info.Name.Substring(0);
            var newCategory = info.Category.Substring(0);
            var newDescription = info.Description.Substring(0);

            var args = new FunctionNamePromptEventArgs
            {
                Name = newName,
                Description = newDescription,
                Category = newCategory,
                CanEditName = false
            };

            dynamoViewModel.Model.OnRequestsFunctionNamePrompt(functionNodeModel, args);

            if (args.Success)
            {
                CustomNodeWorkspaceModel ws;
                dynamoViewModel.Model.CustomNodeManager.TryGetFunctionWorkspace(
                    functionNodeModel.Definition.FunctionId,
                    DynamoModel.IsTestMode,
                    out ws);
                ws.SetInfo(args.Name, args.Category, args.Description);
                
                if (!string.IsNullOrEmpty(ws.FileName))
                    ws.Save(dynamoViewModel.EngineController.LiveRunnerCore);
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