using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Dynamo.GraphNodeManager.ViewModels;

namespace Dynamo.GraphNodeManager.Controls
{
    /// <summary>
    /// Interaction logic for FilterItemControl.xaml
    /// </summary>
    public partial class FilterItemControl : UserControl
    {
        public FilterItemControl()
        {
            InitializeComponent();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var vm = this.DataContext as FilterViewModel;
            if (vm == null) return;

            vm.Toggle(sender);
        }
    }

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
