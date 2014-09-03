using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Wpf;

namespace Dynamo.Wpf
{
    public class DoubleInputNodeCustomization : INodeCustomization<DoubleInput>
    {
        private DoubleInput doubleInput;

        public void CustomizeView(DoubleInput nodeModel, dynNodeView nodeView)
        {
            doubleInput = nodeView.ViewModel.NodeModel as Dynamo.Nodes.DoubleInput;

            //add a text box to the input grid of the control
            var tb = new DynamoTextBox(doubleInput.Value ?? "0.0")
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Background =
                    new SolidColorBrush(Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF))
            };

            nodeView.inputGrid.Children.Add(tb);
            Grid.SetColumn(tb, 0);
            Grid.SetRow(tb, 0);

            tb.DataContext = this;

            tb.BindToProperty(new Binding("Value")
            {
                Mode = BindingMode.TwoWay,
                Converter = new DoubleInputDisplay(),
                NotifyOnValidationError = false,
                Source = this,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            });
        }

        public void Dispose()
        {
        }
    }
}