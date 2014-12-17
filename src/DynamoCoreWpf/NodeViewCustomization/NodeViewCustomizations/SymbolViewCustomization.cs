using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

using Dynamo.Controls;
using Dynamo.Nodes;

namespace Dynamo.Wpf
{
    public class SymbolViewCustomization : INodeViewCustomization<Dynamo.Nodes.Symbol>
    {
        public void CustomizeView(Dynamo.Nodes.Symbol symbol, NodeView nodeView)
        {
            //add a text box to the input grid of the control
            var tb = new DynamoTextBox(symbol.InputSymbol)
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                Background =
                    new SolidColorBrush(Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF))
            };

            nodeView.inputGrid.Children.Add(tb);
            Grid.SetColumn(tb, 0);
            Grid.SetRow(tb, 0);

            tb.DataContext = symbol;
            tb.BindToProperty(
                new Binding("InputSymbol")
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