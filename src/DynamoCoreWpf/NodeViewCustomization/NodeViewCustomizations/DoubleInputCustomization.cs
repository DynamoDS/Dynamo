using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

using Dynamo.Controls;
using Dynamo.Nodes;

namespace Dynamo.Wpf
{
    public class DoubleInputNodeViewCustomization : INodeViewCustomization<DoubleInput>
    {
        public void CustomizeView(DoubleInput nodeModel, NodeView nodeView)
        {
            //add a text box to the input grid of the control
            var tb = new DynamoTextBox(nodeModel.Value ?? "0.0")
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Background =
                    new SolidColorBrush(Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF))
            };

            nodeView.inputGrid.Children.Add(tb);
            Grid.SetColumn(tb, 0);
            Grid.SetRow(tb, 0);

            tb.DataContext = nodeModel;

            tb.BindToProperty(new Binding("Value")
            {
                Mode = BindingMode.TwoWay,
                Converter = new DoubleInputDisplay(),
                NotifyOnValidationError = false,
                Source = nodeModel,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            });
        }

        public void Dispose()
        {
        }
    }
}