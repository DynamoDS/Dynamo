using DSCoreNodesUI;

using Dynamo.Controls;
using Dynamo.Wpf;

namespace Dynamo.Nodes
{
    public abstract class SelectionBaseNodeViewCustomization<TSelection, TResult>
        : INodeViewCustomization<SelectionBase<TSelection, TResult>>
    {
        public void CustomizeView(SelectionBase<TSelection, TResult> model, dynNodeView nodeUI)
        {
            var selectionControl = new ElementSelectionControl { DataContext = model };
            nodeUI.inputGrid.Children.Add(selectionControl);
        }

        public void Dispose()
        {
        }
    }
}