using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Shapes;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.UI;
using Dynamo.ViewModels;

namespace CoreNodeModelsWpf.Controls
{
    /// <summary>
    /// Interaction logic for DynamoSlider.xaml
    /// </summary>
    public partial class DynamoSlider : UserControl
    {
        readonly NodeModel nodeModel;
        private IViewModelView<NodeViewModel> ui;

        public DynamoSlider(NodeModel model, IViewModelView<NodeViewModel> nodeUI)
        {
            InitializeComponent();
            this.slider.IsMoveToPointEnabled = true;            
            nodeModel = model;
            ui = nodeUI;

            slider.PreviewMouseUp += delegate
            {
                nodeUI.ViewModel.DynamoViewModel.OnRequestReturnFocusToView();
            };

        }

        #region Event Handlers

        private void Slider_OnDragStarted(object sender, DragStartedEventArgs e)
        {
            var undoRecorder = ui.ViewModel.WorkspaceViewModel.Model.UndoRecorder;
            WorkspaceModel.RecordModelForModification(nodeModel, undoRecorder);
        }

        private void Slider_OnDragCompleted(object sender, DragCompletedEventArgs e)
        {
            nodeModel.MarkNodeAsModified(true);           
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            var undoRecorder = ui.ViewModel.WorkspaceViewModel.Model.UndoRecorder;
            base.OnPreviewMouseLeftButtonDown(e);
            if (e.OriginalSource is Rectangle)
                WorkspaceModel.RecordModelForModification(nodeModel, undoRecorder);
        }

        private void Slider_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                ui.ViewModel.WorkspaceViewModel.HasUnsavedChanges = true;
            }
        }

        #endregion
    }
}