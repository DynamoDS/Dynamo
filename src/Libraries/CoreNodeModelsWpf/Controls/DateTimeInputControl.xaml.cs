using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
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
            Loaded += DateTimeInputControl_Loaded;
        }

        private void DateTimeInputControl_Loaded(object sender, RoutedEventArgs e)
        {
            var binding = new System.Windows.Data.Binding(nameof(CoreNodeModels.Input.DateTime.ValueText))
            {
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit,
                NotifyOnValidationError = false
            };
            var rule = new DateTimeValidationRule
            {
                ValidationStep = ValidationStep.RawProposedValue
            };
            binding.ValidationRules.Add(rule);
            DateTimeTb.BindToProperty(binding);
            Validation.SetErrorTemplate(DateTimeTb, null);
        }
    }
}
