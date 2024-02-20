using System.Windows;
using System.Windows.Controls;
using CoreNodeModels;
using CoreNodeModelsWpf.Controls;
using Dynamo.Controls;
using Dynamo.Wpf;

namespace CoreNodeModelsWpf.Nodes
{
    /// <summary>
    /// View customizer for Custom Selection node model.
    /// </summary>
    public class DefineDataNodeViewCustomization : DropDownNodeViewCustomization, INodeViewCustomization<DefineData>
    {
        /// <summary>
        /// Customize the visual appearance of the custom dropdown node.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="nodeView"></param>
        public void CustomizeView(DefineData model, NodeView nodeView)
        {
            var formControl = new DefineDataControl(new DefineDataViewModel(model));

            nodeView.inputGrid.Margin = new Thickness(5, 0, 5, 0);
            nodeView.inputGrid.RowDefinitions.Add(new RowDefinition());
            nodeView.inputGrid.RowDefinitions.Add(new RowDefinition());

            Grid.SetRow(formControl, 1);
            nodeView.inputGrid.Children.Add(formControl);

            // Add the dropdown.
            base.CustomizeView(model, nodeView);

            var style = (Style)Dynamo.UI.SharedDictionaryManager.DynamoModernDictionary["NodeViewComboBox"];
            var dropdown = (ComboBox)nodeView.inputGrid.Children[1];
            dropdown.Style = style;

            formControl.BaseComboBox = dropdown;

            // Add margin to the dropdown to show the expander.
            dropdown.Margin = new Thickness(0, 0, 0, 10);
            dropdown.VerticalAlignment = VerticalAlignment.Top;
            dropdown.MinWidth = 220;
            dropdown.FontSize = 12;
        }
    }
}
