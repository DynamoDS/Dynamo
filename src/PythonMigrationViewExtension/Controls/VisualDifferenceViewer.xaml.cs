using DiffPlex.Wpf.Controls;
using System.Windows;

namespace Dynamo.PythonMigration.Controls
{
    /// <summary>
    /// Interaction logic for VisualDifferenceViewer.xaml
    /// </summary>
    public partial class VisualDifferenceViewer : Window
    {
        private PythonMigrationAssistantViewModel ViewModel { get; set; }
        internal VisualDifferenceViewer(PythonMigrationAssistantViewModel viewModel, Window ownerWindow)
        {
            ViewModel = viewModel;
            Owner = ownerWindow;
            InitializeComponent();
            LoadData();
            DiffView.ViewModeChanged += OnViewModeChanged;
            DiffView.Loaded += OnDiffViewLoaded;
            this.Closed += OnVisualDifferenceViewerClosed;
        }

        private void OnDiffViewLoaded(object sender, RoutedEventArgs e)
        {
            SizeWindowToContent();
        }

        private void OnViewModeChanged(object sender, DiffViewer.ViewModeChangedEventArgs e)
        {
            SizeWindowToContent();
        }

        private void LoadData()
        {
            DiffView.OldText = ViewModel.OldCode;
            DiffView.NewText = ViewModel.NewCode;
        }

        private void DiffButton_Click(object sender, RoutedEventArgs e)
        {
            if (DiffView.IsInlineViewMode)
            {
                DiffView.ShowSideBySide();
                return;
            }

            DiffView.ShowInline();
        }

        private void SizeWindowToContent()
        {
            SizeToContent = SizeToContent.WidthAndHeight;
            // We need to set the SizeToContent back to manual otherwise it will freeze
            // the GridSplitter on the DiffView
            SizeToContent = SizeToContent.Manual;
        }

        private void OnVisualDifferenceViewerClosed(object sender, System.EventArgs e)
        {
            DiffView.ViewModeChanged -= OnViewModeChanged;
            DiffView.Loaded -= OnDiffViewLoaded;
            this.Closed -= OnVisualDifferenceViewerClosed;
        }
    }
}
