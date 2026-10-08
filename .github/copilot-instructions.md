# Dynamo Repository Copilot Instructions

## Project Overview

Dynamo is a visual programming tool that aims to be accessible to both non-programmers and programmers alike. It gives users the ability to visually script behavior, define custom pieces of logic, and script using various textual programming languages. Dynamo is primarily developed in C# and WPF, with a focus on Windows compatibility, though the Dynamo engine (DynamoCore) can be built for Linux and macOS.

## Tech Stack

- **Primary Language**: C# (.NET 10)
- **UI Framework**: Windows Presentation Foundation (WPF)
- **Build System**: MSBuild and dotnet CLI
- **IDE**: Visual Studio 2022 (any edition)
- **Testing**: NUnit
- **Node.js**: Required for certain build steps
- **Target Platforms**: Windows (full UI), Linux and macOS (engine only)

## Build and Test Commands

### Building the Project

**Windows (Full Build):**
```bash
# Restore dependencies for Windows
dotnet restore src/Dynamo.All.sln --runtime=win-x64 -p:Configuration=Release -p:DotNet=net10.0

# Build with MSBuild
msbuild src/Dynamo.All.sln /p:Configuration=Release

# CI parity (what build_dynamo_all.yml runs) — PublicAPI analyzers become errors
msbuild src/Dynamo.All.sln /p:Configuration=Release /warnAsError:RS0016,RS0017 /p:PublicApiAnalyzers=true
```

**DynamoCore Only (Cross-Platform):**
```bash
# For Windows
dotnet restore src/DynamoCore.sln --runtime=win-x64 -p:Configuration=Release -p:DotNet=net10.0
msbuild src/DynamoCore.sln /p:Configuration=Release

# For Linux
dotnet restore src/DynamoCore.sln --runtime=linux-x64 -p:Configuration=Release -p:Platform=NET_Linux -p:DotNet=net10.0
dotnet build src/DynamoCore.sln -c Release /p:Platform=NET_Linux
```

**Static analysis (the lint step):** Dynamo has no separate linter. Static checks are the Roslyn analyzers the build runs — security rules CA2327/CA2328/CA2329/CA2330 are always errors; PublicAPI rules RS0016/RS0017 are errors only with the CI flags above (a plain local build only warns). Formatting rules live in `.editorconfig`. Run the CI-parity build before opening a PR.

**CI vs local:** `build_dynamo_all.yml` (Windows) runs the CI-parity build; `build_dynamo_core.yml` builds `DynamoCore.sln` on Linux but runs no tests there — `dotnet test` currently discovers no tests on Linux. Run `dotnet test` on Windows.

### Running Tests

Tests are located in the `test/` directory. Use Visual Studio Test Explorer or dotnet test CLI to run tests.

**Running a single test:**
```bash
# Filter by test name (substring match)
dotnet test test/DynamoCoreTests/DynamoCoreTests.csproj --filter "Name~MyTestClass"

# Filter by NUnit category
dotnet test test/DynamoCoreTests/DynamoCoreTests.csproj --filter "Category=UnitTests"

# Combine with & (AND) or | (OR)
dotnet test test/DynamoCoreTests/DynamoCoreTests.csproj --filter "Name~WhenCondition&Category=UnitTests"
```

UI tests are split across `DynamoCoreWpfTests`, `DynamoCoreWpf2Tests`, and `DynamoCoreWpf3Tests`.

## Code Style and Formatting

### Standards, XML Documentation and Analyzers

- **Coding Standards**: Follow the [Dynamo Coding Standards](https://github.com/DynamoDS/Dynamo/wiki/Coding-Standards)
- **Naming Standards**: Follow the [Dynamo Naming Standards](https://github.com/DynamoDS/Dynamo/wiki/Naming-Standards)
- **EditorConfig**: The repository includes `.editorconfig` with formatting rules: spaces (4-space indentation), LF line endings, UTF-8 encoding, trim trailing whitespace, insert final newline
- All public methods and properties **MUST** have XML documentation comments
- Use clear, concise descriptions that explain what the method does, its parameters, and return values
- RS0016/RS0017 are errors in CI (`/warnAsError:RS0016,RS0017`) and warnings in a plain local build
- Security analyzers are configured with error severity (CA2327, CA2329, CA2330, CA2328)

### Public API Management

- **RS0016 Mitigation**: All new public APIs must be declared in `PublicAPI.Unshipped.txt` files
- Public API files are located in project directories (e.g., `src/DynamoCore/PublicAPI.Unshipped.txt`)
- When adding new public types, methods, or properties, add the API signature to the appropriate `PublicAPI.Unshipped.txt` file in the format `namespace.ClassName.MemberName -> ReturnType`; entries are moved to `PublicAPI.Shipped.txt` upon release
- Existing PublicAPI files: DynamoCore, DynamoUtilities, DynamoCoreWpf, NodeServices

## Project Structure

```
Dynamo/
├── src/                          # Main source code
│   ├── DynamoCore.sln           # Core engine solution
│   ├── Dynamo.All.sln           # Complete solution with UI
│   ├── DynamoCore/              # Core engine
│   ├── DynamoCoreWpf/           # WPF UI components
│   ├── DynamoApplications/      # Application entry points
│   ├── Libraries/               # Node libraries
│   └── ...
├── test/                         # Unit and integration tests
├── doc/                          # Documentation
│   ├── distrib/NodeHelpFiles/   # Node documentation (.dyn, .md, .jpg)
│   └── integration_docs/        # Integration documentation
├── tools/                        # Build and utility tools
└── extern/                       # External dependencies
```

Area READMEs (layout, entry points, tests, contracts): [`src/DynamoCore/README.md`](../src/DynamoCore/README.md), [`src/Engine/README.md`](../src/Engine/README.md), [`src/Libraries/README.md`](../src/Libraries/README.md).

## Contribution Guidelines

### Pull Requests

- PR title must include the Jira ticket: `DYN-1234 concise summary`
- Use one of the [Dynamo PR templates](https://github.com/DynamoDS/Dynamo/wiki/Choosing-a-Pull-Request-Template)
- All template declarations must be satisfied; Release Notes section is mandatory (use `N/A` if not user-facing)
- Include unit tests when adding new features
- Start with a test that highlights broken behavior when fixing bugs

### API Compatibility

- **DO NOT** introduce breaking changes to the public API; follow semantic versioning and keep backwards compatibility
- File an issue before proposing API changes
- Breaking = removed/renamed public members, reduced accessibility, or changed signatures/return types; if unavoidable, version appropriately and document it in the changelog
- If build requirements change, update README.md

## Node Development

### Node Registration Patterns

- **Zero-touch** (static methods) — preferred for pure computation. Place static methods in a class under `src/Libraries/`. Namespace becomes the library category. Use XML `<search>` tags for keywords.
- **Explicit NodeModel** — required for custom UI, dynamic ports, or multi-output nodes. Inherit from `NodeModel` in `src/Libraries/CoreNodeModels/`. Requires two constructors — a `[JsonConstructor]` private one and a public parameterless one.
- Use `[AlsoKnownAs("OldName")]` when renaming nodes to preserve backward compatibility.
- Full code examples (attributes, constructors, `BuildOutputAst`): [`AGENTS.md` § Node Registration Patterns](../AGENTS.md#node-registration-patterns).
- **New nodes**: provide a `.dyn` file (sample graph demonstrating usage), a `.md` file (markdown documentation) and a `.jpg` file (visual preview/screenshot) in `doc/distrib/NodeHelpFiles/`.

### Localization

- New user-facing strings **MUST** be added to appropriate `.resx` files
- UI changes should be documented with screenshots

### Central Package Management

- NuGet package versions for `PackageReference` projects live only in `Directory.Packages.props` at the repo root
- Legacy `packages.config` projects (e.g. `tools/DSTestCaseConverter`) are outside CPM and keep their versions locally
- `PackageReference` entries in csproj files must omit `Version`; bump a dependency by editing the props file
- Conditions selecting *which* package to reference (e.g. LibG Debug/Release in `DynamoCore.csproj`) stay in the csproj

## Security and Restrictions

### Security Rules

- **NEVER** commit secrets or credentials to source code
- **DO NOT** introduce new network connections without explicit documentation and no-network mode testing
- **DO NOT** add data collection without proper user consent checks and documentation
- Security analyzer warnings for XML-related vulnerabilities (CA2327, CA2329, CA2330, CA2328) are treated as errors
- Code changes should contain no files larger than 50 MB (validated by the `check_file_size.yml` workflow)

## Agent Skills and Templates

For detailed task workflows, rules, and templates, see `.claude/README.md`:

- **Skills**: each in `.claude/skills/<name>/SKILL.md` -- dynamo-codebase-patterns, dynamo-content-designer, dynamo-dotnet-expert, dynamo-dotnet-janitor, dynamo-ecosystem-reviewer, dynamo-onboarding, dynamo-pr-description, dynamo-jira-ticket, dynamo-release-notes, dynamo-skill-writer, dynamo-unit-testing, dynamo-ux-designer, dynamo-webview-component-scaffold
- **Templates**: bundled inside skill folders as `template.md` (Jira)

## Debugging Quick Start

- Logs go to `%AppData%\Dynamo\Dynamo Core\<major>.<minor>\Logs\` (logging surface: `DynamoModel.Logger`); debug by attaching VS to `DynamoSandbox`, with `--NoNetworkMode` for repro isolation (see [no-network-mode.md](../doc/distrib/no-network-mode.md)). First breakpoints for node evaluation: `EngineController` and `AstBuilder` under `src/DynamoCore/Engine/`.
- A 403 from the `team-dynamo-nuget` Artifactory feed (`dynamo-nuget.config`) is a credentials gate, not a code problem.
- Full detail (log file names, viewer, common failures): [`AGENTS.md` § Debugging Quick Start](../AGENTS.md#debugging-quick-start).

## Blast Radius

- **Public API**: `src/*/PublicAPI.{Shipped,Unshipped}.txt` (DynamoCore, DynamoCoreWpf, DynamoUtilities, NodeServices) — RS0016/RS0017 fail **CI** on undeclared changes; a plain local build only warns. Breaking changes require an issue + SemVer.
- **Published contracts**: `src/DynamoCore`, `src/DynamoCoreWpf` and `src/Libraries` ship in the `DynamoVisualProgramming.*` NuGet packages (`tools/NuGet/template-nuget/`) to external consumers; `.dyn` files are a public format (`doc/dyn-file-spec.md`), so node serialization changes affect every saved graph.
- **Cross-boundary edits**: `src/Engine/` changes ripple into every evaluation path; `src/Libraries/` node changes require matching `doc/distrib/NodeHelpFiles/` entries; `extern/` (LibG/ASM) version bumps are coordinated PRs across csproj files.
- Full detail (package list, JSON Schema, the DYN-10825 pattern): [`AGENTS.md` § Blast Radius](../AGENTS.md#blast-radius).

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

## Important Documentation

- [Dynamo Wiki](https://github.com/DynamoDS/Dynamo/wiki)
- [Dynamo Coding Standards](https://github.com/DynamoDS/Dynamo/wiki/Coding-Standards)
- [Dynamo Naming Standards](https://github.com/DynamoDS/Dynamo/wiki/Naming-Standards)
- [API Changes](https://github.com/DynamoDS/Dynamo/wiki/API-Changes)
- [Zero-Touch Plugin Development](https://github.com/DynamoDS/Dynamo/wiki/Zero-Touch-Plugin-Development)
- [Developer Resources](https://developer.dynamobim.org/)
- [Dynamo Samples](https://github.com/DynamoDS/DynamoSamples)
- [Contributing Guide](../CONTRIBUTING.md)
