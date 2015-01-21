using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

using Dynamo.Controls;
using Dynamo.Nodes;
using Dynamo.UI.Prompts;
using Dynamo.ViewModels;
using Binding = System.Windows.Data.Binding;
using VerticalAlignment = System.Windows.VerticalAlignment;

namespace Dynamo.Wpf
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

            this.editWindowItem = new MenuItem { Header = "Edit...", IsCheckable = false };
            nodeView.MainContextMenu.Items.Add(editWindowItem);

            editWindowItem.Click += editWindowItem_Click;

            //add a text box to the input grid of the control
            var tb = new StringTextBox
            {
                AcceptsReturn = true,
                AcceptsTab = true,
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = 200,
                VerticalAlignment = VerticalAlignment.Stretch
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

