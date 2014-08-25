using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

using Dynamo.Nodes;

namespace Dynamo.Wpf
{
    internal class VariableInputViewInjector : INodeViewInjector
    {
        public void Inject(Dynamo.Models.NodeModel model, Dynamo.Controls.dynNodeView view)
        {
            var addButton = new DynamoNodeButton(model, "AddInPort") { Content = "+", Width = 20 };
            //addButton.Height = 20;

            var subButton = new DynamoNodeButton(model, "RemoveInPort") { Content = "-", Width = 20 };
            //subButton.Height = 20;

            var wp = new WrapPanel
            {
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            wp.Children.Add(addButton);
            wp.Children.Add(subButton);

            view.inputGrid.Children.Add(wp);

        }
    }
}
