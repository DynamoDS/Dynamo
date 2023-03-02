using System.Windows.Media;
using Dynamo.Core;

namespace Dynamo.Wpf
{
    internal class ColorPaletteViewModel : NotificationObject
    {
        private SolidColorBrush selectedColor;

        public SolidColorBrush SelectedColor
        {
            get
            {
                return selectedColor;
            }
            set
            {
                selectedColor = value;
                RaisePropertyChanged(nameof(SelectedColor));
            }
        }
    }
}
