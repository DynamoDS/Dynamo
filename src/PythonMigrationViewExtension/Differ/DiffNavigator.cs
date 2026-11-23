using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Threading;

namespace Dynamo.PythonMigration.Differ
{
    /// <summary>
    /// Scrolls a diff pane from one change to the next.
    /// "Next" and "previous" are worked out from where the pane is scrolled to right now, so they
    /// still make sense after the user scrolls by hand.
    /// </summary>
    internal class DiffNavigator
    {
        private const int ContextLines = 3;
        private const double Tolerance = 1.0;
        private readonly ScrollViewer scrollViewer;
        private readonly RichTextBox textBox;
        private readonly IReadOnlyList<int> changeStarts;

        // How far down the page each change sits, before limiting to what the pane can scroll to.
        // Null until worked out, and again whenever the page changes size.
        private List<double> changeOffsets;

        /// <summary>
        /// Raised when CanGoToPrevious or CanGoToNext may have changed.
        /// </summary>
        internal event EventHandler StateChanged;

        /// <param name="scrollViewer">Scrolls the pane.</param>
        /// <param name="textBox">Holds the diff table, one row per line of the diff.</param>
        /// <param name="changeStarts">The row where each change starts, in order.</param>
        internal DiffNavigator(ScrollViewer scrollViewer, RichTextBox textBox, IReadOnlyList<int> changeStarts)
        {
            this.scrollViewer = scrollViewer ?? throw new ArgumentNullException(nameof(scrollViewer));
            this.textBox = textBox ?? throw new ArgumentNullException(nameof(textBox));
            this.changeStarts = changeStarts ?? Array.Empty<int>();
            scrollViewer.ScrollChanged += OnScrollChanged;
        }

        internal bool CanGoToPrevious => FindPrevious() != null;

        internal bool CanGoToNext => FindNext() != null;

        internal void GoToPrevious() => ScrollTo(FindPrevious(ensureLayout: true));

        internal void GoToNext() => ScrollTo(FindNext(ensureLayout: true));

        /// <summary>
        /// Jumps to the first change once the pane has been laid out.
        /// </summary>
        internal void GoToFirstWhenReady()
        {
            textBox.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(GoToFirst));
        }

        private void GoToFirst()
        {
            var targets = Targets(ensureLayout: true);
            if (targets.Count > 0)
            {
                scrollViewer.ScrollToVerticalOffset(targets[0]);
            }

            // The first change may already be in view, in which case nothing scrolls and no
            // ScrollChanged arrives, so tell the buttons now.
            RaiseStateChanged();
        }

        private void ScrollTo(double? target)
        {
            if (target is double offset)
            {
                scrollViewer.ScrollToVerticalOffset(offset);
            }
        }

        private double? FindNext(bool ensureLayout = false)
        {
            var current = scrollViewer.VerticalOffset;
            foreach (var target in Targets(ensureLayout))
            {
                if (target > current + Tolerance)
                {
                    return target;
                }
            }

            return null;
        }

        private double? FindPrevious(bool ensureLayout = false)
        {
            var current = scrollViewer.VerticalOffset;
            double? found = null;
            foreach (var target in Targets(ensureLayout))
            {
                if (target >= current - Tolerance)
                {
                    break;
                }

                found = target;
            }

            return found;
        }

        private List<double> Targets(bool ensureLayout)
        {
            if (changeOffsets == null)
            {
                changeOffsets = MeasureChangeOffsets(ensureLayout);
            }

            var offsets = changeOffsets ?? new List<double>();
            var targets = new List<double>(offsets.Count);
            var max = scrollViewer.ScrollableHeight;
            foreach (var offset in offsets)
            {
                targets.Add(Math.Min(Math.Max(0, offset), max));
            }

            return targets;
        }

        private List<double> MeasureChangeOffsets(bool ensureLayout)
        {
            var rows = RichTextBoxHandler.GetRows(textBox);
            if (rows.Count == 0 || changeStarts.Count == 0)
            {
                return new List<double>();
            }

            if (ensureLayout)
            {
                scrollViewer.UpdateLayout();
            }

            // Row positions are measured against the first row, so it does not matter where the
            // text box draws its page.
            var origin = RowTop(rows[0]);
            if (origin == null)
            {
                return null;
            }

            var offsets = new List<double>(changeStarts.Count);
            foreach (var start in changeStarts)
            {
                if (start < 0 || start >= rows.Count)
                {
                    continue;
                }

                var top = RowTop(rows[Math.Max(0, start - ContextLines)]);
                if (top == null)
                {
                    return null;
                }

                offsets.Add(top.Value - origin.Value);
            }

            return offsets;
        }

        private static double? RowTop(TableRow row)
        {
            if (row.Cells.Count == 0)
            {
                return null;
            }

            var block = row.Cells[row.Cells.Count - 1].Blocks.FirstBlock;
            if (block == null)
            {
                return null;
            }

            var rect = block.ContentStart.GetCharacterRect(LogicalDirection.Forward);
            if (rect.IsEmpty || double.IsInfinity(rect.Top) || double.IsNaN(rect.Top))
            {
                return null;
            }

            return rect.Top;
        }

        private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            // The RichTextBox has its own inner scroller; its events bubble up here too. Ignore them.
            if (!ReferenceEquals(e.OriginalSource, scrollViewer))
            {
                return;
            }

            // Rows move when the page changes size (for example lines re-wrap after a resize).
            if (e.ExtentHeightChange != 0 || e.ExtentWidthChange != 0 || e.ViewportWidthChange != 0)
            {
                changeOffsets = null;
            }

            RaiseStateChanged();
        }

        private void RaiseStateChanged() => StateChanged?.Invoke(this, EventArgs.Empty);
    }
}
