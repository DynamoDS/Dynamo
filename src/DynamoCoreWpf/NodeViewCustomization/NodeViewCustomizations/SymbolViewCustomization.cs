using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

using Dynamo.Controls;
using Dynamo.Nodes;
using Dynamo.UI.Controls;

namespace Dynamo.Wpf
{
    public class SymbolViewCustomization : INodeViewCustomization<Dynamo.Nodes.Symbol>
    {
        public void CustomizeView(Dynamo.Nodes.Symbol symbol, NodeView nodeView)
        {
            var input = new ParameterEditor(nodeView.ViewModel);

            nodeView.inputGrid.Children.Add(input);
            Grid.SetColumn(input, 0);
            Grid.SetRow(input, 0);

            input.DataContext = this;
            input.SetBinding(ParameterEditor.ParameterProperty,
                new Binding("InputSymbol")
                {
                    Mode = BindingMode.OneWay,
                    Source = symbol
                });
        }

        public void Dispose()
        {

        }
    }
}