using System;
using System.Windows;
using System.Windows.Data;
using System.Collections.Generic;

using Dynamo.Controls;
using Dynamo.UI.Prompts;

namespace Dynamo.Wpf
{
    public abstract class AbstractStringNodeCustomization : BasicInteractiveCustomization<string>
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
    }
}