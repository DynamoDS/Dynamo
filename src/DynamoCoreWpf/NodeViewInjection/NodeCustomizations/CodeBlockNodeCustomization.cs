using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

using Dynamo.Controls;
using Dynamo.Nodes;
using Dynamo.UI;

namespace Dynamo.Wpf
{
    public class CodeBlockNodeCustomization : INodeCustomization<CodeBlockNodeModel>
    {
        private CodeBlockNodeModel codeBlockNodeModel;

        public void SetupCustomUIElements(CodeBlockNodeModel model, dynNodeView nodeView)
        {
            this.codeBlockNodeModel = model;

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

            nodeView.inputGrid.Children.Add(tb);
            Grid.SetColumn(tb, 0);
            Grid.SetRow(tb, 0);

            tb.DataContext = nodeView.ViewModel;
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