using System.Windows.Controls;
using System.Windows.Data;
using Dynamo.Controls;
using Dynamo.Graph.Nodes.CustomNodes;
using Dynamo.UI.Controls;

namespace Dynamo.Wpf
{
    public class SymbolViewCustomization : INodeViewCustomization<Symbol>
    {
        public void CustomizeView(Symbol symbol, NodeView nodeView)
        {
            var input = new ParameterEditor(nodeView);

            nodeView.inputGrid.Children.Add(input);
            Grid.SetColumn(input, 0);
            Grid.SetRow(input, 0);

            input.DataContext = this;
            input.SetBinding(ParameterEditor.CodeProperty,
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