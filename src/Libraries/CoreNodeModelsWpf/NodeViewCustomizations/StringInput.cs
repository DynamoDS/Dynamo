using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using CoreNodeModels.Input;
using Dynamo.Controls;
using Dynamo.Graph.Workspaces;
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
        private WorkspaceModel workspace;
        private StringInput nodeModel;
        private MenuItem editWindowItem;
        private readonly int minWidthSize = 100;
        private readonly int minHeightSize = 34;
        private readonly int defMaxWidthSize = 200;

        public void CustomizeView(StringInput stringInput, NodeView nodeView)
        {
            this.nodeModel = stringInput;
            this.dynamoViewModel = nodeView.ViewModel.DynamoViewModel;
            this.workspace = this.dynamoViewModel.CurrentSpace;

            this.editWindowItem = new MenuItem
            {
                Header = Dynamo.Wpf.Properties.Resources.StringInputNodeEditMenu, 
                IsCheckable = false
            };
            nodeView.MainContextMenu.Items.Add(editWindowItem);

            editWindowItem.Click += editWindowItem_Click;

            // Add a text box to the input grid of the control
            var tb = new StringTextBox
            {
                AcceptsReturn = true,
                AcceptsTab = true,
                TextWrapping = TextWrapping.Wrap,
                MinHeight = minHeightSize,
                VerticalAlignment = VerticalAlignment.Top,
            };

            // Set the recorded Width, if any
            if (stringInput.SerializedWidth != 0)
            {
                tb.Width = stringInput.SerializedWidth;
            }
            else
            {
                tb.MaxWidth = defMaxWidthSize;
            }

            // Set the recorded Height, if any
            if (stringInput.SerializedHeight != 0)
            {
                tb.Height = stringInput.SerializedHeight;
            }

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

            AddResizeThumb(tb, nodeView.inputGrid, stringInput);
        }

        /// <summary>
        /// Adds a resize thumb to enable dynamic resizing of a StringTextBox.
        /// </summary>
        private void AddResizeThumb(StringTextBox tb, Grid inputGrid, StringInput stringInput)
        {
            var resizeThumb = CreateResizeThumb();
            inputGrid.Children.Add(resizeThumb);

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

                stringInput.SerializedWidth = tb.ActualWidth;
                stringInput.SerializedHeight = tb.ActualHeight;

                // Mark the node as modified
                if(!this.workspace.HasUnsavedChanges)
                    this.workspace.HasUnsavedChanges = true;
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

        #endregion

        public void Dispose()
        {
            editWindowItem.Click -= editWindowItem_Click;
        }
    }
}

