using System.Windows.Controls;
using CoreNodeModelsWpf.Nodes;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using Dynamo.UI;
using Dynamo.ViewModels;

namespace CoreNodeModelsWpf.Controls

{
    /// <summary>
    /// Interaction logic for ColorPalette.xaml
    /// </summary>
    public partial class ColorPaletteUI : UserControl
    {
        readonly NodeModel nodeModel;
        private IViewModelView<NodeViewModel> ui;
        public ColorPaletteUI(NodeModel model, IViewModelView<NodeViewModel> nodeUI)
        {
            InitializeComponent();
            nodeModel = model;
            ui = nodeUI;
        }
        
        private void On_Change(object sender, System.Windows.RoutedPropertyChangedEventArgs<System.Windows.Media.Color?> e)
        {
            var undoRecorder = ui.ViewModel.WorkspaceViewModel.Model.UndoRecorder;           
            WorkspaceModel.RecordModelForModification(nodeModel, undoRecorder);      
        }
        
    }
}
