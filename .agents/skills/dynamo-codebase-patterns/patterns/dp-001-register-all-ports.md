---
id: "dp-001"
name: "RegisterAllPorts() after manual port collection changes"
status: "confirmed"
domain: "DynamoCore/Nodes"
canonical_file: "src/DynamoCore/Graph/Nodes/CustomNodes/Function.cs"
added: "2026-03-03"
last_verified: "2026-03-03"
sightings: 3
seen_in: ["scan:DynamoCore/Nodes"]
---

## Intent
After manually adding or removing entries from `InPorts` or `OutPorts`, always call `RegisterAllPorts()` to finalize port geometry, serialization state, and connections.

## Why non-obvious
A developer familiar with `ObservableCollection` will assume that adding a `PortModel` to `InPorts` is sufficient — the collection change fires, the UI updates. It is not. `RegisterAllPorts()` performs a second pass that sets up port geometry, wires port-to-node back-references, and marks the node for proper serialization. Skipping it produces ports that appear visually but behave incorrectly: they won't serialize, connections break silently, and the graph can corrupt on save/load.

## Correct form
```csharp
public void UpdatePortsForUnresolved(PortModel[] inputs, PortModel[] outputs)
{
    InPorts.Clear();
    for (int i = 0; i < inputs.Length; i++)
        InPorts.Add(new PortModel(PortType.Input, this, new PortData(inputs[i].Name, inputs[i].ToolTip)));

    OutPorts.Clear();
    for (int i = 0; i < outputs.Length; i++)
        OutPorts.Add(new PortModel(PortType.Output, this, new PortData(outputs[i].Name, outputs[i].ToolTip)));

    RegisterAllPorts(); // required
}
```

## Anti-pattern
```csharp
// Missing RegisterAllPorts() — ports appear in UI but won't serialize correctly
InPorts.Clear();
foreach (var input in inputs)
    InPorts.Add(new PortModel(PortType.Input, this, new PortData(input.Name, input.ToolTip)));
```

## When it applies
Any time you directly mutate `InPorts` or `OutPorts` on a `NodeModel` subclass outside of attribute-driven initialization.

## Related patterns
- dp-002
