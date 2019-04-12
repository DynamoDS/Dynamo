using System.ComponentModel;
using System.Windows.Media;
using CoreNodeModels.Input;
using CoreNodeModelsWpf.Controls;
using Dynamo.Controls;
using Dynamo.Core;
using Dynamo.Graph.Workspaces;
using Dynamo.Wpf;
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
        private Converters.MediatoDSColorConverter converter;
        /// <summary>
        ///     Customize View.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="nodeView"></param>
        public void CustomizeView(ColorPalette model, NodeView nodeView)
        {
            viewNode = nodeView;
            colorPaletteNode = model;
            converter = new Converters.MediatoDSColorConverter();
            ColorPaletteUINode = new ColorPaletteUI();
            ColorPaletteUINode.xceedColorPickerControl.Closed += ColorPickerControl_Closed;
            colorPaletteNode.PropertyChanged += ColorPaletteNode_PropertyChanged;
            nodeView.ContentGrid.Children.Add(ColorPaletteUINode);


            var undoRecorder = viewNode.ViewModel.WorkspaceViewModel.Model.UndoRecorder;
            WorkspaceModel.RecordModelForModification(colorPaletteNode, undoRecorder);
            //kick off ui to match initial model state.
            this.ColorPaletteNode_PropertyChanged(ColorPaletteUINode, new PropertyChangedEventArgs("DsColor"));
        }

        private void ColorPaletteNode_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //if the property name was DsColor
           if (e.PropertyName == "DsColor")
            {

                var convertedModelColor = ((Color)(converter.Convert(colorPaletteNode.DsColor, null, null, null)));
                var isSameColor = convertedModelColor
                     .Equals(ColorPaletteUINode.xceedColorPickerControl.SelectedColor);
                //and if the color on the model is different than the selected Color on the view
                //then update the view.
                if (!isSameColor)
                {
                    ColorPaletteUINode.xceedColorPickerControl.SelectedColor = convertedModelColor;
                }
            }
        }

        private void ColorPickerControl_Closed(object sender, System.Windows.RoutedEventArgs e)
        {
            //if the model color is the same as the selected color when the color control is closed
            //we should not record the model for undo again, it's already there.
            var convertedModelColor = ((Color)(converter.Convert(colorPaletteNode.DsColor, null, null, null)));
            var isSameColor = convertedModelColor
                 .Equals(ColorPaletteUINode.xceedColorPickerControl.SelectedColor);
            if (!isSameColor)
            {
                //we need to record the colorPicker node before the model is updated.
                var undoRecorder = viewNode.ViewModel.WorkspaceViewModel.Model.UndoRecorder;
                WorkspaceModel.RecordModelForModification(colorPaletteNode, undoRecorder);
                //now that we have recorded the old state, set the color on the model.
                colorPaletteNode.DsColor = converter.ConvertBack(ColorPaletteUINode.xceedColorPickerControl.SelectedColor, null, null, null) as DSColor;
            }
        }

        /// <summary>
        ///     Dispose.
        /// </summary>
        public void Dispose()
        {
            ColorPaletteUINode.xceedColorPickerControl.Closed -= ColorPickerControl_Closed; ;
            colorPaletteNode.PropertyChanged -= ColorPaletteNode_PropertyChanged;

        }
    }
}
