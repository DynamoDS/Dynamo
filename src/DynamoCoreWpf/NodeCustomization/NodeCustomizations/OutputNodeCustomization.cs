using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

using Dynamo.Controls;
using Dynamo.Nodes;

namespace Dynamo.Wpf
{
    public class OutputNodeCustomization : INodeCustomization<Dynamo.Nodes.Output>
    {
        private Dynamo.Nodes.Output outputNodeModel;

        public void CustomizeView(Nodes.Output outputNodeModel, dynNodeView nodeView)
        {
            this.outputNodeModel = outputNodeModel;

            //add a text box to the input grid of the control
            var tb = new DynamoTextBox(outputNodeModel.Symbol)
            {
                DataContext = nodeView.ViewModel,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                Background =
                    new SolidColorBrush(Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF))
            };

            nodeView.inputGrid.Children.Add(tb);
            Grid.SetColumn(tb, 0);
            Grid.SetRow(tb, 0);

            tb.BindToProperty(
                new Binding("Symbol")
                {
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.Explicit
                });
        }

        public void Dispose()
        {

        }
    }
}