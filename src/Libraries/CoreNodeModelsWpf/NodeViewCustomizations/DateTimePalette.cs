using Dynamo.Controls;
using Dynamo.Core;
using Dynamo.Wpf;

using CoreNodeModelsWpf.Controls;
using CoreNodeModels.Input;
using Dynamo.Graph.Workspaces;

namespace CoreNodeModelsWpf.Nodes
{
    public class DateTimePaletteNodeViewCustomization : NotificationObject, INodeViewCustomization<DateTimePalette>
    {
        /// <summary>
        ///     WPF Control.
        /// </summary>
        public DateTimePaletteUI DateTimePaletteUINode;

        /// <summary>
        ///     Customize View.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="nodeView"></param>
        public void CustomizeView(DateTimePalette model, NodeView nodeView)
        {
            DateTimePaletteUINode = new DateTimePaletteUI(model, nodeView);
            nodeView.inputGrid.Width = 250;
            nodeView.inputGrid.Children.Add(DateTimePaletteUINode);
            DateTimePaletteUINode.DataContext = model;           
        }

        /// <summary>
        ///     Dispose.
        /// </summary>
        public void Dispose() { }
    }
}

