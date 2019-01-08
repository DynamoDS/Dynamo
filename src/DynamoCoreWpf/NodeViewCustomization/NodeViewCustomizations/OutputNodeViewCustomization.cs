using System.Windows.Controls;
using System.Windows.Data;

using Dynamo.Controls;
using Dynamo.Graph.Nodes.CustomNodes;
using Dynamo.UI.Controls;

namespace Dynamo.Wpf
{
    public class OutputNodeViewCustomization : INodeViewCustomization<Output>
    {
        public void CustomizeView(Output outputNodeModel, NodeView nodeView)
        {
            var output = new OutputEditor(nodeView);
            nodeView.inputGrid.Children.Add(output);
            
            Grid.SetColumn(output, 0);
            Grid.SetRow(output, 0);

            output.DataContext = this;
            output.SetBinding(OutputEditor.CodeProperty,
                new Binding("Symbol")
                {
                    Mode = BindingMode.OneWay,
                    Source = outputNodeModel 
                });
        }

        public void Dispose()
        {

        }
    }
}