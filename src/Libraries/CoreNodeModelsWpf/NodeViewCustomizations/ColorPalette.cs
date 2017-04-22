using System.ComponentModel;
using System.Windows.Media;
using Dynamo.Controls;
using Dynamo.Core;
using Dynamo.Wpf;

using CoreNodeModelsWpf.Controls;
using CoreNodeModels.Input;
using Dynamo.Graph.Workspaces;
using DSColor = DSCore.Color;

namespace CoreNodeModelsWpf.Nodes
{
    public class ColorPaletteNodeViewCustomization : NotificationObject, INodeViewCustomization<ColorPalette>
    {
        /// <summary>
        ///     WPF Control.
        /// </summary>
        private ColorPaletteUI ColorPaletteUINode;
        private NodeView viewNode;
        private ColorPalette colorPaletteNode;
        private Color color;
        /// <summary>
        ///     Customize View.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="nodeView"></param>
        public void CustomizeView(ColorPalette model, NodeView nodeView)
        {
            viewNode = nodeView;
            colorPaletteNode = model;
            model.PropertyChanged += Model_PropertyChanged;
            ColorPaletteUINode = new ColorPaletteUI();
            nodeView.inputGrid.Children.Add(ColorPaletteUINode);
            ColorPaletteUINode.DataContext = model;

            var undoRecorder = viewNode.ViewModel.WorkspaceViewModel.Model.UndoRecorder;
            WorkspaceModel.RecordModelForModification(colorPaletteNode, undoRecorder);
            

        }
        private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var undoRecorder = viewNode.ViewModel.WorkspaceViewModel.Model.UndoRecorder;

            if (e.PropertyName == "Update")
            {              
                WorkspaceModel.RecordModelForModification(colorPaletteNode, undoRecorder);
            }
        }

        /// <summary>
        ///     Dispose.
        /// </summary>
        public void Dispose()
        {
            colorPaletteNode.PropertyChanged -= Model_PropertyChanged;
        }
    }
}
