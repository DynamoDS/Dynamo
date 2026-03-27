---
id: "dp-003"
name: "DynamoWebView2 initialization sequence"
status: "confirmed"
domain: "WebView2"
canonical_file: "src/LibraryViewExtensionWebView2/LibraryViewController.cs"
added: "2026-03-03"
last_verified: "2026-03-03"
sightings: 3
seen_in: ["scan:LibraryViewExtensionWebView2"]
---

## Intent
All WebView2 usage in Dynamo must go through `DynamoWebView2` (not raw `WebView2`), initialized via `Initialize()` and configured via `ConfigureSettings()` in that order — never via `EnsureCoreWebView2Async()` directly.

## Why non-obvious
Microsoft's WebView2 docs show `EnsureCoreWebView2Async()` as the standard initialization entry point. `DynamoWebView2` wraps this with disposal-safe logic: it captures the init task so that if `Dispose()` is called during async initialization, it can wait for init to complete before tearing down (avoiding a race condition that crashes in tests and during rapid window close). Calling `EnsureCoreWebView2Async()` directly bypasses this protection. `ConfigureSettings()` must follow `Initialize()` because `CoreWebView2` is null until init completes — calling it before throws `InvalidOperationException`.

## Correct form
```csharp
// 1. Always instantiate DynamoWebView2, never raw WebView2
internal DynamoWebView2 browser;

// 2. In the async init method:
await browser.Initialize(LogToDynamoConsole);          // not EnsureCoreWebView2Async
browser.ConfigureSettings(enableZoomControl: true);    // must follow Initialize()

// 3. Then register host objects and set up events
this.browser.CoreWebView2.AddHostObjectToScript("bridgeTwoWay", twoWayScriptingObject);
```

## Anti-pattern
```csharp
// Wrong: raw WebView2
WebView2 browser = new WebView2();

// Wrong: calling the base method directly
await browser.EnsureCoreWebView2Async(); // bypasses disposal-safe init task capture

// Wrong: configuring before init
browser.ConfigureSettings(enableZoomControl: true); // CoreWebView2 is null here — throws
await browser.Initialize();
```

## When it applies
Everywhere in the codebase that hosts a WebView2 component. `DynamoWebView2` is defined in `src/DynamoCoreWpf/Utilities/WebView2Utilities.cs`.

## Related patterns
- dp-004
- dp-005
