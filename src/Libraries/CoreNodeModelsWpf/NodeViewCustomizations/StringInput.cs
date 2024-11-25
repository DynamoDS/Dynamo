using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using CoreNodeModels.Input;
using Dynamo.Controls;
using Dynamo.Nodes;
using Dynamo.UI.Prompts;
using Dynamo.ViewModels;
using Dynamo.Wpf;
using Binding = System.Windows.Data.Binding;
using VerticalAlignment = System.Windows.VerticalAlignment;

namespace CoreNodeModelsWpf.Nodes
{
    public class StringInputNodeViewCustomization : INodeViewCustomization<StringInput>
    {
        private DynamoViewModel dynamoViewModel;
        private StringInput nodeModel;
        private MenuItem editWindowItem;

        public void CustomizeView(StringInput stringInput, NodeView nodeView)
        {
            this.nodeModel = stringInput;
            this.dynamoViewModel = nodeView.ViewModel.DynamoViewModel;

            this.editWindowItem = new MenuItem
            {
                Header = Dynamo.Wpf.Properties.Resources.StringInputNodeEditMenu, 
                IsCheckable = false
            };
            nodeView.MainContextMenu.Items.Add(editWindowItem);

            editWindowItem.Click += editWindowItem_Click;

            //add a text box to the input grid of the control
            var tb = new StringTextBox
            {
                AcceptsReturn = true,
                AcceptsTab = true,
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = 200,
                VerticalAlignment = VerticalAlignment.Top
            };

            nodeView.inputGrid.Children.Add(tb);
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

            // Add resize thumb using the helper method
            AddResizeThumb(tb, nodeView.inputGrid);
        }

        private void AddResizeThumb(StringTextBox tb, Grid inputGrid)
        {
            // Create the resize thumb
            var resizeThumb = new Thumb
            {
                Width = 10,
                Height = 10,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                Cursor = Cursors.SizeNWSE,
                Margin = new Thickness(0, 0, 3, 3)
            };

            // Define the shape for the thumb
            var template = new ControlTemplate(typeof(Thumb));
            var polygon = new FrameworkElementFactory(typeof(System.Windows.Shapes.Polygon));
            polygon.SetValue(System.Windows.Shapes.Polygon.FillProperty, new SolidColorBrush(Color.FromRgb(175, 175, 175)));
            polygon.SetValue(System.Windows.Shapes.Polygon.PointsProperty, new PointCollection(new[] { new Point(0, 8), new Point(8, 8), new Point(8, 0) }));
            template.VisualTree = polygon;
            resizeThumb.Template = template;

            // Handle resizing
            resizeThumb.DragDelta += (s, e) =>
            {
                if (tb.MaxWidth == 200)
                {
                    tb.MaxWidth = double.PositiveInfinity;
                }

                var newWidth = tb.ActualWidth + e.HorizontalChange;
                var newHeight = tb.ActualHeight + e.VerticalChange;

                tb.MinWidth = 100;
                tb.MinHeight = 38;

                if (newWidth >= tb.MinWidth)
                    tb.Width = newWidth;

                if (newHeight >= tb.MinHeight)
                    tb.Height = newHeight;
            };

            // Add the thumb to the input grid
            inputGrid.Children.Add(resizeThumb);
        }

        public void editWindowItem_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new EditWindow(this.dynamoViewModel) { DataContext = this.nodeModel };
            editWindow.BindToProperty(
                null,
                new Binding("Value")
                {
                    Mode = BindingMode.TwoWay,
                    Converter = new StringDisplay(),
                    NotifyOnValidationError = false,
                    Source = this.nodeModel,
                    UpdateSourceTrigger = UpdateSourceTrigger.Explicit
                });

            editWindow.ShowDialog();
        }

        public void Dispose()
        {
            editWindowItem.Click -= editWindowItem_Click;
        }
    }
}

