---
id: "dp-002"
name: "Suppress RaisesModificationEvents during batch NodeModel changes"
status: "candidate"
domain: "DynamoCore/Nodes"
canonical_file: "src/DynamoCore/Graph/Nodes/NodeModel.cs"
added: "2026-03-03"
last_verified: "2026-03-03"
sightings: 3
seen_in: ["scan:DynamoCore/Nodes"]
---

## Intent
Wrap any batch modification of a `NodeModel` (multiple port additions, bulk state changes) in `RaisesModificationEvents = false / true` to prevent `OnNodeModified()` from firing once per change and invoking the scheduler repeatedly.

## Why non-obvious
`OnNodeModified()` triggers graph re-execution via the scheduler. A developer adding multiple ports in a loop has no reason to expect each `InPorts.Add()` fires this — nothing in the `ObservableCollection` API suggests it. In practice, each addition raises the event, scheduling redundant re-executions that degrade performance and can produce intermediate broken states mid-loop. The flag is not documented on the public API; you only discover it by reading `NodeModel` internals or seeing performance issues in tests.

## Correct form
```csharp
public void RegisterAllPorts()
{
    RaisesModificationEvents = false;

    var inportDatas = GetPortDataFromAttributes(PortType.Input);
    if (inportDatas.Any())
        RegisterInputPorts(inportDatas);

    var outPortDatas = GetPortDataFromAttributes(PortType.Output);
    if (outPortDatas.Any())
        RegisterOutputPorts(outPortDatas);

    RaisesModificationEvents = true; // re-enables notifications only — does NOT fire OnNodeModified() itself
    // If a final scheduler update is required, call OnNodeModified() explicitly here.
}
```

## Anti-pattern
```csharp
// Each RegisterInputPorts call fires OnNodeModified, invoking the scheduler
// once per port — unnecessary and potentially unstable mid-registration
var inportDatas = GetPortDataFromAttributes(PortType.Input);
RegisterInputPorts(inportDatas);
var outPortDatas = GetPortDataFromAttributes(PortType.Output);
RegisterOutputPorts(outPortDatas);
```

## When it applies
When writing code inside `NodeModel` that makes multiple sequential mutations. In practice this pattern is almost entirely contained within `NodeModel.RegisterAllPorts()` itself — direct use elsewhere in the codebase is rare. If you are adding multiple ports or resetting bulk state outside of `RegisterAllPorts()`, wrap the block with this flag. For suppressing `PropertyChanged` notifications specifically, use `PropertyChangeManager` instead (see dp-015).

**Note:** No automated tests enforce this pattern. The consequence (redundant scheduler invocations) degrades performance and can cause intermediate broken state, but will not always produce an obvious failure.

## Related patterns
- dp-001
