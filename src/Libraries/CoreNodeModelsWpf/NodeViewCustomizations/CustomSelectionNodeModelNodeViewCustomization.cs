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
        public void CustomizeView(CustomSelectionNodeModel model, NodeView nodeView)
        {
            nodeView.inputGrid.Children.Add(
                new CustomSelectionControl()
                {
                    DataContext = model
                });
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // cleanup
        }
    }
}