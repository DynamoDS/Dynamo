using System.Windows.Controls;
using Dynamo.ViewModels;

namespace Dynamo.UI.Views
{
    /// <summary>
    /// Interaction logic for LibraryView.xaml
    /// </summary>
    public partial class LibraryView : UserControl
    {
        //TODO: use LibraryViewModel if it will be ready
        private readonly SearchViewModel viewModel;
        private readonly DynamoViewModel dynamoViewModel;

        public LibraryView(SearchViewModel searchViewModel, DynamoViewModel dynamoViewModel)
        {
            this.viewModel = searchViewModel;
            this.dynamoViewModel = dynamoViewModel;

            InitializeComponent();
            Loaded += LibraryViewLoaded;
        }

        void LibraryViewLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            DataContext = this.viewModel;
        }
    }
}
