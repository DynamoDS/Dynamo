using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamo.Nodes;
using Dynamo.Controls;
using System.Windows;
using System.Windows.Controls;

namespace Dynamo.Wpf
{
    internal class DSVarArgFunctionNodeViewCustomization : INodeViewCustomization<DSVarArgFunction>
    {
        public void CustomizeView(Nodes.DSVarArgFunction nodeModel, NodeView nodeView)
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
