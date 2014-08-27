using System.Windows;
using System.Windows.Data;

using Dynamo.Controls;
using Dynamo.UI.Prompts;
using Dynamo.Wpf;

namespace Dynamo.Wpf
{
    public class AbstractString : BasicInteractiveViewInjection
    {
        public override void editWindowItem_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new EditWindow { DataContext = this };
            editWindow.BindToProperty(
                null,
                new Binding("Value")
                {
                    Mode = BindingMode.TwoWay,
                    Converter = new StringDisplay(),
                    NotifyOnValidationError = false,
                    Source = this,
                    UpdateSourceTrigger = UpdateSourceTrigger.Explicit
                });

            editWindow.ShowDialog();
        }

        public void Dispose()
        {

        }

        public void SetupCustomUIElements(dynNodeView nodeView)
        {
        }
    }
}