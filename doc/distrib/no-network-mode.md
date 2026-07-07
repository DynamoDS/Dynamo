# `--NoNetworkMode` Startup Contract

This document describes what Dynamo's `--NoNetworkMode` command-line flag guarantees at startup,
what it cannot control, and how the WebView2 startup surfaces are hardened. It reflects the
implementation from DYN-10642 (execution plan from DYN-8973).

## What `--NoNetworkMode` is

`--NoNetworkMode` is a **CLI-only, process-lifetime** boolean. It is parsed in
`StartupUtils.CommandLineArguments` and stored on `DynamoModel.NoNetworkMode`. It is **not** a
`PreferenceSettings` value and cannot be toggled in a running session; it is surfaced read-only in
Preferences. Hosts (Revit, FormIt, Civil3D, ...) pass it through `IStartConfiguration.NoNetworkMode`.

## Dynamo first-party network sources (gated)

When `--NoNetworkMode` is set, every Dynamo-authored outbound source is already suppressed:

| Surface | Gate location |
| --- | --- |
| Greg / Package Manager | `PackageManagerExtension` (`NoNetworkModeHandler` + compatibility-map skip) |
| LaunchDarkly feature flags | `DynamoModel` (`DynamoFeatureFlags.exe` is not launched) |
| ADP analytics | `DynamoModel` (`Analytics.DisableAnalytics`) |
| IDSDK auth | `StartupUtils` (`AuthProvider` set to null) |
| Notification Center | `NotificationCenterController` (WebView2 not initialized) |
| Connectivity probe | `DynamoViewModel.CheckOnlineAccess` (short-circuits); `NetworkUtilities.InitInternetCheck()` is also skipped so no `HttpClient` is allocated |
| Usage reporting | `UsageReportingManager` |
| CER crash-report upload | `CrashReportTool.ShowCrashErrorReportWindow` (returns early; the local backup crash dialog still shows) |
| ML data pipeline | `DynamoMLDataPipelineExtension.Startup` (auth providers not wired) |

## WebView2 / Microsoft Edge runtime traffic (hardened here)

Dynamo hosts several WebView2 controls at startup (SplashScreen, HomePage, Library,
DocumentationBrowser). The hosted Microsoft Edge runtime performs background networking by default
(component/feature updates, domain reliability, sync, translate, etc.) that Dynamo's first-party
gates cannot reach.

DYN-10642 centralizes a no-network WebView2 policy in `WebView2Utilities`:

- `WebView2Utilities.NoNetworkAdditionalBrowserArguments` — the Edge command-line switches applied.
- `WebView2Utilities.ApplyNoNetworkPolicy(creationProperties, noNetworkMode, logFn)` — the single
  entry point every startup surface calls after building its `CoreWebView2CreationProperties`.

The switches applied when `--NoNetworkMode` is on:

```
--disable-background-networking --disable-component-update --disable-domain-reliability
--disable-sync --disable-translate --disable-default-apps --no-pings
--disable-features=OptimizationGuideModelDownloading,MediaRouter
```

> **Why not `--disable-features=NetworkService`?** Disabling `NetworkService` prevents the Edge
> runtime from rendering even local (`NavigateToString` / virtual-host-mapped) content, which every
> startup surface depends on. The switches above stop the runtime's *own* outbound traffic while
> leaving local rendering intact.

### Startup ordering fix

The SplashScreen is the first WebView2 surface shown and is constructed **before** the
`DynamoModel` exists. The no-network state is therefore threaded into the `SplashScreen` constructor
(`SplashScreen(bool enableSignInButton, bool noNetworkMode)`) from
`DynamoCoreSetup.RunApplication`, so the hardened policy is applied before the splash Edge runtime
starts. Surfaces created after the model exists (HomePage, Library, DocumentationBrowser) read
`NoNetworkMode` from the model / `ViewStartupParams`.

## Known limitations (out of process scope)

These are OS-managed and cannot be controlled by an in-process gate:

- Microsoft Edge WebView2 **updater service** (separate OS process).
- Autodesk **Identity (IDSDK) service** process.
- SmartScreen / certificate-revocation behavior configured at the OS/Edge policy level.

In-process CER crash-report upload **is** now gated (`CrashReportTool` returns before invoking the
external CER tool when `NoNetworkMode` is true), leaving only the local crash dialog. Any OS-level
crash telemetry outside Dynamo's process is out of scope.

## Verifying

1. Launch `DynamoSandbox.exe --NoNetworkMode` and let it sit idle on the home workspace.
2. Capture the process tree (including child `msedgewebview2.exe`) with Wireshark / Process Monitor,
   filtering out loopback (`127.0.0.1`, `::1`).
3. Confirm no outbound connections to third-party hosts.
4. Repeat without the flag to produce a delta of the normal (network-enabled) baseline.
