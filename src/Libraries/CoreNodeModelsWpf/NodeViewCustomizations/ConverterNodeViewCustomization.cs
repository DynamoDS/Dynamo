using DSCoreNodesUI.Input;
using Dynamo.Controls;
using Dynamo.ViewModels;
using Dynamo.Wpf.Controls;
using DSCoreNodesUI;
using Dynamo.Models;
using ProtoCore.AST;

namespace Dynamo.Wpf.NodeViewCustomizations
{
    class ConverterNodeViewCustomization : INodeViewCustomization<DynamoConvert>
    {
        private NodeModel nodeModel;
        private DynamoConverterControl converterControl;
        private NodeViewModel nodeViewModel;
        private DynamoConvert convertModel;

        public void CustomizeView(DynamoConvert model, NodeView nodeView)
        {
            nodeModel = nodeView.ViewModel.NodeModel;
            nodeViewModel = nodeView.ViewModel;
            convertModel = model;
            converterControl = new DynamoConverterControl(model, nodeView)
            {
                DataContext = convertModel
            };

            nodeView.inputGrid.Children.Add(converterControl);
            converterControl.Loaded +=converterControl_Loaded; 
            converterControl.SelectConversionFrom.PreviewMouseUp +=SelectConversionFrom_PreviewMouseUp;
            converterControl.SelectConversionTo.PreviewMouseUp += SelectConversionTo_MouseLeftButtonDown;
        }

        private void SelectConversionFrom_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var undoRecorder = nodeViewModel.WorkspaceViewModel.Model.UndoRecorder;
            WorkspaceModel.RecordModelForModification(nodeModel, undoRecorder);
        }
     
        private void SelectConversionTo_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var undoRecorder = nodeViewModel.WorkspaceViewModel.Model.UndoRecorder;
            WorkspaceModel.RecordModelForModification(nodeModel, undoRecorder);
        }

        private void converterControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            converterControl.SelectConversionFrom.SelectionChanged += OnSelectConversionFromChanged;
            converterControl.SelectConversionTo.SelectionChanged += OnSelectConversionToChanged;
        }

        private void OnSelectConversionToChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {            
            nodeModel.OnNodeModified(true);
            nodeViewModel.WorkspaceViewModel.HasUnsavedChanges = true;           
        }

        void OnSelectConversionFromChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            nodeModel.OnNodeModified(true);
            nodeViewModel.WorkspaceViewModel.HasUnsavedChanges = true;               
        }

        public void Dispose()
        {
            converterControl.SelectConversionFrom.SelectionChanged -= OnSelectConversionFromChanged;
            converterControl.SelectConversionTo.SelectionChanged -= OnSelectConversionToChanged;
        }
    }
}
