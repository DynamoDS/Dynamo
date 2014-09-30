using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

using Dynamo.Controls;
using Dynamo.Nodes;

using Binding = System.Windows.Data.Binding;
using VerticalAlignment = System.Windows.VerticalAlignment;

namespace Dynamo.Wpf
{
    public class StringInputNodeViewCustomization : StringNodeViewCustomization
    {
        public void CustomizeView(StringInput stringInput, dynNodeView nodeUI)
        {
            base.CustomizeView(stringInput, nodeUI);

            //add a text box to the input grid of the control
            var tb = new StringTextBox
            {
                AcceptsReturn = true,
                AcceptsTab = true,
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = 200,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            nodeUI.inputGrid.Children.Add(tb);
            Grid.SetColumn(tb, 0);
            Grid.SetRow(tb, 0);

            tb.DataContext = stringInput;
            tb.BindToProperty(new Binding("Value")
            {
                Mode = BindingMode.TwoWay,
                Converter = new StringDisplay(),
                Source = stringInput,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            });
        }
    }
}

