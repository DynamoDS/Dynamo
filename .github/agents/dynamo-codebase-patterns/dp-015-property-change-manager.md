---
id: "dp-015"
name: "Use PropertyChangeManager with a using block to suppress PropertyChanged notifications"
status: "confirmed"
domain: "DynamoCore/Nodes"
canonical_file: "src/DynamoCore/Core/PropertyChangeManager.cs"
added: "2026-03-03"
last_verified: "2026-03-03"
sightings: 3
seen_in: ["scan:DynamoCore/Core"]
---

## Intent
When setting multiple properties on a `NotificationObject` (including `NodeModel`) that would each fire `PropertyChanged`, suppress them for the duration of the batch using `SetPropsToSuppress()` in a `using` block.

## Why non-obvious
`NotificationObject` inherits `PropertyChangeManager` as an internal field — it is not visible on the public API. A developer who needs to suppress `PropertyChanged` during a batch update would either set a manual flag, unsubscribe and re-subscribe the event handler, or not suppress at all and accept redundant notifications. The `using`-block suppressor exists and is the correct approach, but there is nothing in the type signatures to suggest it.

Note: `PropertyChangeManager` suppresses `PropertyChanged` (WPF binding notifications). It is distinct from `RaisesModificationEvents`, which suppresses `OnNodeModified()` / scheduler re-execution. For batch port operations, both may be needed — see dp-002.

## Correct form
```csharp
// Suppress PropertyChanged for "IsFrozen" during a batch update
using (node.PropertyChangeManager.SetPropsToSuppress("IsFrozen"))
{
    node.IsFrozen = true;
    // ... other related changes
}
// PropertyChanged for "IsFrozen" fires again after the using block exits
```

## Anti-pattern
```csharp
// Wrong: fires PropertyChanged (and triggers WPF binding updates) for every assignment
node.IsFrozen = true;
node.SomeOtherProp = value; // each one re-evaluates bindings independently
```

```csharp
// Also wrong: manual unsubscribe/resubscribe is error-prone and misses other subscribers
node.PropertyChanged -= handler;
node.IsFrozen = true;
node.PropertyChanged += handler;
```

## When it applies
Any time multiple properties on a `NodeModel` (or any `NotificationObject`) are being set in a batch where intermediate `PropertyChanged` notifications would trigger redundant or incorrect UI updates. Common cases: deserialization, undo/redo replay, programmatic state resets.

## Related patterns
- dp-002
