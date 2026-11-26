using System;
using System.Windows;
using System.Windows.Controls;
using Dynamo.PythonMigration.Differ;

namespace Dynamo.PythonMigration.Controls
{
    /// <summary>
    /// Interaction logic for SideBySideControl.xaml
    /// </summary>
    public partial class SideBySideControl : System.Windows.Controls.UserControl
    {
        private const double SyncTolerance = 0.5;
        private DiffNavigator navigator;
        private ScrollViewer echoPane;

        public SideBySideControl()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (navigator == null)
            {
                var viewModel = DataContext as SideBySideViewModel;
                if (viewModel == null)
                {
                    return;
                }

                // Both panes get the same page width and no wrapping, so row N is at the same height on both sides.
                RichTextBoxHandler.TurnOffWrapping(BeforePanel, AfterPanel);

                // A row is part of a change when either side changed. The panes scroll together, so the
                // navigator only needs to move one of them.
                navigator = new DiffNavigator(
                    AfterScroll,
                    AfterPanel,
                    DiffChangeFinder.FindChangeStarts(viewModel.AfterPane.Lines, viewModel.BeforePane.Lines));
                navigator.GoToFirstWhenReady();
            }

            BaseDiffViewer.AttachNavigator(this, navigator);
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            BaseDiffViewer.DetachNavigator(this, navigator);
        }

        private void OnPaneScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            // Ignore the RichTextBox's own inner scroller; only the pane scrollers matter.
            if (!ReferenceEquals(e.OriginalSource, sender) || (e.VerticalChange == 0 && e.HorizontalChange == 0))
            {
                return;
            }

            var source = (ScrollViewer)sender;
            if (ReferenceEquals(source, echoPane))
            {
                // This pane moved because we moved it; do not send the move back.
                echoPane = null;
                return;
            }

            var other = ReferenceEquals(source, BeforeScroll) ? AfterScroll : BeforeScroll;
            if (other == null)
            {
                return;
            }

            // Ask only for what the other pane can reach. If the divider makes the panes different widths,
            // the wider one has less to scroll sideways.
            var vertical = Math.Min(source.VerticalOffset, other.ScrollableHeight);
            var horizontal = Math.Min(source.HorizontalOffset, other.ScrollableWidth);
            var moveVertical = Math.Abs(other.VerticalOffset - vertical) > SyncTolerance;
            var moveHorizontal = Math.Abs(other.HorizontalOffset - horizontal) > SyncTolerance;
            if (!moveVertical && !moveHorizontal)
            {
                return;
            }

            echoPane = other;
            if (moveVertical)
            {
                other.ScrollToVerticalOffset(vertical);
            }

            if (moveHorizontal)
            {
                other.ScrollToHorizontalOffset(horizontal);
            }
        }
    }
}
