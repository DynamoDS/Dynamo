namespace Dynamo.PythonMigration.Differ
{
    public enum State
    {
        NoChanges,
        HasChanges,
        Error
    }

    public interface IDiffViewViewModel
    {
        ViewMode ViewMode { get; }
        State DiffState { get; set; }
    }

    public enum ViewMode
    {
        Inline,
        SideBySide
    }
}
