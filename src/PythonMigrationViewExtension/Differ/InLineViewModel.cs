using DiffPlex.DiffBuilder.Model;

namespace Dynamo.PythonMigration.Differ
{
    public class InLineViewModel : IDiffViewViewModel
    {
        public ViewMode ViewMode { get; set; }
        public DiffPaneModel DiffModel { get; set; }

        public InLineViewModel(DiffPaneModel diffModel)
        {
            ViewMode = ViewMode.Inline;
            DiffModel = diffModel;
        }
    }
}
