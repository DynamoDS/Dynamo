using Dynamo.Nodes;
using Dynamo.Controls;

using Microsoft.Practices.Prism.Commands;

namespace Dynamo.Wpf.Nodes
{
    // Note: Because this is a generic class, it can't be a NodeViewCustomization!
    //       We have to supply a non-generic implementation for NodeViewCustomization
    //       to work.
    public abstract class SelectionBaseNodeViewCustomization<TSelection, TResult>
        : INodeViewCustomization<SelectionBase<TSelection, TResult>>
    {
        public SelectionBase<TSelection, TResult> Model { get; set; }
        public DelegateCommand SelectCommand { get; set; }

        public void CustomizeView(SelectionBase<TSelection, TResult> model, NodeView nodeView)
        {
            Model = model;
            SelectCommand = new DelegateCommand(() => Model.Select(null), Model.CanBeginSelect);
            Model.PropertyChanged += (s, e) => {
                                                    if (e.PropertyName == "CanSelect")
                                                    {
                                                        SelectCommand.RaiseCanExecuteChanged();
                                                    }
                                               };

            var selectionControl = new ElementSelectionControl { DataContext = this };
            nodeView.inputGrid.Children.Add(selectionControl);
        }

        public void Dispose()
        {
        }
    }
}