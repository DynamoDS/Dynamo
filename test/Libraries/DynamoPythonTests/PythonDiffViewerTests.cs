using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using Dynamo.PythonMigration.Controls;
using Dynamo.PythonMigration.Differ;
using DynamoCoreWpfTests.Utility;
using NUnit.Framework;

namespace DynamoPythonTests
{
    /// <summary>
    /// Tests for the before/after panes of the Python review window (DYN-11078):
    /// no line wrapping, the two panes scrolling together, and the previous/next change navigation.
    /// The panes are shown in a plain test window, not the full review window.
    /// </summary>
    [TestFixture]
    [Category("UnitTests")]
    [RequiresThread(ApartmentState.STA)]
    public class PythonDiffViewerTests
    {
        // Must match DiffNavigator.ContextLines: unchanged lines left showing above a change.
        private const int ContextLines = 3;
        private const double Tolerance = 1.0;

        private Window window;

        [TearDown]
        public void CloseWindow()
        {
            window?.Close();
            window = null;
        }

        #region Helpers

        private static string Script(int lineCount, params int[] changedLines)
        {
            var lines = Enumerable.Range(1, lineCount)
                .Select(n => changedLines.Contains(n) ? $"value_{n} = {n * 1000}" : $"value_{n} = {n}");
            return string.Join("\n", lines);
        }

        private static string ScriptWithLongLine(int lineCount, int longLine, int longLineLength, params int[] changedLines)
        {
            var lines = Script(lineCount, changedLines).Split('\n');
            lines[longLine - 1] = $"value_{longLine} = \"" + new string('x', longLineLength) + "\"";
            return string.Join("\n", lines);
        }

        private static SideBySideDiffModel Diff(string before, string after)
        {
            return new SideBySideDiffBuilder().BuildDiffModel(before, after, false);
        }

        private static void Flush()
        {
            DispatcherUtil.DoEvents();
            DispatcherUtil.DoEvents();
        }

        private void ShowInWindow(FrameworkElement content, double width, double height)
        {
            window = new Window
            {
                Width = width,
                Height = height,
                Content = content,
                ShowInTaskbar = false,
                ShowActivated = false,
                WindowStartupLocation = WindowStartupLocation.Manual,
                Left = 0,
                Top = 0
            };
            window.Show();
            Flush();
        }

        private (ScrollViewer scroll, RichTextBox box) ShowPane(DiffPaneModel pane, double width = 400, double height = 300)
        {
            var box = new RichTextBox
            {
                IsReadOnly = true,
                FontFamily = new FontFamily("Consolas"),
                FontSize = 10 * 96.0 / 72.0,
                BorderThickness = new Thickness(0)
            };
            RichTextBoxHandler.SetUpdate(box, pane);
            var scroll = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Visible,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                Content = box
            };

            ShowInWindow(scroll, width, height);
            RichTextBoxHandler.TurnOffWrapping(box);
            Flush();
            return (scroll, box);
        }

        private SideBySideControl ShowSideBySide(SideBySideDiffModel diff, double width = 800, double height = 300)
        {
            var control = new SideBySideControl { DataContext = new SideBySideViewModel(diff) };
            ShowInWindow(control, width, height);
            return control;
        }

        private static DiffNavigator CreateNavigator(ScrollViewer scroll, RichTextBox box, SideBySideDiffModel diff)
        {
            return new DiffNavigator(scroll, box, DiffChangeFinder.FindChangeStarts(diff.NewText.Lines, diff.OldText.Lines));
        }

        private static double RowTop(RichTextBox box, int row)
        {
            var rows = RichTextBoxHandler.GetRows(box);
            return TextTop(rows[row]) - TextTop(rows[0]);
        }

        private static double TextTop(TableRow row)
        {
            var block = row.Cells[row.Cells.Count - 1].Blocks.FirstBlock;
            return block.ContentStart.GetCharacterRect(LogicalDirection.Forward).Top;
        }

        private static double RowHeight(RichTextBox box) => RowTop(box, 1) - RowTop(box, 0);

        private static double ExpectedOffset(ScrollViewer scroll, RichTextBox box, int changeRow)
        {
            var top = RowTop(box, Math.Max(0, changeRow - ContextLines));
            return Math.Min(top, scroll.ScrollableHeight);
        }

        private static void ScrollTo(ScrollViewer scroll, double offset)
        {
            scroll.ScrollToVerticalOffset(offset);
            Flush();
        }

        #endregion

        #region Previous / next change

        [Test]
        public void WhenOpenedThenTheFirstChangeIsShownNearTheTop()
        {
            var diff = Diff(Script(200), Script(200, 20, 100, 190));
            var (scroll, box) = ShowPane(diff.NewText);
            var navigator = CreateNavigator(scroll, box, diff);

            navigator.GoToFirstWhenReady();
            Flush();

            Assert.Greater(scroll.VerticalOffset, 0);
            Assert.AreEqual(ExpectedOffset(scroll, box, 19), scroll.VerticalOffset, Tolerance);
            Assert.IsFalse(navigator.CanGoToPrevious);
            Assert.IsTrue(navigator.CanGoToNext);
        }

        [Test]
        public void WhenNextIsPressedThenEachChangeIsShownInOrder()
        {
            var diff = Diff(Script(200), Script(200, 20, 100, 190));
            var (scroll, box) = ShowPane(diff.NewText);
            var navigator = CreateNavigator(scroll, box, diff);

            Assert.IsFalse(navigator.CanGoToPrevious);
            Assert.IsTrue(navigator.CanGoToNext);

            navigator.GoToNext();
            Flush();
            Assert.AreEqual(ExpectedOffset(scroll, box, 19), scroll.VerticalOffset, Tolerance);

            navigator.GoToNext();
            Flush();
            Assert.AreEqual(ExpectedOffset(scroll, box, 99), scroll.VerticalOffset, Tolerance);

            // The last change is near the end, so the pane can only scroll as far as the bottom.
            navigator.GoToNext();
            Flush();
            Assert.AreEqual(scroll.ScrollableHeight, scroll.VerticalOffset, Tolerance);
            Assert.IsFalse(navigator.CanGoToNext);
            Assert.IsTrue(navigator.CanGoToPrevious);
        }

        [Test]
        public void WhenAtTheLastChangeThenNextDoesNothing()
        {
            var diff = Diff(Script(200), Script(200, 20, 100, 190));
            var (scroll, box) = ShowPane(diff.NewText);
            var navigator = CreateNavigator(scroll, box, diff);
            ScrollTo(scroll, scroll.ScrollableHeight);
            var offset = scroll.VerticalOffset;

            navigator.GoToNext();
            Flush();

            Assert.AreEqual(offset, scroll.VerticalOffset, Tolerance);
        }

        [Test]
        public void WhenPreviousIsPressedThenTheEarlierChangesAreShown()
        {
            var diff = Diff(Script(200), Script(200, 20, 100, 190));
            var (scroll, box) = ShowPane(diff.NewText);
            var navigator = CreateNavigator(scroll, box, diff);
            ScrollTo(scroll, scroll.ScrollableHeight);

            navigator.GoToPrevious();
            Flush();
            Assert.AreEqual(ExpectedOffset(scroll, box, 99), scroll.VerticalOffset, Tolerance);

            navigator.GoToPrevious();
            Flush();
            Assert.AreEqual(ExpectedOffset(scroll, box, 19), scroll.VerticalOffset, Tolerance);
            Assert.IsFalse(navigator.CanGoToPrevious);
            Assert.IsTrue(navigator.CanGoToNext);
        }

        [Test]
        public void WhenScrolledByHandThenTheArrowsCountFromTheNewPosition()
        {
            var diff = Diff(Script(200), Script(200, 20, 100, 190));
            var (scroll, box) = ShowPane(diff.NewText);
            var navigator = CreateNavigator(scroll, box, diff);

            // Between the second and third change.
            var middle = RowTop(box, 150);
            ScrollTo(scroll, middle);
            Assert.IsTrue(navigator.CanGoToPrevious);
            Assert.IsTrue(navigator.CanGoToNext);

            navigator.GoToNext();
            Flush();
            Assert.AreEqual(scroll.ScrollableHeight, scroll.VerticalOffset, Tolerance);

            ScrollTo(scroll, middle);
            navigator.GoToPrevious();
            Flush();
            Assert.AreEqual(ExpectedOffset(scroll, box, 99), scroll.VerticalOffset, Tolerance);
        }

        [Test]
        public void WhenThereAreNoChangesThenBothArrowsAreOff()
        {
            var diff = Diff(Script(200), Script(200));
            var (scroll, box) = ShowPane(diff.NewText);
            var navigator = CreateNavigator(scroll, box, diff);

            Assert.IsFalse(navigator.CanGoToPrevious);
            Assert.IsFalse(navigator.CanGoToNext);

            navigator.GoToNext();
            Flush();
            Assert.AreEqual(0, scroll.VerticalOffset, Tolerance);
        }

        [Test]
        public void WhenTheFirstLineChangedThenPreviousIsOffAtTheTop()
        {
            var diff = Diff(Script(200), Script(200, 1, 100));
            var (scroll, box) = ShowPane(diff.NewText);
            var navigator = CreateNavigator(scroll, box, diff);

            navigator.GoToFirstWhenReady();
            Flush();

            Assert.AreEqual(0, scroll.VerticalOffset, Tolerance);
            Assert.IsFalse(navigator.CanGoToPrevious);
            Assert.IsTrue(navigator.CanGoToNext);
        }

        [Test]
        public void WhenThePaneScrollsThenTheArrowsAreToldToUpdate()
        {
            var diff = Diff(Script(200), Script(200, 20, 100, 190));
            var (scroll, box) = ShowPane(diff.NewText);
            var navigator = CreateNavigator(scroll, box, diff);
            var raised = 0;
            navigator.StateChanged += (sender, e) => raised++;

            ScrollTo(scroll, RowTop(box, 50));

            Assert.Greater(raised, 0);
        }

        #endregion

        #region No wrapping and line numbers

        [Test]
        public void WhenOneSideHasALongLineThenItDoesNotWrapAndTheRowsStayLevel()
        {
            var diff = Diff(ScriptWithLongLine(50, 5, 400), Script(50));
            var control = ShowSideBySide(diff);

            // The long line is one row high, like every other line.
            var before = control.BeforePanel;
            Assert.AreEqual(RowHeight(before), RowTop(before, 5) - RowTop(before, 4), Tolerance);
            Assert.AreEqual(control.BeforeScroll.ExtentHeight, control.AfterScroll.ExtentHeight, Tolerance);
        }

        [Test]
        public void WhenOneSideHasALongLineThenOnlyThatSideScrollsSideways()
        {
            var diff = Diff(ScriptWithLongLine(50, 5, 400), Script(50));
            var control = ShowSideBySide(diff);

            // Each pane is only as wide as its own longest line.
            Assert.Greater(control.BeforeScroll.ScrollableWidth, 0);
            Assert.AreEqual(0, control.AfterScroll.ScrollableWidth, Tolerance);
            Assert.Greater(control.BeforePanel.Document.PageWidth, control.AfterPanel.Document.PageWidth);
        }

        [Test]
        public void WhenTheScriptHasMoreThanAHundredLinesThenEveryRowIsOneLineHigh()
        {
            // Line numbers like "120 -" must fit their column, or the row would grow to two lines.
            var diff = Diff(Script(150), Script(150, 120, 140));
            var control = ShowSideBySide(diff);

            foreach (var box in new[] { control.BeforePanel, control.AfterPanel })
            {
                Assert.AreEqual(149 * RowHeight(box), RowTop(box, 149), 2.0);
            }
        }

        #endregion

        #region Panes scrolling together

        [Test]
        public void WhenTheSideBySideViewOpensThenBothPanesShowTheFirstChange()
        {
            var diff = Diff(Script(200), Script(200, 100));
            var control = ShowSideBySide(diff);

            Assert.AreEqual(ExpectedOffset(control.AfterScroll, control.AfterPanel, 99), control.AfterScroll.VerticalOffset, Tolerance);
            Assert.AreEqual(control.AfterScroll.VerticalOffset, control.BeforeScroll.VerticalOffset, Tolerance);
        }

        [Test]
        public void WhenOnePaneScrollsUpOrDownThenTheOtherFollows()
        {
            var diff = Diff(Script(200), Script(200, 20));
            var control = ShowSideBySide(diff);

            ScrollTo(control.AfterScroll, 500);
            Assert.AreEqual(500, control.BeforeScroll.VerticalOffset, Tolerance);

            ScrollTo(control.BeforeScroll, 100);
            Assert.AreEqual(100, control.AfterScroll.VerticalOffset, Tolerance);
        }

        [Test]
        public void WhenOnePaneScrollsSidewaysThenTheOtherFollows()
        {
            var diff = Diff(ScriptWithLongLine(50, 5, 400), ScriptWithLongLine(50, 5, 400, 20));
            var control = ShowSideBySide(diff);

            control.BeforeScroll.ScrollToHorizontalOffset(150);
            Flush();
            Assert.AreEqual(150, control.AfterScroll.HorizontalOffset, Tolerance);

            control.AfterScroll.ScrollToHorizontalOffset(60);
            Flush();
            Assert.AreEqual(60, control.BeforeScroll.HorizontalOffset, Tolerance);
        }

        [Test]
        public void WhenTheShorterPaneReachesItsEndThenTheLongerPaneKeepsScrolling()
        {
            var diff = Diff(ScriptWithLongLine(50, 5, 600), ScriptWithLongLine(50, 5, 300));
            var control = ShowSideBySide(diff);
            Assert.Greater(control.BeforeScroll.ScrollableWidth, control.AfterScroll.ScrollableWidth);

            control.BeforeScroll.ScrollToHorizontalOffset(control.BeforeScroll.ScrollableWidth);
            Flush();

            // The longer pane is not pulled back; the shorter one stops at its own end.
            Assert.AreEqual(control.BeforeScroll.ScrollableWidth, control.BeforeScroll.HorizontalOffset, Tolerance);
            Assert.AreEqual(control.AfterScroll.ScrollableWidth, control.AfterScroll.HorizontalOffset, Tolerance);
        }

        [Test]
        public void WhenLinesAreDeletedAndAddedThenTheSameRowLinesUpOnBothSides()
        {
            // Line 30 is deleted and a new line is added further down, so each side gets a blank spacer row.
            var afterLines = Script(200).Split('\n').ToList();
            afterLines.RemoveAt(29);
            afterLines.Insert(119, "new_value = 0");
            var diff = Diff(Script(200), string.Join("\n", afterLines));
            Assert.IsTrue(diff.OldText.Lines.Any(line => line.Type == ChangeType.Imaginary));
            Assert.IsTrue(diff.NewText.Lines.Any(line => line.Type == ChangeType.Imaginary));
            var control = ShowSideBySide(diff);

            // A line below both spacers sits on the same row on both sides.
            var row = diff.OldText.Lines.FindIndex(line => line.Text == "value_150 = 150");
            Assert.AreEqual(row, diff.NewText.Lines.FindIndex(line => line.Text == "value_150 = 150"));

            ScrollTo(control.AfterScroll, RowTop(control.AfterPanel, row));

            Assert.AreEqual(control.AfterScroll.VerticalOffset, control.BeforeScroll.VerticalOffset, Tolerance);
            Assert.AreEqual(RowTop(control.AfterPanel, row), RowTop(control.BeforePanel, row), Tolerance);
            Assert.AreEqual(row * RowHeight(control.BeforePanel), RowTop(control.BeforePanel, row), 2.0);
        }

        #endregion

        #region Single-pane view

        [Test]
        public void WhenTheInlineViewOpensThenItJumpsToTheFirstChange()
        {
            var diff = Diff(Script(200), Script(200, 100));
            var viewModel = new InLineViewModel(diff);
            var control = new InLineControl { DataContext = viewModel };
            ShowInWindow(control, 600, 300);

            var firstChange = DiffChangeFinder.FindChangeStarts(viewModel.DiffModel.Lines).First();
            Assert.Greater(control.InlineScroll.VerticalOffset, 0);
            Assert.AreEqual(ExpectedOffset(control.InlineScroll, control.InlinePanel, firstChange), control.InlineScroll.VerticalOffset, Tolerance);
        }

        #endregion
    }
}
