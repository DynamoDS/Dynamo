using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Dynamo.Controls;
using Dynamo.ViewModels;
using Dynamo.Wpf;

namespace CoreNodeModelsWpf.Controls
{
    /// <summary>
    /// Interaction logic for ColorPalette.xaml
    /// </summary>
    public partial class ColorPaletteUI : UserControl
    {

        /// <summary>
        /// Delegate that will be used for manage the event when a color was selected from the Color Picker
        /// </summary>
        public delegate void ColorPickerSelectedColorHandler();

        /// <summary>
        /// Event that will be raised when the user selected a color in the CustomColorPicker popup and then the popup is closed
        /// </summary>
        public event ColorPickerSelectedColorHandler ColorPickerSelectedColor;

        public void OnColorPickerSelectedColor()
        {
            this.ColorPickerSelectedColor?.Invoke();
        }

        private ColorPaletteViewModel viewModel = null;

        public ColorPaletteUI()
        {
            InitializeComponent();
            if(viewModel == null)
            {
                viewModel = new ColorPaletteViewModel();
            }
            this.DataContext = viewModel;

            //By default the ToggleButton will contain the Black color
            viewModel.SelectedColor = new SolidColorBrush(Colors.Black);
        }

        private void ColorPickerPopup_Closed(object sender, System.EventArgs e)
        {
            var colorPickerPopupClosed = sender as CustomColorPicker;
            colorPickerPopupClosed.Closed -= ColorPickerPopup_Closed;
            var colorPickerViewModel = colorPickerPopupClosed.DataContext as CustomColorPickerViewModel;
            if (colorPickerViewModel == null || colorPickerViewModel.ColorPickerFinalSelectedColor == null) return;

            viewModel.SelectedColor = new SolidColorBrush(colorPickerViewModel.ColorPickerFinalSelectedColor.Value);
            OnColorPickerSelectedColor();
        }

        private void ColorToggleButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var colorPickerPopup = new CustomColorPicker();
            colorPickerPopup.Placement = PlacementMode.Bottom;
            colorPickerPopup.PlacementTarget = ColorToggleButton;
            colorPickerPopup.IsOpen = true;
            colorPickerPopup.Closed += ColorPickerPopup_Closed;
        }
    }
}
