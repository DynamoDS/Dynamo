using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using Dynamo.Core;
using Xceed.Wpf.Toolkit;

namespace Dynamo.ViewModels
{
    internal class CustomColorItem : ColorItem, INotifyPropertyChanged
    {
        /// <summary>
        /// This event will help to execute the method  OnPropertyChanged used for xaml bindings (NotificationObject class cannot be used due that we already derive from ColorItem)
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        private bool isColorItemSelected = false;
        public bool IsColorItemSelected
        {
            get
            {
                return isColorItemSelected;
            }
            set
            {
                isColorItemSelected = value;
                OnPropertyChanged(nameof(IsColorItemSelected));
            }
        }

        /// <summary>
        /// Constructor that initialize the base class with the values passed as paramters
        /// </summary>
        /// <param name="color">color that will be displayed in the ColorPicker list</param>
        /// <param name="name">description or color name that will be displayed as tooltip when mouse hover a specific color</param>
        public CustomColorItem(Color? color, string name) : base(color, name)
        {

        }

        // Create the OnPropertyChanged method to raise the event
        // The calling member's name will be used as the parameter.
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    internal class CustomColorPickerViewModel : NotificationObject
    {
        private Color? colorPickerFinalSelectedColor;
        private ObservableCollection<CustomColorItem> basicColors;
        private ObservableCollection<CustomColorItem> customColors;


        /// <summary>
        /// Color Selected in the ColorPicker
        /// </summary>
        public Color? ColorPickerFinalSelectedColor
        {
            get
            {
                return colorPickerFinalSelectedColor;
            }
            set
            {
                colorPickerFinalSelectedColor = value;
                RaisePropertyChanged(nameof(ColorPickerFinalSelectedColor));
            }
        }      

        /// <summary>
        /// List of Basic Colors that will be displayed in the CustomColorPicker
        /// </summary>
        public ObservableCollection<CustomColorItem> BasicColors
        {
            get
            {
                return basicColors;
            }
            set
            {
                basicColors = value;
                RaisePropertyChanged(nameof(BasicColors));
            }
        }

        /// <summary>
        /// List of Custom Colors that will be displayed in the CustomColorPicker
        /// </summary>
        public ObservableCollection<CustomColorItem> CustomColors
        {
            get
            {
                return customColors;
            }
            set
            {
                customColors = value;
                RaisePropertyChanged(nameof(CustomColors));
            }
        }

        public CustomColorPickerViewModel()
        {
            BasicColors = new ObservableCollection<CustomColorItem>();
            BasicColors = CreateBasicColorsCollection();

            CustomColors = new ObservableCollection<CustomColorItem>();
            CustomColors = CreateCustomColorsCollection();
        }

        private static ObservableCollection<CustomColorItem> CreateBasicColorsCollection()
        {
            //This list of colors were taken from the design created by the UX team in the Jira task (based from the Weave component).
            //Also you can find more details of the colors used in the next link:  https://weave.autodesk.com/web/basics/colors-data-viz
            ObservableCollection<CustomColorItem> observableCollection = new ObservableCollection<CustomColorItem>();

            List<(int R, int G, int B)> colors = new List<(int, int, int)>()
            {
                (77, 0, 0),
                (77, 19, 0),
                (77, 38, 0),
                (77, 58, 0),
                (77, 77, 1),
                (58, 76, 2),
                (37, 76, 2),
                (19, 77, 1),
                (0, 77, 1),
                (0, 77, 20),
                (0, 77, 38),
                (0, 77, 57),
                (0, 76, 76),
                (0, 58, 76),
                (0, 39, 76),
                (0, 21, 76),
                (129, 0, 1),
                (129, 31, 1),
                (129, 63, 0),
                (128, 95, 1),
                (128, 127, 5),
                (95, 127, 4),
                (63, 126, 3),
                (30, 127, 2),
                (0, 127, 2),
                (0, 127, 32),
                (0, 128, 63),
                (0, 127, 95),
                (0, 127, 127),
                (0, 95, 127),
                (0, 64, 127),
                (0, 32, 127),
                (155, 0, 2),
                (155, 38, 2),
                (155, 76, 3),
                (154, 114, 3),
                (154, 153, 3),
                (114, 153, 5),
                (77, 153, 3),
                (35, 153, 3),
                (0, 153, 4),
                (0, 153, 38),
                (0, 153, 76),
                (0, 154, 113),
                (0, 153, 153),
                (0, 114, 153),
                (0, 78, 153),
                (0, 39, 153),
                (206, 1, 1),
                (206, 51, 3),
                (206, 101, 4),
                (206, 153, 3),
                (205, 204, 0),
                (153, 205, 6),
                (100, 205, 7),
                (46, 204, 3),
                (0, 204, 5),
                (0, 204, 51),
                (0, 204, 102),
                (0, 204, 153),
                (0, 204, 204),
                (0, 154, 204),
                (0, 102, 204),
                (0, 53, 204),
                (255, 0, 0),
                (255, 64, 0),
                (255, 127, 1),
                (255, 192, 0),
                (255, 255, 0),
                (190, 254, 5),
                (125, 255, 2),
                (59, 254, 5),
                (0, 254, 1),
                (0, 254, 63),
                (0, 255, 127),
                (0, 255, 191),
                (0, 255, 255),
                (0, 192, 255),
                (0, 128, 254),
                (0, 67, 255)
            };


            foreach (var color in colors)
            {
                var colorItem = Color.FromRgb((byte)color.R, (byte)color.G, (byte)color.B);
                observableCollection.Add(new CustomColorItem(colorItem, string.Format("#{0},{1},{2}", color.R, color.G, color.B)));
            }

            return observableCollection;
        }

        /// <summary>
        /// Creates de default List of Custom Colors (this will be modified by the user
        /// </summary>
        /// <returns></returns>
        private static ObservableCollection<CustomColorItem> CreateCustomColorsCollection()
        {
            ObservableCollection<CustomColorItem> observableCollection = new ObservableCollection<CustomColorItem>();
            var colorItem = Color.FromRgb(0, 255, 0);
            observableCollection.Add(new CustomColorItem(colorItem, string.Format("#{0},{1},{2}", 0, 255, 0)));
            return observableCollection;
        }
    }
}
