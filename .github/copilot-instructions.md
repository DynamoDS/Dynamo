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

### Running Tests

Tests are located in the `test/` directory. Use Visual Studio Test Explorer or dotnet test CLI to run tests.

## Code Style and Formatting

### Follow Existing Standards

- **Coding Standards**: Follow the [Dynamo Coding Standards](https://github.com/DynamoDS/Dynamo/wiki/Coding-Standards)
- **Naming Standards**: Follow the [Dynamo Naming Standards](https://github.com/DynamoDS/Dynamo/wiki/Naming-Standards)
- **EditorConfig**: The repository includes `.editorconfig` with formatting rules:
  - Use spaces (4-space indentation)
  - LF line endings
  - UTF-8 encoding
  - Trim trailing whitespace
  - Insert final newline

### XML Documentation

- All public methods and properties **MUST** have XML documentation comments
- Use clear, concise descriptions that explain what the method does, its parameters, and return values

### Code Analysis

- The project uses Roslyn analyzers with specific rules enabled
- Warning-as-error is enabled for specific analyzers (RS0016, RS0017)
- Security analyzers are configured with error severity (CA2327, CA2329, CA2330, CA2328)

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

## Contribution Guidelines

### Pull Requests

- Use one of the [Dynamo PR templates](https://github.com/DynamoDS/Dynamo/wiki/Choosing-a-Pull-Request-Template)
- All template declarations must be satisfied
- Include unit tests when adding new features
- Start with a test that highlights broken behavior when fixing bugs
- PRs are reviewed monthly, oldest to newest
- PR owners have 30 days to respond to feedback

### API Compatibility

- **DO NOT** introduce breaking changes to the public API
- Follow semantic versioning
- Maintain backwards compatibility
- File an issue before proposing API changes

## Node Development

### Detecting New Node Additions

A new node addition is identified by:
- Methods marked with `[IsVisibleInDynamoLibrary(true)]`
- Commit messages or PR descriptions mentioning new nodes

### Required Documentation for New Nodes

For each new node, provide in `doc/distrib/NodeHelpFiles/`:
- A `.dyn` file (sample graph demonstrating usage)
- A `.md` file (markdown documentation)
- A `.jpg` file (visual preview/screenshot)

### Localization

- New user-facing strings **MUST** be added to appropriate `.resx` files
- UI changes should be documented with screenshots

## Security and Restrictions

### Security Rules

- **NEVER** commit secrets or credentials to source code
- **DO NOT** introduce new network connections without explicit documentation and no-network mode testing
- **DO NOT** add data collection without proper user consent checks and documentation
- Security analyzer warnings for XML-related vulnerabilities (CA2327, CA2329, CA2330, CA2328) are treated as errors

### File Size Limits

- Code changes should contain no files larger than 50 MB
- The check_file_size.yml workflow validates this

### Build Requirements Updates

- If build requirements change, update README.md to reflect new dependencies

## API-Breaking Changes

Alert contributors if changes include:
- Removed or renamed public methods or properties
- Changes in method accessibility (e.g., public to private)
- Modified method signatures or return types

**Required Actions:**
- Update versioning appropriately
- Document changes clearly in the changelog
- Follow API compatibility guidelines

## Important Documentation

- [Dynamo Wiki](https://github.com/DynamoDS/Dynamo/wiki)
- [Dynamo Coding Standards](https://github.com/DynamoDS/Dynamo/wiki/Coding-Standards)
- [Dynamo Naming Standards](https://github.com/DynamoDS/Dynamo/wiki/Naming-Standards)
- [API Changes](https://github.com/DynamoDS/Dynamo/wiki/API-Changes)
- [Zero-Touch Plugin Development](https://github.com/DynamoDS/Dynamo/wiki/Zero-Touch-Plugin-Development)
- [Developer Resources](https://developer.dynamobim.org/)
- [Dynamo Samples](https://github.com/DynamoDS/DynamoSamples)
- [Contributing Guide](CONTRIBUTING.md)