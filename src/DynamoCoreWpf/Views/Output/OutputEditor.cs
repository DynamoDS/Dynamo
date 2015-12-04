using Dynamo.Graph.Nodes.CustomNodes;
using Dynamo.Controls;

namespace Dynamo.UI.Controls
{
    /// <summary>
    /// Auto-completion control for input parameter.
    /// </summary>
    public class OutputEditor : CodeCompletionEditor
    {
        private Output output;

        /// <summary>
        /// Create input editor by the view of symbol node.
        /// </summary>
        /// <param name="view"></param>
        public OutputEditor(NodeView view) : base(view)
        {
            output = nodeViewModel.NodeModel as Output;
        }

        protected override void OnEscape()
        {
            var text = InnerTextEditor.Text;
            if (output.Symbol != null && text.Equals(output.Symbol))
            {
                ReturnFocus();
            }
            else
            {
                InnerTextEditor.Text = output.Symbol;
            }
        }

        protected override void OnCommitChange()
        {
            var lastInput = output.Symbol;
            if (lastInput.Equals(InnerTextEditor.Text))
                return;

            UpdateNodeValue("Symbol");
        }
    }
}
