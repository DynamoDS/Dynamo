using Dynamo.Graph.Workspaces.Locking;

/// <summary>
/// Defines the UI decision point used when a graph lock conflict is detected
/// </summary>
internal interface IGraphLockUserPrompt
{
    /// <summary>
    /// Asks the user whether to cancel opening the locked graph or save a copy to open instead.
    /// </summary>
    /// <param name="graphPath">The path of the graph that is already locked.</param>
    /// <param name="existingLock">The existing lock metadata, or null if the lock file could not be read.</param>
    /// <param name="isStale">Whether the existing lock appears stale.</param>
    /// <returns>The user's graph-lock decision.</returns>
    GraphLockUserResponse AskUser(string graphPath, GraphLockInfo existingLock, bool isStale);
}
