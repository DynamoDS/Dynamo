using System.Windows.Controls;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.UI;
using Dynamo.ViewModels;

namespace CoreNodeModelsWpf.Controls
{
    /// <summary>
    /// Interaction logic for DateTimePalette.xaml
    /// </summary>
    public partial class DateTimePaletteUI : UserControl
    {
        readonly NodeModel nodeModel;
        private IViewModelView<NodeViewModel> ui;

        public DateTimePaletteUI(NodeModel model, IViewModelView<NodeViewModel> nodeUI)
        {
            InitializeComponent();
            nodeModel = model;
            ui = nodeUI;
        }
        private void DateTimePicker_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<object> e)
        {
            var undoRecorder = ui.ViewModel.WorkspaceViewModel.Model.UndoRecorder;
            WorkspaceModel.RecordModelForModification(nodeModel, undoRecorder);
        }
    }
}
