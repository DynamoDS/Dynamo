using Dynamo.Controls;
using CoreNodeModels.Input;
using CoreNodeModelsWpf.Controls;
using System.Windows.Controls;
using System.Windows;
using Dynamo.Wpf;

namespace CoreNodeModelsWpf.Nodes
{
    /// <summary>
    /// View customizer for DropDown node model.
    /// </summary>
    public class CustomSelectionNodeModelNodeViewCustomization : DropDownNodeViewCustomization, INodeViewCustomization<CustomSelectionNodeModel>
    {
        /// <summary>
        /// Customize the visual appearance of the custom dropdown node.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="nodeView"></param>
        public void CustomizeView(CustomSelectionNodeModel model, NodeView nodeView)
        {
            nodeView.inputGrid.Children.Add(
                new CustomSelectionControl()
                {
                    DataContext = new CustomSelectionViewModel(model),
                });

            // Add the dropdown.
            base.CustomizeView(model, nodeView);

            var dropdown = (FrameworkElement)nodeView.inputGrid.Children[1];

            // Add margin to the dropdown to show the expander.
            dropdown.Margin = new Thickness(40, 0, 0, 0);
            dropdown.VerticalAlignment = VerticalAlignment.Top;
        }
    }
}