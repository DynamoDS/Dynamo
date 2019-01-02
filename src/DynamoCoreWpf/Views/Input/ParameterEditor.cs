using Dynamo.Controls;
using Dynamo.Graph.Nodes.CustomNodes;

namespace Dynamo.UI.Controls
{
    /// <summary>
    /// Auto-completion control for input parameter.
    /// </summary>
    public class ParameterEditor : CodeCompletionEditor
    {
        private Symbol input;

        /// <summary>
        /// Create input editor by the view of symbol node.
        /// </summary>
        /// <param name="view"></param>
        public ParameterEditor(NodeView view) : base(view)
        {
            input = nodeViewModel.NodeModel as Symbol;
        }

        protected override void OnEscape()
        {
            var text = InnerTextEditor.Text;
            if (input.InputSymbol != null && text.Equals(input.InputSymbol))
            {
                ReturnFocus();
            }
            else
            {
                InnerTextEditor.Text = input.InputSymbol;
            }
        }

        protected override void OnCommitChange()
        {
            var lastInput = input.InputSymbol;
            if (lastInput.Equals(InnerTextEditor.Text))
                return;

            UpdateNodeValue("InputSymbol");
        }
    }
}
