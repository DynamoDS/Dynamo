namespace Dynamo.PythonMigration.Differ
{
    public interface IDiffViewViewModel
    {
        ViewMode ViewMode { get; }
    }

    public enum ViewMode
    {
        Inline,
        SideBySide
    }
}
