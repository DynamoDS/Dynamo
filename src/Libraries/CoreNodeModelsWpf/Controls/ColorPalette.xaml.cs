using System.Windows.Controls;
using System.Windows.Media;
using System.Collections.ObjectModel;

using Xceed.Wpf.Toolkit;

namespace CoreNodeModelsWpf.Controls
{
    /// <summary>
    /// Interaction logic for ColorPalette.xaml
    /// </summary>
    public partial class ColorPaletteUI : UserControl
    {
        /// <summary>
        ///     List of standard colors.
        /// </summary>
        private ObservableCollection<ColorItem> ColorList = new ObservableCollection<ColorItem>()
        {
            new ColorItem(Color.FromArgb(255,48,130,189), "Blue 1"),
            new  ColorItem(Color.FromArgb(255,106,173,213), "Blue 2"),
            new  ColorItem(Color.FromArgb(255,158,202,225), "Blue 3"),
            new  ColorItem(Color.FromArgb(255,199,219,238), "Blue 4"),

            new  ColorItem(Color.FromArgb(255,229,87,37), "Orange 1"),
            new  ColorItem(Color.FromArgb(255,246,140,63), "Orange 2"),
            new  ColorItem(Color.FromArgb(255,229,87,37), "Orange 3"),
            new  ColorItem(Color.FromArgb(255,252,208,162), "Orange 4"),

            new  ColorItem(Color.FromArgb(255,49,163,83), "Green 1"),
            new  ColorItem(Color.FromArgb(255,117,195,118), "Green 2"),
            new  ColorItem(Color.FromArgb(255,162,211,154), "Green 3"),
            new  ColorItem(Color.FromArgb(255,200,228,191), "Green 4"),

            new  ColorItem(Color.FromArgb(255,117,107,177), "Purple 1"),
            new  ColorItem(Color.FromArgb(255,158,154,199), "Purple 2"),
            new  ColorItem(Color.FromArgb(255,189,189,219), "Purple 3"),
            new  ColorItem(Color.FromArgb(255,218,218,234), "Purple 4"),

            new  ColorItem(Color.FromArgb(255,99,99,99), "Grey 1"),
            new  ColorItem(Color.FromArgb(255,150,150,150), "Grey 2"),
            new  ColorItem(Color.FromArgb(255,189,189,189), "Grey 3"),
            new  ColorItem(Color.FromArgb(255,217,217,217), "Grey 4"),
        };

        public ColorPaletteUI()
        {           
            InitializeComponent();
            this._colorpicker.AvailableColors = ColorList;
        }
    }
}
