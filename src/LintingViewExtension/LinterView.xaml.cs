using System.Windows.Controls;

namespace Dynamo.LintingViewExtension
{
    /// <summary>
    /// Interaction logic for LinterView.xaml
    /// </summary>
    public partial class LinterView : UserControl
    {
        public LinterView()
        {
            InitializeComponent();
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }
    }
}
