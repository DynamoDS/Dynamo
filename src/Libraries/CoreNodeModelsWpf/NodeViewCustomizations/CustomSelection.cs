using Dynamo.Controls;
using CoreNodeModels.Input;
using CoreNodeModelsWpf.Controls;
using System.Windows.Controls;
using System.Windows;
using Dynamo.Wpf;
using CoreNodeModels;
using System.Windows.Data;
using System.Data.Common;
using System;

namespace CoreNodeModelsWpf.Nodes
{
    /// <summary>
    /// View customizer for Custom Selection node model.
    /// </summary>
    public class CustomSelectionNodeViewCustomization : DropDownNodeViewCustomization, INodeViewCustomization<CustomSelection>
    {
        private CustomSelectionControl formControl;
        /// <summary>
        /// Customize the visual appearance of the custom dropdown node.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="nodeView"></param>
        public void CustomizeView(CustomSelection model, NodeView nodeView)
        {
            const double leftMargin = 40;
            formControl = new CustomSelectionControl(new CustomSelectionViewModel(model));

            nodeView.inputGrid.Children.Add(formControl);

            // Add the dropdown.
            base.CustomizeView(model, nodeView);

            var dropdown = (ComboBox)nodeView.inputGrid.Children[1];
            dropdown.MaxWidth = formControl.Width - leftMargin;

            formControl.BaseComboBox = dropdown;
            formControl.BaseComboBox.SelectionChanged += BaseComboBox_SelectionChanged;

            // Add margin to the dropdown to show the expander.
            dropdown.Margin = new Thickness(leftMargin, 0, 0, 0);
            dropdown.VerticalAlignment = VerticalAlignment.Top;
            dropdown.ApplyTemplate();

            var dropDownTextBlock = dropdown.Template.FindName("PART_ReadOnlyTextBlock", dropdown) as TextBlock;
            if (dropDownTextBlock != null)
            {
                //IsVisibleDropDownTextBlock will be false by default so the TextBlock (located in the ComboBox template) will be Collapsed then just when is a Custom Selection node we set the value to true and the TextBlock will be visible
                //We used a TextBlock because the normal TextBox doesn't have the TextTrimming property and the requirement was asking for setting TextTrimming="CharacterEllipsis"
                model.IsVisibleDropDownTextBlock = true;
            }

            var dropDownContent = dropdown.Template.FindName("ContentSite", dropdown) as ContentPresenter;
            if (dropDownContent != null)
            {
                dropDownContent.Visibility = Visibility.Collapsed;
            }

            // Bind the TextBlock to the selected item hash.
            var bindingVal = new Binding(nameof(DSDropDownBase.SelectedString))
            {
                Mode = BindingMode.TwoWay,
                Source = model
            };
            dropDownTextBlock.SetBinding(TextBlock.TextProperty, bindingVal);
        }

        private void BaseComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboSender = sender as ComboBox;
            if (comboSender != null)
            {
                comboSender.ToolTip = comboSender.SelectedItem?.ToString();
            }           
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                formControl.BaseComboBox.SelectionChanged -= BaseComboBox_SelectionChanged;
            }
        }
    }
}
