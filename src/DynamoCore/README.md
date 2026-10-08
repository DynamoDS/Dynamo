# DynamoCore

The cross-platform graph model and execution host (`DynamoCore.csproj`, built by both `DynamoCore.sln` and `Dynamo.All.sln`). It owns workspaces, nodes, connectors, the scheduler and the bridge to the DesignScript VM. It is **not** the UI (WPF views live in `src/DynamoCoreWpf/`), not the language runtime (`src/Engine/`), and not the built-in node libraries (`src/Libraries/`).

## Layout

- `Models/` — `DynamoModel.cs` (central model, startup, `Logger`), `DynamoModelCommands.cs` and `RecordableCommands.cs` (command pattern used by UI and tests)
- `Graph/` — `Nodes/NodeModel.cs` (node base class), `Nodes/Attributes.cs` (`NodeName`, `AlsoKnownAs`, `IsDesignScriptCompatible`), `Workspaces/WorkspaceModel.cs` and `HomeWorkspaceModel.cs`, `.dyn` JSON serialization in `Workspaces/SerializationConverters.cs`; also connectors, notes, groups (`Annotations/`), presets
- `Engine/` — graph-to-VM bridge: `EngineController.cs` (run/execution), `CodeGeneration/AstBuilder.cs` (graph → DesignScript AST), `LiveRunnerServices.cs`, NodeToCode, code completion
- `Scheduler/` — `DynamoScheduler.cs` and the async tasks it runs (e.g. `UpdateGraphAsyncTask.cs`)
- `Library/` and `Graph/Nodes/NodeLoaders/`, `Graph/Nodes/ZeroTouch/` — `LibraryServices.cs` imports zero-touch assemblies; the loaders and `DSFunction` turn their methods into nodes
- `Configuration/` (`PreferenceSettings.cs`, `PathManager.cs`), `Search/`, `Extensions/`, `Linting/`, `Logging/`, `Migration/`
- `Properties/Resources.resx` — user-facing strings for this assembly

**Depends on** (`ProjectReference`): all five `src/Engine/` projects, `src/DynamoUtilities`, `src/NodeServices` (DynamoServices) and two projects under `src/Libraries/` — `DesignScriptBuiltin` and `VMDataBridge`. Most other `src/Libraries/` projects reference DynamoCore, not the reverse.

## Tests

`test/DynamoCoreTests/` — partly mirrors this folder (`Engine/`, `Graph/`, `Models/`, `Configuration/`, `Migration/`, …); node-model tests are in `test/DynamoCoreTests/Nodes/`. The `--filter` commands in [AGENTS.md § Running a Single Test](../../AGENTS.md#running-a-single-test) already target `DynamoCoreTests.csproj`. Run tests on Windows (see [Build Commands](../../AGENTS.md#build-commands), "CI vs local").

## Contracts

- **Public API:** this project carries `PublicAPI.Shipped.txt` / `PublicAPI.Unshipped.txt`. Every new or changed public member goes in `PublicAPI.Unshipped.txt`; CI fails on undeclared changes. See [Blast Radius](../../AGENTS.md#blast-radius).
- **Packages:** `DynamoCore.dll` ships in the `DynamoVisualProgramming.Core` NuGet package (`tools/NuGet/template-nuget/`) and reaches external integrators.
- **`.dyn` format:** `NodeModel` constructors and serialization are the saved-graph contract (`doc/dyn-file-spec.md`). Node attributes and the two-constructor rule: [Node Registration Patterns](../../AGENTS.md#node-registration-patterns).
- Proof before a PR: [After Changes — Proof Checklist](../../AGENTS.md#after-changes--proof-checklist).
