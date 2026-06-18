namespace Dynamo.Graph.Workspaces.Locking
{
    /// <summary>
    /// Contains the user's graph-lock conflict decision.
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
