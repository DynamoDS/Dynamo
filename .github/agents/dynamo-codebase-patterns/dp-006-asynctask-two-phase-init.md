---
id: "dp-006"
name: "AsyncTask two-phase init: capture state in Initialize(), use it in Execute()"
status: "confirmed"
domain: "DynamoCore/Scheduler"
canonical_file: "src/DynamoCore/Scheduler/UpdateGraphAsyncTask.cs"
added: "2026-03-03"
last_verified: "2026-03-03"
sightings: 3
seen_in: ["scan:DynamoCore/Core"]
---

## Intent
All mutable workspace state must be read and captured during `Initialize()` (called on the UI thread); `Execute()` (called on the scheduler thread) must operate only on that captured data.

## Why non-obvious
The `Initialize()` / `Execute()` split looks like a setup/run convention. A developer familiar with `BackgroundWorker` or `Task.Run` would assume live workspace state is safe to read anywhere, since it's all in-memory. In Dynamo, `workspace.Nodes` and similar collections are owned by the UI thread. Reading them from the scheduler thread causes race conditions and intermittent corruption that are very hard to reproduce. The only safe window is `Initialize()`, which the scheduler guarantees runs on the UI thread before queuing the task.

## Correct form
```csharp
internal bool Initialize(EngineController controller, WorkspaceModel workspace)
{
    // Called on UI thread — safe to access workspace.Nodes directly
    engineController = controller;
    TargetedWorkspace = workspace;
    ModifiedNodes = ComputeModifiedNodes(workspace); // capture now
    graphSyncData = engineController.ComputeSyncData(workspace.Nodes, ModifiedNodes, verboseLogging);

    // Clear dirty flags here, not in Execute()
    foreach (var nodeGuid in graphSyncData.NodeIDs)
    {
        var node = workspace.Nodes.FirstOrDefault(n => n.GUID.Equals(nodeGuid));
        node?.ClearDirtyFlag();
    }
    return true;
}

protected override void HandleTaskExecutionCore()
{
    // Called on scheduler thread — only uses data captured above
    if (!engineController.IsDisposed)
        engineController.UpdateGraphImmediate(graphSyncData);
}
```

## Anti-pattern
```csharp
protected override void HandleTaskExecutionCore()
{
    // Wrong: accessing workspace state on scheduler thread
    var nodes = workspace.Nodes; // race condition
    foreach (var node in nodes)
        node.UpdateValue(); // corruption
}
```

## When it applies
Any new `AsyncTask` subclass that needs to read workspace, node, or connector state.

## Related patterns
- dp-007
