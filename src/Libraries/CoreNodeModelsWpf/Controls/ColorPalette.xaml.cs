using System.Globalization;
using System.Windows.Controls;
using System.Windows.Markup;

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
            Language = XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag);
        }
    }
}
