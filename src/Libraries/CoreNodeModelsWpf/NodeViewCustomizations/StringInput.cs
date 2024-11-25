using System;
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
        private readonly double defMaxWidthSize = 400;
        private readonly int minWidthSize = 100;
        private readonly int minHeightSize = 33;

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
                //MaxWidth = defMaxWidthSize,
                VerticalAlignment = VerticalAlignment.Top
            };

            RestoreNodeSize(tb, stringInput);

            var c1 = tb.Width;

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

            //add resize thumb using the helper methods
            AddResizeThumb(tb, nodeView.inputGrid, stringInput);
        }

        /// <summary>
        /// Adds a resize thumb to enable dynamic resizing of a StringTextBox, restoring saved dimensions and updating them on resize.
        /// </summary>
        private void AddResizeThumb(StringTextBox tb, Grid inputGrid, StringInput stringInput)
        {
            // Create and configure the resize thumb
            var resizeThumb = CreateResizeThumb();

            var c1 = tb.Width;

            // Restore saved size
            RestoreNodeSize(tb, stringInput);

            var c2 = tb.Width;

            // Handle resizing
            resizeThumb.DragDelta += (s, e) =>
            {
                var c3 = tb.Width;

                if (tb.MaxWidth == defMaxWidthSize)
                {
                    tb.MaxWidth = double.PositiveInfinity;
                }

                var c4 = tb.Width;

                var newWidth = tb.ActualWidth + e.HorizontalChange;
                var newHeight = tb.ActualHeight + e.VerticalChange;
                tb.Width = Math.Max(minWidthSize, newWidth);
                tb.Height = Math.Max(minHeightSize, newHeight);

                // Save the new dimensions to the NodeSizes dictionary
                StringInput.NodeSizes[stringInput.GUID] = new Tuple<double, double>(tb.Width, tb.Height);
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

        #region Helpers

        /// <summary>
        /// Helper to create a resize thumb.
        /// </summary>
        private static Thumb CreateResizeThumb()
        {
            return new Thumb
            {
                Width = 10,
                Height = 10,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                Cursor = Cursors.SizeNWSE,
                Margin = new Thickness(0, 0, 5, 3),
                Template = CreateThumbTemplate()
            };
        }

        /// <summary>
        /// Helper to create the thumb template.
        /// </summary>
        private static ControlTemplate CreateThumbTemplate()
        {
            var template = new ControlTemplate(typeof(Thumb));
            var polygon = new FrameworkElementFactory(typeof(System.Windows.Shapes.Polygon));
            polygon.SetValue(System.Windows.Shapes.Polygon.FillProperty, new SolidColorBrush(Color.FromRgb(175, 175, 175)));
            polygon.SetValue(System.Windows.Shapes.Polygon.PointsProperty, new PointCollection(new[] { new Point(0, 8), new Point(8, 8), new Point(8, 0) }));
            template.VisualTree = polygon;
            return template;
        }

        /// <summary>
        /// Helper to restore node size.
        /// </summary>
        private static void RestoreNodeSize(StringTextBox tb, StringInput stringInput)
        {
            if (StringInput.NodeSizes.TryGetValue(stringInput.GUID, out var savedSize))
            {
                tb.Width = savedSize.Item1;
                tb.Height = savedSize.Item2;

            }
        }

        #endregion

        public void Dispose()
        {
            editWindowItem.Click -= editWindowItem_Click;
        }
    }
}

