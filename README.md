# Open Geometry for Dynamo (`open-geometry-dynamo`)

> **Unofficial Fork Notice:** This repository is an independent, community-maintained fork of [Dynamo](https://github.com/DynamoDS/Dynamo). It is **not affiliated with, endorsed by, or sponsored by Autodesk, Inc. or NVIDIA Corporation**.

**Open Geometry for Dynamo** is a fork of [Dynamo](https://github.com/DynamoDS/Dynamo) that replaces Autodesk's proprietary geometry stack (**ASM**, **LibG**, and the closed-source **Autodesk `ProtoGeometry.dll`**) with open-source geometry kernels.

Currently, the fork backs Dynamo's geometry API with NVIDIA's open-source [`usd-brep`](https://github.com/NVIDIA-Omniverse/usd-brep) BRep solid modeling kernel (`SMLib` + `SM_API`, Apache 2.0) behind a versioned C ABI bridge (`OpenGeometryBridge.dll`), keeping the managed Dynamo layer decoupled from the underlying native geometry kernel.

## What Changes in This Fork

- **Open-Source Native Geometry Kernel (`src/OpenGeometry/native/`):** Builds `SMLib.dll`, `SM_API.dll`, and `OpenGeometryBridge.dll` (currently ABI v12) from an external, commit-pinned checkout of [`NVIDIA-Omniverse/usd-brep`](https://github.com/NVIDIA-Omniverse/usd-brep) ([`usd-brep.ref`](src/OpenGeometry/native/usd-brep.ref)) with parallel/TBB allocators disabled and zero OpenUSD or Python runtime dependencies.
- **Fork-Owned Managed Geometry (`src/OpenGeometry/ProtoGeometry/`):** Implements Dynamo's `Autodesk.DesignScript.Geometry` Zero-Touch API surface, `IGraphicItem` preview tessellation, and open-format geometry JSON serialization as an unsigned, fork-owned `ProtoGeometry.dll` (`Product="OpenGeometry for Dynamo"`, `OpenGeometry.Kernel=SMLib`).
- **Opt-In Sandbox Build Profile (`-p:UseOpenGeometry=true`):** Leaves upstream Dynamo's default build profile intact while staging an isolated Windows x64 Sandbox build under `bin/open-geometry/sandbox/Release` with **zero ASM, LibG, DynamoShapeManager, or Autodesk `ProtoGeometry.dll` dependencies** (verified by `DependencyBoundaryTests`).
- **Tracked Gaps & Unsupported Features:**
  - **T-Splines (`Autodesk.DesignScript.Geometry.TSpline.*`) are not supported:** SMLib does not include a T-Spline subdivision engine, and the 10 `TSpline` types (83 documented members) are omitted from the fork-owned `ProtoGeometry.dll`. Core BRep, NURBS, curve, surface, mesh, and panelization workflows—and all checked-in Dynamo sample graphs—are unaffected, but the following will **not** work:
    - Legacy `.dyn` graphs containing T-Spline nodes (they load as unresolved `DummyNode`s) and DesignScript/Python scripts referencing `Autodesk.DesignScript.Geometry.TSpline.*`.
    - Freeform subdivision-cage modeling workflows (e.g., `BuildPipes`, `BuildFromLines`, cage extrusion/bevel/bridge/crease/symmetry ops, and `ToBRep`/`ToMesh`).
    - Proprietary `.tsm` / `.tss` file import, export, and serialization.
    - The `Geometry > TSpline` library category / `EnableTSpline` toggle and upstream Dynamo tests that specifically assert on T-Spline nodes (`HelixImageComparisonTests`, `PythonEditTests`, `SearchSideEffects`, `NodeAutoCompleteSearchTests`).
  - **Proprietary ACIS/ASM persistence formats (`SAT`, `SAB`, `SolidDef`) are not supported:** Exposed signatures throw explicit `NotSupportedException` diagnostics; use open-format JSON (`ToJson`/`FromJson`), `.obj`/`.stl`/`.ply` meshes, or native SMLib `.smb` instead.

See [src/OpenGeometry/README.md](src/OpenGeometry/README.md) for architecture details and [src/OpenGeometry/COMPATIBILITY.md](src/OpenGeometry/COMPATIBILITY.md) for the full type-by-type compatibility inventory, sample corpus results, and tracked gaps.

## Quick Start (Build & Test OpenGeometry)

### Prerequisites (Windows x64)

- Git
- CMake 3.24+
- Visual Studio 2022 (MSVC x64 toolset, Windows SDK, MSBuild with .NET 10 support)
- [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
- A clean checkout of [`NVIDIA-Omniverse/usd-brep`](https://github.com/NVIDIA-Omniverse/usd-brep) checked out at the commit pinned in [`src/OpenGeometry/native/usd-brep.ref`](src/OpenGeometry/native/usd-brep.ref)

### Run Tests and Build the OpenGeometry Sandbox

```powershell
# 1. Build the native bridge + managed ProtoGeometry and run geometry & DesignScript unit tests
.\tools\OpenGeometry\Test-OpenGeometry.ps1 -UsdBrepSource ..\usd-brep

# 2. Build and stage the isolated OpenGeometry Dynamo Sandbox (bin/open-geometry/sandbox/Release)
.\tools\OpenGeometry\Build-OpenGeometrySandbox.ps1 -UsdBrepSource ..\usd-brep -CleanStaging

# 3. Run full Sandbox integration, sample/JSON corpus, WPF preview, and dependency boundary tests
.\tools\OpenGeometry\Test-OpenGeometrySandbox.ps1 -UsdBrepSource ..\usd-brep
```

For additional developer commands (including `usd-brep` update rehearsal and PublicAPI analyzer checks), see [src/OpenGeometry/README.md](src/OpenGeometry/README.md).

## Licensing, Attribution, and Trademarks

### 1. Fork & Upstream Dynamo License (Apache 2.0)

- This repository is derived from [DynamoDS/Dynamo](https://github.com/DynamoDS/Dynamo) and is licensed under the **Apache License, Version 2.0** (see [LICENSE.txt](LICENSE.txt)).
- Upstream copyright notices (`Copyright © Autodesk, Inc.` and contributors) and third-party attributions are preserved in [LICENSE.txt](LICENSE.txt) and [ABOUT.txt](ABOUT.txt).
- **Notice of Modifications (Apache 2.0 §4(b)):** In addition to new code added under `src/OpenGeometry/`, `test/OpenGeometry/`, and `tools/OpenGeometry/`, this fork modifies upstream Dynamo build files (`Directory.Build.props`, `Directory.Packages.props`, `src/Config/CS_SDK.props`, and selected `.csproj` files) and host/library integration files (`src/DynamoApplications/StartupUtils.cs`, `src/DynamoApplications/PathResolvers.cs`, `src/DynamoCore/Scheduler/UpdateRenderPackageAsyncTask.cs`, `src/DynamoCoreWpf/Views/SplashScreen/SplashScreen.xaml.cs`, `src/Libraries/CoreNodes/Data.cs`, and this `README.md`) to support the opt-in `-p:UseOpenGeometry=true` profile. The complete change history is recorded in Git.

### 2. Open-Source Geometry Kernel (`NVIDIA-Omniverse/usd-brep`)

- The OpenGeometry native bridge builds against `source/SMLib` and `source/SM_API` from [NVIDIA-Omniverse/usd-brep](https://github.com/NVIDIA-Omniverse/usd-brep), which are licensed under the **Apache License, Version 2.0** (`Copyright (c) 2020-2026 NVIDIA CORPORATION & AFFILIATES`).
- This repository **does not vendor or redistribute** `usd-brep` source code or compiled binaries; contributors and users supply a separate checkout pinned to [`src/OpenGeometry/native/usd-brep.ref`](src/OpenGeometry/native/usd-brep.ref).
- **Binary Distribution Notice:** If you distribute compiled binaries that bundle `SMLib.dll` or `SM_API.dll`, you must include a copy of NVIDIA's Apache License 2.0 text and copyright notices in accordance with Section 4 of the Apache License 2.0.

### 3. Clean-Room API Implementation & Excluded Proprietary Components

- Autodesk's closed-source `LibG`, `ASM`, `DynamoShapeManager`, and Autodesk-built `ProtoGeometry.dll` are **not** included in this repository and are **not** linked, restored, or loaded by the OpenGeometry build profile (`-p:UseOpenGeometry=true`). Those proprietary components remain governed by Autodesk's license terms ([`doc/distrib/Autodesk.rtf`](doc/distrib/Autodesk.rtf)) if used in upstream/default builds.
- The fork-owned implementation in [`src/OpenGeometry/ProtoGeometry/`](src/OpenGeometry/ProtoGeometry/) was authored against Dynamo's publicly available Apache-2.0 checked-in node documentation (`doc/distrib/NodeHelpFiles/`), sample graphs (`doc/distrib/Samples/`), and test graphs (`test/core/json/`), without decompiling or copying Autodesk's closed-source geometry binaries.
- The fork-owned `ProtoGeometry.dll` is intentionally unsigned (`PublicKeyToken=null`) and carries `Company="OpenGeometry"` and `Product="OpenGeometry for Dynamo"` metadata to distinguish it unambiguously from Autodesk-built assemblies.

### 4. Trademarks & Non-Affiliation (Apache 2.0 §6)

- The Apache License 2.0 does not grant permission to use the trade names, trademarks, service marks, or product names of Autodesk, Inc., NVIDIA Corporation, or other contributors, except as required for reasonable and customary use in describing the origin of the work and reproducing attribution notices.
- **Autodesk**, **Dynamo**, and **ProtoGeometry** are trademarks or product names of Autodesk, Inc. **NVIDIA** and **Omniverse** are trademarks of NVIDIA Corporation. All other trademarks belong to their respective owners.
- The C# namespace `Autodesk.DesignScript.Geometry`, the assembly filename `ProtoGeometry.dll`, and JSON `$typeid` identifiers (`autodesk.geometry:*`, `autodesk.math:*`) are retained strictly for nominative interoperability with existing `.dyn` graphs and DesignScript code.

### 5. Third-Party & Inherited Upstream Assets (Important for Binary Redistribution)

- Upstream Dynamo references and bundles third-party libraries under a variety of open-source licenses (including MIT, BSD, Apache 2.0, PSF, and LGPL). See [ABOUT.txt](ABOUT.txt) for upstream third-party credits and license terms.
- **Proprietary Font Notice:** Upstream `DynamoDS/Dynamo` includes font files under `extern/Autodesk/Artifakt-Element-Font/` whose [`license.txt`](extern/Autodesk/Artifakt-Element-Font/license.txt) restricts use of the Artifakt font software to authorized Autodesk business purposes. If you package or redistribute standalone binaries built from this fork, you should exclude or replace those font files and verify compliance with all applicable notices in [ABOUT.txt](ABOUT.txt) and `usd-brep`'s `LICENSE`.

## Upstream Dynamo Resources

For general Dynamo architecture, graph format specifications, and upstream documentation:

- [Upstream Dynamo Repository](https://github.com/DynamoDS/Dynamo)
- [Dynamo Wiki](https://github.com/DynamoDS/Dynamo/wiki)
- [Dynamo Developer Resources](https://developer.dynamobim.org/)
- [Dynamo Forum](https://forum.dynamobim.com)
- [Contributing Guidelines (Upstream)](CONTRIBUTING.md)
