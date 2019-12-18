
using System.Windows.Controls;

using Dynamo.LibraryViewExtensionMSWebView.ViewModels;

namespace Dynamo.LibraryViewExtensionMSWebView.Views
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
