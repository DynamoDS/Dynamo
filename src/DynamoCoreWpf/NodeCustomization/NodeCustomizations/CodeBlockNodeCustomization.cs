using System;
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
        public void CustomizeView(CodeBlockNodeModel model, dynNodeView nodeView)
        {
            var tb = new CodeNodeTextBox(model.Code )
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

            if (model.ShouldFocus)
            {
                tb.Focus();
                model.ShouldFocus = false;
            }
        }

        public void Dispose()
        {

        }
    }
}