using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Dynamo.PackageManager.UI
{
    /// <summary>
    /// Interaction logic for NumericUpDownControl.xaml
    /// </summary>
    public partial class NumericUpDownControl : UserControl
    {
        #region Private properties
        private Regex _numMatch;
        private bool mouseClickSelection = true; 
        #endregion

        #region Dependency properties
        // The 'Label' of the control
        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Label.  
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register("Label", typeof(string), typeof(NumericUpDownControl), new PropertyMetadata(string.Empty));



        // The watermark that will be displayed if no value is provided
        public string Watermark
        {
            get { return (string)GetValue(WatermarkProperty); }
            set { SetValue(WatermarkProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Watermark.  
        public static readonly DependencyProperty WatermarkProperty =
            DependencyProperty.Register("Watermark", typeof(string), typeof(NumericUpDownControl),
                new FrameworkPropertyMetadata("0"));

        // The Value of the numerical up/down control. Bind to this property
        public string Value
        {
            get { return (string)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Value. 
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value",
                typeof(string),
                typeof(NumericUpDownControl),
                new FrameworkPropertyMetadata("0", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnValuePropertyChanged)));

        // Setting the input TextBox 
        private static void OnValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var numericUpDownControl = d as NumericUpDownControl;
            numericUpDownControl.inputField.Text = e.NewValue.ToString();
            if (String.IsNullOrEmpty(e.NewValue.ToString())) return;
            if (numericUpDownControl.watermarkLabel.Visibility == Visibility.Visible)
            {
                numericUpDownControl.watermarkLabel.Visibility = Visibility.Collapsed;
            }
        } 
        #endregion

        public NumericUpDownControl()
        {
            InitializeComponent();

            // Allows positive whole numbers
            _numMatch = new Regex(@"^(?:0|[1-9]\d*)$");
        }

        #region UI utility functions
        private void spinnerUp_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(inputField.Text))
            {
                if (Int32.TryParse(watermarkLabel.Content as string, out int watermarkValue))
                {
                    Value = (++watermarkValue).ToString();
                    return;
                }
                else
                {
                    inputField.Text = "0"; // increment from 0 if Watermark is anyhting else but integer
                }
            }
            if (Int32.TryParse(inputField.Text, out int value))
            {
                Value = (++value).ToString();
            };
        }

        private void spinnerDown_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(inputField.Text))
            {
                if (Int32.TryParse(watermarkLabel.Content as string, out int watermarkValue) && watermarkValue > 0)
                {
                    Value = (--watermarkValue).ToString();
                    return;
                }
                else
                {
                    inputField.Text = "0"; // decrement from 0 if Watermark is anyhting else but integer
                }
            }
            if (Int32.TryParse(inputField.Text, out int value))
            {
                if (value == 0) return;
                Value = (--value).ToString();
            };
        }

        /// <summary>
        /// Checks if the input value passes our criteria
        /// </summary>
        /// <param name="text">text string to be tested</param>
        /// <returns></returns>
        internal bool IsValidInput(string text)
        {
            return _numMatch.IsMatch(text);
        }

        // validate data before changing the text (only allow numbers)
        private void inputField_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var tb = (TextBox)sender;
            var text = tb.Text.Insert(tb.CaretIndex, e.Text);

            e.Handled = !IsValidInput(text);
        }

        // Handles the direct input of text (via keyboard)
        private void inputField_TextChanged(object sender, TextChangedEventArgs e)
        {
            Int32.TryParse(inputField.Text, out int value);
            Value = value.ToString();
        }

        // When user clicks on the watermark label at first, hide the watermark and pass the focus to the input textbox
        private void watermarkLabel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            watermarkLabel.Visibility = Visibility.Collapsed;
            inputField.Focus();
        }

        // Select the text inside the TextBox when clicked
        private async void inputField_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var tb = sender as TextBox;
            if (mouseClickSelection)
            {
                await Application.Current.Dispatcher.InvokeAsync(tb.SelectAll);

                //disable the boolean porperty to selection apply only first time 
                mouseClickSelection = false;
            }
        }

        private void inputField_LostFocus(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            if (string.IsNullOrEmpty(tb.Text))
            {
                watermarkLabel.Visibility = Visibility.Visible;
            }

            mouseClickSelection = true;
        }

        private void inputField_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true; // Prevent space from being entered
            }
        }

        #endregion
    }
}
