using System.Windows.Controls;
using System.Windows.Input;

using Dynamo.Controls;
using Dynamo.Models;

namespace Dynamo.UI.Controls
{
    /// <summary>
    /// Interaction logic for DynamoSlider.xaml
    /// </summary>
    public partial class DynamoSlider : UserControl
    {
        NodeModel nodeModel;
        public DynamoSlider(NodeModel model, NodeView nodeUI)
        {
            InitializeComponent();

            nodeModel = model;

            slider.PreviewMouseUp += delegate
            {
                nodeUI.ViewModel.DynamoViewModel.ReturnFocusToSearch();
            };
        }

        #region Event Handlers
        //protected void OnThumbDragStarted(System.Windows.Controls.Primitives.DragStartedEventArgs e)
        //{
        //    base.OnThumbDragStarted(e);
        //    nodeModel.Workspace.RecordModelForModification(nodeModel);
        //    (nodeModel as IBlockingModel).OnBlockingStarted(EventArgs.Empty);
        //}

        //protected override void OnThumbDragCompleted(System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        //{
        //    base.OnThumbDragCompleted(e);
        //    (nodeModel as IBlockingModel).OnBlockingEnded(EventArgs.Empty);
        //    nodeModel.RequiresRecalc = true;
        //}

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);
            if (e.OriginalSource is System.Windows.Shapes.Rectangle)
                nodeModel.Workspace.RecordModelForModification(nodeModel);
        }
        #endregion
    }
}
