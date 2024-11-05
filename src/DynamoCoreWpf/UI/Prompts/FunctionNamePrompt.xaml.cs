using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dynamo.Controls;
using Dynamo.Utilities;
using Dynamo.Wpf.Utilities;
using DynamoUtilities;

namespace Dynamo.Nodes
{
    /// <summary>
    /// Interaction logic for FunctionNamePrompt.xaml
    /// </summary>
    public partial class FunctionNamePrompt : Window
    {
        public FunctionNamePrompt(IEnumerable<string> categories)
        {
            InitializeComponent();

            Owner = WpfUtilities.FindUpVisualTree<DynamoView>(this);
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            nameBox.Focus();

            var sortedCats = categories.ToList();
            sortedCats.Sort();

            foreach (var item in sortedCats)
            {
                categoryBox.Items.Add(item);
            }
        }
       

        void OK_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Text))
            {
                MessageBoxService.Show
                (
                    Wpf.Properties.Resources.MessageCustomNodeNoName,
                    Wpf.Properties.Resources.CustomNodePropertyErrorMessageBoxTitle,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
            else if (PathHelper.IsFileNameInValid(Text))
            {
                MessageBoxService.Show
                (
                   Wpf.Properties.Resources.MessageCustomNodeNameInvalid,
                   Wpf.Properties.Resources.CustomNodePropertyErrorMessageBoxTitle,
                   MessageBoxButton.OK,
                   MessageBoxImage.Error
                );
            }
            else if (string.IsNullOrEmpty(Category))
            {
                MessageBoxService.Show
                (
                    Wpf.Properties.Resources.MessageCustomNodeNeedNewCategory,
                    Wpf.Properties.Resources.CustomNodePropertyErrorMessageBoxTitle,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
            else
            {
                DialogResult = true;
            }
        }

        void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        public string Text
        {
            get { return nameBox.Text; }
        }

        public string Description
        {
            get { return DescriptionInput.Text; }
        }

        public string Category
        {
            get { return categoryBox.Text; }
        }


        /// <summary>
        /// Allows for the dragging of this custom-styled window. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FunctionNamePrompt_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Drag functionality when the TitleBar is clicked with the left button and dragged to another place
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void NameBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (PathHelper.IsFileNameInValid(nameBox.Text))
            {
                ErrorIcon.Visibility = Visibility.Visible;
                ErrorUnderline.Visibility = Visibility.Visible;
            }
            else
            {
                ErrorIcon.Visibility = Visibility.Collapsed;
                ErrorUnderline.Visibility = Visibility.Collapsed;
            }
        }
    }
}
