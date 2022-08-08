using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;

using CoreNodeModels.Input;

using Dynamo.Utilities;

namespace CoreNodeModelsWpf.Controls
{
    /// <summary>
    /// Interaction logic for DropDownControl.xaml
    /// </summary>
    public partial class CustomSelectionControl : UserControl
    {
        /// <summary>
        /// Create the control for the custom dropdown menu and editor
        /// </summary>
        public CustomSelectionControl()
        {
            InitializeComponent();
        }

        private void EnumItemsListbox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var listBox = sender as ListBox;
            if (listBox == null)
                return;

            try
            {
                if (e.Key == Key.Tab)
                {
                    List<TextBox> textBoxes = listBox.ChildrenOfType<TextBox>().ToList();

                    for (int i = 0; i < textBoxes.Count - 1; i++)
                    {
                        if (textBoxes[i].IsKeyboardFocused)
                        {
                            textBoxes[i + 1].Focus();
                            textBoxes[i + 1].CaretIndex = textBoxes[i + 1].Text.Length;
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var viewModel = DataContext as CustomSelectionNodeModel;
                if (viewModel != null)
                {
                    viewModel.Log(ex);
                }
            }
        }
    }
}