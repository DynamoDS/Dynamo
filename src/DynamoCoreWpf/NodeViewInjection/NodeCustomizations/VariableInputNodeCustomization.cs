using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

using Dynamo.Controls;
using Dynamo.Core;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Wpf;

namespace Dynamo.Wpf
{
    public abstract class VariableInputNodeCustomization : INodeCustomization<Dynamo.Nodes.VariableInput>
    {
        public void SetupCustomUIElements(Nodes.VariableInput nodeModel, dynNodeView nodeView)
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