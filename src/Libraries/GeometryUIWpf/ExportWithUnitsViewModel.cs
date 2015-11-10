﻿using System.Collections.Generic;
using Dynamo.Controls;
using Dynamo.Core;
using Dynamo.Graph.Nodes;
using Dynamo.UI.Commands;
using Dynamo.ViewModels;
using DynamoConversions;

using GeometryUI;

namespace Dynamo.Wpf
{
    public class ExportWithUnitsViewModel : NotificationObject 
    {
        private readonly ExportWithUnits exportWithUnitsModel;
        public DelegateCommand ToggleButtonClick { get; set; }
        private readonly NodeViewModel nodeViewModel;
        private readonly NodeModel nodeModel;

        public ConversionUnit SelectedExportedUnit
        {
            get { return exportWithUnitsModel.SelectedExportedUnit; }
            set
            {
                exportWithUnitsModel.SelectedExportedUnit = value;                             
            }
        }

        public List<ConversionUnit> SelectedExportedUnitsSource
        {
            get { return exportWithUnitsModel.SelectedExportedUnitsSource; }
            set
            {
                exportWithUnitsModel.SelectedExportedUnitsSource = value;               
            }
        }

        public ExportWithUnitsViewModel(ExportWithUnits model, NodeView nodeView)
        {
            exportWithUnitsModel = model;           
            nodeViewModel = nodeView.ViewModel;
            nodeModel = nodeView.ViewModel.NodeModel;
            model.PropertyChanged +=model_PropertyChanged;      
        }

        private void model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "SelectedExportedUnit":
                    RaisePropertyChanged("SelectedExportedUnit");
                    break;
                case "SelectedExportedUnitsSource":
                    RaisePropertyChanged("SelectedExportedUnitsSource");
                    break;
            }
        }
    }
}
