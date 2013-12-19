using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dynamo.Controls;
using Dynamo.Core;
using Dynamo.Models;
using Dynamo.UI;
using Dynamo.UI.Prompts;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Microsoft.FSharp.Collections;

using Binding = System.Windows.Data.Binding;
using Button = System.Windows.Controls.Button;
using ComboBox = System.Windows.Controls.ComboBox;
using DialogResult = System.Windows.Forms.DialogResult;
using FolderBrowserDialog = System.Windows.Forms.FolderBrowserDialog;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using Image = System.Windows.Controls.Image;
using MenuItem = System.Windows.Controls.MenuItem;
using RadioButton = System.Windows.Controls.RadioButton;
using TextBox = System.Windows.Controls.TextBox;
using VerticalAlignment = System.Windows.VerticalAlignment;

namespace Dynamo.Nodes
{
    public abstract partial class VariableInput
    {
        public override void SetupCustomUIElements(dynNodeView nodeUI)
        {
            var addButton = new NodeButton
            {
                Content = "+",
                Width = 20,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            //addButton.Height = 20;

            var subButton = new NodeButton
            {
                Content = "-",
                Width = 20,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top
            };
            //subButton.Height = 20;

            var wp = new WrapPanel
            {
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            wp.Children.Add(addButton);
            wp.Children.Add(subButton);

            nodeUI.inputGrid.Children.Add(wp);

            //nodeUI.inputGrid.ColumnDefinitions.Add(new ColumnDefinition());
            //nodeUI.inputGrid.ColumnDefinitions.Add(new ColumnDefinition());

            //nodeUI.inputGrid.Children.Add(addButton);
            //System.Windows.Controls.Grid.SetColumn(addButton, 0);

            //nodeUI.inputGrid.Children.Add(subButton);
            //System.Windows.Controls.Grid.SetColumn(subButton, 1);

            addButton.Click += delegate
            {
                WorkSpace.RecordModelForModification(this);
                AddInput();
                RegisterAllPorts();
            };
            subButton.Click += delegate
            {
                RecordModels();
                RemoveInput();
                RegisterAllPorts();
            };
        }

        private void RecordModels()
        {
            var connectors = InPorts[InPorts.Count - 1].Connectors;
            if (connectors.Count != 0)
            {
                if (connectors.Count != 1)
                {
                    throw new InvalidOperationException(
                        "There should be only one connection to an input port");
                }
                var models = new Dictionary<ModelBase, UndoRedoRecorder.UserAction>
                {
                    { connectors[0], UndoRedoRecorder.UserAction.Deletion },
                    { this, UndoRedoRecorder.UserAction.Modification }
                };
                WorkSpace.RecordModelsForUndo(models);
            }
            else
                WorkSpace.RecordModelForModification(this);
        }

    }

    public partial class VariableInputAndOutput
    {
        public override void SetupCustomUIElements(dynNodeView nodeUI)
        {
            var addButton = new NodeButton
            {
                Content = "+",
                Width = 20,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            var subButton = new NodeButton
            {
                Content = "-",
                Width = 20,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top
            };

            var wp = new WrapPanel
            {
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            wp.Children.Add(addButton);
            wp.Children.Add(subButton);

            nodeUI.inputGrid.Children.Add(wp);

            addButton.Click += delegate
            {
                WorkSpace.RecordModelForModification(this);
                AddInput();
                RegisterAllPorts();
            };
            subButton.Click += delegate
            {
                RecordModels();
                RemoveInput();
                RegisterAllPorts();
            };
        }

        private void RecordModels()
        {
            var connectors = InPorts[InPorts.Count - 1].Connectors;
            if (connectors.Count != 0)
            {
                if (connectors.Count != 1)
                {
                    throw new InvalidOperationException(
                        "There should be only one connection to an input port");
                }

                var models = new Dictionary<ModelBase, UndoRedoRecorder.UserAction>
                {
                    { connectors[0], UndoRedoRecorder.UserAction.Deletion },
                    { this, UndoRedoRecorder.UserAction.Modification }
                };
                WorkSpace.RecordModelsForUndo(models);
            }
            else
                WorkSpace.RecordModelForModification(this);
        }
    }

    public partial class Sublists
    {
        public override void SetupCustomUIElements(dynNodeView nodeUI)
        {
            //add a text box to the input grid of the control
            var tb = new DynamoTextBox
            {
                Background =
                    new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF))
            };

            tb.OnChangeCommitted += processTextForNewInputs;

            tb.HorizontalAlignment = HorizontalAlignment.Stretch;
            tb.VerticalAlignment = VerticalAlignment.Top;

            nodeUI.inputGrid.Children.Add(tb);
            Grid.SetColumn(tb, 0);
            Grid.SetRow(tb, 0);

            tb.DataContext = this;
            tb.BindToProperty(
                new Binding("Value")
                {
                    Mode = BindingMode.TwoWay,
                    Source = this,
                    UpdateSourceTrigger = UpdateSourceTrigger.Explicit
                });
        }

        protected override bool UpdateValueCore(string name, string value)
        {
            if (name == "Value")
            {
                Value = value;
                return true; // UpdateValueCore handled.
            }

            return base.UpdateValueCore(name, value);
        }
    }

    public partial class Breakpoint
    {
        public override void SetupCustomUIElements(dynNodeView nodeUI)
        {
            //add a text box to the input grid of the control
            var button = new NodeButton
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top
            };

            nodeUI.inputGrid.Children.Add(button);
            Grid.SetColumn(button, 0);
            Grid.SetRow(button, 0);
            button.Content = "Continue";

            Enabled = false;

            button.Click += button_Click;

            var bindingVal = new Binding("Enabled")
            {
                Mode = BindingMode.TwoWay,
                NotifyOnValidationError = false,
                Source = this
            };
            button.SetBinding(UIElement.IsEnabledProperty, bindingVal);
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            Deselect();
            Enabled = false;
        }
    }

    public abstract partial class BasicInteractive<T>
    {
        public override void SetupCustomUIElements(dynNodeView nodeUI)
        {
            //add an edit window option to the 
            //main context window
            var editWindowItem = new MenuItem { Header = "Edit...", IsCheckable = false };
            nodeUI.MainContextMenu.Items.Add(editWindowItem);
            editWindowItem.Click += editWindowItem_Click;
        }

        public virtual void editWindowItem_Click(object sender, RoutedEventArgs e)
        {
            //override in child classes
        }
    }

    public partial class DoubleInput
    {
        public override void SetupCustomUIElements(dynNodeView nodeUI)
        {
            //add a text box to the input grid of the control
            var tb = new DynamoTextBox(Value ?? "0.0")
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
                Background =
                    new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF))
            };

            nodeUI.inputGrid.Children.Add(tb);
            Grid.SetColumn(tb, 0);
            Grid.SetRow(tb, 0);

            tb.DataContext = this;
            tb.BindToProperty(
                new Binding("Value")
                {
                    Mode = BindingMode.TwoWay,
                    Converter = new DoubleInputDisplay(),
                    NotifyOnValidationError = false,
                    Source = this,
                    UpdateSourceTrigger = UpdateSourceTrigger.Explicit
                });
        }

        protected override bool UpdateValueCore(string name, string value)
        {
            if (name == "Value")
            {
                Value = value;
                return true; // UpdateValueCore handled.
            }

            return base.UpdateValueCore(name, value);
        }
    }

    //public partial class AngleInput : DoubleInput
    //{

    //    public override void SetupCustomUIElements(object ui)
    //    {
    //        var nodeUI = ui as dynNodeView;

    //        //add a text box to the input grid of the control
    //        var tb = new dynTextBox();
    //        tb.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
    //        tb.VerticalAlignment = System.Windows.VerticalAlignment.Top;
    //        nodeUI.inputGrid.Children.Add(tb);
    //        System.Windows.Controls.Grid.SetColumn(tb, 0);
    //        System.Windows.Controls.Grid.SetRow(tb, 0);
    //        tb.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF));

    //        tb.DataContext = this;
    //        var bindingVal = new System.Windows.Data.Binding("Value")
    //        {
    //            Mode = BindingMode.TwoWay,
    //            Converter = new RadianToDegreesConverter(),
    //            NotifyOnValidationError = false,
    //            Source = this,
    //            UpdateSourceTrigger = UpdateSourceTrigger.Explicit
    //        };
    //        tb.SetBinding(TextBox.TextProperty, bindingVal);
    //    }

    //}

    public partial class DoubleSliderInput
    {
        public override void SetupCustomUIElements(dynNodeView nodeUI)
        {
            //add a slider control to the input grid of the control
            var tbSlider = new DynamoSlider(this)
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                MinWidth = 150,
                TickPlacement = TickPlacement.None
            };

            tbSlider.PreviewMouseUp += delegate
            {
                dynSettings.ReturnFocusToSearch();
            };

            var mintb = new DynamoTextBox
            {
                Width = double.NaN,
                Background =
                    new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF))
            };

            // input value textbox
            var valtb = new DynamoTextBox
            {
                Width = double.NaN,
                Margin = new Thickness(0, 0, 10, 0)
            };

            var maxtb = new DynamoTextBox
            {
                Width = double.NaN,
                Background =
                    new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF))
            };

            var sliderGrid = new Grid();
            sliderGrid.ColumnDefinitions.Add(
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
            sliderGrid.ColumnDefinitions.Add(
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
            sliderGrid.ColumnDefinitions.Add(
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            sliderGrid.ColumnDefinitions.Add(
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });

            sliderGrid.Children.Add(valtb);
            sliderGrid.Children.Add(mintb);
            sliderGrid.Children.Add(tbSlider);
            sliderGrid.Children.Add(maxtb);

            Grid.SetColumn(valtb, 0);
            Grid.SetColumn(mintb, 1);
            Grid.SetColumn(tbSlider, 2);
            Grid.SetColumn(maxtb, 3);
            nodeUI.inputGrid.Children.Add(sliderGrid);

            maxtb.DataContext = this;
            tbSlider.DataContext = this;
            mintb.DataContext = this;
            valtb.DataContext = this;

            // value input
            valtb.BindToProperty(
                new Binding("Value") { Mode = BindingMode.TwoWay, Converter = new DoubleDisplay() });

            // slider value 
            var sliderBinding = new Binding("Value") { Mode = BindingMode.TwoWay, Source = this, };
            tbSlider.SetBinding(RangeBase.ValueProperty, sliderBinding);

            // max value
            maxtb.BindToProperty(
                new Binding("Max")
                {
                    Mode = BindingMode.TwoWay,
                    Converter = new DoubleDisplay(),
                    Source = this,
                    UpdateSourceTrigger = UpdateSourceTrigger.Explicit
                });

            // max slider value
            var bindingMaxSlider = new Binding("Max")
            {
                Mode = BindingMode.OneWay,
                Source = this,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };
            tbSlider.SetBinding(RangeBase.MaximumProperty, bindingMaxSlider);


            // min value
            mintb.BindToProperty(
                new Binding("Min")
                {
                    Mode = BindingMode.TwoWay,
                    Converter = new DoubleDisplay(),
                    Source = this,
                    UpdateSourceTrigger = UpdateSourceTrigger.Explicit
                });

            // min slider value
            var bindingMinSlider = new Binding("Min")
            {
                Mode = BindingMode.OneWay,
                Source = this,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };
            tbSlider.SetBinding(RangeBase.MinimumProperty, bindingMinSlider);
        }

        protected override bool UpdateValueCore(string name, string value)
        {
            var converter = new DoubleDisplay();
            switch (name)
            {
                case "Value":
                    Value = ((double)converter.ConvertBack(value, typeof(double), null, null));
                    return true; // UpdateValueCore handled.
                case "Max":
                    Max = ((double)converter.ConvertBack(value, typeof(double), null, null));
                    return true; // UpdateValueCore handled.
                case "Min":
                    Min = ((double)converter.ConvertBack(value, typeof(double), null, null));
                    return true; // UpdateValueCore handled.
            }

            return base.UpdateValueCore(name, value);
        }
    }

    public partial class IntegerSliderInput
    {
        public override void SetupCustomUIElements(dynNodeView nodeUI)
        {
            //add a slider control to the input grid of the control
            var tbSlider = new DynamoSlider(this)
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                MinWidth = 150,
                TickPlacement = TickPlacement.BottomRight,
                TickFrequency = 1,
                IsSnapToTickEnabled = true
            };

            tbSlider.PreviewMouseUp += delegate
            {
                dynSettings.ReturnFocusToSearch();
            };

            var mintb = new DynamoTextBox
            {
                Width = double.NaN,
                Background =
                    new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF))
            };

            // input value textbox
            var valtb = new DynamoTextBox
            {
                Width = double.NaN,
                Margin = new Thickness(0, 0, 10, 0)
            };

            var maxtb = new DynamoTextBox
            {
                Width = double.NaN,
                Background =
                    new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF))
            };

            var sliderGrid = new Grid();
            sliderGrid.ColumnDefinitions.Add(
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
            sliderGrid.ColumnDefinitions.Add(
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
            sliderGrid.ColumnDefinitions.Add(
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            sliderGrid.ColumnDefinitions.Add(
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });

            sliderGrid.Children.Add(valtb);
            sliderGrid.Children.Add(mintb);
            sliderGrid.Children.Add(tbSlider);
            sliderGrid.Children.Add(maxtb);

            Grid.SetColumn(valtb, 0);
            Grid.SetColumn(mintb, 1);
            Grid.SetColumn(tbSlider, 2);
            Grid.SetColumn(maxtb, 3);
            nodeUI.inputGrid.Children.Add(sliderGrid);

            maxtb.DataContext = this;
            tbSlider.DataContext = this;
            mintb.DataContext = this;
            valtb.DataContext = this;

            // value input
            valtb.BindToProperty(
                new Binding("Value") { Mode = BindingMode.TwoWay, Converter = new IntegerDisplay() });

            // slider value 
            var sliderBinding = new Binding("Value") { Mode = BindingMode.TwoWay, Source = this, };
            tbSlider.SetBinding(RangeBase.ValueProperty, sliderBinding);

            // max value
            maxtb.BindToProperty(
                new Binding("Max")
                {
                    Mode = BindingMode.TwoWay,
                    Converter = new IntegerDisplay(),
                    Source = this,
                    UpdateSourceTrigger = UpdateSourceTrigger.Explicit
                });

            // max slider value
            var bindingMaxSlider = new Binding("Max")
            {
                Mode = BindingMode.OneWay,
                Source = this,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };
            tbSlider.SetBinding(RangeBase.MaximumProperty, bindingMaxSlider);


            // min value
            mintb.BindToProperty(
                new Binding("Min")
                {
                    Mode = BindingMode.TwoWay,
                    Converter = new IntegerDisplay(),
                    Source = this,
                    UpdateSourceTrigger = UpdateSourceTrigger.Explicit
                });

            // min slider value
            var bindingMinSlider = new Binding("Min")
            {
                Mode = BindingMode.OneWay,
                Source = this,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };
            tbSlider.SetBinding(RangeBase.MinimumProperty, bindingMinSlider);
        }

        protected override bool UpdateValueCore(string name, string value)
        {
            var converter = new IntegerDisplay();
            switch (name)
            {
                case "Value":
                    Value = ((int)converter.ConvertBack(value, typeof(int), null, null));
                    return true; // UpdateValueCore handled.
                case "Max":
                    Max = ((int)converter.ConvertBack(value, typeof(int), null, null));
                    return true; // UpdateValueCore handled.
                case "Min":
                    Min = ((int)converter.ConvertBack(value, typeof(int), null, null));
                    return true; // UpdateValueCore handled.
            }

            return base.UpdateValueCore(name, value);
        }
    }

    public partial class BoolSelector
    {
        public override void SetupCustomUIElements(dynNodeView nodeUI)
        {
            //add a text box to the input grid of the control
            var rbTrue = new RadioButton();
            var rbFalse = new RadioButton();
            rbTrue.VerticalAlignment = VerticalAlignment.Center;
            rbFalse.VerticalAlignment = VerticalAlignment.Center;

            //use a unique name for the button group
            //so other instances of this element don't get confused
            string groupName = Guid.NewGuid().ToString();
            rbTrue.GroupName = groupName;
            rbFalse.GroupName = groupName;

            rbTrue.Content = "1";
            rbTrue.Padding = new Thickness(5, 0, 12, 0);
            rbFalse.Content = "0";
            rbFalse.Padding = new Thickness(5, 0, 0, 0);

            var wp = new WrapPanel { HorizontalAlignment = HorizontalAlignment.Center };
            wp.Children.Add(rbTrue);
            wp.Children.Add(rbFalse);
            nodeUI.inputGrid.Children.Add(wp);

            //rbFalse.IsChecked = true;
            rbTrue.Checked += rbTrue_Checked;
            rbFalse.Checked += rbFalse_Checked;

            rbFalse.DataContext = this;
            rbTrue.DataContext = this;

            var rbTrueBinding = new Binding("Value") { Mode = BindingMode.TwoWay, };
            rbTrue.SetBinding(ToggleButton.IsCheckedProperty, rbTrueBinding);

            var rbFalseBinding = new Binding("Value")
            {
                Mode = BindingMode.TwoWay,
                Converter = new InverseBoolDisplay()
            };
            rbFalse.SetBinding(ToggleButton.IsCheckedProperty, rbFalseBinding);
        }

        private void rbFalse_Checked(object sender, RoutedEventArgs e)
        {
            //Value = false;
            dynSettings.ReturnFocusToSearch();
        }

        private void rbTrue_Checked(object sender, RoutedEventArgs e)
        {
            //Value = true;
            dynSettings.ReturnFocusToSearch();
        }
    }

    public partial class StringInput
    {
        public override void SetupCustomUIElements(dynNodeView nodeUI)
        {
            base.SetupCustomUIElements(nodeUI);

            //add a text box to the input grid of the control
            var tb = new StringTextBox
            {
                AcceptsReturn = true,
                AcceptsTab = true,
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = 200,
                VerticalAlignment = VerticalAlignment.Top
            };

            nodeUI.inputGrid.Children.Add(tb);
            Grid.SetColumn(tb, 0);
            Grid.SetRow(tb, 0);

            tb.DataContext = this;
            tb.BindToProperty(
                new Binding("Value")
                {
                    Mode = BindingMode.TwoWay,
                    Converter = new StringDisplay(),
                    Source = this,
                    UpdateSourceTrigger = UpdateSourceTrigger.Explicit
                });
        }

        protected override bool UpdateValueCore(string name, string value)
        {
            if (name == "Value")
            {
                var converter = new StringDisplay();
                Value = ((string)converter.ConvertBack(value, typeof(string), null, null));
                return true; // UpdateValueCore handled.
            }

            // There's another 'UpdateValueCore' method in 'String' base class,
            // since they are both bound to the same property, 'StringInput' 
            // should be given a chance to handle the property value change first
            // before the base class 'String'.
            return base.UpdateValueCore(name, value);
        }
    }

    public partial class StringFilename
    {
        public override void SetupCustomUIElements(dynNodeView nodeUI)
        {
            //add a button to the inputGrid on the dynElement
            var readFileButton = new NodeButton
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top
            };

            //readFileButton.Margin = new System.Windows.Thickness(4);
            readFileButton.Click += readFileButton_Click;
            readFileButton.Content = "Browse...";
            readFileButton.HorizontalAlignment = HorizontalAlignment.Stretch;
            readFileButton.VerticalAlignment = VerticalAlignment.Center;

            var tb = new TextBox();
            if (string.IsNullOrEmpty(Value))
                Value = "No file selected.";

            tb.HorizontalAlignment = HorizontalAlignment.Stretch;
            tb.VerticalAlignment = VerticalAlignment.Center;
            var backgroundBrush =
                new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 0, 0, 0));
            tb.Background = backgroundBrush;
            tb.BorderThickness = new Thickness(0);
            tb.IsReadOnly = true;
            tb.IsReadOnlyCaretVisible = false;
            tb.TextChanged += delegate
            {
                tb.ScrollToHorizontalOffset(double.PositiveInfinity);
                dynSettings.ReturnFocusToSearch();
            };

            var sp = new StackPanel();
            sp.Children.Add(readFileButton);
            sp.Children.Add(tb);
            nodeUI.inputGrid.Children.Add(sp);

            tb.DataContext = this;
            var bindingVal = new Binding("Value")
            {
                Mode = BindingMode.TwoWay,
                Converter = new FilePathDisplayConverter()
            };
            tb.SetBinding(TextBox.TextProperty, bindingVal);
        }


        protected virtual void readFileButton_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new OpenFileDialog { CheckFileExists = false };

            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                Value = openDialog.FileName;
            }
        }

    }

    public abstract partial class DropDrownBase
    {
        public override void SetupCustomUIElements(dynNodeView nodeUI)
        {
            base.SetupCustomUIElements(nodeUI);

            //add a drop down list to the window
            var combo = new ComboBox
            {
                Width = 300,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center
            };
            nodeUI.inputGrid.Children.Add(combo);
            Grid.SetColumn(combo, 0);
            Grid.SetRow(combo, 0);

            combo.DropDownOpened += combo_DropDownOpened;
            combo.SelectionChanged += delegate
            {
                if (combo.SelectedIndex != -1)
                    RequiresRecalc = true;
            };

            combo.DataContext = this;
            //bind this combo box to the selected item hash

            var bindingVal = new Binding("Items") { Mode = BindingMode.TwoWay, Source = this };
            combo.SetBinding(ItemsControl.ItemsSourceProperty, bindingVal);

            //bind the selected index to the 
            var indexBinding = new Binding("SelectedIndex")
            {
                Mode = BindingMode.TwoWay,
                Source = this
            };
            combo.SetBinding(Selector.SelectedIndexProperty, indexBinding);
        }

    }

    public abstract partial class Enum
    {
        public override void SetupCustomUIElements(dynNodeView nodeUI)
        {
            var comboBox = new ComboBox
            {
                MinWidth = 150,
                Padding = new Thickness(8),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center
            };

            nodeUI.inputGrid.Children.Add(comboBox);

            Grid.SetColumn(comboBox, 0);
            Grid.SetRow(comboBox, 0);

            comboBox.ItemsSource = Items;
            comboBox.SelectedIndex = SelectedIndex;

            comboBox.SelectionChanged += delegate
            {
                if (comboBox.SelectedIndex == -1) return;
                RequiresRecalc = true;
                SelectedIndex = comboBox.SelectedIndex;
            };
        }

    }

    public partial class Formula
    {
        public override void SetupCustomUIElements(dynNodeView nodeUI)
        {
            var tb = new DynamoTextBox(FormulaString)
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
                Background =
                    new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF))
            };

            nodeUI.inputGrid.Children.Add(tb);
            Grid.SetColumn(tb, 0);
            Grid.SetRow(tb, 0);

            tb.DataContext = this;
            tb.BindToProperty(
                new Binding("FormulaString")
                {
                    Mode = BindingMode.TwoWay,
                    NotifyOnValidationError = false,
                    Source = this,
                    UpdateSourceTrigger = UpdateSourceTrigger.Explicit
                });
        }

        protected override bool UpdateValueCore(string name, string value)
        {
            if (name == "FormulaString")
            {
                FormulaString = value;
                return true; // UpdateValueCore handled.
            }

            return base.UpdateValueCore(name, value);
        }
    }

    public partial class CodeBlockNodeModel
    {
        public override void SetupCustomUIElements(dynNodeView nodeUI)
        {
            var tb = new CodeNodeTextBox(Code)
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Background =
                    new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF)),
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

    public partial class Output
    {
        public override void SetupCustomUIElements(dynNodeView nodeUI)
        {
            //add a text box to the input grid of the control
            var tb = new DynamoTextBox(Symbol)
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                Background =
                    new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF))
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

    public partial class Symbol
    {
        public override void SetupCustomUIElements(dynNodeView nodeUI)
        {
            //add a text box to the input grid of the control
            var tb = new DynamoTextBox(InputSymbol)
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                Background =
                    new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF))
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

    public partial class Watch
    {
        public override void SetupCustomUIElements(dynNodeView nodeUI)
        {
            watchTree = new WatchTree();

            nodeUI.grid.Children.Add(watchTree);
            watchTree.SetValue(Grid.RowProperty, 2);
            watchTree.SetValue(Grid.ColumnSpanProperty, 3);
            watchTree.Margin = new Thickness(5, 0, 5, 5);

            if (Root == null)
                Root = new WatchNode();
            watchTree.DataContext = Root;

            RequestBindingUnhook += delegate
            {
                BindingOperations.ClearAllBindings(watchTree.treeView1);
            };

            RequestBindingRehook += delegate
            {
                var sourceBinding = new Binding("Children")
                {
                    Mode = BindingMode.TwoWay,
                    Source = Root,
                };
                watchTree.treeView1.SetBinding(ItemsControl.ItemsSourceProperty, sourceBinding);
            };

        }

    }

    public partial class StringDirectory
    {
        protected override void readFileButton_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new FolderBrowserDialog { ShowNewFolderButton = true };

            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                Value = openDialog.SelectedPath;
            }
        }
    }

    public abstract partial class String
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

        protected override bool UpdateValueCore(string name, string value)
        {
            if (name == "Value")
            {
                var converter = new StringDisplay();
                Value = converter.ConvertBack(value, typeof(string), null, null) as string;
                return true;
            }

            return base.UpdateValueCore(name, value);
        }
    }

    public class NodeButton : Button
    {
        public NodeButton()
        {
            Style = (Style)SharedDictionaryManager.DynamoModernDictionary["SNodeTextButton"];
            Margin = new Thickness(1, 0, 1, 0);
        }
    }

    [NodeName("Read Image File")]
    [NodeCategory(BuiltinNodeCategories.IO_FILE)]
    [NodeDescription("Reads data from an image file.")]
    public class ImageFileReader : FileReaderBase
    {
        private Image _image1;

        public ImageFileReader()
        {
            InPortData.Add(
                new PortData("numX", "Number of samples in the X direction.", typeof(object)));
            InPortData.Add(
                new PortData("numY", "Number of samples in the Y direction.", typeof(object)));
            OutPortData.Add(new PortData("contents", "File contents", typeof(FScheme.Value.List)));
            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            storedPath = ((FScheme.Value.String)args[0]).Item;
            double xDiv = ((FScheme.Value.Number)args[1]).Item;
            double yDiv = ((FScheme.Value.Number)args[2]).Item;

            FSharpList<FScheme.Value> result = FSharpList<FScheme.Value>.Empty;
            if (File.Exists(storedPath))
            {
                try
                {
                    using (var bmp = new Bitmap(storedPath))
                    {
                        DispatchOnUIThread(
                            delegate
                            {
                                // how to convert a bitmap to an imagesource http://blog.laranjee.com/how-to-convert-winforms-bitmap-to-wpf-imagesource/ 
                                // TODO - watch out for memory leaks using system.drawing.bitmaps in managed code, see here http://social.msdn.microsoft.com/Forums/en/csharpgeneral/thread/4e213af5-d546-4cc1-a8f0-462720e5fcde
                                // need to call Dispose manually somewhere, or perhaps use a WPF native structure instead of bitmap?

                                var hbitmap = bmp.GetHbitmap();
                                var imageSource = Imaging.CreateBitmapSourceFromHBitmap(
                                    hbitmap,
                                    IntPtr.Zero,
                                    Int32Rect.Empty,
                                    BitmapSizeOptions.FromWidthAndHeight(bmp.Width, bmp.Height));
                                _image1.Source = imageSource;
                            });

                        // Do some processing
                        for (int y = 0; y < yDiv; y++)
                        {
                            for (int x = 0; x < xDiv; x++)
                            {
                                System.Drawing.Color pixelColor =
                                    bmp.GetPixel(x*(int)(bmp.Width/xDiv), y*(int)(bmp.Height/yDiv));
                                result =
                                    FSharpList<FScheme.Value>.Cons(
                                        FScheme.Value.NewContainer(pixelColor),
                                        result);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    DynamoLogger.Instance.Log(e.ToString());
                }

                return FScheme.Value.NewList(result);
            }
            return FScheme.Value.NewList(FSharpList<FScheme.Value>.Empty);
        }

        public override void SetupCustomUIElements(dynNodeView nodeUI)
        {
            _image1 = new Image
            {
                MaxWidth = 400,
                MaxHeight = 400,
                Margin = new Thickness(5),
                HorizontalAlignment = HorizontalAlignment.Center,
                Name = "image1",
                VerticalAlignment = VerticalAlignment.Center
            };

            nodeUI.grid.Children.Add(_image1);
            _image1.SetValue(Grid.RowProperty, 2);
            _image1.SetValue(Grid.ColumnProperty, 0);
            _image1.SetValue(Grid.ColumnSpanProperty, 3);
        }
    }
}
