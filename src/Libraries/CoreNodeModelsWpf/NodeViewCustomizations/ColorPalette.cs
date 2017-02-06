using Dynamo.Controls;
using Dynamo.Wpf;

using CoreNodeModelsWpf.Controls;
using CoreNodeModels.Input;

namespace CoreNodeModelsWpf.Nodes
{
    public class ColorPaletteNodeViewCustomization : INodeViewCustomization<ColorPalette>
    {
        /// <summary>
        ///     WPF Control.
        /// </summary>
        public ColorPaletteUI Colorpickui;

        /// <summary>
        ///     Customize View.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="nodeView"></param>
        public void CustomizeView(ColorPalette model, NodeView nodeView)
        {
            Colorpickui = new ColorPaletteUI();
            nodeView.inputGrid.Children.Add(Colorpickui);
            Colorpickui.DataContext = model;
        }
        /// <summary>
        ///     Dispose.
        /// </summary>
        public void Dispose() { }
    }
}
