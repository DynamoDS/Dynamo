using System.Windows.Controls;
using System.Windows.Media;
using CoreNodeModelsWpf.Nodes;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using Dynamo.UI;
using Dynamo.ViewModels;
using System;
using CoreNodeModels.Input;

namespace CoreNodeModelsWpf.Controls

{
    /// <summary>
    /// Interaction logic for ColorPalette.xaml
    /// </summary>
    public partial class ColorPaletteUI : UserControl
    {
        readonly NodeModel _model;
        private IViewModelView<NodeViewModel> _nodeUI;
        private Color _color;
        public ColorPaletteUI(NodeModel model, IViewModelView<NodeViewModel> nodeUI, Color color)
        {
            InitializeComponent();
            _model = model;
            _nodeUI = nodeUI;
            _color = color;
        }
        private void ColorChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<System.Windows.Media.Color?> e)
        {
            if (e.OldValue.Equals(_color))
            {
                var undoRecorder = _nodeUI.ViewModel.WorkspaceViewModel.Model.UndoRecorder;
                WorkspaceModel.RecordModelForModification(_model, undoRecorder);
            }

        }
    }
}
