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
        private readonly double defMaxWidthSize = 800;
        private readonly int minWidthSize = 100;
        private readonly int minHeightSize = 31;

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
                MaxWidth = defMaxWidthSize,
                VerticalAlignment = VerticalAlignment.Top,
            };

            //RestoreNodeSize(tb, stringInput);

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

            // Apply deserialized Width and Height to the nodeView
            tb.Width = stringInput.Width;  // Node's width
            tb.Height = stringInput.Height; // Node's height

            ////add resize thumb using the helper methods
            AddResizeThumb(tb, nodeView.inputGrid, stringInput);

            
        }

        /// <summary>
        /// Adds a resize thumb to enable dynamic resizing of a StringTextBox.
        /// </summary>
        private void AddResizeThumb(StringTextBox tb, Grid inputGrid, StringInput stringInput)
        {
            var resizeThumb = CreateResizeThumb();
            inputGrid.Children.Add(resizeThumb);

            //// Add event listener to toggle the thumb's visibility based on the text box width
            //tb.SizeChanged += (sender, args) =>
            //{
            //    resizeThumb.Visibility = tb.ActualWidth <= minWidthSize ?
            //    Visibility.Collapsed : Visibility.Visible;
            //};

            //// Ensure the thumb visibility aligns with the initial width
            //resizeThumb.Visibility = stringInput.Width <= minWidthSize ?
            //    Visibility.Collapsed : Visibility.Visible;

            // Handle resizing logic in the resize thumb
            resizeThumb.DragDelta += (s, e) =>
            {
                if (tb.MaxWidth == defMaxWidthSize)
                {
                    tb.MaxWidth = double.PositiveInfinity;
                }

                var newWidth = tb.ActualWidth + e.HorizontalChange;
                var newHeight = tb.ActualHeight + e.VerticalChange;

                tb.Width = Math.Max(minWidthSize, newWidth);
                tb.Height = Math.Max(minHeightSize, newHeight);

                stringInput.Width = tb.ActualWidth;
                stringInput.Height = tb.ActualHeight;
            };
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
                Margin = new Thickness(0, 0, 3, 3),
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
        //private static void RestoreNodeSize(StringTextBox tb, StringInput stringInput)
        //{
        //    if (StringInput.NodeSizes.TryGetValue(stringInput.GUID, out var savedSize))
        //    {
        //        tb.Width = savedSize.Item1;
        //        tb.Height = savedSize.Item2;

        //    }
        //}

        #endregion

        public void Dispose()
        {
            editWindowItem.Click -= editWindowItem_Click;
        }
    }
}

