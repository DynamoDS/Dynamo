using System.Linq;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Dynamo.ViewModels;
using Xceed.Wpf.Toolkit;

namespace Dynamo.Controls
{
    public partial class CustomColorPicker : Popup
    {
        CustomColorPickerViewModel viewModel;
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

        private void xceedColorPickerControl_SelectedColorChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<System.Windows.Media.Color?> e)
        {
            //Cast to ColorPicker so we can use ColorPicker.Template
            var colorPicker = (sender as ColorPicker);

            //Get the ListBox for BasicColors
            var basicColorsListBox = colorPicker.Template.FindName("PART_AvailableColors", colorPicker) as ListBox;
            if (basicColorsListBox == null) return;

            basicColorsListBox.Items.Cast<CustomColorItem>().ToList().ForEach(color => color.IsColorItemSelected = false);

            //Find the color clicked in the ListBox.Items so we can put the IsColorItemSelected to true
            var customColorItemSelected = basicColorsListBox.Items.Cast<CustomColorItem>().Where(item => item.Color.Value == e.NewValue.Value).FirstOrDefault();
            if (customColorItemSelected == null) return;
            customColorItemSelected.IsColorItemSelected = true;
        }
    }
}
