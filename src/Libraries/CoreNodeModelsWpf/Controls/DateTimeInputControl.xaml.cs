using System.Windows;
using System.Windows.Forms;
using UserControl = System.Windows.Controls.UserControl;

namespace CoreNodeModelsWpf.Controls
{
    /// <summary>
    /// Interaction logic for DateTimeInputControl.xaml
    /// </summary>
    public partial class DateTimeInputControl : UserControl
    {
        public DateTimeInputControl()
        {
            InitializeComponent();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var picker = new DateTimePicker
            {
                Format = DateTimePickerFormat.Time,
                ShowUpDown = true,
            };
        }
    }
}
