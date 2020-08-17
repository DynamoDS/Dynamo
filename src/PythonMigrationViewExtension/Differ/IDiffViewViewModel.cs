namespace Dynamo.PythonMigration.Differ
{
    public interface IDiffViewViewModel
    {
        ViewMode ViewMode { get; }
        bool HasChanges { get; }
    }

    public enum ViewMode
    {
        Inline,
        SideBySide
    }
}
