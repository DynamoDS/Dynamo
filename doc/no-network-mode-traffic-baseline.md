# `--NoNetworkMode` Startup Traffic Baseline

This document captures the empirical baseline of outbound network traffic
observed during Dynamo startup with and without `--NoNetworkMode`. The goal is
to distinguish first-party Dynamo traffic from WebView2 / Microsoft Edge
runtime traffic and from OS-level services, so the gating in
[DYN-8973](https://autodesk.atlassian.net/browse/DYN-8973) can target the right
sources.

## Reproduction procedure

1. Use a clean Windows VM with no Dynamo session state. Recommended: Windows
   11 with WebView2 Evergreen Runtime installed and no proxy/AV interception.
2. Install [Wireshark](https://www.wireshark.org/) with npcap and
   [Process Monitor](https://learn.microsoft.com/sysinternals/downloads/procmon).
3. Start Wireshark with capture filter `ip and not host 127.0.0.1`. Apply
   display filter `tcp.flags.syn == 1 and tcp.flags.ack == 0` to focus on
   connection establishment.
4. Start Process Monitor with filter `Operation is TCP Connect` to attribute
   each connection to a PID.
5. Launch `DynamoSandbox.exe --NoNetworkMode` and let the app sit idle on the
   default home workspace for 60 seconds.
6. Stop captures. Export the Wireshark conversation list and the Process
   Monitor TCP Connect events for the `Dynamo.exe`/`DynamoSandbox.exe` process
   tree (including `msedgewebview2.exe`, `DynamoFeatureFlags.exe`,
   `senddmp.exe`, and the IDSDK service).
7. Repeat with `--NoNetworkMode` off and produce a diff against the gated
   run.

## Expected endpoints (from code inspection)

The endpoints below are the static destinations Dynamo first-party code can
reach. They are derived from `App.config` files and constants — not from a
live capture. With `--NoNetworkMode` set, every row in the "First-party" table
is expected to be silent at startup.

### First-party (gated by `NoNetworkMode`)

| Source file                                  | Endpoint                                                                                 | Gate location                                |
| -------------------------------------------- | ---------------------------------------------------------------------------------------- | -------------------------------------------- |
| `src/Notifications/App.config`               | `https://ddehnr4ewobxc.cloudfront.net/dynNotifications.json`                             | `NotificationCenterController.cs:103`        |
| `src/DynamoPackages/App.config`              | `https://www.dynamopackages.com`                                                         | `PackageManagerExtension.cs:168-178`         |
| `src/DynamoCoreWpf/App.config`               | `https://autocomplete.dynamobim.org/autocomplete`, `.../autocompleteclusters`            | Caller-side `NoNetworkMode` checks           |
| `src/DynamoMLDataPipeline/App.config`        | `https://developer.api.autodesk.com/exchange` (stg variant available)                    | `DynamoMLDataPipelineExtension.cs:23-37`     |
| `src/DynamoCore/App.config`                  | IDSDK `IDSDK_CLIENT_ID` (Autodesk Identity Service endpoint)                             | `StartupUtils.cs:443` (null auth provider)   |
| `src/Tools/DynamoFeatureFlags/App.config`    | LaunchDarkly mobile keys                                                                 | `DynamoModel.cs:806-833` (process not spawned) |
| `NetworkUtilities.cs:23`                     | `https://www.google.com`, `https://www.microsoft.com`                                    | `DynamoViewModel.cs:282` (probe), `:947` (gap — see DYN-8973 Task 5) |

### WebView2 / Edge runtime (not gated by Dynamo first-party code)

Each WebView2 surface spawns `msedgewebview2.exe` child processes whose Edge
platform performs background networking by default. None of the surfaces
currently pass `CoreWebView2EnvironmentOptions.AdditionalBrowserArguments` to
suppress these features.

| Surface                  | Initialized at                                                                       | Edge background behavior |
| ------------------------ | ------------------------------------------------------------------------------------ | ------------------------ |
| SplashScreen             | `DynamoCoreSetup.cs:76` (before model)                                               | Component update, SmartScreen, domain reliability, sync, translate |
| HomePage                 | `HomePage.xaml.cs:144-162`                                                           | Same + YouTube embed (passive `Referer` filter) |
| Library                  | `LibraryViewController` (via `ViewExtension.cs:39`)                                  | Same |
| DocumentationBrowser     | `DocumentationBrowserView.xaml.cs:177`                                               | Same |
| NotificationCenter       | `NotificationCenterController.cs:105` (gated)                                        | Not started under `--NoNetworkMode` |
| PackageManagerWizard     | `PackageManagerWizard.xaml.cs:238` (user-initiated)                                  | Same |

Typical Edge background destinations to look for in the capture:

* `*.msedge.net` (component update)
* `*.bing.com`, `*.bingapis.com` (SmartScreen)
* `dl.delivery.mp.microsoft.com` (update delivery)
* `*.googleapis.com` (translate, fonts)
* `optimizationguide-pa.googleapis.com` (Edge ML model downloads)

### OS-level / out of process (out of Dynamo's control)

* Microsoft Edge WebView2 Updater Service (`MicrosoftEdgeUpdate.exe`)
* Autodesk Identity Service / IDSDK background helper
* Windows certificate revocation (CRL/OCSP) checks
* Windows Defender / SmartScreen reputation queries

These are documented as known limitations of `--NoNetworkMode`.

## Captured results

> **TODO (manual):** Fill in the tables below after running the reproduction
> procedure above on a clean VM. Capture sessions were not included with the
> implementation of [DYN-8973](https://autodesk.atlassian.net/browse/DYN-8973)
> because they require a clean Windows VM and live packet capture tools that
> cannot be automated in CI.

### Run A: `DynamoSandbox.exe --NoNetworkMode` (idle 60 s)

| Time (s) | PID | Parent PID | Process            | Destination host | Port | Classification         |
| -------- | --- | ---------- | ------------------ | ---------------- | ---- | ---------------------- |
|          |     |            |                    |                  |      |                        |

### Run B: `DynamoSandbox.exe` (no flag, idle 60 s)

| Time (s) | PID | Parent PID | Process            | Destination host | Port | Classification         |
| -------- | --- | ---------- | ------------------ | ---------------- | ---- | ---------------------- |
|          |     |            |                    |                  |      |                        |

### Delta (Run B − Run A)

> The delta is the set of connections that vanish when `--NoNetworkMode` is
> set. Anything that remains in Run A but is not OS-level is a gap that the
> remaining DYN-8973 tasks must close.

## Related work

* DYN-8973 Tasks 2–4: WebView2 environment factory + plumbing of
  `NoNetworkMode` into every WebView2 init site.
* DYN-8973 Task 5: Guard residual unconditional helpers
  (`NetworkUtilities.InitInternetCheck`, `CrashReportTool.ShowCrashWindow`,
  `DynamoMLDataPipelineExtension.Startup`).
* DYN-8973 Task 6: Regression test that fails on any non-loopback socket
  opened by the Dynamo process tree under `--NoNetworkMode`.
* DYN-8973 Task 7: `doc/distrib/no-network-mode.md` (user-facing contract).
