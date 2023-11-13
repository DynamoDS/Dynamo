using CoreNodeModels;
using CoreNodeModels.Logic;
using CoreNodeModels.Properties;
using Dynamo.Controls;
using Dynamo.UI;
using Dynamo.Wpf;
using Dynamo.Wpf.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace CoreNodeModelsWpf.Nodes
{
    public class GateNodeViewCustomization : INodeViewCustomization<Gate>
    {
        public void CustomizeView(Gate model, NodeView nodeView)
        {
            //add a text box to the input grid of the control
            var rbTrue = new RadioButton();
            var rbFalse = new RadioButton();
            rbTrue.Style = rbFalse.Style = (Style)SharedDictionaryManager.DynamoModernDictionary["RadioButton"];

            //use a unique name for the button group
            //so other instances of this element don't get confused
            string groupName = Guid.NewGuid().ToString();
            rbTrue.GroupName = groupName;
            rbFalse.GroupName = groupName;

            rbTrue.Content = CoreNodeModelWpfResources.GateOpen;
            rbTrue.Padding = new Thickness(0, 0, 12, 0);
            rbFalse.Content = CoreNodeModelWpfResources.GateClose;
            rbFalse.Padding = new Thickness(0);
            var wp = new WrapPanel
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Margin = new Thickness(10, 5, 10, 0),
                Orientation = Orientation.Horizontal
            };

            wp.Children.Add(rbTrue);
            wp.Children.Add(rbFalse);
            nodeView.inputGrid.Children.Add(wp);

            rbFalse.DataContext = model;
            rbTrue.DataContext = model;

            var rbTrueBinding = new Binding("Value") { Mode = BindingMode.TwoWay, };
            rbTrue.SetBinding(ToggleButton.IsCheckedProperty, rbTrueBinding);

            var rbFalseBinding = new Binding("Value")
            {
                Mode = BindingMode.TwoWay,
                Converter = new InverseBoolDisplay()
            };
            rbFalse.SetBinding(ToggleButton.IsCheckedProperty, rbFalseBinding);
        }

        public void Dispose()
        {
            // Nothing to dispose
        }
    }
}
