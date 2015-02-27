using System;
using System.CodeDom.Compiler;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Shapes;

using Dynamo.Models;
using Dynamo.UI;
using Dynamo.ViewModels;

namespace Dynamo.Wpf.Controls
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
                nodeUI.ViewModel.DynamoViewModel.ReturnFocusToSearch();
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

        #endregion
    }
}