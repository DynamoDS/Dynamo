using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using CoreNodeModels;
using CoreNodeModelsWpf.Controls;
using Dynamo.Controls;
using Dynamo.Wpf;
using Res = Dynamo.Wpf.Properties.Resources;

namespace CoreNodeModelsWpf.Nodes
{
    /// <summary>
    /// View customizer for DefineData node model.
    /// </summary>
    public class DefineDataNodeViewCustomization : DropDownNodeViewCustomization, INodeViewCustomization<DefineData>
    {
        private ComboBox dropdown;
        private ToggleButton modeToggleButton;
        private ToggleButton listToggleButton;
        private DefineData _model;

        /// <summary>
        /// Customize the visual appearance of the DefineData node.
        /// </summary>
        /// <param name="model">The DefineData model to customize</param>
        /// <param name="nodeView"></param>
        public void CustomizeView(DefineData model, NodeView nodeView)
        {
            this._model = model;

            var formControl = new DefineDataControl(model);
            listToggleButton = formControl.listToggleBtn;
            listToggleButton.Click += listToggle_IsClicked;

            nodeView.inputGrid.Margin = new Thickness(5, 0, 5, 0);
            nodeView.inputGrid.RowDefinitions.Add(new RowDefinition());
            nodeView.inputGrid.RowDefinitions.Add(new RowDefinition());
            nodeView.inputGrid.ColumnDefinitions.Add(new ColumnDefinition());
            nodeView.inputGrid.ColumnDefinitions.Add(new ColumnDefinition());

            Grid.SetRow(formControl, 1);
            Grid.SetColumn(formControl, 0);
            Grid.SetColumnSpan(formControl, 2); 
            nodeView.inputGrid.Children.Add(formControl);
            nodeView.SnapsToDevicePixels = true;
            nodeView.UseLayoutRounding = true;

            RenderOptions.SetBitmapScalingMode(nodeView, BitmapScalingMode.NearestNeighbor);

            // Add the dropdown
            base.CustomizeView(model, nodeView);

            var style = (Style)Dynamo.UI.SharedDictionaryManager.DynamoModernDictionary["NodeViewComboBox"];
            dropdown = (ComboBox)nodeView.inputGrid.Children[1];
            dropdown.Style = style;

            formControl.BaseComboBox = dropdown;

            // Add margin to the dropdown to show the expander
            dropdown.Margin = new Thickness(0, 0, 0, 5);
            dropdown.VerticalAlignment = VerticalAlignment.Top;
            dropdown.MinWidth = 220;
            dropdown.FontSize = 12;

            // subscribe to the event inside the NodeModel to detect user interacting with the dropdown
            dropdown.DropDownOpened += dropDown_DropDownOpened;
            dropdown.DropDownClosed += dropDown_DropDownClosed;

            // Create the Grid as the root visual for the item template
            var gridFactory = new FrameworkElementFactory(typeof(Grid));
            var col = new FrameworkElementFactory(typeof(ColumnDefinition));
            gridFactory.AppendChild(col);

            var pathFactory = new FrameworkElementFactory(typeof(Path));
            pathFactory.SetValue(Path.StrokeProperty, Brushes.White); 
            pathFactory.SetValue(Path.UseLayoutRoundingProperty, true);
            pathFactory.SetValue(Path.SnapsToDevicePixelsProperty, true);
            pathFactory.SetValue(Path.StrokeThicknessProperty, 0.5); 
            pathFactory.SetValue(Path.StrokeDashArrayProperty, new DoubleCollection(new double[] { 4, 5 }));
            pathFactory.SetValue(Path.MarginProperty, new Thickness(0, -5, 0, -5));
            pathFactory.SetBinding(Path.VisibilityProperty, new Binding("Item.Level") { Converter = new LevelToVisibilityConverter() });

            var pathDataBinding = new MultiBinding { Converter = new LevelAndLastChildPropertyToPathGeometryConverter() };
            pathDataBinding.Bindings.Add(new Binding("Item.Level"));
            pathDataBinding.Bindings.Add(new Binding("Item.IsLastChild"));
            pathFactory.SetBinding(Path.DataProperty, pathDataBinding);
            pathFactory.SetValue(Grid.ColumnProperty, 0);

            gridFactory.AppendChild(pathFactory);

            // TextBlock setup
            FrameworkElementFactory textBlockFactory = new FrameworkElementFactory(typeof(TextBlock));
            textBlockFactory.SetBinding(TextBlock.TextProperty, new Binding("Name"));
            textBlockFactory.SetBinding(TextBlock.MarginProperty, new Binding("Item.Level") { Converter = new LevelToIndentConverter() });
            textBlockFactory.SetValue(Grid.ColumnProperty, 0); 

            // Add line and TextBlock to the Grid
            gridFactory.AppendChild(textBlockFactory);

            // Create and seal the DataTemplate
            DataTemplate itemTemplate = new DataTemplate { VisualTree = gridFactory };
            itemTemplate.Seal();
            dropdown.ItemTemplate = itemTemplate;

            Grid.SetRow(dropdown, 0);
            Grid.SetColumn(dropdown, 0);

            // Mask over the dropdown to display the selected value without the indentation
            var selectedItemDisplay = new TextBox
            {
                IsReadOnly = true,
                Focusable = false,
                IsHitTestVisible = false,
                Margin = new Thickness(0, 0, 0, 0),
                Padding = new Thickness(2, -1, 0, 0),
                VerticalAlignment = VerticalAlignment.Top,
                VerticalContentAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left,
                Width = 160,
                Height = 30,
                FontSize = 12,
                Background = new SolidColorBrush(Color.FromRgb(42, 42, 42)),
                BorderBrush = Brushes.Transparent,
                Foreground = new SolidColorBrush(Color.FromRgb(199, 199, 199)),
            };  
    
            var selectedItemBinding = new Binding("DisplayValue")
            {
                Source = model, 
                Mode = BindingMode.OneWay
            };
            selectedItemDisplay.SetBinding(TextBox.TextProperty, selectedItemBinding);

            Grid.SetRow(selectedItemDisplay, 0); 
            Grid.SetColumn(selectedItemDisplay, 0);

            nodeView.inputGrid.Children.Add(selectedItemDisplay);

            // Add the padlock button
            var toggleButtonStyle = (Style)Dynamo.UI.SharedDictionaryManager.DynamoModernDictionary["PadlockToggleButton"];
            var toggleButtonTooltipStyle = (Style)Dynamo.UI.SharedDictionaryManager.DynamoModernDictionary["GenericToolTipLight"];
            modeToggleButton = new ToggleButton();
            modeToggleButton.Style = toggleButtonStyle;

            Binding isToggleCheckedBinding = new Binding("IsAutoMode");
            isToggleCheckedBinding.Mode = BindingMode.TwoWay; 
            isToggleCheckedBinding.Source = model;
            modeToggleButton.SetBinding(ToggleButton.IsCheckedProperty, isToggleCheckedBinding);

            modeToggleButton.Margin = new Thickness(5, 0, 0, 5);
            modeToggleButton.HorizontalAlignment = HorizontalAlignment.Right;
            modeToggleButton.VerticalAlignment = VerticalAlignment.Center;

            var toggleButtonToolTip = new ToolTip
            {
                Content = Res.ResourceManager.GetString(nameof(Res.DataInputNodeModeLockTooltip), CultureInfo.InvariantCulture),
                Style = toggleButtonTooltipStyle 
            };

            modeToggleButton.ToolTip = toggleButtonToolTip;

            Grid.SetRow(modeToggleButton, 0); 
            Grid.SetColumn(modeToggleButton, 1); 
            nodeView.inputGrid.Children.Add(modeToggleButton);
        }

        public new void Dispose()
        {
            if (dropdown != null)
            {
                dropdown.DropDownOpened -= dropDown_DropDownOpened;
                dropdown.DropDownClosed -= dropDown_DropDownClosed;
            }

            if (listToggleButton != null)
            {
                listToggleButton.Click -= listToggle_IsClicked;
            }
        }

        private void dropDown_DropDownOpened(object sender, EventArgs e)
        {
            prevIndex = (sender as ComboBox).SelectedIndex;
        }

        private int prevIndex = 0;

        private async void dropDown_DropDownClosed(object sender, EventArgs e)
        {
            var dropDown = sender as ComboBox;
            var selection = dropDown.SelectedIndex;
            var selectedValue = dropDown.SelectedValue as DynamoDropDownItem;

            if (modeToggleButton != null && modeToggleButton.IsChecked == true && selection != prevIndex)
            {
                // Set the SelectedString directly, then lock the node
                if (_model != null)
                {
                    _model.SelectedString = selectedValue.Name;
                }
                modeToggleButton.IsChecked = false;
            }
        }

        private void listToggle_IsClicked(object sender, RoutedEventArgs e)
        {
            if (modeToggleButton != null && modeToggleButton.IsChecked == true)
            {
                modeToggleButton.IsChecked = false;
            }
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
            
        public class LevelToVisibilityConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value is int level && level > 0)
                {
                    return Visibility.Visible;
                }
                return Visibility.Hidden;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        public class LevelAndLastChildPropertyToPathGeometryConverter : IMultiValueConverter
        {
            public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
            {
                if (values[0] is int level && values[1] is bool isLastChild)
                {
                    double lineLength = -2 + (level * 20);

                    var simpleGeometry = new PathGeometry();
                    var horLine = new PathFigure { StartPoint = new Point(1, 13) };
                    horLine.Segments.Add(new LineSegment(new Point(lineLength, 13), true)); 
                    simpleGeometry.Figures.Add(horLine);
                        
                    var vertLine = new PathFigure { StartPoint = new Point(0, 0) };
                    vertLine.Segments.Add(new LineSegment(new Point(0, isLastChild ? 14 : 28), true)); 
                    simpleGeometry.Figures.Add(vertLine);
                    RenderOptions.SetEdgeMode(simpleGeometry, EdgeMode.Aliased);

                    return simpleGeometry;
                }

                return null; 
            }

            public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }
            
        public class BoolToVisibilityConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return value is bool && (bool)value ? Visibility.Collapsed : Visibility.Visible;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }
    }
}
