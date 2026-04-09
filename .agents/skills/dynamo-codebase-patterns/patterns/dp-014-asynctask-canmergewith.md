---
id: "dp-014"
name: "Override CanMergeWithCore() on new AsyncTask types to prevent queue explosion"
status: "candidate"
domain: "DynamoCore/Scheduler"
canonical_file: "src/DynamoCore/Scheduler/UpdateGraphAsyncTask.cs"
added: "2026-03-03"
last_verified: "2026-03-03"
sightings: 1
seen_in: ["scan:DynamoCore/Core"]
---

## Intent
`AsyncTask` subclasses that are scheduled at high frequency must override `CanMergeWithCore()` with correct merge logic — the default `KeepBoth` allows the task queue to grow unboundedly when tasks are scheduled faster than they execute.

## Why non-obvious
`CanMergeWithCore()` has a safe-looking default (`KeepBoth`) that compiles and runs without error. The problem only manifests under load: rapid graph edits or event floods schedule many tasks that queue up instead of being coalesced. The merge logic is also easy to get wrong — `UpdateGraphAsyncTask` only merges a newer task over an older one if the newer task's changes are a *superset* of the older task's (`CanReplace()`). Naively returning `KeepOther` for any same-type task drops pending additions/deletions that weren't yet included in the newer task's `graphSyncData`.

## Correct form
```csharp
protected override TaskMergeInstruction CanMergeWithCore(AsyncTask otherTask)
{
    var other = otherTask as UpdateGraphAsyncTask;
    if (other == null)
        return base.CanMergeWithCore(otherTask);

    // Only merge if the surviving task's changes are a superset of the dropped task's
    if (other.IsScheduledAfter(this) && other.CanReplace(this))
        return TaskMergeInstruction.KeepOther;
    if (this.IsScheduledAfter(other) && this.CanReplace(other))
        return TaskMergeInstruction.KeepThis;

    return TaskMergeInstruction.KeepBoth;
}

private bool CanReplace(UpdateGraphAsyncTask other)
{
    // First check ModifiedNodes directly — bail early if other has nodes we don't cover
    if (!other.ModifiedNodes.All(ModifiedNodes.Contains))
        return false;

    // Then confirm no adds or deletes are pending in the other task
    return other.graphSyncData.AddedNodeIDs.Count() == 0 &&
           other.graphSyncData.ModifiedNodeIDs.All(graphSyncData.ModifiedNodeIDs.Contains) &&
           other.graphSyncData.DeletedNodeIDs.Count() == 0;
}
```

## Anti-pattern
```csharp
// Wrong: drops queued adds/deletes if any newer task arrives
protected override TaskMergeInstruction CanMergeWithCore(AsyncTask otherTask)
{
    if (otherTask is UpdateGraphAsyncTask)
        return TaskMergeInstruction.KeepOther; // silently loses pending graph changes
    return TaskMergeInstruction.KeepBoth;
}
```

## When it applies
When implementing a new `AsyncTask` subclass that may be scheduled at high frequency (e.g. in response to property change events or UI interactions).

## Related patterns
- dp-006
