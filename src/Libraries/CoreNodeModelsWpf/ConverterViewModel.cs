using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DSCoreNodesUI;
using Dynamo.Models;
using Dynamo.Core;
using Dynamo.UI.Commands;
using DynamoConversions;

namespace Dynamo.Wpf
{
    public class ConverterViewModel : NotificationObject
    {
        private DynamoConvert dynamoConvertModel;
        public DelegateCommand ToggleButtonClick { get; set; }

        public ConversionUnit SelectedFromConversion
        {
            get { return dynamoConvertModel.SelectedFromConversion; }
            set
            {
                dynamoConvertModel.SelectedFromConversion = value;
                RaisePropertyChanged("SelectedFromConversion");
            }
        }

        public ConversionUnit SelectedToConversion
        {
            get { return dynamoConvertModel.SelectedToConversion; }
            set
            {
                dynamoConvertModel.SelectedToConversion = value;
                RaisePropertyChanged("SelectedToConversion");
            }
        }

        public ConverterViewModel(DynamoConvert model)
        {
            dynamoConvertModel = model;
            ToggleButtonClick = new DelegateCommand(OnToggleButtonClick, CanToggleButton);
        }


        /// <summary>
        /// Called when Toggle button is clicked.
        /// Switches the combo box values
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnToggleButtonClick(object obj)
        {
            var temp = this.SelectedFromConversion;
            this.SelectedFromConversion = this.SelectedToConversion;
            this.SelectedToConversion = temp;
            
        }

        private bool CanToggleButton(object obj)
        {
            return true;
        }



    }
}
