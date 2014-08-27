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
    public abstract class VariableInput : INodeViewInjection
    {
        public void SetupCustomUIElements(dynNodeView nodeUI)
        {
            var addButton = new DynamoNodeButton(nodeUI.ViewModel.NodeModel, "AddInPort") { Content = "+", Width = 20 };
            var subButton = new DynamoNodeButton(nodeUI.ViewModel.NodeModel, "RemoveInPort") { Content = "-", Width = 20 };

            var wp = new WrapPanel
            {
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            wp.Children.Add(addButton);
            wp.Children.Add(subButton);

            nodeUI.inputGrid.Children.Add(wp);
        }

        public void Dispose()
        {

        }
    }
}