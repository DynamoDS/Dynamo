using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

using Dynamo.Controls;
using Dynamo.Nodes;
using Dynamo.Wpf;

namespace DSCoreNodesUI
{
    public class FormulaNodeViewCustomization : INodeViewCustomization<Formula>
    {
        public void CustomizeView(Formula model, NodeView nodeView)
        {
            var tb = new DynamoTextBox(model.FormulaString)
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
                Background = new SolidColorBrush(Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF))
            };

            nodeView.inputGrid.Children.Add(tb);
            Grid.SetColumn(tb, 0);
            Grid.SetRow(tb, 0);

            tb.DataContext = model;
            tb.BindToProperty(new Binding("FormulaString")
            {
                Mode = BindingMode.TwoWay,
                NotifyOnValidationError = false,
                Source = model,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            });
        }

        public void Dispose()
        {
        }
    }
}