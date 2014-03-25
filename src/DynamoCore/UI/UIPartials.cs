using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using Dynamo.Controls;
using Dynamo.Core;
using Dynamo.Models;
using Dynamo.UI;
using Dynamo.UI.Prompts;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Binding = System.Windows.Data.Binding;
using ComboBox = System.Windows.Controls.ComboBox;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using MenuItem = System.Windows.Controls.MenuItem;
using VerticalAlignment = System.Windows.VerticalAlignment;
using DynCmd = Dynamo.ViewModels.DynamoViewModel;

namespace Dynamo.Nodes
{
    public abstract partial class VariableInput
    {
        public void SetupCustomUIElements(dynNodeView nodeUI)
        {
            var addButton = new DynamoNodeButton(this, "AddInPort");
            addButton.Content = "+";
            addButton.Width = 20;
            //addButton.Height = 20;

            var subButton = new DynamoNodeButton(this, "RemoveInPort");
            subButton.Content = "-";
            subButton.Width = 20;
            //subButton.Height = 20;

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

    public partial class VariableInputAndOutput
    {
        public void SetupCustomUIElements(dynNodeView nodeUI)
        {
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

    public partial class Sublists
    {
        public void SetupCustomUIElements(dynNodeView nodeUI)
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

    public abstract partial class BasicInteractive<T>
    {
        public virtual void SetupCustomUIElements(dynNodeView nodeUI)
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
        public void SetupCustomUIElements(dynNodeView nodeUI)
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

            tb.BindToProperty(new System.Windows.Data.Binding("Value")
            {
                Mode = BindingMode.TwoWay,
                Converter = new DoubleInputDisplay(),
                NotifyOnValidationError = false,
                Source = this,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            });

            ((PreferenceSettings)dynSettings.Controller.PreferenceSettings).PropertyChanged += Preferences_PropertyChanged;
        }

        void Preferences_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "NumberFormat":
                    RaisePropertyChanged("Value");
                    break;
            }
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

    public partial class Function : IWpfNode
    {
        public void SetupCustomUIElements(dynNodeView nodeUI)
        {
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
                    var def = (workspace as CustomNodeWorkspaceModel).CustomNodeDefinition;
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

    public abstract partial class DropDrownBase
    {
        public void SetupCustomUIElements(dynNodeView nodeUI)
        {
            // Do not call 'NodeModel.InitializeUI' here since it will cause 
            // that method to dispatch the call back to 'SetupCustomUIElements'
            // method, resulting in an eventual stack overflow.
            // 
            // base.InitializeUI(nodeUI);

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

    public partial class CodeBlockNodeModel
    {
        public void SetupCustomUIElements(dynNodeView nodeUI)
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
        public void SetupCustomUIElements(dynNodeView nodeUI)
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
        public void SetupCustomUIElements(dynNodeView nodeUI)
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
        public void SetupCustomUIElements(dynNodeView nodeUI)
        {
            _watchTree = new WatchTree();

            nodeUI.grid.Children.Add(_watchTree);
            _watchTree.SetValue(Grid.RowProperty, 2);
            _watchTree.SetValue(Grid.ColumnSpanProperty, 3);
            _watchTree.Margin = new Thickness(5, 0, 5, 5);

            if (Root == null)
                Root = new WatchViewModel();
            _watchTree.DataContext = Root;

            RequestBindingUnhook += delegate
            {
                BindingOperations.ClearAllBindings(_watchTree.treeView1);
            };

            RequestBindingRehook += delegate
            {
                var sourceBinding = new Binding("Children")
                {
                    Mode = BindingMode.TwoWay,
                    Source = Root,
                };
                _watchTree.treeView1.SetBinding(ItemsControl.ItemsSourceProperty, sourceBinding);
            };

            var checkedBinding = new Binding("ShowRawData")
            {
                Mode = BindingMode.TwoWay,
                Source = Root
            };

            var rawDataMenuItem = new System.Windows.Controls.MenuItem
            {
                Header = "Show Raw Data", 
                IsCheckable = true,
            };
            rawDataMenuItem.SetBinding(System.Windows.Controls.MenuItem.IsCheckedProperty, checkedBinding);

            nodeUI.MainContextMenu.Items.Add(rawDataMenuItem);

            ((PreferenceSettings)dynSettings.Controller.PreferenceSettings).PropertyChanged += PreferenceSettings_PropertyChanged;

            Root.PropertyChanged += Root_PropertyChanged;
        }

        void Root_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ShowRawData")
            {
                ResetWatch();
            }
        }

        void PreferenceSettings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //if the units settings have been modified in the UI, watch has
            //to immediately update to show unit objects in the correct format
            if (e.PropertyName == "LengthUnit" ||
                e.PropertyName == "AreaUnit" ||
                e.PropertyName == "VolumeUnit" ||
                e.PropertyName == "NumberFormat")
            {
                ResetWatch();
            }
        }

        private void ResetWatch()
        {
            int count = 0;
            DispatchOnUIThread(
                delegate
                {
                    //unhook the binding
                    OnRequestBindingUnhook(EventArgs.Empty);

                    Root.Children.Clear();

                    Root.Children.Add(GetWatchNode());

                    count++;

                    //rehook the binding
                    OnRequestBindingRehook(EventArgs.Empty);
                });
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

    public class DynamoNodeButton : System.Windows.Controls.Button
    {
        private string eventName = string.Empty;
        private ModelBase model = null;

        public DynamoNodeButton()
            : base()
        {
            Style = (Style)SharedDictionaryManager.DynamoModernDictionary["SNodeTextButton"];
            Margin = new Thickness(1, 0, 1, 0);
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
}
