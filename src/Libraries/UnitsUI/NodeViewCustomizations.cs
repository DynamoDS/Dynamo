﻿using Dynamo.Configuration;
using Dynamo.Controls;
using Dynamo.Core;
using Dynamo.Graph.Nodes;
using Dynamo.Nodes;
using Dynamo.UI.Prompts;
using Dynamo.ViewModels;
using Dynamo.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using UnitsUI.Controls;
using UnitsUI.Converters;

namespace UnitsUI
{

    public abstract class MeasurementInputBaseNodeViewCustomization : INodeViewCustomization<MeasurementInputBase>
    {
        private MeasurementInputBase mesBaseModel;
        private DynamoViewModel dynamoViewModel;
        private DynamoTextBox tb;

        public void CustomizeView(MeasurementInputBase model, NodeView nodeView)
        {
            this.mesBaseModel = model;
            this.dynamoViewModel = nodeView.ViewModel.DynamoViewModel;

            //add an edit window option to the 
            //main context window
            var editWindowItem = new MenuItem()
            {
                Header = Properties.Resources.EditHeader,
                IsCheckable = false,
                Tag = nodeView.ViewModel.DynamoViewModel
            };

            nodeView.MainContextMenu.Items.Add(editWindowItem);

            editWindowItem.Click += editWindowItem_Click;

            //add a text box to the input grid of the control
            this.tb = new DynamoTextBox();
            tb.HorizontalAlignment = HorizontalAlignment.Stretch;
            tb.VerticalAlignment = VerticalAlignment.Center;
            nodeView.inputGrid.Children.Add(tb);
            Grid.SetColumn(tb, 0);
            Grid.SetRow(tb, 0);
            tb.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF));

            tb.DataContext = model;
            tb.BindToProperty(new Binding("Value")
            {
                Mode = BindingMode.TwoWay,
                Converter = new MeasureConverter(),
                ConverterParameter = model.Measure,
                NotifyOnValidationError = false,
                Source = model,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            });

            tb.OnChangeCommitted += TextChangehandler;

            (nodeView.ViewModel.DynamoViewModel.Model.PreferenceSettings).PropertyChanged += PreferenceSettings_PropertyChanged;
        }

        private void TextChangehandler()
        {
            mesBaseModel.OnNodeModified();
        }

        void PreferenceSettings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "AreaUnit" ||
                e.PropertyName == "VolumeUnit" ||
                e.PropertyName == "LengthUnit" ||
                e.PropertyName == "NumberFormat")
            {
                this.mesBaseModel.ForceValueRaisePropertyChanged();

                this.mesBaseModel.OnNodeModified();
            }
        }

        private void editWindowItem_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = this.dynamoViewModel;
            var editWindow = new EditWindow(viewModel) { DataContext = this.mesBaseModel };
            editWindow.BindToProperty(null, new Binding("Value")
            {
                Mode = BindingMode.TwoWay,
                Converter = new MeasureConverter(),
                ConverterParameter = this.mesBaseModel.Measure,
                NotifyOnValidationError = false,
                Source = this.mesBaseModel,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            });

            editWindow.ShowDialog();
        }

        public void Dispose()
        {
            tb.OnChangeCommitted -= TextChangehandler;
        }
    }
    public class LengthFromStringNodeViewCustomization : MeasurementInputBaseNodeViewCustomization,
                                                        INodeViewCustomization<LengthFromString>
    {
        public void CustomizeView(LengthFromString model, NodeView nodeView)
        {
            base.CustomizeView(model, nodeView);
        }
    }

    public class AreaFromStringNodeViewCustomization : MeasurementInputBaseNodeViewCustomization,
                                                 INodeViewCustomization<AreaFromString>
    {
        public void CustomizeView(AreaFromString model, NodeView nodeView)
        {
            base.CustomizeView(model, nodeView);
        }
    }

    public class VolumeFromStringNodeViewCustomization : MeasurementInputBaseNodeViewCustomization,
                                               INodeViewCustomization<VolumeFromString>
    {
        public void CustomizeView(VolumeFromString model, NodeView nodeView)
        {
            base.CustomizeView(model, nodeView);
        }
    }

    public class StringInputNodeViewCustomization : INodeViewCustomization<UnitInput>
    {
        private DynamoViewModel dynamoViewModel;
        private UnitInput nodeModel;
        private MenuItem editWindowItem;

        public void CustomizeView(UnitInput stringInput, NodeView nodeView)
        {
            this.nodeModel = stringInput;
            this.dynamoViewModel = nodeView.ViewModel.DynamoViewModel;

            this.editWindowItem = new MenuItem
            {
                Header = Dynamo.Wpf.Properties.Resources.StringInputNodeEditMenu,
                IsCheckable = false
            };
            nodeView.MainContextMenu.Items.Add(editWindowItem);

            editWindowItem.Click += editWindowItem_Click;

            var grid = new Grid()
            {
                Height = Double.NaN,
                Width = Double.NaN
            };

            RowDefinition rowDef1 = new RowDefinition();
            RowDefinition rowDef2 = new RowDefinition();

            grid.RowDefinitions.Add(rowDef1);
            grid.RowDefinitions.Add(rowDef2);


            //add a text box to the input grid of the control
            var tb = new StringTextBox
            {
                TextWrapping = TextWrapping.Wrap,
                MinHeight = Configurations.PortHeightInPixels,
                MaxWidth = 200,
                VerticalAlignment = VerticalAlignment.Stretch

            };
            tb.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF));

            grid.Children.Add(tb);

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

            //add a drop down list to the window
            var combo = new ComboBox
            {
                Width = System.Double.NaN,
                MinWidth = 100,
                Height = Configurations.PortHeightInPixels,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center
            };
            grid.Children.Add(combo);
            Grid.SetColumn(combo, 0);
            Grid.SetRow(combo, 1);

            //combo.DropDownOpened += combo_DropDownOpened;
            combo.SelectionChanged += delegate
            {
                if (combo.SelectedIndex != -1)
                    nodeModel.OnNodeModified();
            };

            combo.DataContext = nodeModel;

            // bind this combo box to the selected item hash
            var bindingVal = new System.Windows.Data.Binding("Items")
            {
                Source = nodeModel
            };
            combo.SetBinding(ItemsControl.ItemsSourceProperty, bindingVal);

            // bind the selected index to the model property SelectedIndex
            var indexBinding = new Binding("SelectedUnit")
            {
                Mode = BindingMode.TwoWay,
                Source = nodeModel
            };
            combo.SetBinding(Selector.SelectedItemProperty, indexBinding);

            nodeView.inputGrid.Children.Add(grid);
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

    class ConverterNodeViewCustomization : INodeViewCustomization<ForgeDynamoConvert>
    {
        private NodeModel nodeModel;
        private ForgeDynamoConverterControl converterControl;
        private NodeViewModel nodeViewModel;
        private ForgeDynamoConvert convertModel;
        private ForgeConverterViewModel converterViewModel;

        public void CustomizeView(ForgeDynamoConvert model, NodeView nodeView)
        {
            nodeModel = nodeView.ViewModel.NodeModel;
            nodeViewModel = nodeView.ViewModel;
            convertModel = model;
            converterControl = new ForgeDynamoConverterControl(model, nodeView)
            {
                DataContext = new ForgeConverterViewModel(model, nodeView),
            };
            converterViewModel = converterControl.DataContext as ForgeConverterViewModel;
            nodeView.inputGrid.Children.Add(converterControl);
            converterControl.Loaded += converterControl_Loaded;
            converterControl.SelectConversionQuantity.PreviewMouseUp += SelectConversionQuantity_PreviewMouseUp;
            converterControl.SelectConversionFrom.PreviewMouseUp += SelectConversionFrom_PreviewMouseUp;
            converterControl.SelectConversionTo.PreviewMouseUp += SelectConversionTo_MouseLeftButtonDown;
        }

        private void SelectConversionQuantity_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            nodeViewModel.WorkspaceViewModel.HasUnsavedChanges = true;
            //var undoRecorder = nodeViewModel.WorkspaceViewModel.Model.UndoRecorder;
            //WorkspaceModel.RecordModelForModification(nodeModel, undoRecorder);
        }

        private void SelectConversionFrom_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            nodeViewModel.WorkspaceViewModel.HasUnsavedChanges = true;
            //var undoRecorder = nodeViewModel.WorkspaceViewModel.Model.UndoRecorder;
            //WorkspaceModel.RecordModelForModification(nodeModel, undoRecorder);
        }

        private void SelectConversionTo_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            nodeViewModel.WorkspaceViewModel.HasUnsavedChanges = true;
            //var undoRecorder = nodeViewModel.WorkspaceViewModel.Model.UndoRecorder;
            //WorkspaceModel.RecordModelForModification(nodeModel, undoRecorder);
        }

        private void converterControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var control = sender as ForgeDynamoConverterControl;
            control.SelectConversionFrom.SelectedIndex = RetrieveSelectedItemIndex(convertModel.SelectedFromConversion.Name, control.SelectConversionFrom.Items);
            control.SelectConversionTo.SelectedIndex = RetrieveSelectedItemIndex(convertModel.SelectedToConversion.Name, control.SelectConversionTo.Items);
            control.SelectConversionQuantity.SelectedIndex = RetrieveSelectedItemIndex(convertModel.SelectedQuantityConversion.Name, control.SelectConversionQuantity.Items);
        }

        public void Dispose()
        {
            converterControl.SelectConversionQuantity.PreviewMouseUp -= SelectConversionQuantity_PreviewMouseUp;
            converterControl.SelectConversionFrom.PreviewMouseUp -= SelectConversionFrom_PreviewMouseUp;
            converterControl.SelectConversionTo.PreviewMouseUp -= SelectConversionTo_MouseLeftButtonDown;
        }

        int RetrieveSelectedItemIndex(string name, ItemCollection items)
        {
            int selectedIndex = -1;
            int index = 0;

            foreach (var item in items)
            {
                if (item.ToString().Contains(name))
                {
                    selectedIndex = index;
                    return selectedIndex;
                }
                index++;
            }
            return selectedIndex;
        }
    }

    class UnitValueOutputDropdownViewCustomization : NotificationObject, INodeViewCustomization<UnitValueOutputDropdown>
    {
        private NodeModel nodeModel;
        private NodeViewModel nodeViewModel;
        private UnitValueOutputDropdown unitValueDropdownModel;
        private UnitValueOutputDropdownViewModel unitValueDropdownViewModel;
        private MenuItem editWindowItem;

        public void CustomizeView(UnitValueOutputDropdown model, NodeView nodeView)
        {
            unitValueDropdownViewModel = new UnitValueOutputDropdownViewModel(model, nodeView);
            nodeModel = model;

            var grid = new Grid();
            grid.Width = 220;
            grid.VerticalAlignment = VerticalAlignment.Stretch;
            grid.HorizontalAlignment = HorizontalAlignment.Stretch;
            grid.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(203, 198, 190));

            //add a text box to the input grid of the control
            var tb = new StringTextBox
            {
                TextWrapping = TextWrapping.Wrap,
                MinHeight = Configurations.PortHeightInPixels,
                Width = 175,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left
            };

            tb.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF));
            tb.IsReadOnly = true;
            tb.DataContext = unitValueDropdownViewModel;
            tb.BindToProperty(new Binding(nameof(UnitValueOutputDropdownViewModel.DisplayValue))
            {
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            });

            grid.Children.Add(tb);

            Expander ex = new Expander
            {
                ExpandDirection = ExpandDirection.Down,
                FlowDirection = FlowDirection.RightToLeft,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Padding = new Thickness(10, 2, 10, 2),
                IsHitTestVisible = true
            };

            grid.Children.Add(ex);

            var lb = new ListBox
            {
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Padding = new Thickness(0, 5, 0, 0),
                Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(203, 198, 190)),
                BorderThickness = new System.Windows.Thickness(0),
                BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(203, 198, 190))
            };

            ///Unit Controls
            var dockPanelUnit = new DockPanel();
            dockPanelUnit.FlowDirection = FlowDirection.LeftToRight;
            dockPanelUnit.Background = null;
            double labelMinWidth = 80;
            double comboMinMidth = 80;
            double comboMaxWidth = 100;
            var unitLabel = new Label
            {
                Content = "Unit",
                MinWidth = labelMinWidth
            };

            var unitCB = new ComboBox
            {
                DataContext = unitValueDropdownViewModel,
                FlowDirection = FlowDirection.LeftToRight,
                ItemsSource = unitValueDropdownViewModel.AllUnits,
                MinWidth = comboMinMidth,
                MaxWidth = comboMaxWidth,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                ToolTip = unitValueDropdownViewModel.SelectedUnit,
            };


            // bind this combo box to the selected item hash
            var bindingValAllUnits = new System.Windows.Data.Binding(nameof(UnitValueOutputDropdownViewModel.AllUnits))
            {
                Source = unitValueDropdownViewModel
            };
            unitCB.SetBinding(ItemsControl.ItemsSourceProperty, bindingValAllUnits);

            // bind the selected index to the model property SelectedIndex
            var indexBindingSelectedUnit = new Binding(nameof(UnitValueOutputDropdownViewModel.SelectedUnit))
            {
                Mode = BindingMode.TwoWay,
                Source = unitValueDropdownViewModel
            };
            unitCB.SetBinding(Selector.SelectedItemProperty, indexBindingSelectedUnit);
            unitCB.SelectionChanged += delegate
            {
                if (unitCB.SelectedIndex != -1)
                    RaisePropertyChanged(nameof(UnitValueOutputDropdownViewModel.SelectedUnit));
            };

            if (unitCB.SelectedItem == null)
            {
                unitCB.SelectedIndex = RetrieveSelectedItemIndex(unitValueDropdownViewModel.SelectedUnit.Name, unitCB.Items);
            }


            dockPanelUnit.Children.Add(unitLabel);
            dockPanelUnit.Children.Add(unitCB);
            lb.Items.Add(dockPanelUnit);

            ///Symbol Controls
            var dockPanelSymbol = new DockPanel();
            dockPanelSymbol.FlowDirection = FlowDirection.LeftToRight;
            dockPanelSymbol.Background = null;
            var symbolLabel = new Label
            {
                Content = "Symbol",
                MinWidth = labelMinWidth
            };
            var symbolCB = new ComboBox
            {
                DataContext = unitValueDropdownViewModel,
                FlowDirection = FlowDirection.LeftToRight,
                ItemsSource = unitValueDropdownViewModel.AllSymbols,
                MinWidth = comboMinMidth,
                MaxWidth = comboMaxWidth,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                ToolTip = unitValueDropdownViewModel.SelectedSymbol
            };

            // bind this combo box to the selected item hash
            var bindingValAllSymbols = new System.Windows.Data.Binding(nameof(UnitValueOutputDropdownViewModel.AllSymbols))
            {
                Source = unitValueDropdownViewModel
            };
            symbolCB.SetBinding(ItemsControl.ItemsSourceProperty, bindingValAllSymbols);

            // bind the selected index to the model property SelectedIndex
            var indexBindingSelectedSymbol = new Binding(nameof(UnitValueOutputDropdownViewModel.SelectedSymbol))
            {
                Mode = BindingMode.TwoWay,
                Source = unitValueDropdownViewModel
            };
            symbolCB.SetBinding(Selector.SelectedItemProperty, indexBindingSelectedSymbol);
            symbolCB.SelectionChanged += delegate
            {
                if (symbolCB.SelectedIndex != -1)
                    RaisePropertyChanged(nameof(UnitValueOutputDropdownViewModel.SelectedSymbol));
            };

            if (symbolCB.SelectedItem == null)
            {
                symbolCB.SelectedIndex = RetrieveSelectedItemIndex(unitValueDropdownViewModel.SelectedSymbol.Text, symbolCB.Items);
            }

            dockPanelSymbol.Children.Add(symbolLabel);
            dockPanelSymbol.Children.Add(symbolCB);
            lb.Items.Add(dockPanelSymbol);

            var dockPanelPrecision = new DockPanel();
            dockPanelPrecision.FlowDirection = FlowDirection.LeftToRight;
            dockPanelPrecision.Background = null;
            var precisionLabel = new Label
            {
                Content = "Precision",
                MinWidth = labelMinWidth
            };
            var precisionCB = new ComboBox
            {
                DataContext = unitValueDropdownViewModel,
                FlowDirection = FlowDirection.LeftToRight,
                ItemsSource = unitValueDropdownViewModel.AllPrecisions,
                MinWidth = comboMinMidth,
                MaxWidth = comboMaxWidth,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                ToolTip = unitValueDropdownViewModel.SelectedPrecision
            };

            // bind this combo box to the selected item hash
            var bindingValAllPrecisions = new System.Windows.Data.Binding(nameof(UnitValueOutputDropdownViewModel.AllPrecisions))
            {
                Source = unitValueDropdownViewModel,
            };
            precisionCB.SetBinding(ItemsControl.ItemsSourceProperty, bindingValAllPrecisions);

            // bind the selected index to the model property SelectedIndex
            var indexBindingSelectedPrecision = new Binding(nameof(UnitValueOutputDropdownViewModel.SelectedPrecision))
            {
                Mode = BindingMode.TwoWay,
                Source = unitValueDropdownViewModel
            };
            precisionCB.SetBinding(Selector.SelectedItemProperty, indexBindingSelectedPrecision);
            precisionCB.SelectionChanged += delegate
            {
                if (precisionCB.SelectedIndex != -1)
                    RaisePropertyChanged(nameof(UnitValueOutputDropdownViewModel.SelectedPrecision));
            };

            if (precisionCB.SelectedItem == null)
            {
                precisionCB.SelectedIndex = RetrieveSelectedItemIndex(unitValueDropdownViewModel.SelectedPrecision.ToString(), precisionCB.Items);
            }

            dockPanelPrecision.Children.Add(precisionLabel);
            dockPanelPrecision.Children.Add(precisionCB);
            lb.Items.Add(dockPanelPrecision);

            var dockPanelFormat = new DockPanel();
            dockPanelFormat.Background = null;
            dockPanelFormat.FlowDirection = FlowDirection.LeftToRight;
            var formatLabel = new Label
            {
                Content = "Format",
                MinWidth = labelMinWidth
            };

            ComboBox formatCB = new ComboBox
            {
                DataContext = unitValueDropdownViewModel,
                FlowDirection = FlowDirection.LeftToRight,
                ItemsSource = unitValueDropdownViewModel.AllFormats,
                MinWidth = comboMinMidth,
                MaxWidth = comboMaxWidth,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                ToolTip = unitValueDropdownViewModel.SelectedFormat
            };

            // bind this combo box to the selected item hash
            var bindingValAllFormats = new System.Windows.Data.Binding(nameof(UnitValueOutputDropdownViewModel.AllFormats))
            {
                Source = unitValueDropdownViewModel
            };
            formatCB.SetBinding(ItemsControl.ItemsSourceProperty, bindingValAllFormats);

            // bind the selected index to the model property SelectedIndex
            var indexBindingSelectedFormat = new Binding(nameof(unitValueDropdownViewModel.SelectedFormat))
            {
                Mode = BindingMode.TwoWay,
                Source = unitValueDropdownViewModel
            };
            formatCB.SetBinding(Selector.SelectedItemProperty, indexBindingSelectedFormat);
            formatCB.SelectionChanged += delegate
            {
                if (formatCB.SelectedIndex != -1)
                    RaisePropertyChanged(nameof(UnitValueOutputDropdownViewModel.SelectedFormat));
            };

            if (formatCB.SelectedItem == null)
            {
                formatCB.SelectedIndex = RetrieveSelectedItemIndex(unitValueDropdownViewModel.SelectedFormat.ToString(), formatCB.Items);
            }

            dockPanelFormat.Children.Add(formatLabel);
            dockPanelFormat.Children.Add(formatCB);
            lb.Items.Add(dockPanelFormat);



            ex.Content = lb;
            nodeView.inputGrid.Children.Add(grid);
        }

        int RetrieveSelectedItemIndex(string name, ItemCollection items)
        {
            int selectedIndex = -1;
            int index = 0;

            foreach (var item in items)
            {
                if (item.ToString().Contains(name))
                {
                    selectedIndex = index;
                    return selectedIndex;
                }
                index++;
            }
            return selectedIndex;
        }



        public void Dispose()
        {

        }
    }

    class UnitValueOutputViewCustomization : INodeViewCustomization<UnitValueOutput>
    {
        private NodeModel nodeModel;
        private NodeViewModel nodeViewModel;
        private UnitValueOutput model;

        public void CustomizeView(UnitValueOutput model, NodeView nodeView)
        {

            //add a text box to the input grid of the control
            var tb = new StringTextBox
            {
                TextWrapping = TextWrapping.Wrap,
                MinHeight = Configurations.PortHeightInPixels,
                MaxWidth = 200,
                VerticalAlignment = VerticalAlignment.Stretch

            };

            tb.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF));
            tb.IsReadOnly = true;
            tb.DataContext = model;
            tb.BindToProperty(new Binding(nameof(UnitValueOutput.DisplayValue))
            {
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });

            nodeView.inputGrid.Children.Add(tb);
        }

        public void Dispose()
        {

        }
    }
}
