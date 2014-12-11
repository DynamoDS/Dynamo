using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

using Dynamo.Controls;
using Dynamo.Nodes;
using Dynamo.Wpf;

namespace Dynamo.Wpf
{
    public class SublistsViewCustomization : INodeViewCustomization<Dynamo.Nodes.Sublists>
    {
        private Dynamo.Nodes.Sublists sublistsNodeModel;
        private DynamoTextBox tb;

        public void CustomizeView(Dynamo.Nodes.Sublists element, NodeView nodeView)
        {
            this.sublistsNodeModel = element;

            //add a text box to the input grid of the control
            var tb = new DynamoTextBox
            {
                Background =
                    new SolidColorBrush(Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF))
            };

            tb.OnChangeCommitted += sublistsNodeModel.ProcessTextForNewInputs;

            tb.HorizontalAlignment = HorizontalAlignment.Stretch;
            tb.VerticalAlignment = VerticalAlignment.Top;

            nodeView.inputGrid.Children.Add(tb);
            Grid.SetColumn(tb, 0);
            Grid.SetRow(tb, 0);

            tb.DataContext = element;
            tb.BindToProperty(
                new Binding("Value")
                {
                    Mode = BindingMode.TwoWay,
                    Source = this.sublistsNodeModel,
                    UpdateSourceTrigger = UpdateSourceTrigger.Explicit
                });
        }

        public void Dispose()
        {
            tb.OnChangeCommitted -= sublistsNodeModel.ProcessTextForNewInputs;
        }
    }
}