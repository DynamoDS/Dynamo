# Open Geometry for Dynamo

OpenGeometry replaces Autodesk's proprietary ASM / LibG / `ProtoGeometry.dll` stack with a fork-owned `ProtoGeometry.dll` backed by NVIDIA's open-source `usd-brep` BRep solid modeling kernel (`SMLib` + `SM_API`) behind a versioned C ABI bridge (`OpenGeometryBridge.dll`, ABI v11).

The Sandbox integration is opt-in (`-p:UseOpenGeometry=true`), keeping upstream Dynamo's default build profile intact while staging an isolated Windows x64 Sandbox build under `bin/open-geometry/sandbox/Release` with zero ASM, LibG, Autodesk ProtoGeometry, OpenUSD, Python, or TBB dependencies.

## Build and Test Entry Points

Prerequisites: Git, CMake 3.24+, MSVC x64 with Windows SDK, Visual Studio MSBuild with .NET 10 support, and the .NET 10 SDK. Point `-UsdBrepSource` at a clean checkout of NVIDIA `usd-brep` pinned to the commit in [usd-brep.ref](native/usd-brep.ref).

1. **Native + managed geometry & DesignScript unit tests** (Primitives, Transforms, Mirror, Booleans, Curves, Rational NURBS, Ellipse/Helix, Surfaces, `NurbsSurface`, Topology, Extrude/Loft/Sweep/Revolve/Offset/Thicken/Fillet/Chamfer/ThinShell/Split/Trim, Intersections, `Mesh` Booleans/PlaneCut/Section/MakeWatertight/OBJ+STL+PLY I/O, `PanelSurface`, and open-format BRep/Geometry JSON serialization):

```powershell
.\tools\OpenGeometry\Test-OpenGeometry.ps1 -UsdBrepSource ..\usd-brep
```

2. **Opt-in Windows x64 Sandbox build** (isolated staging in `bin/open-geometry/sandbox/Release`):

```powershell
.\tools\OpenGeometry\Build-OpenGeometrySandbox.ps1 -UsdBrepSource ..\usd-brep -CleanStaging
```

3. **Full Sandbox integration, 13-sample Dynamo corpus harness, WPF/Helix preview, live `DynamoSandbox.exe` module audit, and dependency boundary verification**:

```powershell
.\tools\OpenGeometry\Test-OpenGeometrySandbox.ps1 -UsdBrepSource ..\usd-brep
```

4. **Independent `usd-brep` pin check & update rehearsal** (verifies `usd-brep.ref` rejects an unpinned worktree, then builds and runs all geometry tests against an alternate `usd-brep` revision in an isolated temporary worktree without touching the pinned checkout):

```powershell
.\tools\OpenGeometry\Test-OpenGeometryUpdateRehearsal.ps1 -UsdBrepSource ..\usd-brep
```

5. **PublicAPI CI gate on fork `ProtoGeometry.dll`**:

```powershell
dotnet build src/OpenGeometry/ProtoGeometry/ProtoGeometry.csproj -c Release /p:PublicApiAnalyzers=true /warnaserror:RS0016,RS0017
```

## Architecture and Compatibility

- **Native boundary**: `src/OpenGeometry/native/` builds `SMLib.dll`, `SM_API.dll`, and `OpenGeometryBridge.dll` from pinned sources with parallel/TBB allocators disabled and no OpenUSD/Python dependencies. All SMLib calls are serialized behind a process-global mutex, and non-mutating Dynamo semantics are enforced by deep-copying operands before kernel mutators/consumers.
- **Managed geometry**: `src/OpenGeometry/ProtoGeometry/` implements `Autodesk.DesignScript.Geometry` types, Zero-Touch signatures, `IGraphicItem` preview tessellation, localized `Messages.resx` diagnostics, and `PublicAPI.Unshipped.txt` tracking.
- **Compatibility inventory**: See [COMPATIBILITY.md](COMPATIBILITY.md) for type-by-type coverage across Slices A–J, the 13/13 checked-in Dynamo sample corpus results, `DSCore.Data` (`StringifyJSON`/`ParseJSON`/`Remember`) persistence, `Analysis` + `GeometryColor` (`ByGeometryColor`/`BySurfaceColors`/`ByMeshColors`) + `Tessellation` (`Voronoi`/`Delaunay`/`ConvexHull`) support, and tracked gaps (T-Splines, SAT/SAB/SolidDef, downstream binary compatibility).
