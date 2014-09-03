using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Wpf;

using Binding = System.Windows.Data.Binding;
using VerticalAlignment = System.Windows.VerticalAlignment;
using DynCmd = Dynamo.ViewModels.DynamoViewModel;

namespace Dynamo.Wpf
{
    public class StringInputNodeCustomization : AbstractStringNodeCustomization
    {
        public void CustomizeView(StringInput stringInput, dynNodeView ui)
        {
            var nodeUI = ui;

            base.CustomizeView(stringInput, nodeUI);

            //add a text box to the input grid of the control
            var tb = new StringTextBox
            {
                AcceptsReturn = true,
                AcceptsTab = true,
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = 200,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            nodeUI.inputGrid.Children.Add(tb);
            Grid.SetColumn(tb, 0);
            Grid.SetRow(tb, 0);

            tb.DataContext = this;
            tb.BindToProperty(new Binding("Value")
            {
                Mode = BindingMode.TwoWay,
                Converter = new StringDisplay(),
                Source = this,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            });
        }
    }
}

