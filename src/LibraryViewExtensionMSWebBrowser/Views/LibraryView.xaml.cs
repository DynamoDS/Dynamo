
using System.Windows.Controls;

using Dynamo.LibraryViewExtensionMSWebBrowser.ViewModels;

namespace Dynamo.LibraryViewExtensionMSWebBrowser.Views
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
