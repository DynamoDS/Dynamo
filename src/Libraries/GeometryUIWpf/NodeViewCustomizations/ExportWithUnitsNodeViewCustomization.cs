using System;
using System.Collections.Generic;
using System.Text;

using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.ViewModels;
using Dynamo.Wpf.Controls;

using GeometryUI;

namespace Dynamo.Wpf.NodeViewCustomizations
{
    public class ExportWithUnitsNodeViewCustomization : INodeViewCustomization<ExportWithUnits>
    {
        private NodeModel nodeModel;
        private ExportWithUnitsControl exporterControl;
        private NodeViewModel nodeViewModel;
        private ExportWithUnits convertModel;
        private ExportWithUnitsViewModel exporterViewModel;

        public void CustomizeView(ExportWithUnits model, NodeView nodeView)
        {
            nodeModel = nodeView.ViewModel.NodeModel;
            nodeViewModel = nodeView.ViewModel;
            convertModel = model;
            
            exporterControl = new ExportWithUnitsControl(model, nodeView)
            {
                DataContext = new ExportWithUnitsViewModel(model, nodeView),
            };

            exporterViewModel = exporterControl.DataContext as ExportWithUnitsViewModel;
            nodeView.inputGrid.Children.Add(exporterControl);
            exporterControl.Loaded += converterControl_Loaded;
            exporterControl.SelectExportedUnit.PreviewMouseUp += SelectExportedUnit_PreviewMouseUp;
        }

        private void SelectExportedUnit_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            nodeViewModel.WorkspaceViewModel.HasUnsavedChanges = true;
            var undoRecorder = nodeViewModel.WorkspaceViewModel.Model.UndoRecorder;
            WorkspaceModel.RecordModelForModification(nodeModel, undoRecorder);
        }

        private void converterControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
        }

        public void Dispose()
        {
            exporterControl.SelectExportedUnit.PreviewMouseUp -= SelectExportedUnit_PreviewMouseUp;
        }
    }
}
