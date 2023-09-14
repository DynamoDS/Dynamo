using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Dynamo.PackageManager.UI
{
    /// <summary>
    /// Interaction logic for FilterTagControl.xaml
    /// Used to create visual representation (bubbles) of a filter property that is currently applied
    /// </summary>
    public partial class FilterTagControl : UserControl
    {
        /// <summary>
        /// The name of the filter Tag
        /// </summary>
        public string TagName
        {
            get { return (string)GetValue(TagNameProperty); }
            set { SetValue(TagNameProperty, value); }
        }
        /// <summary>
        /// Allows to toggle the filter on/off
        /// </summary>
        public bool IsFilterOn
        {
            get { return (bool)GetValue(IsFilterOnProperty); }
            set { SetValue(IsFilterOnProperty, value); }
        }

        /// <summary>
        /// Binds to the 'FilterCommand' of the filter
        /// </summary>
        public ICommand FilterCommand
        {
            get { return (ICommand)GetValue(FilterCommandProperty); }
            set { SetValue(FilterCommandProperty, value); }
        }

        public static readonly DependencyProperty TagNameProperty =
            DependencyProperty.Register(nameof(TagName), typeof(string), typeof(FilterTagControl), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty IsFilterOnProperty =
            DependencyProperty.Register(nameof(IsFilterOn), typeof(bool), typeof(FilterTagControl), new PropertyMetadata(false));

        public static readonly DependencyProperty FilterCommandProperty =
            DependencyProperty.Register(nameof(FilterCommand), typeof(ICommand), typeof(FilterTagControl));


        public FilterTagControl()
        {
            InitializeComponent();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            IsFilterOn = !IsFilterOn;
        }
    }

    /// <summary>
    /// Boolean to solid color brush
    /// </summary>
    public class BooleanToBorderColorConverter : IValueConverter
    {
        public SolidColorBrush ToggleBrush { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return new SolidColorBrush(Colors.Transparent);
            bool val = (bool)value;

            if (val) return ToggleBrush;
            return new SolidColorBrush(Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Boolean to solid color brush based on parameter
    /// </summary>
    public class BooleanToColorConverter : IValueConverter
    {
        enum Parameters
        {
            Default, Hover, Pressed
        }

        public SolidColorBrush DefaultBrush { get; set; }
        public SolidColorBrush HoverBrush { get; set; }
        public SolidColorBrush PressedBrush { get; set; }
        public SolidColorBrush ToggleDefaultBrush { get; set; }
        public SolidColorBrush ToggleHoverBrush { get; set; }
        public SolidColorBrush TogglePressedBrush { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null) return new SolidColorBrush(Colors.Transparent);

            bool val = (bool)value;
            var type = (Parameters)Enum.Parse(typeof(Parameters), (string)parameter);

            switch (type)
            {
                case Parameters.Default:
                    if (!val) return DefaultBrush;
                    else return ToggleDefaultBrush;
                case Parameters.Hover:
                    if (!val) return HoverBrush;
                    else return ToggleHoverBrush;
                case Parameters.Pressed:
                    if (!val) return PressedBrush;
                    else return TogglePressedBrush;
                default:
                    return new SolidColorBrush(Colors.Transparent);
            }
        }

        public object ConvertBack(
            object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
