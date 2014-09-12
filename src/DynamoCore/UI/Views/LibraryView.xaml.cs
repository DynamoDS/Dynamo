using System.Windows.Controls;
using System.Windows.Input;

namespace Dynamo.UI.Views
{
    /// <summary>
    /// Interaction logic for LibraryView.xaml
    /// </summary>
    public partial class LibraryView : UserControl
    {
        public LibraryView()
        {
            InitializeComponent();
        }

        private void OnPreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        private void LibraryGridMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
        }


        private void LibraryGridMouseEnter(object sender, MouseEventArgs e)
        {
            ScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
        }

    }
}
