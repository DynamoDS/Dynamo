using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

namespace Dynamo.PackageManager.UI
{

    /// <summary>
    /// Interaction logic for SearchBoxControl.xaml
    /// </summary>
    public partial class SearchBoxControl : UserControl
    {
        private DispatcherTimer delayTimer;

        // set delay for event 250ms
        private static int delayTime = 250;
        /// <summary>
        /// Constructor
        /// </summary>
        public SearchBoxControl()
        {
            InitializeComponent();

            delayTimer = new DispatcherTimer();
            delayTimer.Interval = TimeSpan.FromMilliseconds(delayTime);
            delayTimer.Tick += DelayTimer_Tick;
        }
            
        private void DelayTimer_Tick(object sender, EventArgs e)
        {
            delayTimer.Stop();
            var textBox = this.SearchTextBox;
            if (textBox == null) return;
            (this.DataContext as PackageManagerSearchViewModel)?.SearchAndUpdateResults(textBox.Text);
        }

        /// <summary>
        /// Handles Search Box Text Changed Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void SearchTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            // When text changes, restart the timer
            delayTimer.Stop();
            delayTimer.Start();
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
    /// A multivalue visibilty converter
    /// value 0 - boolean
    /// value 1 - text
    /// </summary>
    public class MultiBooleanToVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool focusVisible = true;
            bool textVisible = true;

            foreach (object value in values)
            {
                if (value is bool)
                {
                    if ((bool)value)
                    {
                        focusVisible = false;   // If the textbox has the focus, don't show the Control..
                    }
                    else
                    {
                        focusVisible = true;    // If the textbox don't have focus, we can show the Control..
                    }
                }
                else
                {
                    if (value != null && string.IsNullOrWhiteSpace(value.ToString()))
                    {
                        textVisible = true; // If the textbox has no text, we can show the Control..
                    }
                    else
                    {
                        textVisible = false;    // If the textbox has some text, don't show the Control..
                    }
                }
            }

            // Only of both conditions are true, show the Control..
            if (focusVisible && textVisible)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts null or empty string to Visibility Collapsed
    /// </summary>
    public class NonEmptyStringToCollapsedConverter : IValueConverter
    {
        enum Parameters
        {
            Normal, Inverted
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            parameter = parameter ?? "Normal";

            var boolValue = value is string s && !string.IsNullOrEmpty(s);
            var direction = (Parameters)Enum.Parse(typeof(Parameters), (string)parameter);

            if (direction == Parameters.Inverted)
                return !boolValue ? Visibility.Visible : Visibility.Collapsed;

            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
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
