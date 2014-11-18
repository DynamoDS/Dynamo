using DSCoreNodesUI;

using Dynamo.Controls;
using Dynamo.Wpf;

namespace Dynamo.Nodes
{
    public abstract class SelectionBaseNodeViewCustomization<TSelection, TResult>
        : INodeViewCustomization<SelectionBase<TSelection, TResult>>
    {
        public void CustomizeView(SelectionBase<TSelection, TResult> model, dynNodeView nodeView)
        {
            var selectionControl = new ElementSelectionControl { DataContext = model };
            nodeView.inputGrid.Children.Add(selectionControl);
        }

        public void Dispose()
        {
        }
    }
}