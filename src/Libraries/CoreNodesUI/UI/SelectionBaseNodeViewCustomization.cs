using DSCoreNodesUI;

using Dynamo.Controls;
using Dynamo.Wpf;

namespace Dynamo.Nodes
{
    // Note: Because this is a generic class, it can't be a NodeViewCustomization!
    //       We have to supply a non-generic implementation for NodeViewCustomization
    //       to work.
    public abstract class SelectionBaseNodeViewCustomization<TSelection, TResult>
        : INodeViewCustomization<SelectionBase<TSelection, TResult>>
    {
        public void CustomizeView(SelectionBase<TSelection, TResult> model, NodeView nodeView)
        {
            var selectionControl = new ElementSelectionControl { DataContext = model };
            nodeView.inputGrid.Children.Add(selectionControl);
        }

        public void Dispose()
        {
        }
    }
}