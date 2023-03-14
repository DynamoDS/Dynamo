using System.Collections.ObjectModel;
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

        //This list will be passed everytime that we create a new CustomColorPicker (when we want to select a specific color for the node) so this custom colors list will be by node
        private ObservableCollection<CustomColorItem> nodeCustomColors;

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

            nodeCustomColors = new ObservableCollection<CustomColorItem>();
        }

        private void ColorPickerPopup_Closed(object sender, System.EventArgs e)
        {
            var colorPickerPopupClosed = sender as CustomColorPicker;
            colorPickerPopupClosed.Closed -= ColorPickerPopup_Closed;
            viewModel.IsColorPickerShown = false;
            var colorPickerViewModel = colorPickerPopupClosed.DataContext as CustomColorPickerViewModel;
            if (colorPickerViewModel == null || colorPickerViewModel.ColorPickerSelectedColor == null) return;

            viewModel.SelectedColor = new SolidColorBrush(colorPickerViewModel.ColorPickerSelectedColor.Value);
            OnColorPickerSelectedColor();        
        }

        private void ColorToggleButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var colorPickerPopup = new CustomColorPicker();
            //This will set the custom colors list so the custom colors will remain the same just for this node.
            colorPickerPopup.SetCustomColors(nodeCustomColors);
            colorPickerPopup.Placement = PlacementMode.Bottom;
            colorPickerPopup.PlacementTarget = ColorToggleButton;
            colorPickerPopup.IsOpen = true;
            colorPickerPopup.Closed += ColorPickerPopup_Closed;

            if (viewModel.SelectedColor != null)
            {
                //if the current color in the Color Palette node already exists in the CustomColorPicker then it will be selected
                colorPickerPopup.InitializeSelectedColor(viewModel.SelectedColor.Color);
            }
        }
    }
}
