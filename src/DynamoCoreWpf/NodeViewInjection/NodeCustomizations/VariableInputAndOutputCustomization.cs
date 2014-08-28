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
    public class VariableInputAndOutputCustomization : INodeCustomization<Dynamo.Nodes.VariableInputAndOutput>
    {
        public void SetupCustomUIElements(Dynamo.Nodes.VariableInputAndOutput model, dynNodeView nodeView)
        {
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

            nodeView.inputGrid.Children.Add(wp);
        }

        public void Dispose()
        {
        }
    }
}