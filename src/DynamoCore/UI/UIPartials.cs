using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Dynamo.UI.Controls;

namespace Dynamo.Nodes
{
    public abstract partial class VariableInput
    {
        public void SetupCustomUIElements(dynNodeView nodeUI)
        {
            var addButton = new DynamoNodeButton(this, "AddInPort") { Content = "+", Width = 20 };
            //addButton.Height = 20;

            var subButton = new DynamoNodeButton(this, "RemoveInPort") { Content = "-", Width = 20 };
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
                Workspace.RecordModelsForUndo(models);
            }
            else
                Workspace.RecordModelForModification(this);
        }

        protected override bool HandleModelEventCore(string eventName)
        {
            switch (eventName)
            {
                case "AddInPort":
                    AddInput();
                    RegisterAllPorts();
                    return true; // Handled here.
                case "RemoveInPort":
                    Workspace.UndoRecorder.PopFromUndoGroup();
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
            var addButton = new DynamoNodeButton(this, "AddInPort")
            {
                Content = "+",
                Width = 20,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            var subButton = new DynamoNodeButton(this, "RemoveInPort")
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
                Workspace.RecordModelsForUndo(models);
            }
            else
                Workspace.RecordModelForModification(this);
        }

        protected override bool HandleModelEventCore(string eventName)
        {

            switch (eventName)
            {
                case "AddInPort":
                    AddInput();
                    RegisterAllPorts();
                    return true; // Handled here.
                case "RemoveInPort":
                    Workspace.UndoRecorder.PopFromUndoGroup();
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
        public override void SetupCustomUIElements(dynNodeView nodeUI)
        {
            //add a text box to the input grid of the control
            var tb = new DynamoTextBox
            {
                Background =
                    new SolidColorBrush(Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF))
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
            var editWindowItem = new MenuItem
            {
                Header = "Edit...",
                IsCheckable = false,
                Tag = nodeUI.ViewModel.DynamoViewModel
            };

            nodeUI.MainContextMenu.Items.Add(editWindowItem);
            editWindowItem.Click += editWindowItem_Click;
        }

        public virtual void editWindowItem_Click(object sender, RoutedEventArgs e)
        {
            //override in child classes
        }

        protected override bool ShouldDisplayPreviewCore()
        {
            return false; // Previews are not shown for this node type.
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
                VerticalAlignment = VerticalAlignment.Stretch,
                Background =
                    new SolidColorBrush(Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF))
            };

            nodeUI.inputGrid.Children.Add(tb);
            Grid.SetColumn(tb, 0);
            Grid.SetRow(tb, 0);

            tb.DataContext = this;

            tb.BindToProperty(new Binding("Value")
            {
                Mode = BindingMode.TwoWay,
                Converter = new DoubleInputDisplay(),
                NotifyOnValidationError = false,
                Source = this,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            });

            Workspace.DynamoModel.PreferenceSettings.PropertyChanged += Preferences_PropertyChanged;
        }

        void Preferences_PropertyChanged(object sender, PropertyChangedEventArgs e)
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

        protected override bool ShouldDisplayPreviewCore()
        {
            return false; // Previews are not shown for this node type.
        }
    }

    public partial class Function : IWpfNode
    {
        public void SetupCustomUIElements(dynNodeView nodeUI)
        {
            nodeUI.MainContextMenu.Items.Add(new Separator());

            // edit contents
            var editItem = new MenuItem
            {
                Header = "Edit Custom Node...",
                IsCheckable = false
            };
            nodeUI.MainContextMenu.Items.Add(editItem);
            editItem.Click += (sender, args) => GoToWorkspace(nodeUI.ViewModel);

            // edit properties
            var editPropertiesItem = new MenuItem
            {
                Header = "Edit Custom Node Properties...",
                IsCheckable = false
            };
            nodeUI.MainContextMenu.Items.Add(editPropertiesItem);
            editPropertiesItem.Click += (sender, args) => EditCustomNodeProperties();

            // publish
            var publishCustomNodeItem = new MenuItem
            {
                Header = "Publish This Custom Node...",
                IsCheckable = false,
                IsEnabled = nodeUI.ViewModel.DynamoViewModel.PackageManagerClientViewModel.Model.HasAuthenticator
            };
            nodeUI.MainContextMenu.Items.Add(publishCustomNodeItem);
            publishCustomNodeItem.Click += (sender, args) =>
            {
                GoToWorkspace(nodeUI.ViewModel);

                if (nodeUI.ViewModel.DynamoViewModel.PublishCurrentWorkspaceCommand.CanExecute(null))
                {
                    nodeUI.ViewModel.DynamoViewModel.PublishCurrentWorkspaceCommand.Execute(null);
                } 
            };

            nodeUI.UpdateLayout();
        }

        private void EditCustomNodeProperties()
        {
            var workspace = Definition.WorkspaceModel;

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

            Workspace.DynamoModel.OnRequestsFunctionNamePrompt(this, args);

            if (args.Success)
            {
                if (workspace is CustomNodeWorkspaceModel)
                {
                    var def = (workspace as CustomNodeWorkspaceModel).CustomNodeDefinition;
                    this.Workspace.DynamoModel.CustomNodeManager.Refactor(def.FunctionId, args.CanEditName ? args.Name : workspace.Name, args.Category, args.Description);
                }

                if (args.CanEditName) workspace.Name = args.Name;
                workspace.Description = args.Description;
                workspace.Category = args.Category;

                if (workspace.FileName != null)
                    workspace.Save();
            }
        }

        private static void GoToWorkspace( NodeViewModel viewModel )
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
                Width = System.Double.NaN,
                MinWidth = 100,
                Height = Configurations.PortHeightInPixels,
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

            combo.DropDownClosed += delegate
            {
                //disallow selection of nothing
                if (combo.SelectedIndex == -1)
                {
                    SelectedIndex = 0;
                }
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
            var cbe = new CodeBlockEditor(nodeUI.ViewModel);

            nodeUI.inputGrid.Children.Add(cbe);
            Grid.SetColumn(cbe, 0);
            Grid.SetRow(cbe, 0);
            
            cbe.SetBinding(CodeBlockEditor.CodeProperty,
                new Binding("Code")
                {
                    Mode = BindingMode.OneWay,
                    NotifyOnValidationError = false,
                    Source = this,
                });


            if (shouldFocus)
            {
                cbe.Focus();
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
                DataContext = this,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                Background =
                    new SolidColorBrush(Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF))
            };

            nodeUI.inputGrid.Children.Add(tb);
            Grid.SetColumn(tb, 0);
            Grid.SetRow(tb, 0);

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

    public partial class Watch
    {
        public void SetupCustomUIElements(dynNodeView nodeUI)
        {
            this.dynamoViewModel = nodeUI.ViewModel.DynamoViewModel;
            watchTree = new WatchTree();

            // MAGN-2446: Fixes the maximum width/height of watch node so it won't 
            // go too crazy on us. Note that this is only applied to regular watch 
            // node so it won't be limiting the size of image/3D watch nodes.
            // 
            nodeUI.PresentationGrid.MaxWidth = Configurations.MaxWatchNodeWidth;
            nodeUI.PresentationGrid.MaxHeight = Configurations.MaxWatchNodeHeight;
            nodeUI.PresentationGrid.Children.Add(watchTree);
            nodeUI.PresentationGrid.Visibility = Visibility.Visible;

            if (Root == null)
                Root = new WatchViewModel(this.dynamoViewModel.VisualizationManager);
            
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

            var checkedBinding = new Binding("ShowRawData")
            {
                Mode = BindingMode.TwoWay,
                Source = Root
            };

            var rawDataMenuItem = new MenuItem
            {
                Header = "Show Raw Data", 
                IsCheckable = true,
            };
            rawDataMenuItem.SetBinding(MenuItem.IsCheckedProperty, checkedBinding);

            nodeUI.MainContextMenu.Items.Add(rawDataMenuItem);

            ((PreferenceSettings)this.Workspace.DynamoModel.PreferenceSettings).PropertyChanged += PreferenceSettings_PropertyChanged;

            Root.PropertyChanged += Root_PropertyChanged;
        }

        void Root_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ShowRawData")
            {
                ResetWatch();
            }
        }

        void PreferenceSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
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

        protected override bool ShouldDisplayPreviewCore()
        {
            return false; // Previews are not shown for this node type.
        }
    }

    public abstract partial class AbstractString
    {
        public override void editWindowItem_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = GetDynamoViewModelFromMenuItem(sender as MenuItem);
            var editWindow = new EditWindow(viewModel) { DataContext = this };
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

    public partial class StringInput : AbstractString
    {
        public override void SetupCustomUIElements(dynNodeView ui)
        {
            var nodeUI = ui;

            base.SetupCustomUIElements(nodeUI);

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

    public class DynamoNodeButton : Button
    {
        private string eventName = string.Empty;
        private ModelBase model = null;
        private DynamoViewModel dynamoViewModel;
        private DynamoViewModel DynamoViewModel
        {
            get
            {
                if (this.dynamoViewModel != null) return this.dynamoViewModel;

                var f = WPF.FindUpVisualTree<dynNodeView>(this);
                if (f != null) this.dynamoViewModel = f.ViewModel.DynamoViewModel;

                return this.dynamoViewModel;
            }
        }

        public DynamoNodeButton()
        {
            Style = (Style)SharedDictionaryManager.DynamoModernDictionary["SNodeTextButton"];
            Margin = new Thickness(1, 0, 1, 0);
        }

        public DynamoNodeButton(ModelBase model, string eventName)
            : this()
        {
            this.model = model;
            this.eventName = eventName;
            Click += OnDynamoNodeButtonClick;
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
                var command = new DynamoModel.ModelEventCommand(model.GUID, eventName);
                this.DynamoViewModel.ExecuteCommand(command);
            }
        }
    }
}

