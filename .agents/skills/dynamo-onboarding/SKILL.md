---
name: dynamo-onboarding
description: Navigate the Dynamo codebase and produce architecture briefings. Use this skill when learning Dynamo for the first time, when you get a Jira ticket and don't know where to start, when you need to understand how Dynamo's architecture fits together, or when onboarding a teammate. Also use for questions like "where does node evaluation happen?" or "how does the DesignScript engine work?"
---

# Dynamo Onboarding

## When to use

- Learning the Dynamo codebase for the first time.
- Answering "where do I start?" for a Jira ticket in Dynamo.
- Understanding how Dynamo's architecture fits together.
- Producing an architecture briefing for a teammate.

## When not to use

- Writing or reviewing C# code -- use [dynamo-dotnet-expert](../dynamo-dotnet-expert/SKILL.md).
- Architecture / onboarding for DynamoMCP -- that lives in the DynamoMCP repo.

## Inputs expected

A question like "how does X work?", a Jira ticket number, or a request for an architecture brief.

## Output format

A structured briefing following the sections below. Skip sections that don't apply.

---

## Briefing Structure

1. **System purpose** (1 sentence)
2. **Tech stack** (3-6 bullets)
3. **Core components** (component -> responsibility)
4. **Repo map** (key folders and what lives in each)
5. **Flow** (5-9 ordered steps from input to output)
6. **Data contracts** (input/output models, key payload shapes)
7. **Start here** (top 5-8 files to read first)
8. **Pitfalls** (up to 5 common mistakes)
9. **Confidence and unknowns** (what is confirmed vs what needs verification)

## How to build the brief

- Start from entry points (`DynamoModel`, `DynamoSandbox`, extension loader, test harness).
- Trace one real execution path end-to-end (user action -> node evaluation -> engine -> result).
- Name interfaces and concrete classes involved.
- Explicitly list assumptions when code evidence is partial.
- Prefer concise, concrete wording over broad theory.

---

## Project Description

Dynamo is a visual programming tool accessible to non-programmers and programmers alike. Users can visually script behavior, define custom logic, and script using textual programming languages. C# + WPF, .NET 10. Core engine (`DynamoCore.sln`) is cross-platform; full UI (`Dynamo.All.sln`) is Windows-only.

**Build:**
```bash
# Full (Windows)
dotnet restore src/Dynamo.All.sln --runtime=win-x64 -p:Configuration=Release -p:DotNet=net10.0
msbuild src/Dynamo.All.sln /p:Configuration=Release

# Core only (cross-platform)
dotnet build src/DynamoCore.sln -c Release
```

**Test:** NUnit. `dotnet test` or Visual Studio Test Explorer.

**Key areas:**

| Area | Path |
|------|------|
| Core engine | `src/DynamoCore/` |
| WPF UI | `src/DynamoCoreWpf/` |
| Node libraries | `src/Libraries/` (CoreNodes, PythonNodeModels, etc.) |
| Engine (DS compiler) | `src/Engine/` (ProtoCore, ProtoAssociative, etc.) |
| Applications | `src/DynamoApplications/`, `src/DynamoSandbox/` |
| View extensions | `src/` (GraphNodeManager, PackageDetails, DocumentationBrowser, etc.) |
| Tests | `test/DynamoCoreTests/`, `test/Libraries/`, `test/Engine/` |
| PublicAPI tracking | `src/DynamoCore/PublicAPI.*.txt`, `src/DynamoCoreWpf/PublicAPI.*.txt` |
| Node help docs | `doc/distrib/NodeHelpFiles/` |

---

## Architecture Overview

### Core Model

`DynamoModel` is the central class -- it owns the workspace, manages state transitions, and coordinates node evaluation. Key states: `NotStarted`, `StartedUI`, `StartedUIless`.

### Workspace

`WorkspaceModel` holds the graph: nodes, connectors, groups, notes. `HomeWorkspaceModel` extends it with execution (run) support. `CustomNodeWorkspaceModel` represents reusable sub-graphs.

### Nodes

`NodeModel` is the base class for all nodes. Built-in nodes live in `src/Libraries/`. Custom nodes can be created via Zero-Touch (annotated C# methods) or NodeModel subclassing.

### Engine

The DesignScript engine (`src/Engine/`) compiles and evaluates the visual graph. `ProtoCore` handles AST construction, `ProtoScript` runs the execution.

### Extensions

Dynamo supports view extensions (`IViewExtension`) and extensions (`IExtension`). View extensions can add UI panels and interact with the workspace. Examples: GraphNodeManager, PackageDetails, DocumentationBrowser.

---

## Related Projects

| Repo | Role | How it connects to Dynamo |
|------|------|---------------------------|
| **DynamoMCP** | MCP server extension | Exposes Dynamo workspace operations as MCP tools; loads as a view extension |
| **DynamoPlayer** | Graph execution scheduler | Prepares and runs Dynamo graphs; uses Dynamo NuGet packages |
| **Analytics.NET** | Telemetry tracking | Unified event tracking framework used by Dynamo |
| **NodeAutocompleteService** | ML node discovery | Node autocomplete ML service for Dynamo |
| **LibG** | Geometry kernel wrappers | SWIG wrappers around Autodesk Shape Manager (ASM) |
| **DynamoAAS** | Dynamo as a Service | Server-side Dynamo graph execution |
| **DynamoAGTTests** | UI tests | Automated Dynamo UI testing using AGT framework |

---

## Debugging Approaches

### Build failing

1. Check `global.json` SDK version: `.NET 10 SDK` required.
2. Windows full build needs `msbuild` (Visual Studio 2022).
3. Cross-platform build uses `dotnet build` with `DynamoCore.sln` only.
4. Check NuGet feeds: some packages come from internal feeds.

### Test failing

1. Check the test project matches the source project (e.g., `DynamoCoreTests` tests `DynamoCore`).
2. Tests use NUnit -- look for `[Test]`, `[TestCase]`, `[SetUp]` attributes.
3. Some tests need Dynamo running (`DynamoModelTestBase` sets up a headless instance).
4. Check for test data files in `test/core/` that tests may depend on.

### Node not appearing in library

1. Check for `[IsVisibleInDynamoLibrary(true)]` attribute on the method.
2. Verify the assembly is referenced and loaded by Dynamo.
3. Check node naming: does the namespace and method name match expected layout?

### PublicAPI analyzer errors (RS0016/RS0017)

1. RS0016: add the new public API signature to `PublicAPI.Unshipped.txt`.
2. RS0017: the member was removed from the shipped surface -- restore or follow the breaking change process.

---

## "Start Here" Files

For Dynamo newcomers:
1. `src/DynamoCore/Models/DynamoModel.cs` -- central model, state machine
2. `src/DynamoCore/Graph/Workspaces/WorkspaceModel.cs` -- workspace operations
3. `src/DynamoCore/Graph/Nodes/NodeModel.cs` -- node base class
4. `src/Libraries/CoreNodes/` -- built-in node implementations
5. `src/DynamoCoreWpf/` -- UI components
6. `src/Engine/ProtoCore/` -- DesignScript engine core
7. `.github/copilot-instructions.md` -- build/test/style quick reference

---

**Example 1: "I got ticket DYN-5678 to add a new node for string interpolation"**

Briefing output:
- String operations live in `src/Libraries/CoreNodes/`.
- Look at existing string nodes for the pattern (e.g., `String.cs`).
- New node needs `[IsVisibleInDynamoLibrary(true)]` and XML documentation.
- Add help files: `.dyn` sample, `.md` docs, `.jpg` preview in `doc/distrib/NodeHelpFiles/`.
- Tests go in `test/Libraries/CoreNodesTests/`.

**Example 2: "Where does node evaluation happen?"**

Briefing output:
- `HomeWorkspaceModel.Run()` triggers evaluation.
- The graph is compiled to DesignScript AST via the engine in `src/Engine/`.
- `ProtoCore` handles AST construction and type resolution.
- Each node's `BuildAst()` method generates its AST contribution.
- Results flow back through `NodeModel.CachedValue`.
