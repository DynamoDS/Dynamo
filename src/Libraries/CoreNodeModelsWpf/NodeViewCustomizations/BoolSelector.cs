using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using CoreNodeModels.Input;
using Dynamo.Controls;
using Dynamo.UI;
using Dynamo.ViewModels;
using Dynamo.Wpf;
using Color = System.Windows.Media.Color;
using FontStyle = System.Windows.FontStyle;

namespace CoreNodeModelsWpf.Nodes
{
    public class BoolSelectorNodeViewCustomization : INodeViewCustomization<BoolSelector>
    {
        private DynamoViewModel dynamoViewModel;

        private void OnRadioButtonClicked(object sender, RoutedEventArgs e)
        {
            dynamoViewModel.OnRequestReturnFocusToView();
        }

        public void CustomizeView(BoolSelector model, NodeView nodeView)
        {
            this.dynamoViewModel = nodeView.ViewModel.DynamoViewModel;

            //add a text box to the input grid of the control
            var rbTrue = new RadioButton();
            rbTrue.Foreground = new SolidColorBrush(Color.FromArgb(255, 238, 238, 238));
            rbTrue.FontFamily = new System.Windows.Media.FontFamily("Artifakt Element");
            rbTrue.FontSize = 16;

            var rbFalse = new RadioButton();
            rbFalse.Foreground = new SolidColorBrush(Color.FromArgb(255, 238, 238, 238));
            rbFalse.FontFamily = new System.Windows.Media.FontFamily("Artifakt Element");
            rbFalse.FontSize = 16;

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

            rbTrue.Style = (Style)SharedDictionaryManager.DynamoModernDictionary["RadioButton"];
            rbFalse.Style = (Style)SharedDictionaryManager.DynamoModernDictionary["RadioButton"];
        }

        public void Dispose()
        {

        }
    }
}