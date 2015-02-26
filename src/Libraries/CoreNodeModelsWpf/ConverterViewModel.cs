using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DSCoreNodesUI;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Core;
using Dynamo.UI.Commands;
using Dynamo.ViewModels;
using DynamoConversions;

namespace Dynamo.Wpf
{
    public class ConverterViewModel : NotificationObject
    {
        private DynamoConvert dynamoConvertModel;
        public DelegateCommand ToggleButtonClick { get; set; }
        private NodeViewModel nodeViewModel;
        private NodeModel nodeModel;

        public object SelectedMetricConversion
        {
            get { return dynamoConvertModel.SelectedMetricConversion; }
            set
            {
                dynamoConvertModel.SelectedMetricConversion = value;                 
                RaisePropertyChanged("SelectedMetricConversion");
            }
        }

        public object SelectedFromConversion
        {
            get { return dynamoConvertModel.SelectedFromConversion; }
            set
            {
                dynamoConvertModel.SelectedFromConversion = value;               
                RaisePropertyChanged("SelectedFromConversion");
            }
        }

        public object SelectedToConversion
        {
            get { return dynamoConvertModel.SelectedToConversion; }
            set
            {
                dynamoConvertModel.SelectedToConversion = value;                
                RaisePropertyChanged("SelectedToConversion");
            }
        }

        public object SelectedFromConversionSource
        {
            get { return dynamoConvertModel.SelectedFromConversionSource; }
            set
            {
                dynamoConvertModel.SelectedFromConversionSource = value;
                RaisePropertyChanged("SelectedFromConversionSource");
            }
        }

        public object SelectedToConversionSource
        {
            get { return dynamoConvertModel.SelectedToConversionSource; }
            set
            {
                dynamoConvertModel.SelectedFromConversionSource = value;
                RaisePropertyChanged("SelectedToConversionSource");
            }
        }

        public bool IsSelectionFromBoxEnabled
        {
            get { return dynamoConvertModel.IsSelectionFromBoxEnabled; }
            set
            {
                dynamoConvertModel.IsSelectionFromBoxEnabled = value;
                RaisePropertyChanged("IsSelectionFromBoxEnabled");
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
