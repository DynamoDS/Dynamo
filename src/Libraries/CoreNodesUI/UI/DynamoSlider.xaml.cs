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

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);
            if (e.OriginalSource is System.Windows.Shapes.Rectangle)
                nodeModel.Workspace.RecordModelForModification(nodeModel);
        }

        #endregion
    }
}
