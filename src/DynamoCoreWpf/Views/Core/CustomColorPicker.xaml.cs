using System.Linq;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Dynamo.ViewModels;

namespace Dynamo.Controls
{
    public partial class CustomColorPicker : Popup
    {
        private CustomColorPickerViewModel viewModel;

        public CustomColorPicker()
        {
            InitializeComponent();
            if (viewModel == null)
            {
                viewModel = new CustomColorPickerViewModel();
            }
            this.DataContext = viewModel;
        }

        private void Apply_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            viewModel.ColorPickerFinalSelectedColor = xceedColorPickerControl.SelectedColor;
            this.IsOpen = false;
        }

        private void CancelBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            viewModel.ColorPickerFinalSelectedColor = null;
            this.IsOpen = false;
        }

        private void CloseButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            viewModel.ColorPickerFinalSelectedColor = null;
            this.IsOpen = false;
        }

        private void PART_BasicColors_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {         
            SelectColor(sender);
        }

        private void PART_CustomColors_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectColor(sender);
        }

        private void SelectColor(object sender)
        {
            var listbox = sender as ListBox;
            if (listbox == null) return;

            var customColorItemSelected = listbox.SelectedItem as CustomColorItem;
            if (customColorItemSelected == null) return;

            listbox.Items.Cast<CustomColorItem>().ToList().ForEach(color => color.IsColorItemSelected = false);
            customColorItemSelected.IsColorItemSelected = true;
        }
    }
}
