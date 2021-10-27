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
using Utilities = DynamoUnits.Utilities;

namespace UnitsUI
{
    public class UnitConverterViewModel : NotificationObject
    {
        private readonly DynamoUnitConvert dynamoConvertModel;
        
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

        public UnitConverterViewModel(DynamoUnitConvert model, NodeView nodeView)
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
}
