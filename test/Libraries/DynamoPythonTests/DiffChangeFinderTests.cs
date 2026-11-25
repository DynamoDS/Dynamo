using System.Collections.Generic;
using System.Linq;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using Dynamo.PythonMigration.Differ;
using NUnit.Framework;

namespace DynamoPythonTests
{
    /// <summary>
    /// Tests for how the Python review window groups the lines of a diff into changes,
    /// which is what the previous/next change arrows step through.
    /// </summary>
    [TestFixture]
    [Category("UnitTests")]
    public class DiffChangeFinderTests
    {
        #region Helpers

        private static List<DiffPiece> Rows(params ChangeType[] types)
        {
            return types.Select((type, i) => new DiffPiece("line " + i, type, i + 1)).ToList();
        }

        private static string Script(int lineCount, params int[] changedLines)
        {
            var lines = Enumerable.Range(1, lineCount)
                .Select(n => changedLines.Contains(n) ? $"value_{n} = {n * 1000}" : $"value_{n} = {n}");
            return string.Join("\n", lines);
        }

        private static SideBySideDiffModel Diff(string before, string after)
        {
            return new SideBySideDiffBuilder().BuildDiffModel(before, after, false);
        }

        #endregion

        [Test]
        public void WhenLinesAreNullThenNoChangesAreFound()
        {
            CollectionAssert.IsEmpty(DiffChangeFinder.FindChangeStarts(null));
        }

        [Test]
        public void WhenNothingChangedThenNoChangesAreFound()
        {
            var rows = Rows(ChangeType.Unchanged, ChangeType.Unchanged, ChangeType.Unchanged);

            CollectionAssert.IsEmpty(DiffChangeFinder.FindChangeStarts(rows));
        }

        [Test]
        public void WhenChangedRowsAreNeighboursThenTheyAreOneChange()
        {
            var rows = Rows(
                ChangeType.Unchanged,
                ChangeType.Deleted,
                ChangeType.Inserted,
                ChangeType.Modified,
                ChangeType.Unchanged);

            CollectionAssert.AreEqual(new[] { 1 }, DiffChangeFinder.FindChangeStarts(rows));
        }

        [Test]
        public void WhenChangesAreSeparatedByUnchangedRowsThenEachIsFound()
        {
            var rows = Rows(
                ChangeType.Unchanged,
                ChangeType.Modified,
                ChangeType.Unchanged,
                ChangeType.Unchanged,
                ChangeType.Inserted,
                ChangeType.Inserted,
                ChangeType.Unchanged,
                ChangeType.Deleted);

            CollectionAssert.AreEqual(new[] { 1, 4, 7 }, DiffChangeFinder.FindChangeStarts(rows));
        }

        [Test]
        public void WhenFirstAndLastRowsChangedThenBothAreFound()
        {
            var rows = Rows(ChangeType.Modified, ChangeType.Unchanged, ChangeType.Unchanged, ChangeType.Modified);

            CollectionAssert.AreEqual(new[] { 0, 3 }, DiffChangeFinder.FindChangeStarts(rows));
        }

        [Test]
        public void WhenRowIsABlankSpacerThenItCountsAsAChange()
        {
            // A spacer (Imaginary) row is where the other side has a line this side does not.
            var rows = Rows(ChangeType.Unchanged, ChangeType.Imaginary, ChangeType.Unchanged);

            CollectionAssert.AreEqual(new[] { 1 }, DiffChangeFinder.FindChangeStarts(rows));
        }

        [Test]
        public void WhenOnlyTheOtherPaneChangedThenTheRowStillCountsAsAChange()
        {
            var after = Rows(ChangeType.Unchanged, ChangeType.Unchanged, ChangeType.Unchanged);
            var before = Rows(ChangeType.Unchanged, ChangeType.Deleted, ChangeType.Unchanged);

            CollectionAssert.AreEqual(new[] { 1 }, DiffChangeFinder.FindChangeStarts(after, before));
        }

        [Test]
        public void WhenOtherPaneIsShorterThenMissingRowsAreTreatedAsUnchanged()
        {
            var after = Rows(ChangeType.Unchanged, ChangeType.Unchanged, ChangeType.Modified);
            var before = Rows(ChangeType.Unchanged);

            CollectionAssert.AreEqual(new[] { 2 }, DiffChangeFinder.FindChangeStarts(after, before));
        }

        [Test]
        public void WhenRealScriptHasSpreadOutEditsThenEachEditIsAChange()
        {
            // Lines 20, 100 and 190 are edited: rows 19, 99 and 189 (rows count from zero).
            var diff = Diff(Script(200), Script(200, 20, 100, 190));

            var starts = DiffChangeFinder.FindChangeStarts(diff.NewText.Lines, diff.OldText.Lines);

            CollectionAssert.AreEqual(new[] { 19, 99, 189 }, starts);
        }

        [Test]
        public void WhenRealScriptHasDeletedAndAddedLinesThenBothSidesFindTheSameChanges()
        {
            var before = string.Join("\n", "a = 1", "b = 2", "c = 3", "d = 4", "e = 5", "f = 6");
            var after = string.Join("\n", "a = 1", "c = 3", "d = 4", "e = 5", "new = 0", "f = 6");
            var diff = Diff(before, after);

            var fromAfter = DiffChangeFinder.FindChangeStarts(diff.NewText.Lines, diff.OldText.Lines);
            var fromBefore = DiffChangeFinder.FindChangeStarts(diff.OldText.Lines, diff.NewText.Lines);

            Assert.AreEqual(2, fromAfter.Count);
            CollectionAssert.AreEqual(fromAfter, fromBefore);
        }

        [Test]
        public void WhenInlineViewShowsOldAndNewLineTogetherThenTheyAreOneChange()
        {
            // The single-pane view lists the old line (removed) right above the new line (added).
            var diff = Diff(Script(10), Script(10, 5));
            var inline = new InLineViewModel(diff);

            var starts = DiffChangeFinder.FindChangeStarts(inline.DiffModel.Lines);

            // The old line is marked removed and the new line stays marked modified; both belong to one change.
            Assert.AreEqual(1, starts.Count);
            Assert.AreEqual(ChangeType.Deleted, inline.DiffModel.Lines[starts[0]].Type);
            Assert.AreEqual(ChangeType.Modified, inline.DiffModel.Lines[starts[0] + 1].Type);
        }
    }
}
