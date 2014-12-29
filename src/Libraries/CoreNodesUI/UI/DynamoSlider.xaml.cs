using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
        readonly NodeModel nodeModel;

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

        private void Slider_OnDragStarted(object sender, DragStartedEventArgs e)
        {
            //WorkspaceModel.RecordModelForModification(nodeModel, recorder);
        }

        private void Slider_OnDragCompleted(object sender, DragCompletedEventArgs e)
        {
            nodeModel.OnAstUpdated();
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            //base.OnPreviewMouseLeftButtonDown(e);
            //if (e.OriginalSource is Rectangle)
            //    WorkspaceModel.RecordModelForModification(nodeModel, recorder);
        }
        
        #endregion
    }
}
