# AGENTS.md

This file provides guidance to AI agents when working with code in this repository.

## Project

Dynamo is a visual programming tool accessible to non-programmers and programmers alike. Users can visually script behavior, define custom logic, and script using textual programming languages. Built with C# and WPF, targeting .NET 10. The core engine (`DynamoCore.sln`) is cross-platform; the full UI (`Dynamo.All.sln`) is Windows-only.

- **Language**: C# (.NET 10)
- **UI**: WPF (Windows only)
- **Tests**: NUnit
- **Build (Windows full)**: `msbuild src/Dynamo.All.sln /p:Configuration=Release`
- **Build (core only)**: `dotnet build src/DynamoCore.sln -c Release`
- **Test**: `dotnet test` or Visual Studio Test Explorer

## Build Commands

```bash
# Windows full build
dotnet restore src/Dynamo.All.sln --runtime=win-x64 -p:Configuration=Release -p:DotNet=net10.0
msbuild src/Dynamo.All.sln /p:Configuration=Release

# CI parity (what build_dynamo_all.yml runs) — PublicAPI analyzers become errors
msbuild src/Dynamo.All.sln /p:Configuration=Release /warnAsError:RS0016,RS0017 /p:PublicApiAnalyzers=true

# Core only (cross-platform, Windows)
dotnet restore src/DynamoCore.sln --runtime=win-x64 -p:Configuration=Release -p:DotNet=net10.0
msbuild src/DynamoCore.sln /p:Configuration=Release

# Core only (Linux)
dotnet restore src/DynamoCore.sln --runtime=linux-x64 -p:Configuration=Release -p:Platform=NET_Linux -p:DotNet=net10.0
dotnet build src/DynamoCore.sln -c Release /p:Platform=NET_Linux
```

**Static analysis (the lint step):** Dynamo has no separate linter. Static checks are the Roslyn analyzers the build runs — security rules CA2327/CA2328/CA2329/CA2330 are always errors; PublicAPI rules RS0016/RS0017 are errors only with the CI flags above (a plain local build only warns). Formatting rules live in `.editorconfig`. Run the CI-parity build before opening a PR.

**CI vs local:** `build_dynamo_all.yml` (Windows) runs the CI-parity build; `build_dynamo_core.yml` builds `DynamoCore.sln` on Linux but runs no tests there — `dotnet test` currently discovers no tests on Linux. Run `dotnet test` on Windows.

## Architecture

```
src/
├── DynamoCore/          # Cross-platform graph engine, scheduler, AST evaluation
├── DynamoCoreWpf/       # WPF UI, node views, workspace canvas
├── DynamoApplications/  # Application host entry points
├── DynamoSandbox/       # Standalone sandbox app
├── Engine/              # DesignScript language runtime (ProtoCore, ProtoAssociative, etc.)
├── Libraries/           # Built-in node libraries (CoreNodes, Analysis, PythonNodeModels, etc.)
├── Extensions/          # View extensions (Documentation Browser, Library, Linting, etc.)
└── DynamoCLI/           # Headless command-line runner
test/
├── DynamoCoreTests/     # Core engine tests
├── DynamoCoreWpfTests/  # UI tests (split across Wpf, Wpf2, Wpf3 projects)
└── Libraries/           # Per-library test projects (CoreNodesTests, etc.)
```

Key relationships: `DynamoCore` is the graph model and execution engine. `DynamoCoreWpf` depends on it for UI. The `Engine/` DesignScript runtime (`ProtoCore`) is the low-level evaluator that `DynamoCore` drives. Node libraries in `Libraries/` expose zero-touch or explicit node models consumed by both. The full UI (`Dynamo.All.sln`) is Windows-only; `DynamoCore.sln` builds cross-platform (Windows, Linux, macOS).

Area READMEs (layout, entry points, tests, contracts): [`src/DynamoCore/README.md`](src/DynamoCore/README.md), [`src/Engine/README.md`](src/Engine/README.md), [`src/Libraries/README.md`](src/Libraries/README.md).

## Key Conventions

- Follow [Dynamo Coding Standards](https://github.com/DynamoDS/Dynamo/wiki/Coding-Standards) and [Naming Standards](https://github.com/DynamoDS/Dynamo/wiki/Naming-Standards).
- XML documentation required on all public methods and properties.
- New public APIs must be added to `PublicAPI.Unshipped.txt` (format: `namespace.ClassName.MemberName -> ReturnType`).
- Security analyzers CA2327/CA2329/CA2330/CA2328 are errors. Never commit secrets, API keys, or credentials.
- NUnit for all tests. Do not introduce xUnit or MSTest. NUnit packages: `NUnit`, `NUnit3TestAdapter`, `NUnit.Analyzers`.
- Test naming: `WhenConditionThenExpectedBehavior` for new tests (the legacy suite predates this convention — match the surrounding file's style when extending old tests). One behavior per test, Arrange-Act-Assert.
- User-facing strings in `.resx` files.
- NuGet package versions for `PackageReference` projects live only in `Directory.Packages.props` (Central Package Management); legacy `packages.config` projects (e.g. `tools/DSTestCaseConverter`) are outside CPM and keep their versions locally. `PackageReference` entries in csproj files must omit `Version`; bump a dependency by editing the props file. Conditions selecting *which* package to reference (e.g. LibG Debug/Release in `DynamoCore.csproj`) stay in the csproj.
- No files > 50 MB.
- Preserve existing line endings when editing — do not convert. The `.editorconfig` specifies LF, but many files have CRLF from Windows development. New files should follow `.editorconfig` (LF); the Write tool on macOS may need explicit attention.

## Running a Single Test

```bash
# Filter by test name (substring match)
dotnet test test/DynamoCoreTests/DynamoCoreTests.csproj --filter "Name~MyTestClass"

# Filter by NUnit category
dotnet test test/DynamoCoreTests/DynamoCoreTests.csproj --filter "Category=UnitTests"

# Combine with & (AND) or | (OR)
dotnet test test/DynamoCoreTests/DynamoCoreTests.csproj --filter "Name~WhenCondition&Category=UnitTests"
```

UI tests are split across `DynamoCoreWpfTests`, `DynamoCoreWpf2Tests`, and `DynamoCoreWpf3Tests`.

## Node Registration Patterns

**Zero-touch** (static methods) — preferred for pure computation. Place static methods in a class under `src/Libraries/`. The namespace becomes the library category. Use XML `<search>` tags for keywords and `[IsVisibleInDynamoLibrary(false)]` to hide helpers:

```csharp
/// <summary>Brief description.</summary>
/// <param name="x">Input description.</param>
/// <returns>Output description.</returns>
/// <search>keyword1,keyword2</search>
public static double MyFunction(double x) { ... }
```

**Explicit NodeModel** — required for custom UI, dynamic ports, or multi-output nodes. Inherit from `NodeModel` in `src/Libraries/CoreNodeModels/`. Two constructors are required — a `[JsonConstructor]` private one (deserialization) and a public parameterless one (creation):

```csharp
[NodeName("Display Name")]
[NodeCategory("Category.Subcategory")]
[NodeDescription("DescKey", typeof(Resources))]
[InPortNames("x"), InPortTypes("double")]
[OutPortTypes("bool")]
[IsDesignScriptCompatible]
public class MyNode : NodeModel
{
    [JsonConstructor]
    private MyNode(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts)
        : base(inPorts, outPorts) { }

    public MyNode() { /* AddPorts(); RegisterAllPorts(); */ }

    public override IEnumerable<AssociativeNode> BuildOutputAst(
        List<AssociativeNode> inputAstNodes) { ... }
}
```

Use `[AlsoKnownAs("OldName")]` to preserve backward compatibility when renaming nodes.

## Quality Gates

- No `[Ignore]` or `[Explicit]` on tests without an explaining comment.
- No empty `catch { }` blocks — log and rethrow or handle explicitly.
- No `#pragma warning disable` without a justification comment.
- No weakened assertions (e.g., replacing `Assert.AreEqual` with `Assert.IsNotNull`).
- Do not introduce new network connections without explicit documentation and no-network mode testing.
- Do not add data collection without proper user consent checks and documentation.

## Commits and PRs

- Commit message: short summary (50 chars max), blank line, detailed body (72 char wrap). Optionally reference Jira: `DYN-1234`.
- PR title must include Jira ticket: `DYN-1234 concise summary`.
- Fill all sections of `.github/PULL_REQUEST_TEMPLATE.md`. Release Notes is mandatory — use `N/A` if not user-facing (minimum 6 words otherwise).
- Do not introduce breaking API changes without filing an issue and following [Semantic Versioning](https://github.com/DynamoDS/Dynamo/wiki/Dynamo-Versions).

## New Nodes

For each new node, add to `doc/distrib/NodeHelpFiles/`:
- A `.dyn` sample graph
- A `.md` documentation file
- A `.jpg` visual preview

## Debugging Quick Start

- **Logs**: `DynamoLogger` writes `dynamoLog_<guid>.txt` to `%AppData%\Dynamo\Dynamo Core\<major>.<minor>\Logs\` (headless/CLI runs use the version-less parent). The in-app log viewer is under View > Log. `DynamoModel.Logger` (`src/DynamoCore/Models/DynamoModel.cs`) is the logging surface: `Log`, `LogWarning`, `LogError`, `LogInfo`.
- **Attach a debugger**: launch `DynamoSandbox` (or `DynamoSandbox.exe` from `bin\AnyCPU\Debug`) and attach VS to the process. `--NoNetworkMode` disables network surfaces for repro isolation (see [no-network-mode.md](doc/distrib/no-network-mode.md)).
- **Node evaluation issues**: first breakpoints are `EngineController` (`src/DynamoCore/Engine/EngineController.cs`) for run/execution and `AstBuilder` (`src/DynamoCore/Engine/CodeGeneration/AstBuilder.cs`) for graph-to-DS compilation.
- **Common failures**: build breaks after SDK/dependency bumps are usually NuGet source issues — `dynamo-nuget.config` points at the `team-dynamo-nuget` Artifactory feed; a 403 there is a credentials gate, not a code problem. WPF/UI projects only build on Windows (`Dynamo.All.sln`); on Linux/macOS build `DynamoCore.sln` with `/p:Platform=NET_Linux`.

## Blast Radius

- **Public API**: `src/*/PublicAPI.{Shipped,Unshipped}.txt` (DynamoCore, DynamoCoreWpf, DynamoUtilities, NodeServices) — Roslyn analyzers RS0016/RS0017 fail **CI** on undeclared changes (CI passes `/warnAsError:RS0016,RS0017 /p:PublicApiAnalyzers=true`; a plain local build only warns). Breaking changes require an issue + SemVer.
- **Published NuGet packages** (from `tools/NuGet/template-nuget/`): `DynamoVisualProgramming.Core`, `.DynamoCoreNodes`, `.DynamoServices`, `.DynamoSamples`, `.Tests`, `.WpfUILibrary`, `.ZeroTouchLibrary`. Changes to `src/DynamoCore`, `src/DynamoCoreWpf`, or `src/Libraries` land in these packages and reach external consumers (e.g. DynamoRevit, downstream package authors).
- **Graph file format**: `.dyn` files are a public contract — schema documented in `doc/dyn-file-spec.md` (JSON Schema: `doc/dyn-file-spec.json`). Changes to node serialization (`NodeModel` constructors, `AlsoKnownAs` handling) affect every saved graph.
- **Cross-boundary edits**: `src/Engine/` (DesignScript runtime) changes ripple into every evaluation path; `src/Libraries/` node changes require matching `doc/distrib/NodeHelpFiles/` entries; `extern/` submodules pin native dependencies (LibG/ASM) — version bumps there are coordinated PRs across csproj files (see DYN-10825 for the pattern).

## After Changes — Proof Checklist

Run what matches your change. Builds work on Windows (and `DynamoCore.sln` on Linux); run the `dotnet test` lines on Windows:
- [ ] `dotnet build src/DynamoCore.sln -c Release` — after any `src/DynamoCore*`, `src/Engine/`, or `src/Libraries/` change
- [ ] `msbuild src/Dynamo.All.sln /p:Configuration=Release` — after WPF/UI changes (Windows only)
- [ ] `msbuild src/Dynamo.All.sln /p:Configuration=Release /warnAsError:RS0016,RS0017 /p:PublicApiAnalyzers=true` — before opening a PR (what CI runs; fails on undeclared public API)
- [ ] `dotnet test test/DynamoCoreTests/DynamoCoreTests.csproj --filter "Category=UnitTests"` — after engine/core changes
- [ ] `dotnet test test/Libraries/<TestDir>/<TestProject>.csproj --filter "Category=UnitTests"` — after node-library changes (names vary: `ls test/Libraries`, e.g. `NodeServicesTest/DynamoServicesTests.csproj`)
- [ ] New public member → added to the project's `PublicAPI.Unshipped.txt`
- [ ] New node → `.dyn` + `.md` + `.jpg` under `doc/distrib/NodeHelpFiles/`
- [ ] User-facing string → moved to a `.resx` file

## Detailed Guidance

Read `.claude/` for comprehensive skills and templates:

- **Skills** (task workflows): `.claude/skills/<skill-name>/SKILL.md`
  - `dynamo-codebase-patterns` -- Discover and enforce Dynamo-specific architectural patterns
  - `dynamo-content-designer` -- Technical content for docs, guides, and tutorials
  - `dynamo-dotnet-expert` -- C#/.NET patterns, testing, PublicAPI
  - `dynamo-dotnet-janitor` -- Janitorial cleanup and modernization for C#/.NET code
  - `dynamo-ecosystem-reviewer` -- Cross-repo compatibility and platform constraints review
  - `dynamo-jira-ticket` -- creating/refining Jira tickets (includes template.md)
  - `dynamo-onboarding` -- Dynamo architecture, ecosystem, debugging
  - `dynamo-pr-description` -- PR descriptions matching Dynamo template (uses `.github/PULL_REQUEST_TEMPLATE.md`)
  - `dynamo-release-notes` -- Curate, sweep, draft, cross-check, and publish the GitHub wiki Release-Notes page for a Dynamo release
  - `dynamo-skill-writer` -- Author and maintain Dynamo agent skills
  - `dynamo-unit-testing` -- NUnit test writing following Dynamo patterns
  - `dynamo-ux-designer` -- UX planning and Weave-aligned interface design guidance
  - `dynamo-webview-component-scaffold` -- Scaffold Dynamo WebView2 view-extension repos

## Important Links

- [Dynamo Wiki](https://github.com/DynamoDS/Dynamo/wiki)
- [Dynamo Coding Standards](https://github.com/DynamoDS/Dynamo/wiki/Coding-Standards)
- [API Changes](https://github.com/DynamoDS/Dynamo/wiki/API-Changes)
- [Developer Resources](https://developer.dynamobim.org/)
- [`--NoNetworkMode` startup contract](doc/distrib/no-network-mode.md) -- what the no-network flag gates, and how WebView2 surfaces are hardened at startup
