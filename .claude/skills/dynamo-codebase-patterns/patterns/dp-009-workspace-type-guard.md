---
id: "dp-009"
name: "Guard against CustomNodeWorkspaceModel before creating extension UI"
status: "confirmed"
domain: "ViewExtensions"
canonical_file: "src/NodeAutoCompleteViewExtension/NodeAutoCompleteViewExtension.cs"
added: "2026-03-03"
last_verified: "2026-03-03"
sightings: 3
seen_in: ["scan:DynamoCoreWpf"]
---

## Intent
Before accessing `HomeWorkspaceModel`-specific APIs from a view extension, check that `CurrentWorkspaceModel` is a `HomeWorkspaceModel` — if not, call `this.Closed()` and return.

## Why non-obvious
Dynamo has two workspace types: `HomeWorkspaceModel` (the main graph) and `CustomNodeWorkspaceModel` (opened when editing a custom node). `CurrentWorkspaceModel` can be either at the time an extension activates. A developer building an extension assumes there is one workspace type. Attempting to cast to `HomeWorkspaceModel`, accessing home-workspace-specific services, or adding sidebar UI when a custom node workspace is active causes `InvalidCastException` or broken UI state that persists after the user returns to the home workspace.

## Correct form
```csharp
internal void AddToSidebar()
{
    if (!(this.viewLoadedParamsReference.CurrentWorkspaceModel is HomeWorkspaceModel))
    {
        this.Closed(); // signal: this extension cannot operate here
        return;
    }

    this.viewLoadedParamsReference?.AddToExtensionsSideBar(this, DependencyView);
}
```

## Anti-pattern
```csharp
internal void AddToSidebar()
{
    // Wrong: assumes CurrentWorkspaceModel is always HomeWorkspaceModel
    var homeWorkspace = (HomeWorkspaceModel)viewLoadedParamsReference.CurrentWorkspaceModel; // throws
    viewLoadedParamsReference.AddToExtensionsSideBar(this, DependencyView);
}
```

## When it applies
Only when your extension accesses `HomeWorkspaceModel`-specific APIs (casting `CurrentWorkspaceModel` to `HomeWorkspaceModel`, accessing home-workspace services or state). Extensions that only add generic sidebar UI or menu items do not need this check — most extensions in the codebase don't implement it. Call the check at the point of API access, not in `Loaded()`.

## Related patterns
- dp-008
