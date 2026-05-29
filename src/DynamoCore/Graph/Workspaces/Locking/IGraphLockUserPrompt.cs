using Dynamo.Graph.Workspaces.Locking;

internal interface IGraphLockUserPrompt
{
    GraphLockUserResponse AskUser(string graphPath, GraphLockInfo existingLock, bool isStale);
}
