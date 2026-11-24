using System.Windows;
using Dynamo.PythonMigration.Differ;

namespace Dynamo.PythonMigration.Controls
{
    /// <summary>
    /// Interaction logic for InLineControl.xaml
    /// </summary>
    public partial class InLineControl : System.Windows.Controls.UserControl
    {
        private DiffNavigator navigator;

        public InLineControl()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (navigator == null)
            {
                var viewModel = DataContext as InLineViewModel;
                if (viewModel == null)
                {
                    return;
                }

                // One pane, so long lines may keep wrapping; only the line numbers must stay on one row.
                RichTextBoxHandler.FitLineNumberColumn(InlinePanel);

                navigator = new DiffNavigator(
                    InlineScroll,
                    InlinePanel,
                    DiffChangeFinder.FindChangeStarts(viewModel.DiffModel.Lines));
                navigator.GoToFirstWhenReady();
            }

            BaseDiffViewer.AttachNavigator(this, navigator);
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            BaseDiffViewer.DetachNavigator(this, navigator);
        }
    }
}
