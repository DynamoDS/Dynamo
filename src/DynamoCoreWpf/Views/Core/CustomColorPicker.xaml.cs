using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Dynamo.ViewModels;

namespace Dynamo.Controls
{
    /// <summary>
    /// Interaction logic for CustomColorPicker.xaml
    /// </summary>
    public partial class CustomColorPicker : Popup
    {
        private CustomColorPickerViewModel viewModel;

        internal CustomColorPicker()
        {
            InitializeComponent();
            if (viewModel == null)
            {
                viewModel = new CustomColorPickerViewModel();
            }
            this.DataContext = viewModel;
        }

        /// <summary>
        /// Set the list of custom colors (it should be set from the client because otherwise the colors won't persist)
        /// </summary>
        /// <param name="customColors"></param>
        internal void SetCustomColors(ObservableCollection<CustomColorItem> customColors)
        {
            viewModel.CustomColors = customColors;
        }

        /// <summary>
        /// This method will automatically select a color from the ColorPicker
        /// </summary>
        /// <param name="color"></param>
        internal void InitializeSelectedColor(Color? color)
        {
            var initialColor = new CustomColorItem(color, string.Format("#{0},{1},{2}", color.Value.R, color.Value.G, color.Value.B));

            CleanSelectedColor();

            var availableColorsList = PART_AvailableColors.Items.Cast<CustomColorItem>().ToList();
            var customColorsList = PART_CustomColors.Items.Cast<CustomColorItem>().ToList();

            if (availableColorsList.Contains(initialColor))
                SelectColorFromList(availableColorsList, color);
            else if (customColorsList.Contains(initialColor))
                SelectColorFromList(customColorsList, color);
        }

        private void SelectColorFromList(List<CustomColorItem> colorsList, Color? color)
        {
            var colorItemFound = colorsList.Find(x => x.Color.Value.R == color.Value.R && x.Color.Value.G == color.Value.G && x.Color.Value.B == color.Value.B);
            colorItemFound.IsColorItemSelected = true;
            colorCanvasControl.SelectedColor = colorItemFound.Color;
        }

        private void PART_BasicColors_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectColor(sender);
        }

        private void PART_CustomColors_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectColor(sender);
        }

        private void CleanSelectedColor()
        {
            PART_CustomColors.Items.Cast<CustomColorItem>().ToList().ForEach(color => color.IsColorItemSelected = false);
            PART_AvailableColors.Items.Cast<CustomColorItem>().ToList().ForEach(color => color.IsColorItemSelected = false);
        }

        private void SelectColor(object sender)
        {

            var listbox = sender as ListBox;
            if (listbox == null) return;

            var customColorItemSelected = listbox.SelectedItem as CustomColorItem;
            if (customColorItemSelected == null) return;

            //Clean the selected color in both list (Available Colors and Custom Colors)
            CleanSelectedColor();

            customColorItemSelected.IsColorItemSelected = true;
            colorCanvasControl.SelectedColor = customColorItemSelected.Color;
            
        }

        private void DefineColorBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            viewModel.IsCustomColorSelectionEnabled = true;
            if(colorCanvasControl.SelectedColor == null)
                colorCanvasControl.SelectedColor = Colors.Black;
        }

        private void CustomColorsCloseButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.IsOpen = false;
        }

        private void CustomColorsCancelBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.IsOpen = false;
        }

        private void CustomColorsApplyBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.IsOpen = false;
            viewModel.ColorPickerSelectedColor = colorCanvasControl.SelectedColor;

            //Just when we selected a custom color should be added to the CustomColors list
            if (viewModel.ColorPickerSelectedColor != null)
            {
                var color = viewModel.ColorPickerSelectedColor.Value;
                var colorItem = new CustomColorItem(viewModel.ColorPickerSelectedColor.Value, string.Format("#{0},{1},{2}", color.R, color.G, color.B));
                if(!viewModel.CustomColors.Contains(colorItem))
                    viewModel.CustomColors.Add(colorItem);
            }
        }

        private void NormalColorsCancelBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.IsOpen = false;
        }

        private void NormalColorsApplyBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.IsOpen = false;
            viewModel.ColorPickerSelectedColor = colorCanvasControl.SelectedColor;
        }

        private void NormalColorsCloseButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.IsOpen = false;
        }
    }
}
