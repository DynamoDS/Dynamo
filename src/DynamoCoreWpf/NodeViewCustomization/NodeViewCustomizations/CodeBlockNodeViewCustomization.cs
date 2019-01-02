using System.Windows.Controls;
using System.Windows.Data;

using Dynamo.Controls;
using Dynamo.Graph.Nodes;
using Dynamo.UI.Controls;

namespace Dynamo.Wpf
{
    public class CodeBlockNodeViewCustomization : INodeViewCustomization<CodeBlockNodeModel>
    {
        public void CustomizeView(CodeBlockNodeModel model, NodeView nodeView)
        {
            var cbe = new CodeBlockEditor(nodeView);

            nodeView.inputGrid.Children.Add(cbe);
            Grid.SetColumn(cbe, 0);
            Grid.SetRow(cbe, 0);

            cbe.SetBinding(CodeBlockEditor.CodeProperty,
                new Binding("Code")
                {
                    Mode = BindingMode.OneWay,
                    NotifyOnValidationError = false,
                    Source = model,
                });

            cbe.GotFocus += (s, args) => nodeView.TogglePreviewControlAllowance();
            cbe.LostFocus += (s, args) => nodeView.TogglePreviewControlAllowance();
            if (model.ShouldFocus)
            {
                cbe.Focus();
                model.ShouldFocus = false;
            }
        }

        public void Dispose()
        {

        }
    }
}