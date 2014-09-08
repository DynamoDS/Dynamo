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

            ProcessRootCategories();

            InitializeComponent();
            Loaded += LibraryViewLoaded;
        }

        private void LibraryViewLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            DataContext = this.viewModel;
        }

        private void OnPreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        /// <summary>
        /// Method goes through all BrowserCategories collections and
        /// sets correct value of IsPlaceHolder. If need it populates
        /// member collections for category.Classdetails instance
        /// </summary>
        private void ProcessRootCategories()
        {
            var rootCategories = viewModel.Model.BrowserRootCategories;
            for (int i = 0; i < rootCategories.Count; i++)
            {
                rootCategories[i].SpecifyIsPlaceHolderProperty();

                if (rootCategories[i].IsPlaceholder)
                {
                    rootCategories[i].ClassDetails.PopulateMemberCollections(rootCategories[i]);
                    rootCategories[i].ClassDetails.ClassDetailsVisibility = true;
                }
            }
        }
    }
}
