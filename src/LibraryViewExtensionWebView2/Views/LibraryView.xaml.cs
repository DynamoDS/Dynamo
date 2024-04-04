
using System.Windows.Controls;
using Dynamo.LibraryViewExtensionWebView2.ViewModels;

namespace Dynamo.LibraryViewExtensionWebView2.Views
{
    /// <summary>
    /// Interaction logic for LibraryView.xaml
    /// </summary>
    public partial class LibraryView : UserControl
    {
        public LibraryView(LibraryViewModel viewModel)
        {
            this.DataContext = viewModel;
            InitializeComponent();

        }
    }
}
