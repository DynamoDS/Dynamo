using System.Windows.Controls;
using Dynamo.Controls;
using Dynamo.Graph.Nodes.CustomNodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using Dynamo.ViewModels;
using Dynamo.Wpf.Properties;


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
                Header = Resources.ContextMenuEditCustomNode,
                IsCheckable = false
            };
            nodeView.MainContextMenu.Items.Add(editItem);
            editItem.Click += (sender, args) => GoToWorkspace(nodeView.ViewModel);

            // edit properties
            var editPropertiesItem = new MenuItem
            {
                Header = Resources.ContextMenuEditCustomNodeProperty,
                IsCheckable = false
            };
            nodeView.MainContextMenu.Items.Add(editPropertiesItem);
            editPropertiesItem.Click += (sender, args) => EditCustomNodeProperties();

            // Check if the workspace is read-only or not, disable editPropertiesItem accordingly
            CustomNodeWorkspaceModel ws;
            dynamoViewModel.Model.CustomNodeManager.TryGetFunctionWorkspace(
                functionNodeModel.Definition.FunctionId,
                DynamoModel.IsTestMode,
                out ws);

            if (ws != null && ws.IsReadOnly)
            {
                editPropertiesItem.IsEnabled = false;
            }

            // publish
            var publishCustomNodeItem = new MenuItem
            {
                Header = Resources.ContextMenuPublishCustomNode,
                IsCheckable = false
            };
            nodeView.MainContextMenu.Items.Add(publishCustomNodeItem);

            publishCustomNodeItem.Command = nodeView.ViewModel.DynamoViewModel.PublishSelectedNodesCommand;
            publishCustomNodeItem.CommandParameter = functionNodeModel;

            nodeView.UpdateLayout();
        }

        private void EditCustomNodeProperties()
        {
            CustomNodeInfo info;
            var model = dynamoViewModel.Model;

            if (!model.CustomNodeManager.TryGetNodeInfo(functionNodeModel.Definition.FunctionId,
                    out info))
            {
                return;
            }
            
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

            model.OnRequestsFunctionNamePrompt(functionNodeModel, args);

            if (args.Success)
            {
                CustomNodeWorkspaceModel ws;
                model.CustomNodeManager.TryGetFunctionWorkspace(
                    functionNodeModel.Definition.FunctionId,
                    DynamoModel.IsTestMode,
                    out ws);
                ws.SetInfo(args.Name, args.Category, args.Description);

                if (!string.IsNullOrEmpty(ws.FileName))
                    model.SaveWorkspace(ws.FileName, ws);
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