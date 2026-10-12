# Libraries (built-in nodes)

The node libraries that ship with Dynamo: zero-touch assemblies (public static C# methods become nodes) and explicit `NodeModel` projects (custom UI, dynamic ports, multi-output). The graph model, scheduler and node base classes are **not** here (`src/DynamoCore/`); neither is the language runtime (`src/Engine/`).

## Projects

- **Zero-touch:** `CoreNodes/` (assembly `DSCoreNodes`: `List.cs`, `Math.cs`, `String.cs`, `Color.cs`, `FileSystem.cs`, …), `Analysis/`, `GeometryColor/`, `Tesellation/` (folder spelling; project `Tessellation.csproj`), `DSOffice/` (Excel and CSV), `DynamoUnits/` (project `UnitsCore.csproj`, assembly `DynamoUnits`)
- **Explicit `NodeModel`:** `CoreNodeModels/` (`Input/`, `Logic/`, `HigherOrder/`, `Watch.cs`, `Formula.cs`, …), `PythonNodeModels/`, `UnitsNodeModels/`, `Watch3DNodeModels/`, `GeometryUI/`
- **WPF views for those nodes** (`<UILib>true</UILib>`, Windows-only, built only in `Dynamo.All.sln`): `CoreNodeModelsWpf/`, `PythonNodeModelsWpf/`, `GeometryUIWpf/`, `UnitsUI/`, `Watch3DNodeModelsWpf/`
- **Low-level support:** `DesignScriptBuiltin/` (DesignScript built-ins such as `Dictionary.cs`) and `VMDataBridge/` (callbacks from the VM into node models). Both sit *below* DynamoCore: `DesignScriptBuiltin` is referenced by `src/Engine/ProtoCore` and DynamoCore, `VMDataBridge` by DynamoCore. `DSOfficeUtilities/` and `DynamoConversions/` are helpers for the other libraries.

## Adding or changing a node

- Zero-touch: a public static method in e.g. `CoreNodes/List.cs`; the namespace becomes the library category, remapped per namespace in `CoreNodes/DSCoreNodes_DynamoCustomization.xml`; renamed nodes get a hint in `CoreNodes/DSCoreNodes.Migrations.xml`.
- Explicit: subclass `NodeModel` in `CoreNodeModels/` (see `CoreNodeModels/Logic/If.cs`); its WPF view, if any, goes in `CoreNodeModelsWpf/NodeViewCustomizations/`.
- Required attributes, the two constructors and `[AlsoKnownAs]`: [AGENTS.md § Node Registration Patterns](../../AGENTS.md#node-registration-patterns).
- User-facing strings go in the project's `.resx` (e.g. `CoreNodes/Properties/Resources.resx`).
- Every new node needs `.dyn`, `.md` and `.jpg` help files: author them in `doc/distrib/NodeHelpFiles/en-US/` (other locales are localized copies). See [New Nodes](../../AGENTS.md#new-nodes).

## Tests

Per-library projects in `test/Libraries/`: `CoreNodesTests/`, `AnalysisTests/`, `GeometryColorTests/`, `TessellationTests/`, `DynamoPythonTests/`, `DynamoMSOfficeTests/`, `DataBridgeTests/`. Many `CoreNodeModels` nodes are tested in `test/DynamoCoreTests/Nodes/`; Watch3D in `test/VisualizationTests/`. Command form: [AGENTS.md § Running a Single Test](../../AGENTS.md#running-a-single-test) with the library's test `.csproj`, on Windows.

## Contracts

- No project here carries `PublicAPI.*.txt`, but these assemblies are published: `DynamoVisualProgramming.DynamoCoreNodes` (Analysis, GeometryColor, DSCoreNodes, CoreNodeModels, Watch3DNodeModels, UnitsNodeModels), `.Core` (DesignScriptBuiltin, VMDataBridge), `.WpfUILibrary` (CoreNodeModelsWpf) and `.ZeroTouchLibrary` (DynamoUnits) — nuspecs in `tools/NuGet/template-nuget/`.
- Node names, namespaces and port layouts are referenced by saved `.dyn` graphs. See [Blast Radius](../../AGENTS.md#blast-radius).
