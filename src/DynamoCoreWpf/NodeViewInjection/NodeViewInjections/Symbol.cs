using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

using Dynamo.Controls;
using Dynamo.Nodes;

namespace Dynamo.Wpf
{
    public class Symbol : INodeViewInjection
    {
        public void SetupCustomUIElements(dynNodeView nodeUI)
        {
            var symbol = nodeUI.ViewModel.NodeModel as Dynamo.Nodes.Symbol;

            //add a text box to the input grid of the control
            var tb = new DynamoTextBox(symbol.InputSymbol)
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                Background =
                    new SolidColorBrush(Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF))
            };

            nodeUI.inputGrid.Children.Add(tb);
            Grid.SetColumn(tb, 0);
            Grid.SetRow(tb, 0);

            tb.DataContext = this;
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