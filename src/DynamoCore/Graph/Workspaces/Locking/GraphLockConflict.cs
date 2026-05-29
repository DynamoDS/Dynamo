namespace Dynamo.Graph.Workspaces.Locking
{
    internal enum GraphLockConflict
    {
        Acquired,
        Cancelled,
        Unavailable
    }

    internal sealed record GraphLockAcquireResult(GraphLockConflict Conflict, string GraphPath = null, GraphLockInfo ExistingLock = null)
    {
        internal bool ShouldOpen => Conflict is GraphLockConflict.Acquired or GraphLockConflict.Unavailable;

        internal static GraphLockAcquireResult Acquired(string graphPath = null)
        {
            return new GraphLockAcquireResult(GraphLockConflict.Acquired, graphPath);
        }

        internal static GraphLockAcquireResult Cancelled(GraphLockInfo existingLock)
        {
            return new GraphLockAcquireResult(GraphLockConflict.Cancelled, ExistingLock: existingLock);
        }

        internal static GraphLockAcquireResult Unavailable(string graphPath = null)
        {
            return new GraphLockAcquireResult(GraphLockConflict.Unavailable, graphPath);
        }
    }

    internal enum GraphLockUserDecision
    {
        Cancel,
        SaveAs
    }

    internal sealed record GraphLockUserResponse(GraphLockUserDecision Decision, string SaveAsPath = null)
    {
        internal static GraphLockUserResponse Cancel()
        {
            return new GraphLockUserResponse(GraphLockUserDecision.Cancel);
        }

        internal static GraphLockUserResponse SaveAs(string saveAsPath)
        {
            return new GraphLockUserResponse(GraphLockUserDecision.SaveAs, saveAsPath);
        }
    }
}
