using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

namespace Dynamo.UI.Controls
{
    /// <summary>
    /// Interaction logic for ToolTipWindow.xaml
    /// </summary>
    public partial class ToolTipWindow : UserControl
    {

        public ToolTipWindow()
        {
            InitializeComponent();
        }

        public void CopyIconMouseClick(object sender, MouseButtonEventArgs e)
        {
            //Clipboard.SetText(code.Text);
        }

        private void RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

    }
}
