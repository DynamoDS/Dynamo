using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using CoreNodeModels.Input;

using Dynamo.Utilities;

namespace CoreNodeModelsWpf.Controls
{
    /// <summary>
    /// Interaction logic for CustomSelectionControl.xaml
    /// </summary>
    public partial class CustomSelectionControl : UserControl
    {
        private readonly CustomSelection model;

        /// <summary>
        /// Create the control for the custom dropdown menu and editor
        /// </summary>
        public CustomSelectionControl(CustomSelectionViewModel viewModel)
        {
            DataContext = viewModel;
            model = viewModel.Model;

            InitializeComponent();
        }

        /// <summary>
        /// Dropdown menu to select an item. Created by <see cref="Nodes.DropDownNodeViewCustomization"/>.
        /// </summary>
        internal ComboBox BaseComboBox { get; set; }

        private void EnumItemsListbox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!( sender is ListBox listBox ))
            {
                return;
            }

            if (e.Key == Key.Tab)
            {
                int offset = ( e.KeyboardDevice.Modifiers == System.Windows.Input.ModifierKeys.Shift ) ?
                    0 : 1;
                var textBoxes = listBox.ChildrenOfType<TextBox>().ToList();

                for (int i = 0; i < textBoxes.Count; i++)
                {
                    if (textBoxes[i].IsKeyboardFocused)
                    {
                        TextBox nextBox = textBoxes[( i + offset ) % textBoxes.Count];
                        nextBox.Focus();
                        nextBox.CaretIndex = nextBox.Text.Length;

                        return;
                    }
                }
            }
        }

        private void ItemNameChanged(object sender, RoutedEventArgs e)
        {
            int selectedIndex = BaseComboBox.SelectedIndex;

            if (selectedIndex != -1)
            {
                // This hack forces the combo box to update the display of the selected item.
                BaseComboBox.SelectedIndex = -1;
                BaseComboBox.SelectedIndex = selectedIndex;
            }

            BaseComboBox?.Items.Refresh();
        }

        private void ItemValueChanged(object sender, RoutedEventArgs e)
        {
            BaseComboBox?.Items.Refresh();

            model.OnNodeModified();
        }
    }
}