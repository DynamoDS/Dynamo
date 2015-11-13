using DynCmd = Dynamo.Models.DynamoModel;
using Dynamo.Graph.Nodes.CustomNodes;
using Dynamo.Controls;

namespace Dynamo.UI.Controls
{
    /// <summary>
    /// Auto-completion control for input parameter.
    /// </summary>
    public class ParameterEditor : CodeCompletionEditor
    {
        /// <summary>
        /// Create input editor by the view of symbol node.
        /// </summary>
        /// <param name="view"></param>
        public ParameterEditor(NodeView view) : base(view)
        {
        }

        /// <summary>
        /// Handle escape. 
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

        protected override void OnCommitChange()
        {
            var lastInput = (nodeViewModel.NodeModel as Symbol).InputSymbol;
            if (lastInput.Equals(InnerTextEditor.Text))
                return;

            dynamoViewModel.ExecuteCommand(
                new DynCmd.UpdateModelValueCommand(
                    nodeViewModel.WorkspaceViewModel.Model.Guid,
                    nodeViewModel.NodeModel.GUID, "InputSymbol",
                    InnerTextEditor.Text));
        }
    }
}
