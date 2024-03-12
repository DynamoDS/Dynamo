using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using CoreNodeModels;
using CoreNodeModelsWpf.Controls;
using Dynamo.Controls;
using Dynamo.Wpf;

namespace CoreNodeModelsWpf.Nodes
{
    /// <summary>
    /// View customizer for DefineData node model.
    /// </summary>
    public class DefineDataNodeViewCustomization : DropDownNodeViewCustomization, INodeViewCustomization<DefineData>
    {
        /// <summary>
        /// Customize the visual appearance of the DefineData node.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="nodeView"></param>
        public void CustomizeView(DefineData model, NodeView nodeView)
        {
            var formControl = new DefineDataControl(model);

            nodeView.inputGrid.Margin = new Thickness(5, 0, 5, 0);
            nodeView.inputGrid.RowDefinitions.Add(new RowDefinition());
            nodeView.inputGrid.RowDefinitions.Add(new RowDefinition());
            nodeView.inputGrid.ColumnDefinitions.Add(new ColumnDefinition());
            nodeView.inputGrid.ColumnDefinitions.Add(new ColumnDefinition());

            Grid.SetRow(formControl, 1);
            Grid.SetColumn(formControl, 0);
            Grid.SetColumnSpan(formControl, 2); 
            nodeView.inputGrid.Children.Add(formControl);

            // Add the dropdown.
            base.CustomizeView(model, nodeView);

            var style = (Style)Dynamo.UI.SharedDictionaryManager.DynamoModernDictionary["NodeViewComboBox"];
            var dropdown = (ComboBox)nodeView.inputGrid.Children[1];
            dropdown.Style = style;

            formControl.BaseComboBox = dropdown;

            // Add margin to the dropdown to show the expander.
            dropdown.Margin = new Thickness(0, 0, 0, 5);
            dropdown.VerticalAlignment = VerticalAlignment.Top;
            dropdown.MinWidth = 220;
            dropdown.FontSize = 12;

            var indentConverter = new LevelToIndentConverter();

            // Dynamically create a DataTemplate for ComboBox items
            var textBlockFactory = new FrameworkElementFactory(typeof(TextBlock));
            textBlockFactory.SetBinding(TextBlock.TextProperty, new Binding("Name")); 
            textBlockFactory.SetBinding(TextBlock.MarginProperty, new Binding("Item.Level") { Converter = indentConverter }); 

            DataTemplate itemTemplate = new DataTemplate { VisualTree = textBlockFactory };
            itemTemplate.Seal();

            // Apply the DataTemplate to the ComboBox
            dropdown.ItemTemplate = itemTemplate;

            Grid.SetRow(dropdown, 0);
            Grid.SetColumn(dropdown, 0); 

            // Add the padlock button
            var toggleButtonStyle = (Style)Dynamo.UI.SharedDictionaryManager.DynamoModernDictionary["PadlockToggleButton"];
            var toggleButton = new ToggleButton();
            toggleButton.Style = toggleButtonStyle;

            Binding isToggleCheckedBinding = new Binding("IsAutoMode");
            isToggleCheckedBinding.Mode = BindingMode.TwoWay; 
            isToggleCheckedBinding.Source = model; 
            toggleButton.SetBinding(ToggleButton.IsCheckedProperty, isToggleCheckedBinding);

            toggleButton.Margin = new Thickness(5, 0, 0, 5); 
            toggleButton.HorizontalAlignment = HorizontalAlignment.Right;
            toggleButton.VerticalAlignment = VerticalAlignment.Center;

            Grid.SetRow(toggleButton, 0); 
            Grid.SetColumn(toggleButton, 1); 
            nodeView.inputGrid.Children.Add(toggleButton);
        }

        public class LevelToIndentConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value is int level)
                {
                    return new Thickness(level * 20, 0, 0, 0); 
                }
                return new Thickness(0);
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }
    }
}
