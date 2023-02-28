using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Dynamo.Controls;

namespace CoreNodeModelsWpf.Controls
{
    /// <summary>
    /// Interaction logic for ColorPalette.xaml
    /// </summary>
    public partial class ColorPaletteUI : UserControl
    {
        public ColorPaletteUI()
        {
            InitializeComponent();
        }

        private void xceedColorPickerControl_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var colorPickerPopup = new CustomColorPicker();
            colorPickerPopup.Placement = PlacementMode.Bottom;
            colorPickerPopup.PlacementTarget = xceedColorPickerControl;
            colorPickerPopup.IsOpen = true;
        }
    }
}
