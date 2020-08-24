﻿using DiffPlex.DiffBuilder.Model;

namespace Dynamo.PythonMigration.Differ
{
    public class SideBySideViewModel : IDiffViewViewModel
    {
        public ViewMode ViewMode { get; set; }
        public SideBySideDiffModel DiffModel { get; set; }
        public DiffPaneModel AfterPane { get { return DiffModel.NewText; } }
        public DiffPaneModel BeforePane { get { return DiffModel.OldText; } }
        public bool HasChanges { get { return DiffModel.NewText.HasDifferences | DiffModel.OldText.HasDifferences; } }

        public SideBySideViewModel(SideBySideDiffModel diffModel)
        {
            ViewMode = ViewMode.SideBySide;
            DiffModel = diffModel;
        }
    }
}
