namespace Dynamo.Graph.Workspaces.Locking
{
    /// <summary>
    /// Describes the possible outcome of trying to lock a graph.
    /// </summary>
    internal enum GraphLockOutcome
    {
        Opened,
        Cancelled,
        RedirectedToCopy
    }
}
