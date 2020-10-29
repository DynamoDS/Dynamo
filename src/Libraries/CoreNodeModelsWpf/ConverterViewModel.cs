using System.Collections.Generic;
using CoreNodeModels;
using Dynamo.Controls;
using Dynamo.Core;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.UI.Commands;
using Dynamo.ViewModels;
using DynamoConversions;

namespace CoreNodeModelsWpf
{
    public class ConverterViewModel : NotificationObject
    {
        private readonly DynamoConvert dynamoConvertModel;
        public DelegateCommand ToggleButtonClick { get; set; }
        private readonly NodeViewModel nodeViewModel;
        private readonly NodeModel nodeModel;

        public ConversionMetricUnit SelectedMetricConversion
        {
            get { return dynamoConvertModel.SelectedMetricConversion; }
            set
            {
                dynamoConvertModel.SelectedMetricConversion = value;                                
            }
        }

        public ConversionUnit SelectedFromConversion
        {
            get { return dynamoConvertModel.SelectedFromConversion; }
            set
            {
                dynamoConvertModel.SelectedFromConversion = value;                             
            }
        }

        public ConversionUnit SelectedToConversion
        {
            get { return dynamoConvertModel.SelectedToConversion; }
            set
            {
                dynamoConvertModel.SelectedToConversion = value;                            
            }
        }

        public List<ConversionUnit> SelectedFromConversionSource
        {
            get { return dynamoConvertModel.SelectedFromConversionSource; }
            set
            {
                dynamoConvertModel.SelectedFromConversionSource = value;               
            }
        }

        public List<ConversionUnit> SelectedToConversionSource
        {
            get { return dynamoConvertModel.SelectedToConversionSource; }
            set
            {
                dynamoConvertModel.SelectedFromConversionSource = value;             
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

        public ConverterViewModel(DynamoConvert model,NodeView nodeView)
        {
            dynamoConvertModel = model;           
            nodeViewModel = nodeView.ViewModel;
            nodeModel = nodeView.ViewModel.NodeModel;
            model.PropertyChanged +=model_PropertyChanged;
            ToggleButtonClick = new DelegateCommand(OnToggleButtonClick, CanToggleButton);         
        }

        private void model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "SelectedMetricConversion":
                    RaisePropertyChanged("SelectedMetricConversion");
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
            var undoRecorder = nodeViewModel.WorkspaceViewModel.Model.UndoRecorder;
            WorkspaceModel.RecordModelForModification(nodeModel, undoRecorder);   
            dynamoConvertModel.ToggleDropdownValues();
            nodeViewModel.WorkspaceViewModel.HasUnsavedChanges = true;             
        }

        private bool CanToggleButton(object obj)
        {
            return true;
        }
    }
}
