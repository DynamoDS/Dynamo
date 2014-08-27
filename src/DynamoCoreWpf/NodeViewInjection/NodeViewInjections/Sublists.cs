using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

using Dynamo.Controls;
using Dynamo.Nodes;
using Dynamo.Wpf;

namespace Dynamo.Wpf
{
    public class Sublists : INodeViewInjection
    {
        private Dynamo.Nodes.Sublists sublistsNodeModel;

        public void SetupCustomUIElements(dynNodeView nodeUI)
        {
            this.sublistsNodeModel = nodeUI.ViewModel.NodeModel as Dynamo.Nodes.Sublists;

            //add a text box to the input grid of the control
            var tb = new DynamoTextBox
            {
                Background =
                    new SolidColorBrush(Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF))
            };

            tb.OnChangeCommitted += sublistsNodeModel.ProcessTextForNewInputs;

            tb.HorizontalAlignment = HorizontalAlignment.Stretch;
            tb.VerticalAlignment = VerticalAlignment.Top;

            nodeUI.inputGrid.Children.Add(tb);
            Grid.SetColumn(tb, 0);
            Grid.SetRow(tb, 0);

            tb.DataContext = this;
            tb.BindToProperty(
                new Binding("Value")
                {
                    Mode = BindingMode.TwoWay,
                    Source = this,
                    UpdateSourceTrigger = UpdateSourceTrigger.Explicit
                });
        }

        public void Dispose()
        {

        }
    }
}