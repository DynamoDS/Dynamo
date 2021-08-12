using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CoreNodeModels.Input;

namespace CoreNodeModelsWpf.Controls
{
    /// <summary>
    /// Interaction logic for DropDownControl.xaml
    /// </summary>
    public partial class CustomSelectionControl : UserControl
    {
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
                    var textBoxes = new List<TextBox>();
                    CollectChildrenOfType<TextBox>(listBox,textBoxes);

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
                    viewModel.Log(ex);
            }
        }


        public static void CollectChildrenOfType<T>(DependencyObject dependencyObj, List<T> children) where T : DependencyObject
        {
            if (dependencyObj == null)
                return;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(dependencyObj); i++)
            {
                var child = VisualTreeHelper.GetChild(dependencyObj, i);

                var result = child as T;
                if (result != null)
                    children.Add(result);
                else
                    CollectChildrenOfType<T>(child, children);
            }
        }
    }
}