using System.Windows.Controls;
using System.Windows.Input;

namespace Dynamo.Controls
{
    /// <summary>
    /// Interaction logic for ExecutionTimerControl.xaml
    /// </summary>
    public partial class ExecutionTimerControl : UserControl
    {
        public ExecutionTimerControl()
        {
            InitializeComponent();
        }

        private void UIElement_OnKeyUp(object sender, KeyEventArgs e)
        {
            var prop = IntervalTb.GetBindingExpression(TextBox.TextProperty);
            if (prop != null)
            {
                prop.UpdateSource();
            }
        }
    }
}
