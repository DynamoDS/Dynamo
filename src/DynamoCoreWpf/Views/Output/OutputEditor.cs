using Dynamo.Controls;
using Dynamo.Graph.Nodes.CustomNodes;

namespace Dynamo.UI.Controls
{
    /// <summary>
    /// Syntax highligh editor for Output node.
    /// </summary>
    public class OutputEditor : CodeCompletionEditor
    {
        private Output output;

        /// <summary>
        /// Create syntax highligh editor by the view of output node.
        /// </summary>
        /// <param name="view"></param>
        public OutputEditor(NodeView view) : base(view)
        {
            output = nodeViewModel.NodeModel as Output;
        }

        /// <summary>
        /// Handler for escape key.
        /// </summary>
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

        /// <summary>
        /// Handler for commit. The commit event is triggered by
        /// clicking mouse outside the editor.
        /// </summary>
        protected override void OnCommitChange()
        {
            var lastInput = output.Symbol;
            if (lastInput.Equals(InnerTextEditor.Text))
                return;

            UpdateNodeValue("Symbol");
        }
    }
}
