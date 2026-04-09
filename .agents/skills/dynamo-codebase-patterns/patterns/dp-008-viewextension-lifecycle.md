---
id: "dp-008"
name: "IViewExtension: store ViewLoadedParams, unsubscribe everything in Dispose()"
status: "confirmed"
domain: "ViewExtensions"
canonical_file: "src/LintingViewExtension/LintingViewExtension.cs"
added: "2026-03-03"
last_verified: "2026-03-03"
sightings: 3
seen_in: ["scan:DynamoCoreWpf"]
---

## Intent
Store the `ViewLoadedParams` reference as a field in `Loaded()` and unsubscribe every event handler registered there in `Dispose()`.

## Why non-obvious
`Loaded()` is the only guaranteed entry point to Dynamo's UI infrastructure (menus, sidebar, `DynamoViewModel`, `DynamoModel`). A developer who doesn't store the reference loses access to all of these for the extension's remaining lifetime. More critically: event handlers registered in `Loaded()` on `MenuItem`, `viewLoadedParams`, and model services are not automatically cleaned up when the extension is disposed. The extension object persists as a GC root held by those events, leaking memory and firing handlers after logical shutdown.

## Correct form
```csharp
public override void Loaded(ViewLoadedParams viewLoadedParams)
{
    this.viewLoadedParamsReference = viewLoadedParams; // store it

    this.linterMenuItem = new MenuItem { Header = Resources.MenuItemText, IsCheckable = true };
    this.linterMenuItem.Checked += MenuItemCheckHandler;       // remember to unsubscribe
    this.linterMenuItem.Unchecked += MenuItemUnCheckedHandler; // remember to unsubscribe

    viewLoadedParams.ViewExtensionOpenRequest += OnViewExtensionOpenRequest; // remember to unsubscribe
    this.linterManager.PropertyChanged += OnLinterManagerPropertyChange;     // remember to unsubscribe
}

public override void Dispose()
{
    // Mirror of Loaded() — every += has a matching -=
    if (linterMenuItem != null)
    {
        linterMenuItem.Checked -= MenuItemCheckHandler;
        linterMenuItem.Unchecked -= MenuItemUnCheckedHandler;
    }
    viewLoadedParamsReference?.ViewExtensionOpenRequest -= OnViewExtensionOpenRequest;
    linterManager?.PropertyChanged -= OnLinterManagerPropertyChange;
}
```

## Anti-pattern
```csharp
public override void Loaded(ViewLoadedParams viewLoadedParams)
{
    // Not stored — can't unsubscribe later, can't access services after this method returns
    viewLoadedParams.ViewExtensionOpenRequest += OnViewExtensionOpenRequest;
    linterManager.PropertyChanged += OnLinterManagerPropertyChange;
}

public override void Dispose()
{
    // Can't unsubscribe — memory leak, handlers fire after disposal
}
```

## When it applies
Every `IViewExtension` implementation.

**Known violation:** `GraphNodeManagerViewExtension` subscribes to `graphNodeManagerMenuItem.Checked` and `graphNodeManagerMenuItem.Unchecked` in `Loaded()` but never unsubscribes them in `Dispose()` — a live memory leak in the codebase. PRs touching that file should flag it.

## Related patterns
- dp-009
