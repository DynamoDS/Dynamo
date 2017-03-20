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
        private ColorPalette colorPaletteNode;
        private Color mcolor;
        /// <summary>
        /// Selected Color
        /// </summary>
        public Color MColor
        {
            get { return mcolor; }
            set
            {
                mcolor = value;
                colorPaletteNode.dsColor = DSColor.ByARGB(mcolor.A, mcolor.R, mcolor.G, mcolor.B);
                RaisePropertyChanged("MColor");
            }
        }
        /// <summary>
        ///     Customize View.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="nodeView"></param>
        public void CustomizeView(ColorPalette model, NodeView nodeView)
        {
            colorPaletteNode = model;
            mcolor = Color.FromArgb(model.dsColor.Alpha, model.dsColor.Red, model.dsColor.Green, model.dsColor.Blue);
            ColorPaletteUINode = new ColorPaletteUI(model, nodeView);
            nodeView.inputGrid.Children.Add(ColorPaletteUINode);
            ColorPaletteUINode.DataContext = this;
        }
        /// <summary>
        ///     Dispose.
        /// </summary>
        public void Dispose() { }
    }
}
