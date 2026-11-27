# Open Geometry proof of concept

This is the first testable slice of the SMLib geometry stack for Windows x64. It builds a fork-owned `ProtoGeometry.dll` and a C ABI bridge. The NUnit tests use Dynamo's DesignScript engine and render-package interface. The Sandbox startup and saved `.dyn` integration are future steps.

## Run the tests

Use Git, CMake 3.24 or newer, MSVC x64 with the Windows SDK, Visual Studio MSBuild with .NET 10 support and the .NET 10 SDK. The CMake generator defaults to Visual Studio 2022. Pass `-CMakeGenerator` if you use a different installed generator with x64 support.

From the Dynamo repository root, point the test script to a checkout of NVIDIA's `usd-brep` at the commit in [usd-brep.ref](native/usd-brep.ref). Keep its `source/SMLib` and `source/SM_API` directories clean. A sibling `usd-brep` checkout is the default.

```powershell
.\tools\OpenGeometry\Test-OpenGeometry.ps1 -UsdBrepSource ..\usd-brep
```

The script builds SMLib, SM_API and the bridge from source into `bin/open-geometry/native/Release`. It uses Visual Studio MSBuild for the engine's resource generation, then runs NUnit via `dotnet test`. The native build uses the pinned sources with the optional parallel and TBB allocator paths disabled. It needs no OpenUSD or Python packages. The test process loads the three native libraries and the Visual C++ runtime.

The fixture subtracts a radius-2 cylinder from a centered 10 × 10 × 10 cuboid. It checks the analytic volume, reuse of both original solids after disposing of the result, and an open bore in Dynamo's triangle render package. A DesignScript test imports the replacement by its managed identity and checks the volume on both graph branches.
