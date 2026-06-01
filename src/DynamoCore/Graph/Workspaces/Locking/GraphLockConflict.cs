namespace Dynamo.Graph.Workspaces.Locking
{
    /// <summary>
    /// Describes the result of trying to acquire a graph lock
    /// </summary>
    internal enum GraphLockConflict
    {
        Acquired,
        Cancelled,
        Unavailable
    }

    /// <summary>
    /// Contains the outcome of a graph-lock acquisition attempt
    /// </summary>
    internal sealed record GraphLockAcquireResult(GraphLockConflict Conflict, string GraphPath = null, GraphLockInfo ExistingLock = null)
    {
        internal bool ShouldOpen => Conflict is GraphLockConflict.Acquired or GraphLockConflict.Unavailable;

        // Creates a successful graph-lock result
        internal static GraphLockAcquireResult Acquired(string graphPath = null)
        {
            return new GraphLockAcquireResult(GraphLockConflict.Acquired, graphPath);
        }

        // Creates a cancelled graph-lock result
        internal static GraphLockAcquireResult Cancelled(GraphLockInfo existingLock)
        {
            return new GraphLockAcquireResult(GraphLockConflict.Cancelled, ExistingLock: existingLock);
        }

        // Creates a result for lock-file access failures
        internal static GraphLockAcquireResult Unavailable(string graphPath = null)
        {
            return new GraphLockAcquireResult(GraphLockConflict.Unavailable, graphPath);
        }
    }

    /// <summary>
    /// Contains the user's graph-lock conflict decision
    /// </summary>
    internal sealed record GraphLockUserResponse(string SaveAsPath = null)
    {
        internal bool ShouldSaveAs => !string.IsNullOrEmpty(SaveAsPath);

        // Creates a response for a cancelled graph-lock prompt
        internal static GraphLockUserResponse Cancel()
        {
            return new GraphLockUserResponse();
        }

        // Creates a response for saving the locked graph as a copy
        internal static GraphLockUserResponse SaveAs(string saveAsPath)
        {
            return new GraphLockUserResponse(saveAsPath);
        }
    }
}
