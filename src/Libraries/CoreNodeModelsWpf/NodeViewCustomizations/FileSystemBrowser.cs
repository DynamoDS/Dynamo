using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using CoreNodeModels.Input;
using Dynamo.Configuration;
using Dynamo.Controls;
using Dynamo.Nodes;
using Dynamo.Wpf;

namespace DSCore.File
{
    public abstract class FileSystemBrowserNodeViewCustomization : INodeViewCustomization<FileSystemBrowser>
    {
        public void CustomizeView(FileSystemBrowser model, NodeView nodeView)
        {
            //add a button to the inputGrid on the dynElement
            var readFileButton = new DynamoNodeButton
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
                Height = Configurations.PortHeightInPixels
            };

            readFileButton.Click += readFileButton_Click;
            readFileButton.Content = Dynamo.Wpf.Properties.Resources.BrowserNodeButtonLabel;
            readFileButton.HorizontalAlignment = HorizontalAlignment.Stretch;
            readFileButton.VerticalAlignment = VerticalAlignment.Center;

            var tb = new TextBox();
            if (string.IsNullOrEmpty(model.Value))
                model.Value = Dynamo.Wpf.Properties.Resources.BrowserNodeNoFileSelected;

            tb.HorizontalAlignment = HorizontalAlignment.Stretch;
            tb.VerticalAlignment = VerticalAlignment.Center;
            var backgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 0, 0, 0));
            tb.Background = backgroundBrush;
            tb.BorderThickness = new Thickness(0);
            tb.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 238, 238, 238));
            tb.FontFamily = new System.Windows.Media.FontFamily("Artifakt Element");
            tb.FontSize = 16;
            tb.IsReadOnly = true;
            tb.IsReadOnlyCaretVisible = false;
            tb.TextChanged += delegate
            {
                tb.ScrollToHorizontalOffset(double.PositiveInfinity);
                nodeView.ViewModel.DynamoViewModel.OnRequestReturnFocusToView();
            };
            tb.Margin = new Thickness(6, 8, 0, 5);

            var sp = new StackPanel();
            sp.Children.Add(readFileButton);
            nodeView.inputGrid.Children.Add(sp);

            nodeView.grid.Children.Add(tb);
            Grid.SetColumn(tb, 1);
            Grid.SetRow(tb, 3);
            Canvas.SetZIndex(tb, 5);

            tb.DataContext = model;
            var bindingVal = new Binding("Value")
            {
                Mode = BindingMode.TwoWay,
                Converter = new FilePathDisplayConverter()
            };
            tb.SetBinding(TextBox.TextProperty, bindingVal);
        }

        protected virtual void readFileButton_Click(object sender, RoutedEventArgs e) { }

        public void Dispose()
        {
        }
    }
}