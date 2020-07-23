using DiffPlex.DiffBuilder;
using Dynamo.Core;
using Dynamo.PythonMigration.Differ;
using Dynamo.PythonMigration.MigrationAssistant;
using PythonNodeModels;

namespace Dynamo.PythonMigration
{
    internal class PythonMigrationAssistantViewModel : NotificationObject
    {
        public string OldCode { get; set; }
        public string NewCode { get; set; }
        private PythonNode PythonNode { get; set; }

        private IDiffViewViewModel currentViewModel;
        public IDiffViewViewModel CurrentViewModel
        {
            get { return currentViewModel; }
            set { currentViewModel = value; RaisePropertyChanged(nameof(CurrentViewModel)); }
        }

        public PythonMigrationAssistantViewModel(PythonNode pythonNode)
        {
            PythonNode = pythonNode;
            OldCode = pythonNode.Script;
            MigrateCode();
            SetSideBySideViewModel();
        }

        private void MigrateCode()
        {
            NewCode = ScriptMigrator.MigrateCode(OldCode);
        }

        public void ChangeCode()
        {
            PythonNode.MigrateCode(NewCode);
        }

        internal void ChangeViewModel(ViewMode viewMode)
        {
            switch (viewMode)
            {
                case Differ.ViewMode.Inline:
                    SetInlineViewModel();
                    break;
                case Differ.ViewMode.SideBySide:
                    SetSideBySideViewModel();
                    break;
                default:
                    break;
            }
        }

        private void SetSideBySideViewModel()
        {
            var sidebyside = new SideBySideDiffBuilder();
            var sidebysideModel = sidebyside.BuildDiffModel(OldCode, NewCode);
            CurrentViewModel = new SideBySideViewModel(sidebysideModel);
        }

        private void SetInlineViewModel()
        {
            var inline = new InlineDiffBuilder();
            var inlineModel = inline.BuildDiffModel(OldCode, NewCode);
            CurrentViewModel = new InLineViewModel(inlineModel);
        }
    }
}
