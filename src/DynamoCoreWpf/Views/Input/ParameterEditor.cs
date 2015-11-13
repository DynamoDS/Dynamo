using Dynamo.ViewModels;
using System.Windows;
using DynCmd = Dynamo.Models.DynamoModel;
using Dynamo.Graph.Nodes.CustomNodes;

namespace Dynamo.UI.Controls
{
    /// <summary>
    /// Auto-completion control for input parameter.
    /// </summary>
    public class ParameterEditor : CodeCompletionEditor
    {
        public ParameterEditor(NodeViewModel viewModel) : base(viewModel)
        {
        }

        /// <summary>
        /// Handler for Esc.
        /// </summary>
        protected override void OnEscape()
        {
            var text = InnerTextEditor.Text;
            var input = DataContext as Symbol;
            if (input == null || input.InputSymbol != null && text.Equals(input.InputSymbol))
            {
                dynamoViewModel.ReturnFocusToSearch();
            }
            else
            {
                InnerTextEditor.Text = (DataContext as Symbol).InputSymbol;
            }
        }

        protected override void OnCommitChange(string code)
        {
            var lastInput = (nodeViewModel.NodeModel as Symbol).InputSymbol;
            if (lastInput.Equals(InnerTextEditor.Text))
                return;

            dynamoViewModel.ExecuteCommand(
                new DynCmd.UpdateModelValueCommand(
                    nodeViewModel.WorkspaceViewModel.Model.Guid,
                    nodeViewModel.NodeModel.GUID, "InputSymbol",
                    code));
        }
    }
}
