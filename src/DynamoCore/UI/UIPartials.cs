using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
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
using Dynamo.Nodes;
using Microsoft.FSharp.Collections;
using Binding = System.Windows.Data.Binding;
using Color = System.Windows.Media.Color;
using ComboBox = System.Windows.Controls.ComboBox;
using DialogResult = System.Windows.Forms.DialogResult;
using FolderBrowserDialog = System.Windows.Forms.FolderBrowserDialog;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using TextBox = System.Windows.Controls.TextBox;
using TreeView = System.Windows.Controls.TreeView;
using VerticalAlignment = System.Windows.VerticalAlignment;
using DynCmd = Dynamo.ViewModels.DynamoViewModel;

namespace Dynamo.Nodes
{
    public abstract partial class VariableInput : NodeWithOneOutput
    {
        public override void SetupCustomUIElements(object ui)
        {
            var nodeUI = ui as dynNodeView;

            var addButton = new DynamoNodeButton(this, "AddInPort");
            addButton.Content = "+";
            addButton.Width = 20;
            //addButton.Height = 20;
            addButton.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            addButton.VerticalAlignment = System.Windows.VerticalAlignment.Center;

            var subButton = new DynamoNodeButton(this, "RemoveInPort");
            subButton.Content = "-";
            subButton.Width = 20;
            //subButton.Height = 20;
            subButton.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            subButton.VerticalAlignment = System.Windows.VerticalAlignment.Top;

            var wp = new WrapPanel
            {
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            wp.Children.Add(addButton);
            wp.Children.Add(subButton);

            nodeUI.inputGrid.Children.Add(wp);
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
                Dictionary<ModelBase, UndoRedoRecorder.UserAction> models;
                models = new Dictionary<ModelBase, UndoRedoRecorder.UserAction>();
                models.Add(connectors[0], UndoRedoRecorder.UserAction.Deletion);
                models.Add(this, UndoRedoRecorder.UserAction.Modification);
                this.WorkSpace.RecordModelsForUndo(models);
            }
            else
                this.WorkSpace.RecordModelForModification(this);
        }

        protected override bool HandleModelEventCore(string eventName)
        {
            if (eventName == "AddInPort")
            {
                AddInput();
                RegisterAllPorts();
                return true; // Handled here.
            }
            else if (eventName == "RemoveInPort")
            {
                // When an in-port is removed, it is possible that a connector 
                // is almost removed along with it. Both node modification and 
                // connector deletion have to be recorded as one action group.
                // But before HandleModelEventCore is called, node modification 
                // has already been recorded (in WorkspaceModel.SendModelEvent).
                // For that reason, that entry on the undo-stack needs to be 
                // popped (the node modification will be recorded here instead).
                // 
                this.WorkSpace.UndoRecorder.PopFromUndoGroup();

                RecordModels();
                RemoveInput();
                RegisterAllPorts();
                return true; // Handled here.
            }

            return base.HandleModelEventCore(eventName);
        }
    }

    public partial class VariableInputAndOutput : NodeModel
    {
        public override void SetupCustomUIElements(object ui)
        {
            var nodeUI = ui as dynNodeView;

            var addButton = new DynamoNodeButton(this, "AddInPort");
            addButton.Content = "+";
            addButton.Width = 20;
            addButton.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            addButton.VerticalAlignment = System.Windows.VerticalAlignment.Center;

            var subButton = new DynamoNodeButton(this, "RemoveInPort");
            subButton.Content = "-";
            subButton.Width = 20;
            subButton.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            subButton.VerticalAlignment = System.Windows.VerticalAlignment.Top;

            var wp = new WrapPanel
            {
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            wp.Children.Add(addButton);
            wp.Children.Add(subButton);

            nodeUI.inputGrid.Children.Add(wp);
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
                Dictionary<ModelBase, UndoRedoRecorder.UserAction> models;
                models = new Dictionary<ModelBase, UndoRedoRecorder.UserAction>();
                models.Add(connectors[0], UndoRedoRecorder.UserAction.Deletion);
                models.Add(this, UndoRedoRecorder.UserAction.Modification);
                this.WorkSpace.RecordModelsForUndo(models);
            }
            else
                this.WorkSpace.RecordModelForModification(this);
        }

        protected override bool HandleModelEventCore(string eventName)
        {
            if (eventName == "AddInPort")
            {
                AddInput();
                RegisterAllPorts();
                return true; // Handled here.
            }
            else if (eventName == "RemoveInPort")
            {
                // When an in-port is removed, it is possible that a connector 
                // is almost removed along with it. Both node modification and 
                // connector deletion have to be recorded as one action group.
                // But before HandleModelEventCore is called, node modification 
                // has already been recorded (in WorkspaceModel.SendModelEvent).
                // For that reason, that entry on the undo-stack needs to be 
                // popped (the node modification will be recorded here instead).
                // 
                this.WorkSpace.UndoRecorder.PopFromUndoGroup();

                RecordModels();
                RemoveInput();
                RegisterAllPorts();
                return true; // Handled here.
            }

            return base.HandleModelEventCore(eventName);
        }
    }

    public partial class Sublists : BasicInteractive<string>
    {
        public override void SetupCustomUIElements(object ui)
        {
            var nodeUI = ui as dynNodeView;

            //add a text box to the input grid of the control
            var tb = new DynamoTextBox
            {
                Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF))
            };

            tb.OnChangeCommitted += processTextForNewInputs;

            tb.HorizontalAlignment = HorizontalAlignment.Stretch;
            tb.VerticalAlignment = VerticalAlignment.Top;

            nodeUI.inputGrid.Children.Add(tb);
            Grid.SetColumn(tb, 0);
            Grid.SetRow(tb, 0);

            tb.DataContext = this;
            tb.BindToProperty(new System.Windows.Data.Binding("Value")
            {
                Mode = BindingMode.TwoWay,
                //Converter = new StringDisplay(),
                Source = this,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            });
        }

        protected override bool UpdateValueCore(string name, string value)
        {
            if (name == "Value")
            {
                this.Value = value;
                return true; // UpdateValueCore handled.
            }

            return base.UpdateValueCore(name, value);
        }
    }

    public partial class Breakpoint : NodeWithOneOutput
    {
        //System.Windows.Controls.Button button;

        public override void SetupCustomUIElements(object ui)
        {
            var nodeUI = ui as dynNodeView;

            //add a text box to the input grid of the control
            var button = new DynamoNodeButton();
            button.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            button.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            //inputGrid.RowDefinitions.Add(new RowDefinition());
            nodeUI.inputGrid.Children.Add(button);
            System.Windows.Controls.Grid.SetColumn(button, 0);
            System.Windows.Controls.Grid.SetRow(button, 0);
            button.Content = "Continue";

            Enabled = false;

            button.Click += new RoutedEventHandler(button_Click);

            var bindingVal = new System.Windows.Data.Binding("Enabled")
            {
                Mode = BindingMode.TwoWay,
                NotifyOnValidationError = false,
                Source = this
            };
            button.SetBinding(UIElement.IsEnabledProperty, bindingVal);
        }

        void button_Click(object sender, RoutedEventArgs e)
        {
            Deselect();
            Enabled = false;
        }

    }

    public abstract partial class BasicInteractive<T> : NodeWithOneOutput
    {
        public override void SetupCustomUIElements(object ui)
        {
            var nodeUI = ui as dynNodeView;

            //add an edit window option to the 
            //main context window
            var editWindowItem = new System.Windows.Controls.MenuItem
            {
                Header = "Edit...",
                IsCheckable = false
            };

            nodeUI.MainContextMenu.Items.Add(editWindowItem);

            editWindowItem.Click += editWindowItem_Click;
        }

        public virtual void editWindowItem_Click(object sender, RoutedEventArgs e)
        {
            //override in child classes
        }

    }

    public partial class DoubleInput : NodeWithOneOutput
    {
        public override void SetupCustomUIElements(object ui)
        {
            var nodeUI = ui as dynNodeView;

            //add a text box to the input grid of the control
            var tb = new DynamoTextBox(Value ?? "0.0")
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
                Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF))
            };

            nodeUI.inputGrid.Children.Add(tb);
            Grid.SetColumn(tb, 0);
            Grid.SetRow(tb, 0);

            tb.DataContext = this;
            tb.BindToProperty(new System.Windows.Data.Binding("Value")
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
                this.Value = value;
                return true; // UpdateValueCore handled.
            }

            return base.UpdateValueCore(name, value);
        }
    }

    public partial class Function : NodeWithOneOutput
    {
        public override void SetupCustomUIElements(object ui)
        {
            var nodeUI = ui as dynNodeView;

            nodeUI.MainContextMenu.Items.Add(new System.Windows.Controls.Separator());

            // edit contents
            var editItem = new System.Windows.Controls.MenuItem
            {
                Header = "Edit Custom Node...",
                IsCheckable = false
            };
            nodeUI.MainContextMenu.Items.Add(editItem);
            editItem.Click += (sender, args) => GoToWorkspace(nodeUI.ViewModel);

            // edit properties
            var editPropertiesItem = new System.Windows.Controls.MenuItem
            {
                Header = "Edit Custom Node Properties...",
                IsCheckable = false
            };
            nodeUI.MainContextMenu.Items.Add(editPropertiesItem);
            editPropertiesItem.Click += (sender, args) => EditCustomNodeProperties();

            // publish
            var publishCustomNodeItem = new System.Windows.Controls.MenuItem
            {
                Header = "Publish This Custom Node...",
                IsCheckable = false
            };
            nodeUI.MainContextMenu.Items.Add(publishCustomNodeItem);
            publishCustomNodeItem.Click += (sender, args) =>
            {
                GoToWorkspace(nodeUI.ViewModel);
                if (dynSettings.Controller.DynamoViewModel.PublishCurrentWorkspaceCommand.CanExecute(null))
                {
                    dynSettings.Controller.DynamoViewModel.PublishCurrentWorkspaceCommand.Execute(null);
                } 
            };

            nodeUI.UpdateLayout();
        }

        private void EditCustomNodeProperties()
        {
            var workspace = this.Definition.WorkspaceModel;

            // copy these strings
            var newName = workspace.Name.Substring(0);
            var newCategory = workspace.Category.Substring(0);
            var newDescription = workspace.Description.Substring(0);

            var args = new FunctionNamePromptEventArgs
            {
                Name = newName,
                Description = newDescription,
                Category = newCategory,
                CanEditName = false
            };

            dynSettings.Controller.DynamoModel.OnRequestsFunctionNamePrompt(this, args);

            if (args.Success)
            {
                if (workspace is CustomNodeWorkspaceModel)
                {
                    var def = (workspace as CustomNodeWorkspaceModel).FunctionDefinition;
                    dynSettings.CustomNodeManager.Refactor(def.FunctionId, args.CanEditName ? args.Name : workspace.Name, args.Category, args.Description);
                }

                if (args.CanEditName) workspace.Name = args.Name;
                workspace.Description = args.Description;
                workspace.Category = args.Category;

                workspace.Save();
            }
        }

        private void GoToWorkspace( NodeViewModel viewModel )
        {
            if (viewModel == null) return;

            if (viewModel.GotoWorkspaceCommand.CanExecute(null))
            {
                viewModel.GotoWorkspaceCommand.Execute(null);
            }
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

    public partial class DoubleSliderInput : Double
    {
        public override void SetupCustomUIElements(object ui)
        {
            var nodeUI = ui as dynNodeView;

            //add a slider control to the input grid of the control
            var tb_slider = new DynamoSlider(this);
            tb_slider.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            tb_slider.VerticalAlignment = System.Windows.VerticalAlignment.Center;

            tb_slider.MinWidth = 150;

            tb_slider.TickPlacement = System.Windows.Controls.Primitives.TickPlacement.None;

            tb_slider.PreviewMouseUp += delegate
            {
                dynSettings.ReturnFocusToSearch();
            };

            var mintb = new DynamoTextBox();
            mintb.Width = double.NaN;

            mintb.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF));

            // input value textbox
            var valtb = new DynamoTextBox();
            valtb.Width = double.NaN;
            valtb.Margin = new Thickness(0, 0, 10, 0);

            var maxtb = new DynamoTextBox();
            maxtb.Width = double.NaN;

            maxtb.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF));

            var sliderGrid = new Grid();
            sliderGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
            sliderGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
            sliderGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            sliderGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });

            sliderGrid.Children.Add(valtb);
            sliderGrid.Children.Add(mintb);
            sliderGrid.Children.Add(tb_slider);
            sliderGrid.Children.Add(maxtb);

            Grid.SetColumn(valtb, 0);
            Grid.SetColumn(mintb, 1);
            Grid.SetColumn(tb_slider, 2);
            Grid.SetColumn(maxtb, 3);
            nodeUI.inputGrid.Children.Add(sliderGrid);

            maxtb.DataContext = this;
            tb_slider.DataContext = this;
            mintb.DataContext = this;
            valtb.DataContext = this;

            // value input
            valtb.BindToProperty(new System.Windows.Data.Binding("Value")
            {
                Mode = BindingMode.TwoWay,
                Converter = new DoubleDisplay()
            });

            // slider value 
            var sliderBinding = new System.Windows.Data.Binding("Value")
            {
                Mode = BindingMode.TwoWay,
                Source = this,
            };
            tb_slider.SetBinding(Slider.ValueProperty, sliderBinding);

            // max value
            maxtb.BindToProperty(new System.Windows.Data.Binding("Max")
            {
                Mode = BindingMode.TwoWay,
                Converter = new DoubleDisplay(),
                Source = this,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            });

            // max slider value
            var bindingMaxSlider = new System.Windows.Data.Binding("Max")
            {
                Mode = BindingMode.OneWay,
                Source = this,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };
            tb_slider.SetBinding(Slider.MaximumProperty, bindingMaxSlider);


            // min value
            mintb.BindToProperty(new System.Windows.Data.Binding("Min")
            {
                Mode = BindingMode.TwoWay,
                Converter = new DoubleDisplay(),
                Source = this,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            });

            // min slider value
            var bindingMinSlider = new System.Windows.Data.Binding("Min")
            {
                Mode = BindingMode.OneWay,
                Source = this,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };
            tb_slider.SetBinding(Slider.MinimumProperty, bindingMinSlider);
        }

        protected override bool UpdateValueCore(string name, string value)
        {
            var converter = new DoubleDisplay();
            switch (name)
            {
                case "Value":
                    this.Value = ((double)converter.ConvertBack(value, typeof(double), null, null));
                    return true; // UpdateValueCore handled.
                case "Max":
                    this.Max = ((double)converter.ConvertBack(value, typeof(double), null, null));
                    return true; // UpdateValueCore handled.
                case "Min":
                    this.Min = ((double)converter.ConvertBack(value, typeof(double), null, null));
                    return true; // UpdateValueCore handled.
            }

            return base.UpdateValueCore(name, value);
        }
    }

    public partial class IntegerSliderInput : Integer
    {
        public override void SetupCustomUIElements(object ui)
        {
            var nodeUI = ui as dynNodeView;

            //add a slider control to the input grid of the control
            var tb_slider = new DynamoSlider(this);
            tb_slider.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            tb_slider.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            tb_slider.MinWidth = 150;
            tb_slider.TickPlacement = System.Windows.Controls.Primitives.TickPlacement.BottomRight;
            tb_slider.TickFrequency = 1;
            tb_slider.IsSnapToTickEnabled = true;

            tb_slider.PreviewMouseUp += delegate
            {
                dynSettings.ReturnFocusToSearch();
            };

            var mintb = new DynamoTextBox();
            mintb.Width = double.NaN;

            mintb.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF));

            // input value textbox
            var valtb = new DynamoTextBox();
            valtb.Width = double.NaN;
            valtb.Margin = new Thickness(0, 0, 10, 0);

            var maxtb = new DynamoTextBox();
            maxtb.Width = double.NaN;

            maxtb.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF));

            var sliderGrid = new Grid();
            sliderGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
            sliderGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
            sliderGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            sliderGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });

            sliderGrid.Children.Add(valtb);
            sliderGrid.Children.Add(mintb);
            sliderGrid.Children.Add(tb_slider);
            sliderGrid.Children.Add(maxtb);

            Grid.SetColumn(valtb, 0);
            Grid.SetColumn(mintb, 1);
            Grid.SetColumn(tb_slider, 2);
            Grid.SetColumn(maxtb, 3);
            nodeUI.inputGrid.Children.Add(sliderGrid);

            maxtb.DataContext = this;
            tb_slider.DataContext = this;
            mintb.DataContext = this;
            valtb.DataContext = this;

            // value input
            valtb.BindToProperty(new System.Windows.Data.Binding("Value")
            {
                Mode = BindingMode.TwoWay,
                Converter = new IntegerDisplay()
            });

            // slider value 
            var sliderBinding = new System.Windows.Data.Binding("Value")
            {
                Mode = BindingMode.TwoWay,
                Source = this,
            };
            tb_slider.SetBinding(Slider.ValueProperty, sliderBinding);

            // max value
            maxtb.BindToProperty(new System.Windows.Data.Binding("Max")
            {
                Mode = BindingMode.TwoWay,
                Converter = new IntegerDisplay(),
                Source = this,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            });

            // max slider value
            var bindingMaxSlider = new System.Windows.Data.Binding("Max")
            {
                Mode = BindingMode.OneWay,
                Source = this,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };
            tb_slider.SetBinding(Slider.MaximumProperty, bindingMaxSlider);


            // min value
            mintb.BindToProperty(new System.Windows.Data.Binding("Min")
            {
                Mode = BindingMode.TwoWay,
                Converter = new IntegerDisplay(),
                Source = this,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            });

            // min slider value
            var bindingMinSlider = new System.Windows.Data.Binding("Min")
            {
                Mode = BindingMode.OneWay,
                Source = this,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };
            tb_slider.SetBinding(Slider.MinimumProperty, bindingMinSlider);
        }

        protected override bool UpdateValueCore(string name, string value)
        {
            var converter = new IntegerDisplay();
            switch (name)
            {
                case "Value":
                    this.Value = ((int)converter.ConvertBack(value, typeof(int), null, null));
                    return true; // UpdateValueCore handled.
                case "Max":
                    this.Max = ((int)converter.ConvertBack(value, typeof(int), null, null));
                    return true; // UpdateValueCore handled.
                case "Min":
                    this.Min = ((int)converter.ConvertBack(value, typeof(int), null, null));
                    return true; // UpdateValueCore handled.
            }

            return base.UpdateValueCore(name, value);
        }
    }

    public partial class BoolSelector : Bool
    {
        public override void SetupCustomUIElements(object ui)
        {
            var nodeUI = ui as dynNodeView;

            //add a text box to the input grid of the control
            var rbTrue = new System.Windows.Controls.RadioButton();
            var rbFalse = new System.Windows.Controls.RadioButton();
            rbTrue.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            rbFalse.VerticalAlignment = System.Windows.VerticalAlignment.Center;

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
            rbTrue.Checked += new System.Windows.RoutedEventHandler(rbTrue_Checked);
            rbFalse.Checked += new System.Windows.RoutedEventHandler(rbFalse_Checked);

            rbFalse.DataContext = this;
            rbTrue.DataContext = this;

            var rbTrueBinding = new System.Windows.Data.Binding("Value")
            {
                Mode = BindingMode.TwoWay,
            };
            rbTrue.SetBinding(System.Windows.Controls.RadioButton.IsCheckedProperty, rbTrueBinding);

            var rbFalseBinding = new System.Windows.Data.Binding("Value")
            {
                Mode = BindingMode.TwoWay,
                Converter = new InverseBoolDisplay()
            };
            rbFalse.SetBinding(System.Windows.Controls.RadioButton.IsCheckedProperty, rbFalseBinding);
        }

        void rbFalse_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            //Value = false;
            dynSettings.ReturnFocusToSearch();
        }

        void rbTrue_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            //Value = true;
            dynSettings.ReturnFocusToSearch();
        }
    }

    public partial class StringInput : String
    {
        public override void SetupCustomUIElements(object ui)
        {
            var nodeUI = ui as dynNodeView;

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
            System.Windows.Controls.Grid.SetColumn(tb, 0);
            System.Windows.Controls.Grid.SetRow(tb, 0);

            tb.DataContext = this;
            tb.BindToProperty(new System.Windows.Data.Binding("Value")
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
                this.Value = ((string)converter.ConvertBack(value, typeof(string), null, null));
                return true; // UpdateValueCore handled.
            }

            // There's another 'UpdateValueCore' method in 'String' base class,
            // since they are both bound to the same property, 'StringInput' 
            // should be given a chance to handle the property value change first
            // before the base class 'String'.
            return base.UpdateValueCore(name, value);
        }
    }

    public partial class StringFilename : BasicInteractive<string>
    {
        public override void SetupCustomUIElements(object ui)
        {
            var nodeUI = ui as dynNodeView;

            //add a button to the inputGrid on the dynElement
            var readFileButton = new DynamoNodeButton();

            //readFileButton.Margin = new System.Windows.Thickness(4);
            readFileButton.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            readFileButton.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            readFileButton.Click += new System.Windows.RoutedEventHandler(readFileButton_Click);
            readFileButton.Content = "Browse...";
            readFileButton.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            readFileButton.VerticalAlignment = System.Windows.VerticalAlignment.Center;

            var tb = new TextBox();
            if (string.IsNullOrEmpty(Value))
                Value = "No file selected.";

            tb.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            tb.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            var backgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 0, 0, 0));
            tb.Background = backgroundBrush;
            tb.BorderThickness = new Thickness(0);
            tb.IsReadOnly = true;
            tb.IsReadOnlyCaretVisible = false;
            tb.TextChanged += delegate { tb.ScrollToHorizontalOffset(double.PositiveInfinity); dynSettings.ReturnFocusToSearch(); };

            StackPanel sp = new StackPanel();
            sp.Children.Add(readFileButton);
            sp.Children.Add(tb);
            nodeUI.inputGrid.Children.Add(sp);

            tb.DataContext = this;
            var bindingVal = new System.Windows.Data.Binding("Value")
            {
                Mode = BindingMode.TwoWay,
                Converter = new FilePathDisplayConverter()
            };
            tb.SetBinding(TextBox.TextProperty, bindingVal);
        }


        protected virtual void readFileButton_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new OpenFileDialog
            {
                CheckFileExists = false
            };

            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                Value = openDialog.FileName;
            }
        }
 
    }

    public abstract partial class DropDrownBase : NodeWithOneOutput
    {
        public override void SetupCustomUIElements(object ui)
        {
            var nodeUI = ui as dynNodeView;

            base.SetupCustomUIElements(nodeUI);

            //add a drop down list to the window
            var combo = new ComboBox
            {
                Width = 300,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
                VerticalAlignment = System.Windows.VerticalAlignment.Center
            };
            nodeUI.inputGrid.Children.Add(combo);
            System.Windows.Controls.Grid.SetColumn(combo, 0);
            System.Windows.Controls.Grid.SetRow(combo, 0);

            combo.DropDownOpened += new EventHandler(combo_DropDownOpened);
            combo.SelectionChanged += delegate
            {
                if (combo.SelectedIndex != -1)
                    RequiresRecalc = true;
            };

            combo.DataContext = this;
            //bind this combo box to the selected item hash

            var bindingVal = new System.Windows.Data.Binding("Items")
            {
                Mode = BindingMode.TwoWay,
                Source = this
            };
            combo.SetBinding(ComboBox.ItemsSourceProperty, bindingVal);

            //bind the selected index to the 
            var indexBinding = new System.Windows.Data.Binding("SelectedIndex")
            {
                Mode = BindingMode.TwoWay,
                Source = this
            };
            combo.SetBinding(ComboBox.SelectedIndexProperty, indexBinding);
        }

    }

    public partial class Formula
    {
        public override void SetupCustomUIElements(object ui)
        {
            var nodeUI = ui as dynNodeView;

            var tb = new DynamoTextBox(FormulaString)
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
                Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF))
            };

            nodeUI.inputGrid.Children.Add(tb);
            Grid.SetColumn(tb, 0);
            Grid.SetRow(tb, 0);

            tb.DataContext = this;
            tb.BindToProperty(new Binding("FormulaString")
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
                this.FormulaString = value;
                return true; // UpdateValueCore handled.
            }

            return base.UpdateValueCore(name, value);
        }
    }

    public partial class Output
    {
        public override void SetupCustomUIElements(object ui)
        {
            var nodeUI = ui as dynNodeView;

            //add a text box to the input grid of the control
            DynamoTextBox tb = new DynamoTextBox(Symbol)
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF))
            };

            nodeUI.inputGrid.Children.Add(tb);
            Grid.SetColumn(tb, 0);
            Grid.SetRow(tb, 0);

            tb.DataContext = this;
            tb.BindToProperty(new Binding("Symbol")
            {
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            });
        }

        protected override bool UpdateValueCore(string name, string value)
        {
            if (name == "Symbol")
            {
                this.Symbol = value;
                return true; // UpdateValueCore handled.
            }

            return base.UpdateValueCore(name, value);
        }
    }

    public partial class Symbol
    {
        public override void SetupCustomUIElements(object ui)
        {
            var nodeUI = ui as dynNodeView;

            //add a text box to the input grid of the control
            DynamoTextBox tb = new DynamoTextBox(InputSymbol)
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF))
            };

            nodeUI.inputGrid.Children.Add(tb);
            Grid.SetColumn(tb, 0);
            Grid.SetRow(tb, 0);

            tb.DataContext = this;
            tb.BindToProperty(new Binding("InputSymbol")
            {
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            });
        }

        protected override bool UpdateValueCore(string name, string value)
        {
            if (name == "InputSymbol")
            {
                this.InputSymbol = value;
                return true; // UpdateValueCore handled.
            }

            return base.UpdateValueCore(name, value);
        }
    }

    public partial class Watch : NodeWithOneOutput
    {
        public override void SetupCustomUIElements(object ui)
        {
            var nodeUI = ui as dynNodeView;

            watchTree = new WatchTree();

            nodeUI.grid.Children.Add(watchTree);
            watchTree.SetValue(Grid.RowProperty, 2);
            watchTree.SetValue(Grid.ColumnSpanProperty, 3);
            watchTree.Margin = new Thickness(5, 0, 5, 5);

            if (Root == null)
                Root = new WatchNode();
            watchTree.DataContext = Root;

            this.RequestBindingUnhook += new EventHandler(delegate
            {
                BindingOperations.ClearAllBindings(watchTree.treeView1);
            });

            this.RequestBindingRehook += new EventHandler(delegate
            {
                var sourceBinding = new Binding("Children")
                {
                    Mode = BindingMode.TwoWay,
                    Source = Root,
                };
                watchTree.treeView1.SetBinding(ItemsControl.ItemsSourceProperty, sourceBinding);
            });

        }

    }

    public partial class StringDirectory : StringFilename
    {
        protected override void readFileButton_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new FolderBrowserDialog
            {
                ShowNewFolderButton = true
            };

            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                Value = openDialog.SelectedPath;
            }
        }
    }

    public abstract partial class String : BasicInteractive<string>
    {
        public override void editWindowItem_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new EditWindow { DataContext = this };
            editWindow.BindToProperty(null, new System.Windows.Data.Binding("Value")
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
                this.Value = converter.ConvertBack(value, typeof(string), null, null) as string;
                return true;
            }

            return base.UpdateValueCore(name, value);
        }
    }

    public abstract partial class MeasurementInputBase
    {
        public override void SetupCustomUIElements(object ui)
        {
            var nodeUI = ui as dynNodeView;

            //add an edit window option to the 
            //main context window
            var editWindowItem = new System.Windows.Controls.MenuItem();
            editWindowItem.Header = "Edit...";
            editWindowItem.IsCheckable = false;

            nodeUI.MainContextMenu.Items.Add(editWindowItem);

            editWindowItem.Click += new RoutedEventHandler(editWindowItem_Click);
            //add a text box to the input grid of the control
            var tb = new DynamoTextBox();
            tb.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            tb.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            nodeUI.inputGrid.Children.Add(tb);
            System.Windows.Controls.Grid.SetColumn(tb, 0);
            System.Windows.Controls.Grid.SetRow(tb, 0);
            tb.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF));

            tb.DataContext = this;
            tb.BindToProperty(new System.Windows.Data.Binding("Value")
            {
                Mode = BindingMode.TwoWay,
                Converter = new Controls.MeasureConverter(),
                ConverterParameter = _measure,
                NotifyOnValidationError = false,
                Source = this,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            });

            tb.OnChangeCommitted += delegate { RequiresRecalc = true; };

            dynSettings.Controller.PreferenceSettings.PropertyChanged += PreferenceSettings_PropertyChanged;
        }

        void PreferenceSettings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            RaisePropertyChanged("Value");
            RequiresRecalc = true;
        }

        protected override bool UpdateValueCore(string name, string value)
        {
            if (name == "Value")
            {
                var converter = new Controls.MeasureConverter();
                this.Value = ((double)converter.ConvertBack(value, typeof(double), _measure, null));
                return true; // UpdateValueCore handled.
            }

            return base.UpdateValueCore(name, value);
        }

        private void editWindowItem_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new EditWindow() { DataContext = this };
            editWindow.BindToProperty(null, new System.Windows.Data.Binding("Value")
            {
                Mode = BindingMode.TwoWay,
                Converter = new Controls.MeasureConverter(),
                ConverterParameter = _measure,
                NotifyOnValidationError = false,
                Source = this,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            });

            editWindow.ShowDialog();
        }
    }

    public class DynamoNodeButton : System.Windows.Controls.Button
    {
        private string eventName = string.Empty;
        private ModelBase model = null;

        public DynamoNodeButton()
            : base()
        {
            Style = (Style)SharedDictionaryManager.DynamoModernDictionary["SNodeTextButton"];

            this.Margin = new Thickness(1, 0, 1, 0);
        }

        public DynamoNodeButton(ModelBase model, string eventName)
            : this()
        {
            this.model = model;
            this.eventName = eventName;
            this.Click += OnDynamoNodeButtonClick;
        }

        private void OnDynamoNodeButtonClick(object sender, RoutedEventArgs e)
        {
            // If this DynamoNodeButton was created with an associated model 
            // and the event name, then the owner of this button (a ModelBase) 
            // needs the "DynCmd.ModelEventCommand" to be sent when user clicks
            // on the button.
            // 
            if (null != this.model && (!string.IsNullOrEmpty(this.eventName)))
            {
                var command = new DynCmd.ModelEventCommand(model.GUID, eventName);
                dynSettings.Controller.DynamoViewModel.ExecuteCommand(command);
            }
        }
    }

    [NodeName("Read Image File")]
    [NodeCategory(BuiltinNodeCategories.IO_FILE)]
    [NodeDescription("Reads data from an image file.")]
    public class ImageFileReader : FileReaderBase
    {
        System.Windows.Controls.Image image1;

        public ImageFileReader()
        {
            InPortData.Add(new PortData("numX", "Number of samples in the X direction.", typeof(object)));
            InPortData.Add(new PortData("numY", "Number of samples in the Y direction.", typeof(object)));
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
                        DispatchOnUIThread(delegate
                        {
                            // how to convert a bitmap to an imagesource http://blog.laranjee.com/how-to-convert-winforms-bitmap-to-wpf-imagesource/ 
                            // TODO - watch out for memory leaks using system.drawing.bitmaps in managed code, see here http://social.msdn.microsoft.com/Forums/en/csharpgeneral/thread/4e213af5-d546-4cc1-a8f0-462720e5fcde
                            // need to call Dispose manually somewhere, or perhaps use a WPF native structure instead of bitmap?

                            var hbitmap = bmp.GetHbitmap();
                            var imageSource = Imaging.CreateBitmapSourceFromHBitmap(hbitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(bmp.Width, bmp.Height));
                            image1.Source = imageSource;
                        });

                        // Do some processing
                        for (int y = 0; y < yDiv; y++)
                        {
                            for (int x = 0; x < xDiv; x++)
                            {
                                System.Drawing.Color pixelColor = bmp.GetPixel(x * (int)(bmp.Width / xDiv), y * (int)(bmp.Height / yDiv));
                                result = FSharpList<FScheme.Value>.Cons(FScheme.Value.NewContainer(pixelColor), result);
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
            else
                return FScheme.Value.NewList(FSharpList<FScheme.Value>.Empty);
        }

        public override void SetupCustomUIElements(object ui)
        {
            var nodeUI = ui as dynNodeView;

            image1 = new System.Windows.Controls.Image
            {
                MaxWidth = 400,
                MaxHeight = 400,
                Margin = new Thickness(5),
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                Name = "image1",
                VerticalAlignment = System.Windows.VerticalAlignment.Center
            };

            nodeUI.grid.Children.Add(image1);
            image1.SetValue(Grid.RowProperty, 2);
            image1.SetValue(Grid.ColumnProperty, 0);
            image1.SetValue(Grid.ColumnSpanProperty, 3);
        }
    }
}
