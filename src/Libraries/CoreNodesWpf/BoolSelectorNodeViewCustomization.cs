using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

using Dynamo.Controls;
using Dynamo.ViewModels;
using Dynamo.Wpf;

namespace DSCoreNodesUI
{
    public class BoolSelectorNodeViewCustomization : INodeViewCustomization<BoolSelector>
    {
        private DynamoViewModel dynamoViewModel;

        private void OnRadioButtonClicked(object sender, RoutedEventArgs e)
        {
            dynamoViewModel.ReturnFocusToSearch();
        }

        public void CustomizeView(BoolSelector model, NodeView nodeView)
        {
            this.dynamoViewModel = nodeView.ViewModel.DynamoViewModel;

            //add a text box to the input grid of the control
            var rbTrue = new RadioButton();
            var rbFalse = new RadioButton();
            rbTrue.VerticalAlignment = VerticalAlignment.Center;
            rbFalse.VerticalAlignment = VerticalAlignment.Center;

            //use a unique name for the button group
            //so other instances of this element don't get confused
            string groupName = Guid.NewGuid().ToString();
            rbTrue.GroupName = groupName;
            rbFalse.GroupName = groupName;

            rbTrue.Content = "True";
            rbTrue.Padding = new Thickness(0,0,12,0);
            rbFalse.Content = "False";
            rbFalse.Padding = new Thickness(0);
            var wp = new WrapPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Margin = new Thickness(10,5,10,0),
                Orientation = Orientation.Horizontal
            };

            wp.Children.Add(rbTrue);
            wp.Children.Add(rbFalse);
            nodeView.inputGrid.Children.Add(wp);

            //rbFalse.IsChecked = true;
            rbTrue.Checked += OnRadioButtonClicked;
            rbFalse.Checked += OnRadioButtonClicked;

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

        }
    }
}