namespace Dynamo.Graph.Workspaces.Locking
{
    /// <summary>
    /// Describes the outcome of trying to acquire a graph lock.
    /// </summary>
    internal enum GraphLockOutcome
    {
        Opened,
        Cancelled,
        RedirectedToCopy
    }
}
