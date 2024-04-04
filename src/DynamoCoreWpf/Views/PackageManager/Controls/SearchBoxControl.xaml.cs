using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Dynamo.PackageManager.UI
{

    /// <summary>
    /// Interaction logic for SearchBoxControl.xaml
    /// </summary>
    public partial class SearchBoxControl : UserControl
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SearchBoxControl()
        {
            InitializeComponent();
        }

        private void OnSearchClearButtonClicked(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.SearchTextBox.Clear();
        }

        private void SearchTextBox_OnKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null) return;

            if (string.IsNullOrEmpty(textBox.Text) && !textBox.IsKeyboardFocusWithin)
            {
                SearchTextBoxWatermark.Visibility = Visibility.Visible;
            }
            else
            {
                SearchTextBoxWatermark.Visibility = Visibility.Collapsed;
            }
        }
    }

    /// <summary>
    /// Converts null or empty string to Visibility Collapsed
    /// </summary>
    public class EmptyStringToCollapsedConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var boolValue = value is string s && !string.IsNullOrEmpty(s);

            return boolValue ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
