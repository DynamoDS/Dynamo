using DiffPlex.DiffBuilder.Model;
using System.Collections.Generic;
using System.Linq;

namespace Dynamo.PythonMigration.Differ
{
    public class InLineViewModel : IDiffViewViewModel
    {
        public ViewMode ViewMode { get; set; }
        public DiffPaneModel DiffModel { get; set; }
        public bool HasChanges { get { return DiffModel.HasDifferences; } }

        private State diffState;
        public State DiffState
        {
            get
            {
                if (diffState == State.Error) return State.Error;

                diffState = HasChanges ? State.HasChanges : State.NoChanges;

                return diffState;
            }
            set { diffState = value; }
        }

        public InLineViewModel(SideBySideDiffModel diffModel)
        {
            ViewMode = ViewMode.Inline;
            DiffModel = ConvertToInline(diffModel);
        }

        /// <summary>
        /// Converts the side by side diff representation into an inline one.
        /// This is different to standard DiffPlex Inline behaviour as it retains
        /// all changed fragments in a line, indicating each character that's changed.
        /// </summary>
        /// <param name="diffModel">The diff model to convert</param>
        /// <returns>The combined DiffPaneModel that can be used in an inline diff display.</returns>
        private DiffPaneModel ConvertToInline(SideBySideDiffModel diffModel)
        {
            var tempDiff = new DiffPaneModel();
            var newDiff = new DiffPaneModel();

            // first process old lines
            tempDiff.Lines.AddRange(ProcessOldLines(diffModel.OldText));

            // then we add all new lines, which also contain the unchanged ones, etc
            tempDiff.Lines.AddRange(diffModel.NewText.Lines);

            // order the lines by their position indicator
            // so any lines with changes show the old then new line one below the other
            newDiff.Lines.AddRange(tempDiff.Lines.OrderBy(x => x.Position).ToList());

            return newDiff;
        }

        /// <summary>
        /// Filter and modify which lines from old text should be included in the inline diff.
        /// </summary>
        /// <param name="oldTextModel">The diff model for the old text.</param>
        /// <returns>The list of diff pieces that represent the old text to display.</returns>
        private IEnumerable<DiffPiece> ProcessOldLines(DiffPaneModel oldTextModel)
        {
            var modifiedOldLines = oldTextModel.Lines.Where(ShouldAddOldLine);
            foreach (var oldLine in modifiedOldLines)
            {
                // mark all the old lines as deleted otherwise 
                // they show up with added (+) line indicator
                oldLine.Type = ChangeType.Deleted;
            }
            return modifiedOldLines;
        }

        /// <summary>
        /// Determine if a line from old text should be used in the inline diff.
        /// Disregards any lines that haven't changed.
        /// </summary>
        /// <param name="piece">The piece to evaluate.</param>
        /// <returns>True if it should be included, false otherwise.</returns>
        private bool ShouldAddOldLine(DiffPiece piece)
        {
            if (piece.Type == ChangeType.Unchanged ||
               piece.Type == ChangeType.Imaginary)
                return false;

            return true;
        }
    }
}
