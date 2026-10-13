# Engine (DesignScript runtime)

The DesignScript language: parser, compilers, virtual machine and runners. `src/DynamoCore/` drives it — it builds an AST from the graph and executes it through these projects. This folder does **not** contain nodes (`src/Libraries/`), the graph model (`src/DynamoCore/`) or any UI. All five projects build in `DynamoCore.sln`.

## Projects

- `ProtoCore/` — the core: AST (`Parser/AST.cs`, `AssociativeAST.cs`, `ImperativeAST.cs`), the VM in `DSASM/` (`Executive.cs`, `Interpreter.cs`, `Heap.cs`), function dispatch in `Lang/` with replication in `Lang/Replication/`, .NET interop for zero-touch calls in `FFI/`, AST visitors in `SyntaxAnalysis/`, and `Core.cs` / `RuntimeCore.cs`
- `ProtoAssociative/` — associative-language code generator (`CodeGen.cs`, `CodeGen_SSA.cs`)
- `ProtoImperative/` — imperative-block code generator and executive
- `ProtoScript/` — runners in `Runners/`; `LiveRunner.cs` is what DynamoCore's `LiveRunnerServices` uses
- `GraphLayout/` — standalone auto-layout algorithm (no project references), used by `src/DynamoCore/Graph/Workspaces/LayoutExtensions.cs`

**Parser:** `ProtoCore/Parser/Parser.cs` and `Scanner.cs` are Coco/R output. A grammar change edits `Parser/atg/*.atg` and regenerates with `Parser/GenerateParser.bat`; past grammar fixes commit the `.atg` and the regenerated `.cs` files together.

**Dependencies** (`ProjectReference`): `ProtoCore` references `src/DynamoUtilities`, `src/NodeServices` (DynamoServices) and `src/Libraries/DesignScriptBuiltin`. `ProtoAssociative`, `ProtoImperative` and `ProtoScript` build on `ProtoCore`. Nothing here references DynamoCore. DynamoCore references all five, and many `src/Libraries/` projects reference `ProtoCore` directly.

## Tests

- `test/Engine/ProtoTest/` — language and VM tests, grouped like the runtime (`Associative/`, `Imperative/`, `DSASM/`, `FFITests/`, `ParserTest/`, `LiveRunnerTests/`, …)
- `test/Engine/ProtoTestFx/` — shared test harness; `test/Engine/FFITarget/` — .NET types that FFI tests call into
- Graph-level engine behaviour (AST building, `EngineController`): `test/DynamoCoreTests/Engine/`

Use the `dotnet test <project>.csproj --filter ...` form from [AGENTS.md § Running a Single Test](../../AGENTS.md#running-a-single-test) with `test/Engine/ProtoTest/ProtoTest.csproj`, on Windows.

## Contracts

- No project here carries `PublicAPI.*.txt` files, but `ProtoCore.dll` ships in the `DynamoVisualProgramming.Core` NuGet package (`tools/NuGet/template-nuget/`).
- Changes here ripple into every evaluation path, including how existing graphs evaluate. See the cross-boundary rules in [Blast Radius](../../AGENTS.md#blast-radius). The [Proof Checklist](../../AGENTS.md#after-changes--proof-checklist) lists the build and the `DynamoCoreTests` run for `src/Engine/` changes; `ProtoTest` covers the language itself.
