using CoreNodeModels;
using CoreNodeModelsWpf.Controls;
using Dynamo.Controls;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.ViewModels;
using Dynamo.Wpf;

namespace CoreNodeModelsWpf.NodeViewCustomizations
{
    class ConverterNodeViewCustomization : INodeViewCustomization<DynamoConvert>
    {
        private NodeModel nodeModel;
        private DynamoConverterControl converterControl;
        private NodeViewModel nodeViewModel;
        private DynamoConvert convertModel;
        private ConverterViewModel converterViewModel;
       
        public void CustomizeView(DynamoConvert model, NodeView nodeView)
        {
            nodeModel = nodeView.ViewModel.NodeModel;
            nodeViewModel = nodeView.ViewModel;
            convertModel = model;
            converterControl = new DynamoConverterControl(model, nodeView)
            {
                DataContext = new ConverterViewModel(model, nodeView),                 
            };
            converterViewModel = converterControl.DataContext as ConverterViewModel;
            nodeView.inputGrid.Children.Add(converterControl);
            converterControl.Loaded +=converterControl_Loaded;                                
            converterControl.SelectConversionMetric.PreviewMouseUp +=SelectConversionMetric_PreviewMouseUp;
            converterControl.SelectConversionFrom.PreviewMouseUp +=SelectConversionFrom_PreviewMouseUp;
            converterControl.SelectConversionTo.PreviewMouseUp += SelectConversionTo_MouseLeftButtonDown;
        }

        private void SelectConversionMetric_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            nodeViewModel.WorkspaceViewModel.HasUnsavedChanges = true;
            var undoRecorder = nodeViewModel.WorkspaceViewModel.Model.UndoRecorder;
            WorkspaceModel.RecordModelForModification(nodeModel, undoRecorder);  
        }

        private void SelectConversionFrom_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            nodeViewModel.WorkspaceViewModel.HasUnsavedChanges = true;
            var undoRecorder = nodeViewModel.WorkspaceViewModel.Model.UndoRecorder;
            WorkspaceModel.RecordModelForModification(nodeModel, undoRecorder);           
        }
     
        private void SelectConversionTo_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
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
            converterControl.SelectConversionMetric.PreviewMouseUp -= SelectConversionMetric_PreviewMouseUp;
            converterControl.SelectConversionFrom.PreviewMouseUp -= SelectConversionFrom_PreviewMouseUp;
            converterControl.SelectConversionTo.PreviewMouseUp -= SelectConversionTo_MouseLeftButtonDown;
        }
    }
}
