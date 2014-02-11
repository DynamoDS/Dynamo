using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Dynamo.Controls;
using Dynamo.Nodes;
using Dynamo.UI;

namespace Dynamo.Nodes
{
    public partial class CodeBlockNodeModel : IWpfNode
    {
        public void SetupCustomUIElements(dynNodeView nodeUI)
        {
            var tb = new CodeNodeTextBox(Code)
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Background =
                    new SolidColorBrush(Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF)),
                AcceptsReturn = true,
                MaxWidth = Configurations.CBNMaxTextBoxWidth,
                TextWrapping = TextWrapping.Wrap
            };


            nodeUI.inputGrid.Children.Add(tb);
            Grid.SetColumn(tb, 0);
            Grid.SetRow(tb, 0);

            tb.DataContext = this;
            tb.BindToProperty(
                new Binding("Code")
                {
                    Mode = BindingMode.TwoWay,
                    NotifyOnValidationError = false,
                    Source = this,
                    UpdateSourceTrigger = UpdateSourceTrigger.Explicit
                });

            if (shouldFocus)
            {
                tb.Focus();
                shouldFocus = false;
            }
        }
    }

    public partial class Symbol : IWpfNode
    {
        public void SetupCustomUIElements(dynNodeView nodeUI)
        {
            //add a text box to the input grid of the control
            var tb = new DynamoTextBox(InputSymbol)
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                Background =
                    new SolidColorBrush(Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF))
            };

            nodeUI.inputGrid.Children.Add(tb);
            Grid.SetColumn(tb, 0);
            Grid.SetRow(tb, 0);

            tb.DataContext = this;
            tb.BindToProperty(
                new Binding("InputSymbol")
                {
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.Explicit
                });
        }

        protected override bool UpdateValueCore(string name, string value)
        {
            if (name == "InputSymbol")
            {
                InputSymbol = value;
                return true; // UpdateValueCore handled.
            }

            return base.UpdateValueCore(name, value);
        }
    }

    public partial class Output : IWpfNode
    {
        public void SetupCustomUIElements(dynNodeView nodeUI)
        {
            //add a text box to the input grid of the control
            var tb = new DynamoTextBox(Symbol)
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                Background =
                    new SolidColorBrush(Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF))
            };

            nodeUI.inputGrid.Children.Add(tb);
            Grid.SetColumn(tb, 0);
            Grid.SetRow(tb, 0);

            tb.DataContext = this;
            tb.BindToProperty(
                new Binding("Symbol")
                {
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.Explicit
                });
        }

        protected override bool UpdateValueCore(string name, string value)
        {
            if (name == "Symbol")
            {
                Symbol = value;
                return true; // UpdateValueCore handled.
            }

            return base.UpdateValueCore(name, value);
        }
    }
}