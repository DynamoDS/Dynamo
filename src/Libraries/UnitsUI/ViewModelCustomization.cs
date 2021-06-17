using Dynamo.Controls;
using Dynamo.Core;
using Dynamo.Graph.Nodes;
using Dynamo.UI.Commands;
using Dynamo.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitsUI
{
    public class ForgeConverterViewModel : NotificationObject
    {
        private readonly ForgeDynamoConvert dynamoConvertModel;
        public DelegateCommand ToggleButtonClick { get; set; }
        private readonly NodeViewModel nodeViewModel;
        private readonly NodeModel nodeModel;

        public DynamoUnits.Quantity SelectedQuantityConversion
        {
            get { return dynamoConvertModel.SelectedQuantityConversion; }
            set
            {
                dynamoConvertModel.SelectedQuantityConversion = value;
            }
        }

        public DynamoUnits.Unit SelectedFromConversion
        {
            get { return dynamoConvertModel.SelectedFromConversion; }
            set
            {
                if (value == null)
                    return;

                dynamoConvertModel.SelectedFromConversion = value;
            }
        }

        public DynamoUnits.Unit SelectedToConversion
        {
            get { return dynamoConvertModel.SelectedToConversion; }
            set
            {
                if (value == null)
                    return;

                dynamoConvertModel.SelectedToConversion = value;
            }
        }

        public List<DynamoUnits.Quantity> QuantityConversionSource
        {
            get { return dynamoConvertModel.QuantityConversionSource; }
        }

        public List<DynamoUnits.Unit> SelectedFromConversionSource
        {
            get { return dynamoConvertModel.SelectedFromConversionSource; }
            set
            {
                dynamoConvertModel.SelectedFromConversionSource = value;
            }
        }

        public List<DynamoUnits.Unit> SelectedToConversionSource
        {
            get { return dynamoConvertModel.SelectedToConversionSource; }
            set
            {
                dynamoConvertModel.SelectedToConversionSource = value;
            }
        }

        public bool IsSelectionFromBoxEnabled
        {
            get { return dynamoConvertModel.IsSelectionFromBoxEnabled; }
            set
            {
                dynamoConvertModel.IsSelectionFromBoxEnabled = value;
            }
        }

        public string SelectionFromBoxToolTip
        {
            get { return dynamoConvertModel.SelectionFromBoxToolTip; }
            set
            {
                dynamoConvertModel.SelectionFromBoxToolTip = value;
            }
        }

        public ForgeConverterViewModel(ForgeDynamoConvert model, NodeView nodeView)
        {
            dynamoConvertModel = model;
            nodeViewModel = nodeView.ViewModel;
            nodeModel = nodeView.ViewModel.NodeModel;
            model.PropertyChanged += model_PropertyChanged;
            ToggleButtonClick = new DelegateCommand(OnToggleButtonClick, CanToggleButton);
        }

        private void model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "SelectedQuantityConversionSource":
                    RaisePropertyChanged("SelectedQuantityConversionSource");
                    break;
                case "SelectedQuantityConversion":
                    RaisePropertyChanged("SelectedQuantityConversion");
                    break;
                case "SelectedFromConversionSource":
                    RaisePropertyChanged("SelectedFromConversionSource");
                    break;
                case "SelectedToConversionSource":
                    RaisePropertyChanged("SelectedToConversionSource");
                    break;
                case "SelectedFromConversion":
                    RaisePropertyChanged("SelectedFromConversion");
                    break;
                case "SelectedToConversion":
                    RaisePropertyChanged("SelectedToConversion");
                    break;
                case "IsSelectionFromBoxEnabled":
                    RaisePropertyChanged("IsSelectionFromBoxEnabled");
                    break;
                case "SelectionFromBoxToolTip":
                    RaisePropertyChanged("SelectionFromBoxToolTip");
                    break;
            }
        }

        /// <summary>
        /// Called when Toggle button is clicked.
        /// Switches the combo box values
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnToggleButtonClick(object obj)
        {
            //var undoRecorder = nodeViewModel.WorkspaceViewModel.Model.UndoRecorder;
            //WorkspaceModel.RecordModelForModification(nodeModel, undoRecorder);
            dynamoConvertModel.ToggleDropdownValues();
            nodeViewModel.WorkspaceViewModel.HasUnsavedChanges = true;
        }

        private bool CanToggleButton(object obj)
        {
            return true;
        }
    }
    public class UnitValueOutputDropdownViewModel : NotificationObject
    {
        private readonly UnitValueOutputDropdown unitValueDropdownModel;
       // public DelegateCommand ToggleButtonClick { get; set; }
        private readonly NodeViewModel nodeViewModel;
        private readonly NodeModel nodeModel;

        public string DisplayValue
        {
            get { return unitValueDropdownModel.DisplayValue; }
        }

        public DynamoUnits.Unit SelectedUnit
        {
            get { return unitValueDropdownModel.SelectedUnit; }
            set
            {
                unitValueDropdownModel.SelectedUnit = value;
            }
        }

        public DynamoUnits.UnitSymbol SelectedSymbol
        {
            get { return unitValueDropdownModel.SelectedSymbol; }
            set
            {
                unitValueDropdownModel.SelectedSymbol = value;
            }
        }

        public int SelectedPrecision
        {
            get { return unitValueDropdownModel.SelectedPrecision; }
            set
            {
                unitValueDropdownModel.SelectedPrecision = value;
            }
        }

        public bool SelectedFormat
        {
            get { return unitValueDropdownModel.SelectedFormat; }
            set
            {
                unitValueDropdownModel.SelectedFormat = value;
            }
        }

        public List<DynamoUnits.Unit> AllUnits
        {
            get { return unitValueDropdownModel.AllUnits; }
            set
            {
                unitValueDropdownModel.AllUnits = value;
            }
        }
        public List<DynamoUnits.UnitSymbol> AllSymbols
        {
            get { return unitValueDropdownModel.AllSymbols; }
            set
            {
                unitValueDropdownModel.AllSymbols = value;
            }
        }

        public List<int> AllPrecisions
        {
            get { return unitValueDropdownModel.AllPrecisions; }
            set
            {
                unitValueDropdownModel.AllPrecisions = value;
            }
        }
        public List<bool> AllFormats
        {
            get { return unitValueDropdownModel.AllFormats; }
            set
            {
                unitValueDropdownModel.AllFormats = value;
            }
        }
        public UnitValueOutputDropdownViewModel(UnitValueOutputDropdown model, NodeView nodeView)
        {
            unitValueDropdownModel = model;
            nodeViewModel = nodeView.ViewModel;
            nodeModel = nodeView.ViewModel.NodeModel;
            unitValueDropdownModel.PropertyChanged += model_PropertyChanged;
            //ToggleButtonClick = new DelegateCommand(OnToggleButtonClick, CanToggleButton);
        }

        private void model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(UnitValueOutputDropdown.SelectedUnit):
                    RaisePropertyChanged(nameof(SelectedUnit));
                    break;
                case nameof(UnitValueOutputDropdown.SelectedSymbol):
                    RaisePropertyChanged(nameof(SelectedSymbol));
                    break;
                case nameof(UnitValueOutputDropdown.SelectedPrecision):
                    RaisePropertyChanged(nameof(SelectedPrecision));
                    break;
                case nameof(UnitValueOutputDropdown.SelectedFormat):
                    RaisePropertyChanged(nameof(SelectedFormat));
                    break;
                case nameof(UnitValueOutputDropdown.DisplayValue):
                    RaisePropertyChanged(nameof(DisplayValue));
                    break;
                case "CachedValue":
                    unitValueDropdownModel.DisplayValue = unitValueDropdownModel.CachedValue.StringData;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Called when Toggle button is clicked.
        /// Switches the combo box values
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        //private void OnToggleButtonClick(object obj)
        //{
        //    //var undoRecorder = nodeViewModel.WorkspaceViewModel.Model.UndoRecorder;
        //    //WorkspaceModel.RecordModelForModification(nodeModel, undoRecorder);
        //    unitValueDropdownModel.ToggleDropdownValues();
        //    nodeViewModel.WorkspaceViewModel.HasUnsavedChanges = true;
        //}

        //private bool CanToggleButton(object obj)
        //{
        //    return true;
        //}
    }
}
