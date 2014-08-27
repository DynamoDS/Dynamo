using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

using Dynamo.Controls;
using Dynamo.Core;
using Dynamo.Models;
using Dynamo.Nodes;

namespace Dynamo.Wpf
{
    public class VariableInputAndOutput : INodeViewInjection
    {
        public void SetupCustomUIElements(dynNodeView nodeUI)
        {
            var model = nodeUI.ViewModel.NodeModel;

            var addButton = new DynamoNodeButton(model, "AddInPort")
            {
                Content = "+",
                Width = 20,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            var subButton = new DynamoNodeButton(model, "RemoveInPort")
            {
                Content = "-",
                Width = 20,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top
            };

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