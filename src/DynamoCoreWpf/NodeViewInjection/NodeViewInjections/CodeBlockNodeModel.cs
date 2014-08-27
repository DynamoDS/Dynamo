using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

using Dynamo.Controls;
using Dynamo.Nodes;
using Dynamo.UI;
using Dynamo.Wpf;

namespace Dynamo.Wpf
{
    public class CodeBlock : INodeViewInjection
    {
        private CodeBlockNodeModel codeBlockNodeModel;

        public void SetupCustomUIElements(dynNodeView nodeUI)
        {
            this.codeBlockNodeModel = nodeUI.ViewModel.NodeModel as CodeBlockNodeModel;

            var tb = new CodeNodeTextBox(this.codeBlockNodeModel.Code )
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Background =
                    new SolidColorBrush(Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF)),
                AcceptsReturn = true,
                MaxWidth = Configurations.CBNMaxTextBoxWidth,
                TextWrapping = TextWrapping.Wrap
            };

            nodeUI.inputGrid.Children.Add(tb);
            Grid.SetColumn(tb, 0);
            Grid.SetRow(tb, 0);

            tb.DataContext = nodeUI.ViewModel;
            tb.BindToProperty(
                new Binding("Code")
                {
                    Mode = BindingMode.TwoWay,
                    NotifyOnValidationError = false,
                    Source = this,
                    UpdateSourceTrigger = UpdateSourceTrigger.Explicit
                });

            if (this.codeBlockNodeModel.ShouldFocus)
            {
                tb.Focus();
                this.codeBlockNodeModel.ShouldFocus = false;
            }
        }

        public void Dispose()
        {

        }
    }
}