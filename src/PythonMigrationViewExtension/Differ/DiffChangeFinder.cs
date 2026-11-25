using System.Collections.Generic;
using DiffPlex.DiffBuilder.Model;

namespace Dynamo.PythonMigration.Differ
{
    /// <summary>
    /// Groups the lines of a diff into changes: a change is a run of neighbouring lines that are
    /// not unchanged (added, deleted, modified or blank spacer lines).
    /// </summary>
    internal static class DiffChangeFinder
    {
        /// <summary>
        /// The row where each change starts, in order.
        /// </summary>
        /// <param name="lines">The rows of the pane.</param>
        /// <param name="otherLines">For side-by-side, the rows of the other pane: a row is changed when
        /// either side is. Null for a single pane.</param>
        internal static List<int> FindChangeStarts(IList<DiffPiece> lines, IList<DiffPiece> otherLines = null)
        {
            var starts = new List<int>();
            if (lines == null)
            {
                return starts;
            }

            var inChange = false;
            for (var row = 0; row < lines.Count; row++)
            {
                var changed = IsChanged(lines[row]) ||
                              (otherLines != null && row < otherLines.Count && IsChanged(otherLines[row]));
                if (changed && !inChange)
                {
                    starts.Add(row);
                }

                inChange = changed;
            }

            return starts;
        }

        private static bool IsChanged(DiffPiece line) => line != null && line.Type != ChangeType.Unchanged;
    }
}
