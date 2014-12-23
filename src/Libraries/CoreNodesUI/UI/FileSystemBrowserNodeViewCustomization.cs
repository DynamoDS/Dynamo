using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

using Dynamo.Controls;
using Dynamo.Nodes;
using Dynamo.UI;
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
            readFileButton.Content = "Browse...";
            readFileButton.HorizontalAlignment = HorizontalAlignment.Stretch;
            readFileButton.VerticalAlignment = VerticalAlignment.Center;

            var tb = new TextBox();
            if (string.IsNullOrEmpty(model.Value))
                model.Value = "No file selected.";

            tb.HorizontalAlignment = HorizontalAlignment.Stretch;
            tb.VerticalAlignment = VerticalAlignment.Center;
            var backgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 0, 0, 0));
            tb.Background = backgroundBrush;
            tb.BorderThickness = new Thickness(0);
            tb.IsReadOnly = true;
            tb.IsReadOnlyCaretVisible = false;
            tb.TextChanged += delegate
            {
                tb.ScrollToHorizontalOffset(double.PositiveInfinity);
                nodeView.ViewModel.DynamoViewModel.ReturnFocusToSearch();
            };
            tb.Margin = new Thickness(0, 5, 0, 5);

            var sp = new StackPanel();
            sp.Children.Add(readFileButton);
            sp.Children.Add(tb);
            nodeView.inputGrid.Children.Add(sp);

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