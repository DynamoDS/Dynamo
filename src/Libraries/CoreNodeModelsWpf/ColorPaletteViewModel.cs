using System.Windows.Media;
using Dynamo.Core;

namespace Dynamo.Wpf
{
    internal class ColorPaletteViewModel : NotificationObject
    {
        private SolidColorBrush selectedColor;
        private bool isColorPickerShown;

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

        /// <summary>
        /// When the Color Picker is opened this value will have true otherwise will be false, this helps to control the arrow color in the ToggleButton
        /// </summary>
        public bool IsColorPickerShown
        {
            get
            {
                return isColorPickerShown;
            }
            set
            {
                isColorPickerShown = value;
                RaisePropertyChanged(nameof(IsColorPickerShown));
            }
        }
    }
}
