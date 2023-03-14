using System.Windows.Media;
using Dynamo.Core;

namespace Dynamo.Wpf
{
    internal class ColorPaletteViewModel : NotificationObject
    {
        private SolidColorBrush selectedColor;

        /// <summary>
        /// This Property will contain the selected color in the CustomColorPicker popup
        /// </summary>
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
