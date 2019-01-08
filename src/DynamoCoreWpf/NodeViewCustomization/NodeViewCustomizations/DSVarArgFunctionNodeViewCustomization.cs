using System.Windows;
using System.Windows.Controls;
using Dynamo.Controls;
using Dynamo.Graph.Nodes.ZeroTouch;
using Dynamo.Nodes;

namespace Dynamo.Wpf
{
    internal class DSVarArgFunctionNodeViewCustomization : INodeViewCustomization<DSVarArgFunction>
    {
        public void CustomizeView(DSVarArgFunction nodeModel, NodeView nodeView)
        {
            var addButton = new DynamoNodeButton(nodeView.ViewModel.NodeModel, "AddInPort") { Content = "+", Width = 20 };
            var subButton = new DynamoNodeButton(nodeView.ViewModel.NodeModel, "RemoveInPort") { Content = "-", Width = 20 };

            var wp = new WrapPanel
            {
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            wp.Children.Add(addButton);
            wp.Children.Add(subButton);

            nodeView.inputGrid.Children.Add(wp);
        }

        public void Dispose()
        {
        }
    }
}
