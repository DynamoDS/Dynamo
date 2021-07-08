using Dynamo.Controls;
using Dynamo.Core;
using Dynamo.Graph.Nodes;
using Dynamo.UI.Commands;
using Dynamo.ViewModels;
using DynamoUnits;
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
        
        private readonly NodeViewModel nodeViewModel;
        private readonly NodeModel nodeModel;

        /// <summary>
        /// Command that fires when one of the unit conversion controls is clicked,
        /// triggering a recalculation of the conversion.
        /// </summary>
        public DelegateCommand ToggleButtonClick { get; set; }

        public DynamoUnits.Quantity SelectedQuantityConversion
        {
            get { return dynamoConvertModel.SelectedQuantityConversion; }
            set
            {
                dynamoConvertModel.SelectedQuantityConversion = value;
                RaisePropertyChanged(nameof(SelectedQuantityConversion));
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
                RaisePropertyChanged(nameof(SelectedFromConversion));
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
                RaisePropertyChanged(nameof(SelectedToConversion));
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
                RaisePropertyChanged(nameof(SelectedFromConversionSource));
            }
        }

        public List<DynamoUnits.Unit> SelectedToConversionSource
        {
            get { return dynamoConvertModel.SelectedToConversionSource; }
            set
            {
                dynamoConvertModel.SelectedToConversionSource = value;
                RaisePropertyChanged(nameof(SelectedToConversionSource));

            }
        }

        public bool IsSelectionFromBoxEnabled
        {
            get { return dynamoConvertModel.IsSelectionFromBoxEnabled; }
            set
            {
                dynamoConvertModel.IsSelectionFromBoxEnabled = value;
                RaisePropertyChanged(nameof(IsSelectionFromBoxEnabled));
            }
        }

        public string SelectionFromBoxToolTip
        {
            get { return dynamoConvertModel.SelectionFromBoxToolTip; }
            set
            {
                dynamoConvertModel.SelectionFromBoxToolTip = value;
                RaisePropertyChanged(nameof(SelectionFromBoxToolTip));
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
                case nameof(QuantityConversionSource):
                    RaisePropertyChanged(nameof(QuantityConversionSource));
                    break;
                case nameof(SelectedQuantityConversion):
                    RaisePropertyChanged(nameof(SelectedQuantityConversion));
                    break;
                case nameof(SelectedFromConversionSource):
                    RaisePropertyChanged(nameof(SelectedFromConversionSource));
                    break;
                case nameof(SelectedToConversionSource):
                    RaisePropertyChanged(nameof(SelectedToConversionSource));
                    break;
                case nameof(SelectedFromConversion):
                    RaisePropertyChanged(nameof(SelectedFromConversion));
                    break;
                case nameof(SelectedToConversion):
                    RaisePropertyChanged(nameof(SelectedToConversion));
                    break;
                case nameof(IsSelectionFromBoxEnabled):
                    RaisePropertyChanged(nameof(IsSelectionFromBoxEnabled));
                    break;
                case nameof(SelectionFromBoxToolTip):
                    RaisePropertyChanged(nameof(SelectionFromBoxToolTip));
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
            dynamoConvertModel.ToggleDropdownValues();
            nodeViewModel.WorkspaceViewModel.HasUnsavedChanges = true;
        }

        private bool CanToggleButton(object obj)
        {
            return true;
        }
    }

    /// <summary>
    ///  This viewmodel allows for better communication between the nodemodel and the nodeviewcustomization.
    /// </summary>
    public class UnitValueOutputDropdownViewModel : NotificationObject
    {
        private readonly UnitValueOutputDropdown unitValueDropdownModel;
        private readonly NodeViewModel nodeViewModel;
        private readonly NodeModel nodeModel;

        public string DisplayValue
        {
            get { return unitValueDropdownModel.DisplayValue; }
            set
            {
                RaisePropertyChanged(nameof(DisplayValue));
            }
        }

        public DynamoUnits.Unit SelectedUnit
        {
            get { return unitValueDropdownModel.SelectedUnit; }
            set
            {
                unitValueDropdownModel.SelectedUnit = value;
                RaisePropertyChanged(nameof(SelectedUnit));
            }
        }

        public DynamoUnits.UnitSymbol SelectedSymbol
        {
            get { return unitValueDropdownModel.SelectedSymbol; }
            set
            {
                unitValueDropdownModel.SelectedSymbol = value;
                RaisePropertyChanged(nameof(SelectedSymbol));
            }
        }

        public int SelectedPrecision
        {
            get { return unitValueDropdownModel.SelectedPrecision; }
            set
            {
                unitValueDropdownModel.SelectedPrecision = value;
                RaisePropertyChanged(nameof(SelectedPrecision));
            }
        }

        public NumberFormat SelectedFormat
        {
            get { return unitValueDropdownModel.SelectedFormat; }
            set
            {
                unitValueDropdownModel.SelectedFormat = value;
                RaisePropertyChanged(nameof(SelectedFormat));
            }
        }

        public List<DynamoUnits.Unit> AllUnits
        {
            get { return unitValueDropdownModel.AllUnits; }
            set
            {
                unitValueDropdownModel.AllUnits = value;
                RaisePropertyChanged(nameof(AllUnits));
            }
        }
        public List<DynamoUnits.UnitSymbol> AllSymbols
        {
            get { return unitValueDropdownModel.AllSymbols; }
            set
            {
                unitValueDropdownModel.AllSymbols = value;
                RaisePropertyChanged(nameof(AllSymbols));
            }
        }

        public List<int> AllPrecisions
        {
            get { return unitValueDropdownModel.AllPrecisions; }
            set
            {
                unitValueDropdownModel.AllPrecisions = value;
                RaisePropertyChanged(nameof(AllPrecisions));
            }
        }
        public List<NumberFormat> AllFormats
        {
            get { return unitValueDropdownModel.AllFormats; }
            set
            {
                unitValueDropdownModel.AllFormats = value;
                RaisePropertyChanged(nameof(AllFormats));
            }
        }


        public UnitValueOutputDropdownViewModel(UnitValueOutputDropdown model, NodeView nodeView)
        {
            unitValueDropdownModel = model;
            nodeViewModel = nodeView.ViewModel;
            nodeModel = nodeView.ViewModel.NodeModel;
            unitValueDropdownModel.PropertyChanged += model_PropertyChanged;
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
                case nameof(UnitValueOutputDropdown.CachedValue):
                    //if the cached data is empty, we simply return, no need to update the display value
                    if (unitValueDropdownModel.CachedValue.StringData == "{}") return;
                    unitValueDropdownModel.DisplayValue = unitValueDropdownModel.CachedValue.StringData;
                    break;
                default:
                    break;
            }
        }
    }
}
