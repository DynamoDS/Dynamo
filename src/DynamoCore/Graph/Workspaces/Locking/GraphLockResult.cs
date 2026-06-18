namespace Dynamo.Graph.Workspaces.Locking
{
    /// <summary>
    /// Contains the outcome of a graph-lock acquisition attempt
    /// </summary>
    internal sealed record GraphLockAcquireResult(GraphLockOutcome Conflict, string GraphPath = null, GraphLockInfo ExistingLock = null)
    {
        internal bool ShouldOpen => Conflict is GraphLockOutcome.Opened or GraphLockOutcome.RedirectedToCopy;

        // Creates a successful graph-lock result
        internal static GraphLockAcquireResult Acquired(string graphPath = null)
        {
            return new GraphLockAcquireResult(GraphLockOutcome.Opened, graphPath);
        }

        // Creates a cancelled graph-lock result
        internal static GraphLockAcquireResult Cancelled(GraphLockInfo existingLock)
        {
            return new GraphLockAcquireResult(GraphLockOutcome.Cancelled, ExistingLock: existingLock);
        }

        // Creates a result for lock-file access failures
        internal static GraphLockAcquireResult Unavailable(string graphPath = null)
        {
            return new GraphLockAcquireResult(GraphLockOutcome.RedirectedToCopy, graphPath);
        }
    }
}
