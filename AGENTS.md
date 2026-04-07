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

# Core only (cross-platform, Windows)
dotnet restore src/DynamoCore.sln --runtime=win-x64 -p:Configuration=Release -p:DotNet=net10.0
msbuild src/DynamoCore.sln /p:Configuration=Release

# Core only (Linux)
dotnet restore src/DynamoCore.sln --runtime=linux-x64 -p:Configuration=Release -p:Platform=NET_Linux -p:DotNet=net10.0
dotnet build src/DynamoCore.sln -c Release /p:Platform=NET_Linux
```

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

## Key Conventions

- Follow [Dynamo Coding Standards](https://github.com/DynamoDS/Dynamo/wiki/Coding-Standards) and [Naming Standards](https://github.com/DynamoDS/Dynamo/wiki/Naming-Standards).
- XML documentation required on all public methods and properties.
- New public APIs must be added to `PublicAPI.Unshipped.txt` (format: `namespace.ClassName.MemberName -> ReturnType`).
- Security analyzers CA2327/CA2329/CA2330/CA2328 are errors.
- NUnit for all tests. Do not introduce xUnit or MSTest.
- Test naming: `WhenConditionThenExpectedBehavior`. One behavior per test, Arrange-Act-Assert.
- User-facing strings in `.resx` files.
- No files > 50 MB.
- Preserve existing line endings when editing — do not convert. The `.editorconfig` specifies LF, but many files have CRLF from Windows development. New files should follow `.editorconfig` (LF); the Write tool on macOS may need explicit attention.

## Running a Single Test

```bash
# Filter by test name (substring match)
dotnet test src/DynamoCoreTests/DynamoCoreTests.csproj --filter "Name~MyTestClass"

# Filter by NUnit category
dotnet test src/DynamoCoreTests/DynamoCoreTests.csproj --filter "Category=UnitTests"

# Combine with & (AND) or | (OR)
dotnet test src/DynamoCoreTests/DynamoCoreTests.csproj --filter "Name~WhenCondition&Category=UnitTests"
```

UI tests are split across `DynamoCoreWpfTests`, `DynamoCoreWpfTests2`, and `DynamoCoreWpfTests3`.

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

- PR title must include Jira ticket: `DYN-1234 concise summary`.
- Fill all sections of `.github/PULL_REQUEST_TEMPLATE.md`. Release Notes is mandatory — use `N/A` if not user-facing (minimum 6 words otherwise).
- Do not introduce breaking API changes without filing an issue and following [Semantic Versioning](https://github.com/DynamoDS/Dynamo/wiki/Dynamo-Versions).

## New Nodes

For each new node, add to `doc/distrib/NodeHelpFiles/`:
- A `.dyn` sample graph
- A `.md` documentation file
- A `.jpg` visual preview

## Detailed Guidance

Read `.agents/` for comprehensive skills, rules, and templates:

- **Skills** (task workflows): `.agents/skills/<skill-name>/SKILL.md`
  - `dynamo-codebase-patterns` -- Discover and enforce Dynamo-specific architectural patterns
  - `dynamo-content-designer` -- Technical content for docs, guides, and release notes
  - `dynamo-dotnet-expert` -- C#/.NET patterns, testing, PublicAPI
  - `dynamo-dotnet-janitor` -- Janitorial cleanup and modernization for C#/.NET code
  - `dynamo-ecosystem-reviewer` -- Cross-repo compatibility and platform constraints review
  - `dynamo-jira-ticket` -- creating/refining Jira tickets (includes template.md)
  - `dynamo-onboarding` -- Dynamo architecture, ecosystem, debugging
  - `dynamo-pr-description` -- PR descriptions matching Dynamo template (uses `.github/PULL_REQUEST_TEMPLATE.md`)
  - `dynamo-skill-writer` -- Author and maintain Dynamo agent skills
  - `dynamo-unit-testing` -- NUnit test writing following Dynamo patterns
  - `dynamo-ux-designer` -- UX planning and Weave-aligned interface design guidance
  - `dynamo-webview-component-scaffold` -- Scaffold Dynamo WebView2 view-extension repos
- **Rules** (constraints): `.agents/rules/`
  - `dynamo-core-rules.md` -- .NET/Dynamo constraints

## Important Links

- [Dynamo Wiki](https://github.com/DynamoDS/Dynamo/wiki)
- [Dynamo Coding Standards](https://github.com/DynamoDS/Dynamo/wiki/Coding-Standards)
- [API Changes](https://github.com/DynamoDS/Dynamo/wiki/API-Changes)
- [Developer Resources](https://developer.dynamobim.org/)
