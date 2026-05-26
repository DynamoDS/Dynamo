---
id: "dp-007"
name: "Task completion handlers run on the scheduler thread"
status: "confirmed"
domain: "DynamoCore/Scheduler"
canonical_file: "src/DynamoCore/Graph/Workspaces/HomeWorkspaceModel.cs"
added: "2026-03-03"
last_verified: "2026-03-03"
sightings: 3
seen_in: ["scan:DynamoCore/Core"]
---

## Intent
Handlers subscribed to `AsyncTask.Completed` run on the scheduler thread, not the UI thread — any UI updates or WPF element access inside them must be marshaled via the dispatcher.

## Why non-obvious
Subscribing to a `Completed` event looks identical to any other .NET event subscription. Every other completion callback pattern in WPF (`BackgroundWorker.RunWorkerCompleted`, `Task.ContinueWith` with a UI scheduler) runs back on the UI thread by default. Dynamo's scheduler does not do this. A handler that touches a WPF control, raises `PropertyChanged` on a bound property, or modifies an `ObservableCollection` directly will either throw a cross-thread exception or silently corrupt state.

## Correct form
```csharp
// Documented in source: "This callback is invoked in the context of ISchedulerThread"
private void OnUpdateGraphCompleted(AsyncTask task)
{
    // Safe: only uses data captured during Initialize(), not live workspace state
    var updateTask = (UpdateGraphAsyncTask)task;
    var workspace = updateTask.TargetedWorkspace;

    // Safe: working with pre-captured node data
    foreach (var warning in warnings)
    {
        var node = workspace.Nodes.FirstOrDefault(n => n.GUID == guid);
        node?.Warning(warning.Value);
    }

    // If UI update needed, marshal explicitly:
    Dispatcher.BeginInvoke(() => SomeUIProperty = newValue);
}
```

## Anti-pattern
```csharp
task.Completed += (t) =>
{
    // Wrong: accessing live UI state from scheduler thread
    var node = dynamoViewModel.CurrentSpaceViewModel.Nodes[0]; // cross-thread exception
    someObservableCollection.Add(item); // silent corruption
};
```

## When it applies
Any code that subscribes to `AsyncTask.Completed` or `EvaluationCompleted` events on workspace models.

## Related patterns
- dp-006
