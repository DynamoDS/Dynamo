using Dynamo.Controls;
using CoreNodeModels.Input;
using CoreNodeModelsWpf.Controls;
using System.Windows.Controls;
using System.Windows;
using Dynamo.Wpf;

namespace CoreNodeModelsWpf.Nodes
{
    /// <summary>
    /// View customizer for Custom Selection node model.
    /// </summary>
    public class CustomSelectionNodeViewCustomization : DropDownNodeViewCustomization, INodeViewCustomization<CustomSelection>
    {
        /// <summary>
        /// Customize the visual appearance of the custom dropdown node.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="nodeView"></param>
        public void CustomizeView(CustomSelection model, NodeView nodeView)
        {
            var formControl = new CustomSelectionControl(new CustomSelectionViewModel(model));

            nodeView.inputGrid.Children.Add(formControl);

            // Add the dropdown.
            base.CustomizeView(model, nodeView);

            var dropdown = (ComboBox)nodeView.inputGrid.Children[1];

            formControl.BaseComboBox = dropdown;

            // Add margin to the dropdown to show the expander.
            dropdown.Margin = new Thickness(40, 0, 0, 0);
            dropdown.VerticalAlignment = VerticalAlignment.Top;
        }
    }
}