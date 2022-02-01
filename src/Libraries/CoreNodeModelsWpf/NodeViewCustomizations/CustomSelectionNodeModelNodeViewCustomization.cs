using Dynamo.Controls;
using Dynamo.Wpf;
using System;
using CoreNodeModels.Input;
using CoreNodeModelsWpf.Controls;


namespace CoreNodeModelsWpf.Nodes
{
    /// <summary>
    /// View customizer for DropDown node model.
    /// </summary>
    public class CustomSelectionNodeModelNodeViewCustomization : INodeViewCustomization<CustomSelectionNodeModel>
    {
        /// <summary>
        /// Customize the visual appearance of the custom dropdown node
        /// </summary>
        /// <param name="model"></param>
        /// <param name="nodeView"></param>
        public void CustomizeView(CustomSelectionNodeModel model, NodeView nodeView)
        {
            nodeView.inputGrid.Children.Add(
                new CustomSelectionControl()
                {
                    DataContext = model
                });
        }


        /// <summary>
        /// This method does not do anything for now
        /// </summary>
        public void Dispose()
        {
        }
    }
}